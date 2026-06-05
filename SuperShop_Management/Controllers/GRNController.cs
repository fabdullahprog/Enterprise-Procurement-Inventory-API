using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.DTOs.GRN;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;
using SuperShop_Management.Data;
using System.Security.Claims;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GRNController : ControllerBase
    {
        private readonly IGRNRepository _grnRepo;
        private readonly IPurchaseOrderRepository _poRepo;
        private readonly IPOItemRepository _poItemRepo;
        private readonly IInventoryRepository _inventoryRepo;
        private readonly IBatchRepository _batchRepo;
        private readonly IProductRepository _productRepo;
        private readonly IStockMovementRepository _stockMovementRepo;
        private readonly AppDbContext _dbContext;
        private readonly UserManager<IdentityUser<int>> _userManager;

        public GRNController(
            IGRNRepository grnRepo,
            IPurchaseOrderRepository poRepo,
            IPOItemRepository poItemRepo,
            IInventoryRepository inventoryRepo,
            IBatchRepository batchRepo,
            IProductRepository productRepo,
            IStockMovementRepository stockMovementRepo,
            AppDbContext dbContext,
            UserManager<IdentityUser<int>> userManager)
        {
            _grnRepo = grnRepo;
            _poRepo = poRepo;
            _poItemRepo = poItemRepo;
            _inventoryRepo = inventoryRepo;
            _batchRepo = batchRepo;
            _productRepo = productRepo;
            _stockMovementRepo = stockMovementRepo;
            _dbContext = dbContext;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var grns = await _grnRepo.GetActiveGRNsAsync();
            var response = grns.Select(MapToResponse);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var grn = await _grnRepo.GetByIdAsync(id);
            if (grn == null || !grn.IsActive)
                return NotFound(new { message = "GRN not found" });
            return Ok(MapToResponse(grn));
        }

        // POST: api/GRN/direct-receive
        [HttpPost("direct-receive")]
        [Authorize(Roles = "Admin, StoreManager, WarehouseManager")]
        public async Task<IActionResult> DirectReceive([FromBody] DirectReceiveDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "User not authenticated" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            // 1. Validate PO
            var purchaseOrder = await _poRepo.GetByIdAsync(dto.PurchaseOrderId);
            if (purchaseOrder == null || !purchaseOrder.IsActive)
                return BadRequest(new { success = false, message = "Invalid Purchase Order" });

            if (purchaseOrder.Status != "Sent" && purchaseOrder.Status != "Approved")
                return BadRequest(new { success = false, message = "Only 'Sent' or 'Approved' Purchase Orders can be received" });

            var existing = await _grnRepo.GetByPurchaseOrderIdAsync(dto.PurchaseOrderId);
            if (existing.Any())
                return BadRequest(new { success = false, message = "A GRN already exists for this Purchase Order" });

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var grnNumber = await GenerateGRNNumber();

                // 2. Create GRN Header
                var grn = new GRN
                {
                    GRNNumber = grnNumber,
                    ReceivedDate = DateTime.Now,
                    ReceiveType = "full",
                    Status = "Approved", // Instant approval
                    ReceivedQuantity = dto.Items.Sum(i => i.ReceivedQuantity),
                    VehicleNumber = dto.VehicleNumber,
                    DeliveryPersonName = dto.DeliveryPersonName,
                    Notes = dto.Notes,
                    PurchaseOrderId = dto.PurchaseOrderId,
                    ReceivedById = int.Parse(userId),
                    StoreApprovedById = int.Parse(userId),
                    StoreApprovedAt = DateTime.Now,
                    IsActive = true,
                    CreatedBy = user.Email ?? user.UserName,
                    CreatedDate = DateTime.Now
                };

                await _grnRepo.AddAsync(grn);
                await _dbContext.SaveChangesAsync(); // Need GRN Id for children

                // 3. Process Items and Inventory
                foreach (var itemDto in dto.Items)
                {
                    var poItem = await _poItemRepo.GetByIdAsync(itemDto.POItemId);
                    if (poItem == null || !poItem.IsActive)
                        throw new Exception($"POItem {itemDto.POItemId} not found");

                    if (itemDto.AcceptedQuantity > itemDto.ReceivedQuantity)
                        throw new Exception($"Accepted quantity ({itemDto.AcceptedQuantity}) cannot exceed received quantity ({itemDto.ReceivedQuantity}).");

                    var rejectedQty = itemDto.ReceivedQuantity - itemDto.AcceptedQuantity;

                    var grnItem = new GRNItem
                    {
                        GRNId = grn.Id,
                        POItemId = itemDto.POItemId,
                        ReceivedQuantity = itemDto.ReceivedQuantity,
                        AcceptedQuantity = itemDto.AcceptedQuantity,
                        RejectedQuantity = rejectedQty,
                        Condition = itemDto.Condition ?? "good",
                        Remarks = itemDto.Remarks,
                        IsActive = true,
                        CreatedBy = user.Email ?? user.UserName,
                        CreatedDate = DateTime.Now
                    };
                    
                    _dbContext.GRNItems.Add(grnItem);

                    if (itemDto.AcceptedQuantity > 0)
                    {
                        var product = await _productRepo.GetByIdAsync(poItem.ProductId);
                        if (product == null) throw new Exception($"Product {poItem.ProductId} not found");

                        var batchNumber = await GenerateBatchNumber();
                        var batch = new Batch
                        {
                            BatchNumber = batchNumber,
                            ManufacturingDate = DateTime.Now.AddMonths(-1),
                            ExpiryDate = DateTime.Now.AddYears(1),
                            ReceivedQuantity = itemDto.AcceptedQuantity,
                            RemainingQuantity = itemDto.AcceptedQuantity,
                            Status = "Active",
                            ProductId = poItem.ProductId,
                            SupplierId = purchaseOrder.SupplierId,
                            GRNId = grn.Id,
                            IsActive = true,
                            CreatedBy = user.Email ?? user.UserName,
                            CreatedDate = DateTime.Now
                        };
                        await _batchRepo.AddAsync(batch);
                        await _dbContext.SaveChangesAsync(); // Need Batch Id

                        var inventory = new Inventory
                        {
                            ProductId = poItem.ProductId,
                            AvailableQuantity = itemDto.AcceptedQuantity,
                            GrnQuantity = itemDto.AcceptedQuantity, // Preserve original GRN received quantity
                            ReservedQuantity = 0,
                            MinQuantity = 10,
                            MaxQuantity = 1000,
                            LastUpdated = DateTime.Now,
                            BatchId = batch.Id,
                            GRNId = grn.Id,
                            
                            WarehouseId = dto.WarehouseId,
                            FloorId = dto.FloorId,
                            ZoneId = dto.ZoneId,
                            AisleId = dto.AisleId,
                            RackId = dto.RackId,
                            ShelfId = dto.ShelfId,
                            BinId = dto.BinId,

                            IsActive = true,
                            CreatedBy = user.Email ?? user.UserName,
                            CreatedDate = DateTime.Now
                        };
                        await _inventoryRepo.AddAsync(inventory);
                        await _dbContext.SaveChangesAsync(); // Need Inventory Id

                        var stockMovement = new StockMovement
                        {
                            InventoryId = inventory.Id,
                            MovementType = "GRN_IN",
                            Direction = "IN",
                            Quantity = itemDto.AcceptedQuantity,
                            Reason = "Direct Goods Received Note",
                            RelatedDocumentId = grn.Id,
                            RelatedDocumentType = "GRN",
                            CreatedById = int.Parse(userId),
                            CreatedBy = user.Email ?? user.UserName,
                            CreatedDate = DateTime.Now,
                            IsActive = true,
                            
                            ToWarehouseId = dto.WarehouseId,
                            ToFloorId = dto.FloorId,
                            ToZoneId = dto.ZoneId,
                            ToAisleId = dto.AisleId,
                            ToRackId = dto.RackId,
                            ToShelfId = dto.ShelfId,
                            ToBinId = dto.BinId
                        };
                        await _stockMovementRepo.AddAsync(stockMovement);

                        product.CurrentStock += itemDto.AcceptedQuantity;
                        _productRepo.Update(product);
                    }
                }

                // 4. Update PO Status
                purchaseOrder.Status = "Received";
                _poRepo.Update(purchaseOrder);

                await _dbContext.SaveChangesAsync();
                
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "Goods successfully received and inventory updated in a single transaction." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = $"An error occurred during GRN processing: {ex.Message}. The operation has been rolled back completely." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin, StoreManager, WarehouseManager")]
        public async Task<IActionResult> Create([FromBody] GRNRequestDto dto)
        {
            // Validate Purchase Order
            var purchaseOrder = await _poRepo.GetByIdAsync(dto.PurchaseOrderId);
            if (purchaseOrder == null || !purchaseOrder.IsActive)
                return BadRequest(new { message = "Invalid Purchase Order" });

            if (purchaseOrder.Status != "Sent")
                return BadRequest(new { message = "Only 'Sent' Purchase Orders can be received" });

            // Check if GRN already exists for this PO
            var existing = await _grnRepo.GetByPurchaseOrderIdAsync(dto.PurchaseOrderId);
            if (existing.Any())
                return BadRequest(new { message = "A GRN already exists for this Purchase Order" });

            // Get current user
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();
            var user = await _userManager.FindByIdAsync(currentUserId);
            if (user == null) return Unauthorized();

            // Validate each item: POItem exists and not already fully received
            foreach (var itemDto in dto.Items)
            {
                var poItem = await _poItemRepo.GetByIdAsync(itemDto.POItemId);
                if (poItem == null || !poItem.IsActive)
                    return BadRequest(new { message = $"POItem {itemDto.POItemId} not found" });

                if (itemDto.ReceivedQuantity > poItem.OrderedQuantity)
                    return BadRequest(new { message = $"Received quantity exceeds ordered quantity for product {poItem.Product?.Name}" });

                // Check if this POItem was already received in previous GRN (if partial receive allowed)
                // Here we can implement later
            }

            var grnNumber = await GenerateGRNNumber();

            var grn = new GRN
            {
                GRNNumber = grnNumber,
                ReceivedDate = DateTime.Now,
                ReceiveType = dto.ReceiveType ?? "full",
                Status = "Draft",  // Changed from Pending to Draft
                ReceivedQuantity = dto.Items.Sum(i => i.ReceivedQuantity),
                VehicleNumber = dto.VehicleNumber,
                DeliveryPersonName = dto.DeliveryPersonName,
                Notes = dto.Notes,
                PurchaseOrderId = dto.PurchaseOrderId,
                ReceivedById = int.Parse(currentUserId),
                IsActive = true,
                CreatedBy = user.Email ?? user.UserName,
                CreatedDate = DateTime.Now
            };

            // Add GRN Items
            foreach (var itemDto in dto.Items)
            {
                var poItem = await _poItemRepo.GetByIdAsync(itemDto.POItemId);
                var grnItem = new GRNItem
                {
                    POItemId = itemDto.POItemId,
                    ReceivedQuantity = itemDto.ReceivedQuantity,
                    AcceptedQuantity = 0,   // Will be set during approval
                    RejectedQuantity = 0,   // Will be set during approval
                    Condition = itemDto.Condition ?? "good",  // good, damaged, partial
                    Remarks = itemDto.Remarks,
                    IsActive = true,
                    CreatedBy = user.Email ?? user.UserName,
                    CreatedDate = DateTime.Now
                };
                grn.GRNItems.Add(grnItem);
            }

            await _grnRepo.AddAsync(grn);
            await _grnRepo.SaveChangesAsync();

            return Ok(new { success = true, message = "GRN created successfully (Draft)", data = new { id = grn.Id, number = grn.GRNNumber } });
        }

        // PATCH: api/GRN/{id}/submit
        [HttpPatch("{id}/submit")]
        public async Task<IActionResult> Submit(int id)
        {
            var grn = await _grnRepo.GetByIdAsync(id);
            if (grn == null || !grn.IsActive)
                return NotFound(new { success = false, message = "GRN not found" });

            if (grn.Status != "Draft")
                return BadRequest(new { success = false, message = "Only draft GRNs can be submitted" });

            grn.Status = "PendingStoreApproval";
            grn.UpdatedBy = User.Identity?.Name;
            grn.UpdatedDate = DateTime.Now;

            _grnRepo.Update(grn);
            await _grnRepo.SaveChangesAsync();

            return Ok(new { success = true, message = "GRN submitted for approval" });
        }

        // PATCH: api/GRN/{id}/approve
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Admin, StoreManager, WarehouseManager")]
        public async Task<IActionResult> Approve(int id, [FromBody] ApproveGRNDto locationDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "User not authenticated" });

            var grn = await _grnRepo.GetByIdAsync(id);
            if (grn == null || !grn.IsActive)
                return NotFound(new { success = false, message = "GRN not found" });

            if (grn.Status != "PendingStoreApproval")
                return BadRequest(new { success = false, message = "Only pending GRNs can be approved" });

            // Wrap in a Transaction
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // CRITICAL: Process Inventory and StockMovement based on strictly Accepted QC Quantities
                foreach (var itemDto in locationDto.Items)
                {
                    var grnItem = grn.GRNItems.FirstOrDefault(i => i.Id == itemDto.GRNItemId && i.IsActive);
                    if (grnItem == null) continue;

                    if (itemDto.AcceptedQuantity > grnItem.ReceivedQuantity)
                        return BadRequest(new { success = false, message = $"Accepted quantity ({itemDto.AcceptedQuantity}) cannot exceed received quantity ({grnItem.ReceivedQuantity}) for item {grnItem.Id}." });

                    // Update QC math
                    grnItem.AcceptedQuantity = itemDto.AcceptedQuantity;
                    grnItem.RejectedQuantity = grnItem.ReceivedQuantity - itemDto.AcceptedQuantity;

                    // Only update inventory if there are accepted items
                    if (grnItem.AcceptedQuantity > 0)
                    {
                        var poItem = await _poItemRepo.GetByIdAsync(grnItem.POItemId);
                        if (poItem == null) continue;

                        var product = await _productRepo.GetByIdAsync(poItem.ProductId);
                        if (product == null) continue;

                        // Create batch for this GRN item
                        var batchNumber = await GenerateBatchNumber();
                        var batch = new Batch
                        {
                            BatchNumber = batchNumber,
                            ManufacturingDate = DateTime.Now.AddMonths(-1), // Default, should come from DTO
                            ExpiryDate = DateTime.Now.AddYears(1), // Default, should come from DTO
                            ReceivedQuantity = grnItem.AcceptedQuantity, // Only track accepted in batch
                            RemainingQuantity = grnItem.AcceptedQuantity,
                            Status = "Active",
                            ProductId = poItem.ProductId,
                            SupplierId = grn.PurchaseOrder?.SupplierId ?? 1,
                            GRNId = grn.Id,
                            IsActive = true,
                            CreatedBy = User.Identity?.Name,
                            CreatedDate = DateTime.Now
                        };
                        await _batchRepo.AddAsync(batch);
                        await _dbContext.SaveChangesAsync(); // Save to get Batch Id

                        // Create or update inventory
                        var inventory = new Inventory
                        {
                            ProductId = poItem.ProductId,
                            AvailableQuantity = grnItem.AcceptedQuantity, // Increment only by accepted
                            GrnQuantity = grnItem.AcceptedQuantity, // Preserve original GRN received quantity
                            ReservedQuantity = 0,
                            MinQuantity = 10, // Default, should be configurable
                            MaxQuantity = 1000, // Default, should be configurable
                            LastUpdated = DateTime.Now,
                            BatchId = batch.Id,
                            GRNId = grn.Id,
                            
                            // Target Location
                            WarehouseId = locationDto.WarehouseId,
                            FloorId = locationDto.FloorId,
                            ZoneId = locationDto.ZoneId,
                            AisleId = locationDto.AisleId,
                            RackId = locationDto.RackId,
                            ShelfId = locationDto.ShelfId,
                            BinId = locationDto.BinId,

                            IsActive = true,
                            CreatedBy = User.Identity?.Name,
                            CreatedDate = DateTime.Now
                        };
                        await _inventoryRepo.AddAsync(inventory);
                        await _dbContext.SaveChangesAsync(); // Save to get Inventory Id

                        // Create StockMovement
                        var stockMovement = new StockMovement
                        {
                            InventoryId = inventory.Id,
                            MovementType = "GRN_IN",
                            Direction = "IN",
                            Quantity = grnItem.AcceptedQuantity, // Reflect accepted quantity
                            Reason = "Goods Received Note Approval",
                            RelatedDocumentId = grn.Id,
                            RelatedDocumentType = "GRN",
                            CreatedById = int.Parse(userId),
                            CreatedBy = User.Identity?.Name,
                            CreatedDate = DateTime.Now,
                            IsActive = true,

                            // To Location
                            ToWarehouseId = locationDto.WarehouseId,
                            ToFloorId = locationDto.FloorId,
                            ToZoneId = locationDto.ZoneId,
                            ToAisleId = locationDto.AisleId,
                            ToRackId = locationDto.RackId,
                            ToShelfId = locationDto.ShelfId,
                            ToBinId = locationDto.BinId
                        };
                        await _stockMovementRepo.AddAsync(stockMovement);

                        // Update product current stock
                        product.CurrentStock += grnItem.AcceptedQuantity;
                        _productRepo.Update(product);
                    }
                }

                // Update GRN status
                grn.Status = "Approved";
                grn.StoreApprovedById = int.Parse(userId);
                grn.StoreApprovedAt = DateTime.Now;
                grn.UpdatedBy = User.Identity?.Name;
                grn.UpdatedDate = DateTime.Now;

                _grnRepo.Update(grn);

                // Update PO status
                var purchaseOrder = await _poRepo.GetByIdAsync(grn.PurchaseOrderId);
                if (purchaseOrder != null)
                {
                    purchaseOrder.Status = "Received";
                    _poRepo.Update(purchaseOrder);
                }

                await _dbContext.SaveChangesAsync();
                
                // Commit the transaction
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "GRN approved and inventory updated successfully" });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "An error occurred while approving the GRN. The operation has been rolled back." });
            }
        }

        // PATCH: api/GRN/{id}/reject
        [HttpPatch("{id}/reject")]
        [Authorize(Roles = "Admin, StoreManager, WarehouseManager")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectGRNDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "User not authenticated" });

            var grn = await _grnRepo.GetByIdAsync(id);
            if (grn == null || !grn.IsActive)
                return NotFound(new { success = false, message = "GRN not found" });

            if (grn.Status != "PendingStoreApproval")
                return BadRequest(new { success = false, message = "Only pending GRNs can be rejected" });

            grn.Status = "Rejected";
            grn.StoreApprovedById = int.Parse(userId);
            grn.StoreApprovedAt = DateTime.Now;
            grn.Notes = (grn.Notes ?? "") + $" | Rejected: {dto.Reason}";
            grn.UpdatedBy = User.Identity?.Name;
            grn.UpdatedDate = DateTime.Now;

            _grnRepo.Update(grn);
            await _grnRepo.SaveChangesAsync();

            return Ok(new { success = true, message = "GRN rejected" });
        }

        [HttpPut("{id}/qc-complete")]
        [Authorize(Roles = "Admin, WarehouseManager")]
        public async Task<IActionResult> CompleteQC(int id, [FromBody] List<QCUpdateDto> qcUpdates)
        {
            var grn = await _grnRepo.GetByIdAsync(id);
            if (grn == null || !grn.IsActive)
                return NotFound(new { message = "GRN not found" });

            if (grn.Status != "Draft")
                return BadRequest(new { message = "GRN already processed" });

            // Update each GRNItem accepted/rejected quantities
            foreach (var update in qcUpdates)
            {
                var grnItem = grn.GRNItems.FirstOrDefault(i => i.Id == update.GRNItemId);
                if (grnItem == null) continue;
                grnItem.AcceptedQuantity = update.AcceptedQuantity;
                grnItem.RejectedQuantity = update.RejectedQuantity;
                grnItem.UpdatedBy = User.Identity?.Name;
                grnItem.UpdatedDate = DateTime.Now;
            }

            // Determine GRN status
            bool allAccepted = grn.GRNItems.All(i => i.RejectedQuantity == 0);
            bool allRejected = grn.GRNItems.All(i => i.AcceptedQuantity == 0);
            bool partial = !allAccepted && !allRejected;

            if (allAccepted)
                grn.Status = "Accepted";
            else if (allRejected)
                grn.Status = "Rejected";
            else
                grn.Status = "PartiallyAccepted";

            grn.UpdatedBy = User.Identity?.Name;
            grn.UpdatedDate = DateTime.Now;

            _grnRepo.Update(grn);
            await _grnRepo.SaveChangesAsync();

            return Ok(new { message = "QC completed", status = grn.Status });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var grn = await _grnRepo.GetByIdAsync(id);
            if (grn == null || !grn.IsActive)
                return NotFound(new { message = "GRN not found" });

            _grnRepo.SoftDelete(grn);
            await _grnRepo.SaveChangesAsync();

            return Ok(new { message = "GRN deleted" });
        }

        private GRNResponseDto MapToResponse(GRN grn)
        {
            return new GRNResponseDto
            {
                Id = grn.Id,
                GRNNumber = grn.GRNNumber,
                ReceivedDate = grn.ReceivedDate,
                Status = grn.Status,
                ReceiveType = grn.ReceiveType,
                VehicleNumber = grn.VehicleNumber,
                DeliveryPersonName = grn.DeliveryPersonName,
                Notes = grn.Notes,
                PurchaseOrderId = grn.PurchaseOrderId,
                PONumber = grn.PurchaseOrder?.PONumber,
                SupplierName = grn.PurchaseOrder?.Supplier?.Name,
                ReceivedQuantity = grn.ReceivedQuantity,
                ReceivedById = grn.ReceivedById,
                ReceivedByName = grn.ReceivedBy?.Email,
                StoreApprovedById = grn.StoreApprovedById,
                StoreApprovedByName = grn.StoreApprovedBy?.Email,
                StoreApprovedAt = grn.StoreApprovedAt,
                Items = grn.GRNItems.Where(i => i.IsActive).Select(i => new GRNItemResponseDto
                {
                    Id = i.Id,
                    POItemId = i.POItemId,
                    ProductId = i.POItem?.ProductId ?? 0,
                    ProductName = i.POItem?.Product?.Name,
                    OrderedQuantity = i.POItem?.OrderedQuantity ?? 0,
                    ReceivedQuantity = i.ReceivedQuantity,
                    AcceptedQuantity = i.AcceptedQuantity,
                    RejectedQuantity = i.RejectedQuantity,
                    Condition = i.Condition,
                    Remarks = i.Remarks
                }).ToList()
            };
        }

        private async Task<string> GenerateGRNNumber()
        {
            var year = DateTime.Now.Year;
            var all = await _grnRepo.FindAsync(g => g.GRNNumber.StartsWith($"GRN-{year}-"));
            int next = 1;
            if (all.Any())
            {
                var maxNum = all.Max(g =>
                {
                    var parts = g.GRNNumber.Split('-');
                    return parts.Length == 3 && int.TryParse(parts[2], out int num) ? num : 0;
                });
                next = maxNum + 1;
            }
            return $"GRN-{year}-{next:D4}";
        }

        private async Task<string> GenerateBatchNumber()
        {
            var year = DateTime.Now.Year;
            var all = await _batchRepo.FindAsync(b => b.BatchNumber.StartsWith($"BM-{year}-"));
            int next = 1;
            if (all.Any())
            {
                var maxNum = all.Max(b =>
                {
                    var parts = b.BatchNumber.Split('-');
                    return parts.Length == 3 && int.TryParse(parts[2], out int num) ? num : 0;
                });
                next = maxNum + 1;
            }
            return $"BM-{year}-{next:D4}";
        }
    }

    public class QCUpdateDto
    {
        public int GRNItemId { get; set; }
        public int AcceptedQuantity { get; set; }
        public int RejectedQuantity { get; set; }
    }

    public class RejectGRNDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class ApproveGRNDto
    {
        public int? WarehouseId { get; set; }
        public int? FloorId { get; set; }
        public int? ZoneId { get; set; }
        public int? AisleId { get; set; }
        public int? RackId { get; set; }
        public int? ShelfId { get; set; }
        public int? BinId { get; set; }

        public List<ApproveGRNItemDto> Items { get; set; } = new();
    }

    public class ApproveGRNItemDto
    {
        public int GRNItemId { get; set; }
        public int AcceptedQuantity { get; set; }
    }
}