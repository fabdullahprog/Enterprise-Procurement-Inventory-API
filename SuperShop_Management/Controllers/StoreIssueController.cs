using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.Data;
using SuperShop_Management.DTOs.StoreIssue;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;
using System.Security.Claims;

namespace SuperShop_Management.Controllers
{
    [Route("api/store")]
    [ApiController]
    [Authorize]
    public class StoreIssueController : ControllerBase
    {
        private readonly IStoreIssueRepository _storeIssueRepo;
        private readonly IEmployeeRequisitionRepository _empRequisitionRepo;
        private readonly IRequisitionRepository _purchaseRequisitionRepo;
        private readonly IInventoryRepository _inventoryRepo;
        private readonly IProductRepository _productRepo;
        private readonly UserManager<IdentityUser<int>> _userManager;
        private readonly AppDbContext _context;

        public StoreIssueController(
            IStoreIssueRepository storeIssueRepo,
            IEmployeeRequisitionRepository empRequisitionRepo,
            IRequisitionRepository purchaseRequisitionRepo,
            IInventoryRepository inventoryRepo,
            IProductRepository productRepo,
            UserManager<IdentityUser<int>> userManager,
            AppDbContext context)
        {
            _storeIssueRepo = storeIssueRepo;
            _empRequisitionRepo = empRequisitionRepo;
            _purchaseRequisitionRepo = purchaseRequisitionRepo;
            _inventoryRepo = inventoryRepo;
            _productRepo = productRepo;
            _userManager = userManager;
            _context = context;
        }

     
        [HttpGet("pending-requisitions")]
        public async Task<IActionResult> GetPendingRequisitions()
        {
            // Prior behavior: return requisition line snapshot stock (EmployeeRequisitionItem.CurrentStock)
            // and do not compute live inventory totals per item.
            var requisitions = (await _empRequisitionRepo.GetForwardedToStoreAsync()).ToList();
            var response = requisitions.Select(r => ProjectPendingRequisition(r));

            return Ok(new { success = true, data = response });
        }

        /// <summary>
        /// Single pending (forwarded-to-store) requisition using line snapshot stock.
        /// </summary>
        [HttpGet("pending-requisitions/{id:int}")]
        public async Task<IActionResult> GetPendingRequisitionById(int id)
        {
            var requisition = await _empRequisitionRepo.GetForwardedToStoreByIdAsync(id);
            if (requisition == null)
                return NotFound(new { success = false, message = "Requisition not found or not pending at store" });

            var dto = ProjectPendingRequisition(requisition);
            return Ok(new { success = true, data = dto });
        }

        // POST: api/store/issue
        /// <summary>
        /// Issue product from store (partial or full)
        /// NOTE: Currently handles first item only. Multi-item support coming soon.
        /// </summary>
        [HttpPost("issue")]
        public async Task<IActionResult> IssueProduct([FromBody] StoreIssueRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "User not authenticated" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized(new { success = false, message = "User not found" });

            // Get requisition
            var requisition = await _empRequisitionRepo.GetByIdAsync(dto.RequisitionId);
            if (requisition == null || !requisition.IsActive)
                return NotFound(new { success = false, message = "Requisition not found" });

            if (requisition.Status != "forwarded_to_store")
                return BadRequest(new { success = false, message = "Only forwarded requisitions can be issued" });

            // Get first item (multi-item support coming soon)
            var firstItem = requisition.Items.FirstOrDefault();
            if (firstItem == null)
                return BadRequest(new { success = false, message = "No items found in requisition" });

            // Check stock availability
            var inventories = await _inventoryRepo.GetByProductIdAsync(firstItem.ItemId);
            
            var targetInventories = inventories.AsEnumerable();
            if (dto.WarehouseId.HasValue && dto.WarehouseId > 0)
            {
                targetInventories = targetInventories.Where(i => i.WarehouseId == dto.WarehouseId);
            }
            if (dto.FloorId.HasValue && dto.FloorId > 0)
            {
                targetInventories = targetInventories.Where(i => i.FloorId == dto.FloorId);
            }
            if (dto.BinId.HasValue && dto.BinId > 0)
            {
                targetInventories = targetInventories.Where(i => i.BinId == dto.BinId);
            }
            if (dto.BatchId.HasValue && dto.BatchId > 0)
            {
                targetInventories = targetInventories.Where(i => i.BatchId == dto.BatchId);
            }

            var matchingInventories = targetInventories.ToList();
            int availableStock = matchingInventories.Sum(i => i.AvailableQuantity);

            if (availableStock <= 0)
            {
                // Stock not available - update status
                requisition.Status = "stock_not_available";
                requisition.UpdatedBy = user.Email ?? user.UserName;
                requisition.UpdatedDate = DateTime.Now;
                _empRequisitionRepo.Update(requisition);
                await _empRequisitionRepo.SaveChangesAsync();

                return Ok(new
                {
                    success = false,
                    message = "Stock not available in the selected location/batch. Please create a purchase requisition.",
                    availableStock = 0,
                    requiresPurchaseRequisition = true
                });
            }

            // Validate issued quantity
            if (dto.IssuedQty > availableStock)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Cannot issue {dto.IssuedQty} units. Only {availableStock} units available in the selected location/batch.",
                    availableStock
                });
            }

            if (dto.IssuedQty > firstItem.RequiredQty)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Cannot issue more than required quantity ({firstItem.RequiredQty} units)."
                });
            }

            // Determine issue type
            string issueType = dto.IssuedQty >= firstItem.RequiredQty ? "full" : "partial";

            // Create store issue record
            var storeIssue = new StoreIssue
            {
                RequisitionId = requisition.Id,
                IssuedQty = dto.IssuedQty,
                IssueType = issueType,
                IssuedById = int.Parse(userId),
                Status = "issued",
                Remarks = dto.Remarks,
                IssuedAt = DateTime.Now,
                IsActive = true,
                CreatedBy = user.Email ?? user.UserName,
                CreatedDate = DateTime.Now
            };

            await _storeIssueRepo.AddAsync(storeIssue);

            // Reduce inventory stock
            await ReduceInventoryStock(firstItem.ItemId, dto.IssuedQty, user.Email ?? user.UserName, dto.WarehouseId, dto.FloorId, dto.BinId, dto.BatchId);

            // Update requisition status
            if (issueType == "full")
            {
                requisition.Status = "completed";
            }
            else
            {
                requisition.Status = "partially_issued";
            }

            requisition.UpdatedBy = user.Email ?? user.UserName;
            requisition.UpdatedDate = DateTime.Now;
            _empRequisitionRepo.Update(requisition);

            await _storeIssueRepo.SaveChangesAsync();
            await _empRequisitionRepo.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Product issued successfully ({issueType})",
                data = new
                {
                    storeIssueId = storeIssue.Id,
                    issuedQuantity = dto.IssuedQty,
                    issueType,
                    remainingQuantity = firstItem.RequiredQty - dto.IssuedQty,
                    requisitionStatus = requisition.Status
                }
            });
        }

        // POST: api/store/forward-to-purchase
        /// <summary>
        /// Create purchase requisition when stock is not available
        /// NOTE: Currently handles first item only. Multi-item support coming soon.
        /// </summary>
        [HttpPost("forward-to-purchase")]
        public async Task<IActionResult> ForwardToPurchase([FromBody] ForwardToPurchaseDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "User not authenticated" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized(new { success = false, message = "User not found" });

            // Get employee requisition with Items included
            var empReq = await _empRequisitionRepo.GetByIdAsync(dto.RequisitionId);
            if (empReq == null || !empReq.IsActive)
                return NotFound(new { success = false, message = "Requisition not found" });

            // Check if Items collection is loaded
            if (empReq.Items == null || !empReq.Items.Any())
                return BadRequest(new { success = false, message = "No items found in requisition" });

            if (empReq.Status != "forwarded_to_store" && empReq.Status != "stock_not_available")
                return BadRequest(new
                {
                    success = false,
                    message = "Can only create purchase requisition for forwarded or stock unavailable requisitions"
                });

            // Check if purchase requisition already exists
            var existingPR = await _purchaseRequisitionRepo.FindAsync(pr =>
                pr.SourceRequisitionId == empReq.Id && pr.IsActive);

            if (existingPR.Any())
            {
                var existing = existingPR.First();
                return BadRequest(new
                {
                    success = false,
                    message = "Purchase requisition already exists for this employee requisition",
                    purchaseRequisitionId = existing.Id,
                    purchaseRequisitionNumber = existing.RequisitionNumber
                });
            }

            // Generate purchase requisition number
            var prNumber = await GeneratePurchaseRequisitionNumber();

            // Create purchase requisition
            var purchaseReq = new PurchaseRequisition
            {
                RequisitionNumber = prNumber,
                RequisitionDate = DateTime.Now,
                Status = "Pending",
                RequiredByDate = DateTime.Now.AddDays(7),
                DepartmentId = empReq.DepartmentId,
                RequestedById = int.Parse(userId),
                Notes = $"Created from Employee Requisition: {empReq.RequisitionNo}. {dto.Remarks}",
                SourceRequisitionId = empReq.Id,
                IsActive = true,
                CreatedBy = user.Email ?? user.UserName,
                CreatedDate = DateTime.Now
            };

            // Add requisition items (all items from employee requisition)
            foreach (var item in empReq.Items.Where(i => i.IsActive))
            {
                purchaseReq.RequisitionItems.Add(new RequisitionItem
                {
                    ProductId = item.ItemId,
                    RequiredQuantity = item.RequiredQty,
                    Remarks = $"Stock not available. Required for: {item.ItemName}",
                    IsActive = true,
                    CreatedBy = user.Email ?? user.UserName,
                    CreatedDate = DateTime.Now
                });
            }

            await _purchaseRequisitionRepo.AddAsync(purchaseReq);

            // Update employee requisition status
            empReq.Status = "purchase_requisition_created";
            empReq.UpdatedBy = user.Email ?? user.UserName;
            empReq.UpdatedDate = DateTime.Now;
            _empRequisitionRepo.Update(empReq);

            // Create store issue record to track forwarding
            var storeIssue = new StoreIssue
            {
                RequisitionId = empReq.Id,
                IssuedQty = 0,
                IssueType = "forwarded",
                IssuedById = int.Parse(userId),
                Status = "forwarded_to_purchase",
                Remarks = dto.Remarks ?? "Stock not available - forwarded to purchase department",
                IssuedAt = DateTime.Now,
                IsActive = true,
                CreatedBy = user.Email ?? user.UserName,
                CreatedDate = DateTime.Now
            };

            await _storeIssueRepo.AddAsync(storeIssue);

            await _purchaseRequisitionRepo.SaveChangesAsync();
            await _empRequisitionRepo.SaveChangesAsync();
            await _storeIssueRepo.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Purchase requisition created and forwarded to purchase department",
                data = new
                {
                    purchaseRequisitionId = purchaseReq.Id,
                    purchaseRequisitionNumber = purchaseReq.RequisitionNumber,
                    employeeRequisitionStatus = empReq.Status
                }
            });
        }

        // GET: api/store/issues
        /// <summary>
        /// Get all store issues
        /// </summary>
        [HttpGet("issues")]
        public async Task<IActionResult> GetAllIssues()
        {
            var issues = await _storeIssueRepo.GetAllWithDetailsAsync();
            var response = issues.Select(MapToResponse);
            return Ok(new { success = true, data = response });
        }

        // GET: api/store/issues/{id}
        /// <summary>
        /// Get store issue by ID
        /// </summary>
        [HttpGet("issues/{id}")]
        public async Task<IActionResult> GetIssueById(int id)
        {
            var issue = await _storeIssueRepo.GetByIdWithDetailsAsync(id);
            if (issue == null || !issue.IsActive)
                return NotFound(new { success = false, message = "Store issue not found" });

            var response = MapToResponse(issue);
            return Ok(new { success = true, data = response });
        }

        // GET: api/store/issues/by-requisition/{requisitionId}
        /// <summary>
        /// Get all issues for a specific requisition
        /// </summary>
        [HttpGet("issues/by-requisition/{requisitionId}")]
        public async Task<IActionResult> GetIssuesByRequisition(int requisitionId)
        {
            var issues = await _storeIssueRepo.GetByRequisitionIdAsync(requisitionId);
            var response = issues.Select(MapToResponse);
            return Ok(new { success = true, data = response });
        }

        // GET: api/store/check-stock/{productId}
        /// <summary>
        /// Check available stock for a product (includes batch details)
        /// </summary>
        [HttpGet("check-stock/{productId}")]
        public async Task<IActionResult> CheckStock(int productId)
        {
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null || !product.IsActive)
                return NotFound(new { success = false, message = "Product not found" });

            var inventories = await _inventoryRepo.GetByProductIdAsync(productId);
            int availableStock = inventories.Sum(i => i.AvailableQuantity);

            return Ok(new
            {
                success = true,
                data = new
                {
                    productId,
                    productName = product.Name,
                    availableStock,
                    inventoryDetails = inventories.Select(inv => new
                    {
                        warehouseId = inv.WarehouseId,
                        floorId = inv.FloorId,
                        zoneId = inv.ZoneId,
                        aisleId = inv.AisleId,
                        rackId = inv.RackId,
                        shelfId = inv.ShelfId,
                        binId = inv.BinId,
                        locationLevel = inv.LocationLevel,
                        availableQuantity = inv.AvailableQuantity,
                        reservedQuantity = inv.ReservedQuantity,
                        batchId = inv.BatchId,
                        batchNumber = inv.Batch != null ? inv.Batch.BatchNumber : $"Batch #{inv.BatchId}",
                        expiryDate = inv.Batch != null ? inv.Batch.ExpiryDate : (DateTime?)null
                    })
                }
            });
        }

        // ========== HELPER METHODS ==========

        private static object ProjectPendingItem(EmployeeRequisitionItem i)
        {
            var name = !string.IsNullOrWhiteSpace(i.ItemName) ? i.ItemName : (i.Item?.Name ?? "Unknown");
            var category = i.Item?.ItemCategory?.CategoryName;
            return new
            {
                itemId = i.ItemId,
                itemName = name,
                category,
                categoryName = category,
                requiredQty = i.RequiredQty,
                currentStock = i.CurrentStock,
                remarks = i.Remarks
            };
        }

        private static object ProjectPendingRequisition(Requisition r)
        {
            return new
            {
                id = r.Id,
                requisitionNo = r.RequisitionNo,
                items = r.Items.Where(i => i.IsActive).Select(i => ProjectPendingItem(i)).ToList(),
                requestedBy = r.RequestedBy,
                requestedByName = r.RequestedByUser?.UserName ?? r.RequestedByUser?.Email,
                departmentId = r.DepartmentId,
                departmentName = r.Department?.DepartmentName,
                status = r.Status,
                notes = r.Notes,
                forwardedAt = r.ForwardedAt,
                createdDate = r.CreatedDate
            };
        }

        private StoreIssueResponseDto MapToResponse(StoreIssue issue)
        {
            // Get first item for display (multi-item support coming soon)
            var firstItem = issue.Requisition?.Items?.FirstOrDefault();
            
            return new StoreIssueResponseDto
            {
                Id = issue.Id,
                RequisitionId = issue.RequisitionId,
                RequisitionNo = issue.Requisition?.RequisitionNo,
                ItemName = firstItem?.ItemName ?? "N/A",
                RequiredQty = firstItem?.RequiredQty ?? 0,
                IssuedQty = issue.IssuedQty,
                IssueType = issue.IssueType,
                IssuedById = issue.IssuedById,
                IssuedByName = issue.IssuedBy?.UserName ?? issue.IssuedBy?.Email,
                Status = issue.Status,
                Remarks = issue.Remarks,
                IssuedAt = issue.IssuedAt
            };
        }

        private async Task ReduceInventoryStock(
            int productId, 
            int quantity, 
            string updatedBy,
            int? warehouseId = null,
            int? floorId = null,
            int? binId = null,
            int? batchId = null)
        {
            var inventories = await _inventoryRepo.GetByProductIdAsync(productId);
            
            var query = inventories.AsEnumerable();
            if (warehouseId.HasValue && warehouseId > 0)
                query = query.Where(i => i.WarehouseId == warehouseId);
            if (floorId.HasValue && floorId > 0)
                query = query.Where(i => i.FloorId == floorId);
            if (binId.HasValue && binId > 0)
                query = query.Where(i => i.BinId == binId);
            if (batchId.HasValue && batchId > 0)
                query = query.Where(i => i.BatchId == batchId);

            var sortedInventories = query
                .Where(i => i.AvailableQuantity > 0)
                .OrderBy(i => i.CreatedDate) // FIFO within selection
                .ToList();

            int remainingQty = quantity;

            foreach (var inventory in sortedInventories)
            {
                if (remainingQty <= 0) break;

                int deductQty = Math.Min(inventory.AvailableQuantity, remainingQty);
                inventory.AvailableQuantity -= deductQty;
                // NOTE: GrnQuantity is intentionally NOT deducted — it preserves the original GRN received amount
                inventory.UpdatedBy = updatedBy;
                inventory.UpdatedDate = DateTime.Now;

                _inventoryRepo.Update(inventory);
                remainingQty -= deductQty;
            }

            // ── BUG FIX: Also deduct the issued quantity from Product.CurrentStock ──
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.CurrentStock -= quantity;
                if (product.CurrentStock < 0) product.CurrentStock = 0; // Safety guard
                product.UpdatedBy = updatedBy;
                product.UpdatedDate = DateTime.Now;
                _context.Products.Update(product);
            }

            // Ensure SaveChangesAsync captures both Inventory and Product updates
            await _context.SaveChangesAsync();
        }

        private async Task<string> GeneratePurchaseRequisitionNumber()
        {
            var year = DateTime.Now.Year;
            var all = await _purchaseRequisitionRepo.FindAsync(pr =>
                pr.RequisitionNumber.StartsWith($"PR-{year}-"));

            int next = 1;
            if (all.Any())
            {
                var maxNumber = all.Max(pr =>
                {
                    var parts = pr.RequisitionNumber.Split('-');
                    return parts.Length == 3 && int.TryParse(parts[2], out int num) ? num : 0;
                });
                next = maxNumber + 1;
            }

            return $"PR-{year}-{next:D4}";
        }
    }
}
