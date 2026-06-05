using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.DTOs.ComparativeStatement;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;
using System.Security.Claims;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ComparativeStatementController : ControllerBase
    {
        private readonly ICSRepository _csRepo;
        private readonly IRFQRepository _rfqRepo;
        private readonly ISupplierQuotationRepository _quotationRepo;
        private readonly IQuotationItemRepository _quotationItemRepo;
        private readonly ICSSupplierRowRepository _csSupplierRowRepo;
        private readonly IProductRepository _productRepo;
        private readonly UserManager<IdentityUser<int>> _userManager;

        public ComparativeStatementController(
            ICSRepository csRepo,
            IRFQRepository rfqRepo,
            ISupplierQuotationRepository quotationRepo,
            IQuotationItemRepository quotationItemRepo,
            ICSSupplierRowRepository csSupplierRowRepo,
            IProductRepository productRepo,
            UserManager<IdentityUser<int>> userManager)
        {
            _csRepo = csRepo;
            _rfqRepo = rfqRepo;
            _quotationRepo = quotationRepo;
            _quotationItemRepo = quotationItemRepo;
            _csSupplierRowRepo = csSupplierRowRepo;
            _productRepo = productRepo;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var csList = await _csRepo.GetActiveCSAsync();
            var response = new List<CSResponseDto>();
            foreach (var cs in csList)
            {
                response.Add(await MapToResponse(cs));
            }
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cs = await _csRepo.GetByIdAsync(id);
            if (cs == null || !cs.IsActive)
                return NotFound(new { message = "Comparative Statement not found" });
            return Ok(await MapToResponse(cs));
        }

        [HttpPost]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> Create([FromBody] CSRequestDto dto)
        {
            // Validate RFQ
            var rfq = await _rfqRepo.GetByIdAsync(dto.RFQId);
            if (rfq == null || !rfq.IsActive)
                return BadRequest(new { success = false, message = "Invalid RFQ" });

            if (rfq.Status != "QuotationReceived" && rfq.Status != "Sent")
                return BadRequest(new { success = false, message = "RFQ must have quotations received before creating CS" });

            // Check if CS already exists for this RFQ
            var existingCS = await _csRepo.GetByRFQIdAsync(dto.RFQId);
            if (existingCS != null)
                return BadRequest(new { success = false, message = "A Comparative Statement already exists for this RFQ" });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(currentUserId);
            if (user == null) return Unauthorized();

            // Get all quotations for this RFQ
            var quotations = await _quotationRepo.GetByRFQIdAsync(dto.RFQId);
            if (!quotations.Any())
                return BadRequest(new { success = false, message = "No quotations found for this RFQ" });

            var csNumber = await GenerateCSNumber();

            var cs = new ComparativeStatement
            {
                CSNumber = csNumber,
                CSDate = DateTime.Now,
                Status = "Draft",
                TotalBDTAmount = 0, // Will calculate after selection
                Remarks = dto.Remarks,
                RFQId = dto.RFQId,
                CreatedById = int.Parse(currentUserId),
                IsActive = true,
                CreatedBy = user.Email ?? user.UserName,
                CreatedDate = DateTime.Now,
                CSItems = new List<CSItem>()
            };

            // Auto-generate CSSupplierRows from quotations
            // Group by product, then create rows for each supplier
            var allQuotationItems = quotations.SelectMany(q => q.QuotationItems.Where(i => i.IsActive)).ToList();
            var productGroups = allQuotationItems.GroupBy(qi => qi.ProductId);

            foreach (var productGroup in productGroups)
            {
                var csItem = new CSItem
                {
                    ProductId = productGroup.Key,
                    IsActive = true,
                    CreatedBy = user.Email ?? user.UserName,
                    CreatedDate = DateTime.Now,
                    SupplierRows = new List<CSSupplierRow>()
                };

                foreach (var quotationItem in productGroup)
                {
                    var csRow = new CSSupplierRow
                    {
                        CS = cs, // Explicitly link root entity so EF Core auto-maps CSId
                        SupplierId = quotationItem.SupplierQuotation!.SupplierId,
                        QuotationItemId = quotationItem.Id,
                        Unit = quotationItem.Product?.Unit?.NameOfUnit ?? "pcs",
                        UnitPrice = quotationItem.UnitPrice,
                        Qty = quotationItem.OfferedQuantity,
                        TotalPrice = quotationItem.TotalPrice,
                        Rating = "average",
                        IsSelected = false,
                        IsActive = true,
                        CreatedBy = user.Email ?? user.UserName,
                        CreatedDate = DateTime.Now
                    };
                    csItem.SupplierRows.Add(csRow);
                }
                cs.CSItems.Add(csItem);
            }

            await _csRepo.AddAsync(cs);
            await _csRepo.SaveChangesAsync();

            return Ok(new { 
                success = true, 
                message = "Comparative Statement created successfully.", 
                id = cs.Id,
                number = cs.CSNumber
            });
        }

        // POST: api/ComparativeStatement/{id}/select-suppliers
        [HttpPost("{id}/select-suppliers")]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> SelectSuppliers(int id, [FromBody] SelectSuppliersDto dto)
        {
            var cs = await _csRepo.GetByIdAsync(id);
            if (cs == null || !cs.IsActive)
                return NotFound(new { success = false, message = "CS not found" });

            if (cs.Status != "Draft")
                return BadRequest(new { success = false, message = "Only Draft CS can have suppliers selected" });

            if (dto.SelectedRowIds == null || !dto.SelectedRowIds.Any())
                return BadRequest(new { success = false, message = "At least one supplier row must be selected" });

            // Get all CS rows
            var allRows = await _csSupplierRowRepo.FindAsync(r => r.CSId == id && r.IsActive);

            // First, deselect all
            foreach (var row in allRows)
            {
                row.IsSelected = false;
            }

            // Then select the chosen ones
            decimal totalBDT = 0;
            foreach (var rowId in dto.SelectedRowIds)
            {
                var row = allRows.FirstOrDefault(r => r.Id == rowId);
                if (row != null)
                {
                    row.IsSelected = true;
                    row.SelectedReason = dto.Remarks;
                    totalBDT += row.TotalPrice;
                    _csSupplierRowRepo.Update(row);
                }
            }

            // Update CS total
            cs.TotalBDTAmount = totalBDT;
            cs.UpdatedBy = User.Identity?.Name;
            cs.UpdatedDate = DateTime.Now;
            _csRepo.Update(cs);

            await _csSupplierRowRepo.SaveChangesAsync();
            await _csRepo.SaveChangesAsync();

            return Ok(new { success = true, message = "Suppliers selected successfully" });
        }

        // GET: api/ComparativeStatement/{id}/print
        [HttpGet("{id}/print")]
        public async Task<IActionResult> PrintCS(int id)
        {
            var cs = await _csRepo.GetByIdAsync(id);
            if (cs == null || !cs.IsActive)
                return NotFound(new { success = false, message = "CS not found" });

            var rows = await _csSupplierRowRepo.FindAsync(r => r.CSId == id && r.IsActive);

            var printData = new
            {
                cs.CSNumber,
                cs.CSDate,
                cs.Status,
                cs.TotalBDTAmount,
                cs.Remarks,
                cs.ApprovedAt,
                RFQ = new
                {
                    cs.RFQ?.RFQNumber,
                    cs.RFQ?.RFQDate,
                    cs.RFQ?.QuotationDeadline
                },
                Products = rows.GroupBy(r => r.CSItem?.ProductId).Select(g => new
                {
                    ProductId = g.Key,
                    ProductName = g.First().CSItem?.Product?.Name,
                    ProductBarcode = g.First().CSItem?.Product?.Barcode,
                    Suppliers = g.Select(r => new
                    {
                        r.Supplier?.Name,
                        OfferedQuantity = r.Qty,
                        r.UnitPrice,
                        r.TotalPrice,
                        BDTAmount = r.TotalPrice,
                        DeliveryDays = r.QuotationItem?.SupplierQuotation?.DeliveryDays ?? 0,
                        r.IsSelected,
                        SelectionRemarks = r.SelectedReason
                    }).ToList()
                }).ToList(),
                CreatedBy = cs.CreatedByUser?.Email,
                ApprovedBy = cs.ApprovedBy?.Email
            };

            return Ok(new { success = true, data = printData });
        }

        [HttpPut("{id}/review")]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> MarkReviewed(int id)
        {
            var cs = await _csRepo.GetByIdAsync(id);
            if (cs == null || !cs.IsActive)
                return NotFound(new { message = "CS not found" });

            if (cs.Status != "Draft")
                return BadRequest(new { message = "Only Draft CS can be marked as Reviewed" });

            cs.Status = "Reviewed";
            cs.UpdatedBy = User.Identity?.Name;
            cs.UpdatedDate = DateTime.Now;
            _csRepo.Update(cs);
            await _csRepo.SaveChangesAsync();

            return Ok(new { message = "CS marked as Reviewed" });
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin, MD")]
        public async Task<IActionResult> Approve(int id)
        {
            var cs = await _csRepo.GetByIdAsync(id);
            if (cs == null || !cs.IsActive)
                return NotFound(new { message = "CS not found" });

            if (cs.Status != "Reviewed")
                return BadRequest(new { message = "Only Reviewed CS can be approved" });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            cs.Status = "Approved";
            cs.ApprovedById = int.Parse(currentUserId!);
            cs.ApprovedAt = DateTime.Now;
            cs.UpdatedBy = User.Identity?.Name;
            cs.UpdatedDate = DateTime.Now;
            _csRepo.Update(cs);
            await _csRepo.SaveChangesAsync();

            return Ok(new { message = "CS approved successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var cs = await _csRepo.GetByIdAsync(id);
            if (cs == null || !cs.IsActive)
                return NotFound(new { message = "CS not found" });

            if (cs.Status != "Draft")
                return BadRequest(new { message = "Only Draft CS can be deleted" });

            _csRepo.SoftDelete(cs);
            await _csRepo.SaveChangesAsync();

            return Ok(new { message = "CS deleted successfully" });
        }

        private async Task<CSResponseDto> MapToResponse(ComparativeStatement cs)
        {
            var rows = await _csSupplierRowRepo.GetByCSIdAsync(cs.Id);

            return new CSResponseDto
            {
                Id = cs.Id,
                CSNumber = cs.CSNumber,
                CSDate = cs.CSDate,
                Status = cs.Status,
                TotalBDTAmount = cs.TotalBDTAmount,
                Remarks = cs.Remarks,
                ApprovedAt = cs.ApprovedAt,
                RFQId = cs.RFQId,
                RFQNumber = cs.RFQ?.RFQNumber,
                CreatedById = cs.CreatedById,
                CreatedByEmail = cs.CreatedByUser?.Email,
                ApprovedById = cs.ApprovedById,
                Items = cs.CSItems.Where(i => i.IsActive).Select(i => new CSItemResponseDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name,
                    SelectedQuotationItemId = i.SelectedQuotationItemId ?? 0,
                    UnitPrice = i.SelectedQuotationItem?.UnitPrice ?? 0,
                    OfferedQuantity = i.SelectedQuotationItem?.OfferedQuantity ?? 0,
                    TotalPrice = i.SelectedQuotationItem?.TotalPrice ?? 0,
                    BDTAmount = i.SelectedQuotationItem?.BDTAmount ?? 0,
                    SupplierId = i.SelectedQuotationItem?.SupplierQuotation?.SupplierId ?? 0,
                    SupplierName = i.SelectedQuotationItem?.SupplierQuotation?.Supplier?.Name,
                    Remarks = i.Remarks,
                    IsSelected = i.IsSelected
                }).ToList(),
                SupplierRows = rows.Select(r => new CSSupplierRowDto
                {
                    Id = r.Id,
                    ProductId = r.QuotationItem?.ProductId ?? r.CSItem?.ProductId ?? 0,
                    ProductName = r.QuotationItem?.Product?.Name ?? r.CSItem?.Product?.Name ?? "Unknown Product",
                    SupplierId = r.SupplierId,
                    SupplierName = r.Supplier?.Name,
                    RowNumber = 0, // Not in entity
                    OfferedQuantity = r.Qty,
                    UnitPrice = r.UnitPrice,
                    TotalPrice = r.TotalPrice,
                    BDTAmount = r.TotalPrice, // Using TotalPrice as BDTAmount
                    DeliveryDays = r.QuotationItem?.SupplierQuotation?.DeliveryDays ?? 0,
                    IsSelected = r.IsSelected,
                    SelectionRemarks = r.SelectedReason
                }).ToList()
            };
        }

        private async Task<string> GenerateCSNumber()
        {
            var year = DateTime.Now.Year;
            var all = await _csRepo.FindAsync(c => c.CSNumber.StartsWith($"CS-{year}-"));
            int next = 1;
            if (all.Any())
            {
                var maxNum = all.Max(c =>
                {
                    var parts = c.CSNumber.Split('-');
                    return parts.Length == 3 && int.TryParse(parts[2], out int num) ? num : 0;
                });
                next = maxNum + 1;
            }
            return $"CS-{year}-{next:D3}";
        }
    }
}