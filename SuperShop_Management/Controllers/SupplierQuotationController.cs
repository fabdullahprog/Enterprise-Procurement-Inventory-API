using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperShop_Management.DTOs.SupplierQuotation;
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
    public class SupplierQuotationController : ControllerBase
    {
        private readonly ISupplierQuotationRepository _quotationRepo;
        private readonly IRFQRepository _rfqRepo;
        private readonly ISupplierRepository _supplierRepo;
        private readonly IProductRepository _productRepo;
        private readonly ICurrencyRepository _currencyRepo;

        public SupplierQuotationController(
            ISupplierQuotationRepository quotationRepo,
            IRFQRepository rfqRepo,
            ISupplierRepository supplierRepo,
            IProductRepository productRepo,
            ICurrencyRepository currencyRepo)
        {
            _quotationRepo = quotationRepo;
            _rfqRepo = rfqRepo;
            _supplierRepo = supplierRepo;
            _productRepo = productRepo;
            _currencyRepo = currencyRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var quotations = await _quotationRepo.GetActiveQuotationsAsync();
            var response = quotations.Select(MapToResponse);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var quotation = await _quotationRepo.GetByIdAsync(id);
            if (quotation == null || !quotation.IsActive)
                return NotFound(new { message = "Quotation not found" });
            return Ok(MapToResponse(quotation));
        }

        [HttpGet("byrfq/{rfqId}")]
        public async Task<IActionResult> GetByRFQId(int rfqId)
        {
            var quotations = await _quotationRepo.GetByRFQIdAsync(rfqId);
            var response = quotations.Select(MapToResponse);
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> Create([FromBody] SupplierQuotationRequestDto dto)
        {
            // Validate RFQ
            var rfq = await _rfqRepo.GetByIdAsync(dto.RFQId);
            if (rfq == null || !rfq.IsActive || rfq.Status == "Closed")
                return BadRequest(new { message = "Invalid RFQ" });

            // Validate Supplier
            var supplier = await _supplierRepo.GetByIdAsync(dto.SupplierId);
            if (supplier == null || !supplier.IsActive)
                return BadRequest(new { message = "Invalid Supplier" });

            // Check duplicate quotation for same RFQ+Supplier
            var existing = await _quotationRepo.GetByRFQIdAsync(dto.RFQId);
            if (existing.Any(q => q.SupplierId == dto.SupplierId))
                return BadRequest(new { message = "Quotation already submitted for this supplier for the same RFQ" });

            // Exchange rate
            decimal exchangeRate = 1;
            if (dto.CurrencyId.HasValue)
            {
                var currency = await _currencyRepo.GetByIdAsync(dto.CurrencyId.Value);
                if (currency == null || !currency.IsActive)
                    return BadRequest(new { message = "Invalid Currency" });
                exchangeRate = currency.ExchangeRate;
            }

            // Validate items
            foreach (var itemDto in dto.Items)
            {
                var product = await _productRepo.GetByIdAsync(itemDto.ProductId);
                if (product == null || !product.IsActive)
                    return BadRequest(new { message = $"Product {itemDto.ProductId} not found" });
            }

            var quotationNumber = await GenerateQuotationNumber();

            var quotation = new SupplierQuotation
            {
                QuotationNumber = quotationNumber,
                QuotationDate = DateTime.Now,
                Status = "Pending",
                TotalBDTAmount = 0, // will calculate after items
                DeliveryDays = dto.DeliveryDays,
                Notes = dto.Notes,
                RFQId = dto.RFQId,
                SupplierId = dto.SupplierId,
                CurrencyId = dto.CurrencyId,
                IsActive = true,
                CreatedBy = User.Identity?.Name,
                CreatedDate = DateTime.Now,
                QuotationItems = new List<QuotationItem>()
            };

            decimal totalBDT = 0;
            foreach (var itemDto in dto.Items)
            {
                var product = await _productRepo.GetByIdAsync(itemDto.ProductId);
                decimal totalPrice = itemDto.UnitPrice * itemDto.OfferedQuantity;
                decimal bdtAmount = totalPrice * exchangeRate;
                totalBDT += bdtAmount;

                var quotationItem = new QuotationItem
                {
                    ProductId = itemDto.ProductId,
                    OfferedQuantity = itemDto.OfferedQuantity,
                    UnitPrice = itemDto.UnitPrice,
                    TotalPrice = totalPrice,
                    BDTAmount = bdtAmount,
                    IsActive = true,
                    CreatedBy = User.Identity?.Name,
                    CreatedDate = DateTime.Now
                };
                quotation.QuotationItems.Add(quotationItem);
            }

            quotation.TotalBDTAmount = totalBDT;

            await _quotationRepo.AddAsync(quotation);
            await _quotationRepo.SaveChangesAsync();

            // Update RFQ status to QuotationReceived (if not already)
            if (rfq.Status == "Sent")
            {
                rfq.Status = "QuotationReceived";
                _rfqRepo.Update(rfq);
                await _rfqRepo.SaveChangesAsync();
            }

            return Ok(new { message = "Quotation created successfully", id = quotation.Id, number = quotation.QuotationNumber });
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateQuotationStatusDto dto)
        {
            var quotation = await _quotationRepo.GetByIdAsync(id);
            if (quotation == null || !quotation.IsActive)
                return NotFound(new { success = false, message = "Quotation not found" });

            var validStatuses = new[] { "Pending", "Selected", "Rejected", "Accepted" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest(new { success = false, message = "Invalid status. Valid values: Pending, Selected, Rejected, Accepted" });

            quotation.Status = dto.Status;
            quotation.Notes = string.IsNullOrEmpty(dto.Notes) ? quotation.Notes : dto.Notes;
            quotation.UpdatedBy = User.Identity?.Name;
            quotation.UpdatedDate = DateTime.Now;

            _quotationRepo.Update(quotation);
            await _quotationRepo.SaveChangesAsync();

            return Ok(new { success = true, message = $"Quotation status updated to {dto.Status}" });
        }

        [HttpPut("{id}/select")]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> SelectQuotation(int id)
        {
            var quotation = await _quotationRepo.GetByIdAsync(id);
            if (quotation == null || !quotation.IsActive)
                return NotFound(new { message = "Quotation not found" });

            if (quotation.Status == "Selected")
                return BadRequest(new { message = "Quotation already selected" });

            quotation.Status = "Selected";
            quotation.UpdatedBy = User.Identity?.Name;
            quotation.UpdatedDate = DateTime.Now;
            _quotationRepo.Update(quotation);
            await _quotationRepo.SaveChangesAsync();

            return Ok(new { message = "Quotation marked as Selected" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var quotation = await _quotationRepo.GetByIdAsync(id);
            if (quotation == null || !quotation.IsActive)
                return NotFound(new { message = "Quotation not found" });

            _quotationRepo.SoftDelete(quotation);
            await _quotationRepo.SaveChangesAsync();

            return Ok(new { message = "Quotation deleted successfully" });
        }

        private SupplierQuotationResponseDto MapToResponse(SupplierQuotation q)
        {
            return new SupplierQuotationResponseDto
            {
                Id = q.Id,
                QuotationNumber = q.QuotationNumber,
                QuotationDate = q.QuotationDate,
                Status = q.Status,
                TotalBDTAmount = q.TotalBDTAmount,
                DeliveryDays = q.DeliveryDays,
                Notes = q.Notes,
                RFQId = q.RFQId,
                RFQNumber = q.RFQ?.RFQNumber,
                SupplierId = q.SupplierId,
                SupplierName = q.Supplier?.Name,
                CurrencyId = q.CurrencyId,
                CurrencyCode = q.Currency?.Code,
                Items = q.QuotationItems.Where(i => i.IsActive).Select(i => new QuotationItemResponseDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name,
                    OfferedQuantity = i.OfferedQuantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice,
                    BDTAmount = i.BDTAmount
                }).ToList()
            };
        }

        private async Task<string> GenerateQuotationNumber()
        {
            var year = DateTime.Now.Year;
            var all = await _quotationRepo.FindAsync(q => q.QuotationNumber.StartsWith($"SQ-{year}-"));
            int next = 1;
            if (all.Any())
            {
                var maxNum = all.Max(q => {
                    var parts = q.QuotationNumber.Split('-');
                    return parts.Length == 3 && int.TryParse(parts[2], out int num) ? num : 0;
                });
                next = maxNum + 1;
            }
            return $"SQ-{year}-{next:D3}";
        }
    }

    public class UpdateQuotationStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}