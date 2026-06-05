using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperShop_Management.Data;
using SuperShop_Management.DTOs.PurchaseOrder;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IPurchaseOrderRepository _poRepo;
        private readonly ICSRepository _csRepo;
        private readonly ISupplierRepository _supplierRepo;
        private readonly IProductRepository _productRepo;
        private readonly UserManager<IdentityUser<int>> _userManager;
        private readonly AppDbContext _context;   // ✅ DbContext Inject করা হলো

        public PurchaseOrderController(
            IPurchaseOrderRepository poRepo,
            ICSRepository csRepo,
            ISupplierRepository supplierRepo,
            IProductRepository productRepo,
            UserManager<IdentityUser<int>> userManager,
            AppDbContext context)                 // ✅ Constructor এ যোগ করুন
        {
            _poRepo = poRepo;
            _csRepo = csRepo;
            _supplierRepo = supplierRepo;
            _productRepo = productRepo;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var poList = await _poRepo.GetActivePurchaseOrdersAsync();
                var response = poList.Select(MapToResponse);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    message = ex.Message, 
                    inner = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace 
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var po = await _poRepo.GetByIdWithDetailsAsync(id);
                if (po == null)
                    return NotFound(new { message = "Purchase Order not found" });
                return Ok(MapToResponse(po));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        [HttpGet("FromCS/{csId}")]
        public async Task<IActionResult> GetFromCS(int csId)
        {
            // ── 1. Load CS with full includes (repo now loads SupplierRows too) ──
            var cs = await _csRepo.GetByIdAsync(csId);
            if (cs == null || !cs.IsActive)
                return NotFound(new { message = "Comparative Statement not found" });

            if (cs.Status != "Approved")
                return BadRequest(new { message = "Only approved Comparative Statement can be used to create Purchase Order" });

            var csItems = cs.CSItems.Where(i => i.IsActive).ToList();
            if (!csItems.Any())
                return BadRequest(new { message = "CS has no items" });

            // ── 2. Determine winning supplier – support BOTH old & new structures ──
            Supplier? winningSupplier = null;

            // Strategy A: Try the NEW CSSupplierRow path first (IsSelected = true)
            var allSelectedRows = csItems
                .SelectMany(ci => ci.SupplierRows ?? Enumerable.Empty<CSSupplierRow>())
                .Where(sr => sr.IsSelected && sr.IsActive)
                .ToList();

            if (allSelectedRows.Any())
            {
                // The winning supplier is the one marked IsSelected
                winningSupplier = allSelectedRows.First().Supplier;
            }

            // Strategy B: Fall back to OLD SelectedQuotationItemId path
            if (winningSupplier == null)
            {
                var firstItemWithSelection = csItems
                    .FirstOrDefault(ci => ci.SelectedQuotationItemId.HasValue && ci.SelectedQuotationItemId > 0);

                if (firstItemWithSelection?.SelectedQuotationItem?.SupplierQuotation?.Supplier != null)
                {
                    winningSupplier = firstItemWithSelection.SelectedQuotationItem.SupplierQuotation.Supplier;
                }
            }

            // Strategy C: Last resort – query DB directly
            if (winningSupplier == null)
            {
                var firstSelectedQIId = csItems
                    .Select(ci => ci.SelectedQuotationItemId)
                    .FirstOrDefault(id => id.HasValue && id > 0);

                if (firstSelectedQIId.HasValue)
                {
                    var qi = await _context.QuotationItems
                        .Include(q => q.SupplierQuotation)
                            .ThenInclude(sq => sq.Supplier)
                        .FirstOrDefaultAsync(q => q.Id == firstSelectedQIId.Value);

                    winningSupplier = qi?.SupplierQuotation?.Supplier;
                }
            }

            if (winningSupplier == null)
            {
                return BadRequest(new
                {
                    message = "Cannot create PO: This CS does not have a properly selected winning supplier or quotation data. " +
                              "Please go back to the CS and select a winning supplier before creating a PO."
                });
            }

            // ── 3. Build items list – again support both paths ──
            var poRequest = new PurchaseOrderRequestDto
            {
                ComparativeStatementId = cs.Id,
                OrderDate = DateTime.Now,
                ExpectedDeliveryDate = DateTime.Now.AddDays(7),
                Items = new List<POItemRequestDto>()
            };

            var itemsData = new List<object>();

            foreach (var csItem in csItems)
            {
                int productId = csItem.ProductId;
                string productName = csItem.Product?.Name ?? csItem.ItemName ?? "Unknown Product";
                int quantity = 0;
                decimal supplierRate = 0;

                // Path A: Try CSSupplierRow (new structure)
                var selectedRow = csItem.SupplierRows?
                    .FirstOrDefault(sr => sr.IsSelected && sr.IsActive && sr.SupplierId == winningSupplier.Id);

                if (selectedRow != null)
                {
                    quantity = selectedRow.Qty;
                    supplierRate = selectedRow.UnitPrice;
                    // Use product name from QuotationItem if available
                    if (selectedRow.QuotationItem?.Product?.Name != null)
                        productName = selectedRow.QuotationItem.Product.Name;
                }
                else
                {
                    // Path B: Try SelectedQuotationItem (old structure)
                    var sqItem = csItem.SelectedQuotationItem;

                    if (sqItem == null && csItem.SelectedQuotationItemId.HasValue)
                    {
                        // Fallback: load from DB if not eagerly loaded
                        sqItem = await _context.QuotationItems
                            .Include(q => q.Product)
                            .FirstOrDefaultAsync(q => q.Id == csItem.SelectedQuotationItemId.Value);
                    }

                    if (sqItem != null)
                    {
                        quantity = sqItem.OfferedQuantity;
                        supplierRate = sqItem.UnitPrice;
                        if (sqItem.Product?.Name != null)
                            productName = sqItem.Product.Name;
                    }
                    else
                    {
                        // Skip items with no quotation data at all
                        continue;
                    }
                }

                // Guard against zero quantity
                if (quantity <= 0) quantity = 1;

                poRequest.Items.Add(new POItemRequestDto
                {
                    ProductId = productId,
                    OrderedQuantity = quantity,
                    SupplierRate = supplierRate,
                    PORate = supplierRate // Default PO Rate = Supplier Rate
                });

                itemsData.Add(new
                {
                    ProductId = productId,
                    ProductName = productName,
                    OrderedQuantity = quantity,
                    SupplierRate = supplierRate
                });
            }

            if (!poRequest.Items.Any())
            {
                return BadRequest(new
                {
                    message = "Cannot create PO: None of the CS items have valid quotation data for the winning supplier."
                });
            }

            // ── 4. Return pre-filled response ──
            return Ok(new
            {
                SupplierName = winningSupplier.Name,
                SupplierId = winningSupplier.Id,
                SupplierAddress = winningSupplier.Address,
                SupplierContact = winningSupplier.ContactPerson,
                CSNumber = cs.CSNumber,
                TotalBDTAmount = cs.TotalBDTAmount,
                ItemsData = itemsData,
                poRequest
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> Create([FromBody] PurchaseOrderRequestDto dto)
        {
            // Validate CS
            var cs = await _csRepo.GetByIdAsync(dto.ComparativeStatementId);
            if (cs == null || !cs.IsActive)
                return BadRequest(new { message = "Invalid Comparative Statement" });

            if (cs.Status != "Approved")
                return BadRequest(new { message = "Only approved Comparative Statement can be used to create Purchase Order" });

            if (dto.Items == null || !dto.Items.Any())
                return BadRequest(new { message = "PO must contain items" });

            // Strict Validation
            decimal calculatedTotalBDT = 0;
            foreach(var item in dto.Items)
            {
                if (item.PORate > item.SupplierRate)
                {
                    return BadRequest(new { message = "PO Rate cannot exceed Supplier Rate. Corruption flag triggered." });
                }
                calculatedTotalBDT += (item.PORate * item.OrderedQuantity);
            }

            var csItems = cs.CSItems.Where(i => i.IsActive).ToList();
            if (!csItems.Any())
                return BadRequest(new { message = "CS has no items" });

            // ── Determine winning supplier – apply robust logic from GetFromCS ──
            Supplier? winningSupplier = null;

            // Strategy A: Try the NEW CSSupplierRow path first (IsSelected = true)
            var allSelectedRows = csItems
                .SelectMany(ci => ci.SupplierRows ?? Enumerable.Empty<CSSupplierRow>())
                .Where(sr => sr.IsSelected && sr.IsActive)
                .ToList();

            if (allSelectedRows.Any())
            {
                winningSupplier = allSelectedRows.First().Supplier;
            }

            // Strategy B: Fall back to OLD SelectedQuotationItemId path
            if (winningSupplier == null)
            {
                var firstItemWithSelection = csItems
                    .FirstOrDefault(ci => ci.SelectedQuotationItemId.HasValue && ci.SelectedQuotationItemId > 0);

                if (firstItemWithSelection?.SelectedQuotationItem?.SupplierQuotation?.Supplier != null)
                {
                    winningSupplier = firstItemWithSelection.SelectedQuotationItem.SupplierQuotation.Supplier;
                }
            }

            // Strategy C: Last resort – query DB directly
            if (winningSupplier == null)
            {
                var firstSelectedQIId = csItems
                    .Select(ci => ci.SelectedQuotationItemId)
                    .FirstOrDefault(id => id.HasValue && id > 0);

                if (firstSelectedQIId.HasValue)
                {
                    var qi = await _context.QuotationItems
                        .Include(q => q.SupplierQuotation)
                            .ThenInclude(sq => sq.Supplier)
                        .FirstOrDefaultAsync(q => q.Id == firstSelectedQIId.Value);

                    winningSupplier = qi?.SupplierQuotation?.Supplier;
                }
            }

            if (winningSupplier == null)
            {
                return BadRequest(new { message = "Invalid winning supplier or quotation item in CS" });
            }

            var supplierId = winningSupplier.Id;

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();
            var user = await _userManager.FindByIdAsync(currentUserId);
            if (user == null) return Unauthorized();

            // Check if PO already exists for this CS
            var existingPO = await _poRepo.GetByCSIdAsync(cs.Id);
            if (existingPO.Any())
                return BadRequest(new { message = "A Purchase Order already exists for this CS" });

            var poNumber = await GeneratePONumber();

            var purchaseOrder = new PurchaseOrder
            {
                PONumber = poNumber,
                OrderDate = dto.OrderDate,
                ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
                Status = "Draft",
                TotalBDTAmount = calculatedTotalBDT,
                DeliveryAddress = dto.DeliveryAddress,
                PaymentTerms = dto.PaymentTerms,
                Notes = dto.Notes,
                SupplierId = supplierId,
                ComparativeStatementId = cs.Id,
                CreatedById = int.Parse(currentUserId),
                IsActive = true,
                CreatedBy = user.Email ?? user.UserName,
                CreatedDate = DateTime.Now
            };

            // Add POItems based on DTO
            foreach (var reqItem in dto.Items)
            {
                var poItem = new POItem
                {
                    ProductId = reqItem.ProductId,
                    OrderedQuantity = reqItem.OrderedQuantity,
                    SupplierRate = reqItem.SupplierRate,
                    PORate = reqItem.PORate,
                    TotalPrice = reqItem.PORate * reqItem.OrderedQuantity,
                    BDTAmount = reqItem.PORate * reqItem.OrderedQuantity,
                    IsActive = true,
                    CreatedBy = user.Email ?? user.UserName,
                    CreatedDate = DateTime.Now
                };
                purchaseOrder.POItems.Add(poItem);
            }

            await _poRepo.AddAsync(purchaseOrder);
            await _poRepo.SaveChangesAsync();

            cs.Status = "POCreated";
            _csRepo.Update(cs);
            await _csRepo.SaveChangesAsync();

            return Ok(new { message = "Purchase Order created successfully", id = purchaseOrder.Id, number = purchaseOrder.PONumber });
        }

        [HttpPut("{id}/submit")]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> SubmitForApproval(int id)
        {
            var po = await _poRepo.GetByIdAsync(id);
            if (po == null || !po.IsActive)
                return NotFound(new { message = "Purchase Order not found" });

            if (po.Status != "Draft")
                return BadRequest(new { message = "Only Draft PO can be submitted for approval" });

            po.Status = "PendingApproval";
            po.UpdatedBy = User.Identity?.Name;
            po.UpdatedDate = DateTime.Now;
            _poRepo.Update(po);
            await _poRepo.SaveChangesAsync();

            return Ok(new { message = "Purchase Order submitted for approval" });
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin, MD, PurchaseManager")]
        public async Task<IActionResult> Approve(int id)
        {
            var po = await _poRepo.GetByIdAsync(id);
            if (po == null || !po.IsActive)
                return NotFound(new { message = "Purchase Order not found" });

            if (po.Status != "PendingApproval")
                return BadRequest(new { message = "Only Pending Approval PO can be approved" });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            po.Status = "Approved";
            po.ApprovedById = int.Parse(currentUserId!);
            po.ApprovedAt = DateTime.Now;
            po.UpdatedBy = User.Identity?.Name;
            po.UpdatedDate = DateTime.Now;
            _poRepo.Update(po);
            await _poRepo.SaveChangesAsync();

            return Ok(new { message = "Purchase Order approved" });
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin, MD, PurchaseManager")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectPODto dto)
        {
            var po = await _poRepo.GetByIdAsync(id);
            if (po == null || !po.IsActive)
                return NotFound(new { success = false, message = "Purchase Order not found" });

            if (po.Status != "PendingApproval")
                return BadRequest(new { success = false, message = "Only Pending Approval PO can be rejected" });

            po.Status = "Rejected";
            po.Notes = (po.Notes ?? "") + $" | Rejected by MD: {dto.Reason}";
            po.UpdatedBy = User.Identity?.Name;
            po.UpdatedDate = DateTime.Now;
            _poRepo.Update(po);
            await _poRepo.SaveChangesAsync();

            return Ok(new { success = true, message = "Purchase Order rejected" });
        }

        [HttpGet("{id}/print")]
        public async Task<IActionResult> PrintPO(int id)
        {
            var po = await _poRepo.GetByIdAsync(id);
            if (po == null || !po.IsActive)
                return NotFound(new { success = false, message = "Purchase Order not found" });

            var printData = new
            {
                po.PONumber,
                po.OrderDate,
                po.ExpectedDeliveryDate,
                po.Status,
                po.TotalBDTAmount,
                po.DeliveryAddress,
                po.PaymentTerms,
                po.Notes,
                po.ApprovedAt,
                Supplier = new
                {
                    po.Supplier?.Name,
                    po.Supplier?.ContactPerson,
                    po.Supplier?.Email,
                    po.Supplier?.Phone,
                    po.Supplier?.Address
                },
                Items = po.POItems.Where(i => i.IsActive).Select(i => new
                {
                    ProductName = i.Product?.Name,
                    ProductBarcode = i.Product?.Barcode,
                    i.OrderedQuantity,
                    i.SupplierRate,
                    i.PORate,
                    i.TotalPrice,
                    i.BDTAmount,
                    UnitName = i.Product?.Unit?.NameOfUnit
                }).ToList(),
                CreatedBy = po.CreatedByUser?.Email,
                ApprovedBy = po.ApprovedBy?.Email,
                CreatedDate = po.CreatedDate
            };

            return Ok(new { success = true, data = printData });
        }

        [HttpPut("{id}/send")]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> SendToSupplier(int id)
        {
            var po = await _poRepo.GetByIdAsync(id);
            if (po == null || !po.IsActive)
                return NotFound(new { message = "Purchase Order not found" });

            if (po.Status != "Approved")
                return BadRequest(new { message = "Only Approved PO can be sent to supplier" });

            po.Status = "Sent";
            po.UpdatedBy = User.Identity?.Name;
            po.UpdatedDate = DateTime.Now;
            _poRepo.Update(po);
            await _poRepo.SaveChangesAsync();

            return Ok(new { message = "Purchase Order sent to supplier" });
        }

        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "Admin, MD, PurchaseManager")]
        public async Task<IActionResult> Cancel(int id)
        {
            var po = await _poRepo.GetByIdAsync(id);
            if (po == null || !po.IsActive)
                return NotFound(new { message = "Purchase Order not found" });

            if (po.Status == "Received" || po.Status == "Cancelled")
                return BadRequest(new { message = "Purchase Order cannot be cancelled" });

            po.Status = "Cancelled";
            po.UpdatedBy = User.Identity?.Name;
            po.UpdatedDate = DateTime.Now;
            _poRepo.Update(po);
            await _poRepo.SaveChangesAsync();

            return Ok(new { message = "Purchase Order cancelled" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var po = await _poRepo.GetByIdAsync(id);
            if (po == null || !po.IsActive)
                return NotFound(new { message = "Purchase Order not found" });

            if (po.Status != "Draft")
                return BadRequest(new { message = "Only Draft PO can be deleted" });

            _poRepo.SoftDelete(po);
            await _poRepo.SaveChangesAsync();

            return Ok(new { message = "Purchase Order deleted" });
        }

        private PurchaseOrderResponseDto MapToResponse(PurchaseOrder po)
        {
            return new PurchaseOrderResponseDto
            {
                Id = po.Id,
                PONumber = po.PONumber,
                OrderDate = po.OrderDate,
                ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                Status = po.Status,
                TotalBDTAmount = po.TotalBDTAmount,
                DeliveryAddress = po.DeliveryAddress,
                PaymentTerms = po.PaymentTerms,
                Notes = po.Notes,
                ApprovedAt = po.ApprovedAt,
                SupplierId = po.SupplierId,
                SupplierName = po.Supplier?.Name,
                ComparativeStatementId = po.ComparativeStatementId,
                CSNumber = po.ComparativeStatement?.CSNumber,
                CreatedById = po.CreatedById,
                CreatedByEmail = po.CreatedByUser?.Email,   // ✅ Navigation property CreatedBy
                ApprovedById = po.ApprovedById,
                Items = po.POItems.Where(i => i.IsActive).Select(i => new POItemResponseDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name,
                    OrderedQuantity = i.OrderedQuantity,
                    SupplierRate = i.SupplierRate,
                    PORate = i.PORate,
                    TotalPrice = i.TotalPrice,
                    BDTAmount = i.BDTAmount
                }).ToList()
            };
        }

        private async Task<string> GeneratePONumber()
        {
            var year = DateTime.Now.Year;
            var all = await _poRepo.FindAsync(po => po.PONumber.StartsWith($"PO-{year}-"));
            int next = 1;
            if (all.Any())
            {
                var maxNum = all.Max(po =>
                {
                    var parts = po.PONumber.Split('-');
                    return parts.Length == 3 && int.TryParse(parts[2], out int num) ? num : 0;
                });
                next = maxNum + 1;
            }
            return $"PO-{year}-{next:D3}";
        }
    }

    public class RejectPODto
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
    }
}