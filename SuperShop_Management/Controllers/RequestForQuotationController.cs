using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.DTOs.RequestForQuotation;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;
using System.Security.Claims;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RequestForQuotationController : ControllerBase
    {
        private readonly IRFQRepository _rfqRepo;
        private readonly IRequisitionRepository _requisitionRepo;
        private readonly IRFQSupplierRepository _rfqSupplierRepo;
        private readonly ISupplierRepository _supplierRepo;
        private readonly ICSRepository _csRepo;
        private readonly UserManager<IdentityUser<int>> _userManager;

        public RequestForQuotationController(
            IRFQRepository rfqRepo,
            IRequisitionRepository requisitionRepo,
            IRFQSupplierRepository rfqSupplierRepo,
            ISupplierRepository supplierRepo,
            ICSRepository csRepo,
            UserManager<IdentityUser<int>> userManager)
        {
            _rfqRepo = rfqRepo;
            _requisitionRepo = requisitionRepo;
            _rfqSupplierRepo = rfqSupplierRepo;
            _supplierRepo = supplierRepo;
            _csRepo = csRepo;
            _userManager = userManager;
        }

        // GET: api/RequestForQuotation
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rfqs = await _rfqRepo.GetActiveRFQsAsync();
            var response = new List<RFQResponseDto>();
            foreach(var rfq in rfqs)
            {
                response.Add(await MapToResponseAsync(rfq));
            }
            return Ok(response);
        }

        // GET: api/RequestForQuotation/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var rfq = await _rfqRepo.GetByIdAsync(id);
            if (rfq == null || !rfq.IsActive)
                return NotFound(new { message = "RFQ not found" });
            return Ok(await MapToResponseAsync(rfq));
        }

        // POST: api/RequestForQuotation
        [HttpPost]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> Create([FromBody] RFQRequestDto dto)
        {
            // Check if requisition exists and is approved
            var requisition = await _requisitionRepo.GetByIdAsync(dto.RequisitionId);
            if (requisition == null || !requisition.IsActive)
                return BadRequest(new { message = "Requisition not found" });

            if (requisition.Status != "Approved")
                return BadRequest(new { message = "Only approved requisitions can be used for RFQ" });

            // Check if RFQ already exists for this requisition
            var existingRFQs = await _rfqRepo.GetByRequisitionIdAsync(dto.RequisitionId);
            if (existingRFQs.Any())
                return BadRequest(new { message = "An RFQ already exists for this requisition" });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(currentUserId);
            if (user == null)
                return Unauthorized();

            var rfqNumber = await GenerateRFQNumber();

            var rfq = new RequestForQuotation
            {
                RFQNumber = rfqNumber,
                RFQDate = DateTime.Now,
                QuotationDeadline = dto.QuotationDeadline,
                Notes = dto.Notes,
                Status = "Sent",
                RequisitionId = dto.RequisitionId,
                CreatedById = int.Parse(currentUserId),
                IsActive = true,
                CreatedBy = user.Email ?? user.UserName,
                CreatedDate = DateTime.Now
            };

            await _rfqRepo.AddAsync(rfq);
            await _rfqRepo.SaveChangesAsync();

            // Update requisition status (optional)
            requisition.Status = "RFQSent";
            _requisitionRepo.Update(requisition);
            await _requisitionRepo.SaveChangesAsync();

            return Ok(new { message = "RFQ created successfully", id = rfq.Id, number = rfq.RFQNumber });
        }

        // POST: api/RequestForQuotation/5/send
        [HttpPost("{id}/send")]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> SendToSuppliers(int id, [FromBody] SendRFQDto dto)
        {
            var rfq = await _rfqRepo.GetByIdAsync(id);
            if (rfq == null || !rfq.IsActive)
                return NotFound(new { success = false, message = "RFQ not found" });

            if (rfq.Status == "Closed")
                return BadRequest(new { success = false, message = "Cannot send closed RFQ" });

            if (dto.SupplierIds == null || !dto.SupplierIds.Any())
                return BadRequest(new { success = false, message = "At least one supplier must be selected" });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(currentUserId);
            if (user == null)
                return Unauthorized();

            // Create RFQSupplier records for each supplier
            foreach (var supplierId in dto.SupplierIds)
            {
                var supplier = await _supplierRepo.GetByIdAsync(supplierId);
                if (supplier == null || !supplier.IsActive)
                    continue;

                // Check if already sent to this supplier
                var existing = await _rfqSupplierRepo.FindAsync(rs => 
                    rs.RFQId == id && rs.SupplierId == supplierId && rs.IsActive);
                if (existing.Any())
                    continue;

                var rfqSupplier = new RFQSupplier
                {
                    RFQId = id,
                    SupplierId = supplierId,
                    SentAt = DateTime.Now,
                    IsActive = true,
                    CreatedBy = user.Email ?? user.UserName,
                    CreatedDate = DateTime.Now
                };

                await _rfqSupplierRepo.AddAsync(rfqSupplier);
            }

            await _rfqSupplierRepo.SaveChangesAsync();

            // Update RFQ status
            rfq.Status = "Sent";
            rfq.UpdatedBy = user.Email ?? user.UserName;
            rfq.UpdatedDate = DateTime.Now;
            _rfqRepo.Update(rfq);
            await _rfqRepo.SaveChangesAsync();

            return Ok(new { success = true, message = $"RFQ sent to {dto.SupplierIds.Count} supplier(s)" });
        }

        // GET: api/RequestForQuotation/5/print
        [HttpGet("{id}/print")]
        public async Task<IActionResult> PrintRFQ(int id)
        {
            var rfq = await _rfqRepo.GetByIdAsync(id);
            if (rfq == null || !rfq.IsActive)
                return NotFound(new { success = false, message = "RFQ not found" });

            // Get RFQ suppliers
            var rfqSuppliers = await _rfqSupplierRepo.FindAsync(rs => rs.RFQId == id && rs.IsActive);

            var printData = new
            {
                rfq.RFQNumber,
                rfq.RFQDate,
                rfq.QuotationDeadline,
                rfq.Notes,
                Requisition = new
                {
                    rfq.Requisition?.RequisitionNumber,
                    RequestedDate = rfq.Requisition?.RequisitionDate,
                    Items = rfq.Requisition?.RequisitionItems?.Where(i => i.IsActive).Select(i => new
                    {
                        ProductName = i.Product?.Name,
                        ProductBarcode = i.Product?.Barcode,
                        RequestedQuantity = i.RequiredQuantity,
                        UnitName = i.Product?.Unit?.NameOfUnit
                    }).ToList()
                },
                Suppliers = rfqSuppliers.Select(rs => new
                {
                    rs.Supplier?.Name,
                    rs.Supplier?.ContactPerson,
                    rs.Supplier?.Email,
                    rs.Supplier?.Phone,
                    SentDate = rs.SentAt
                }).ToList(),
                CreatedBy = rfq.CreatedByUser?.Email,
                CreatedDate = rfq.CreatedDate
            };

            return Ok(new { success = true, data = printData });
        }

        // PUT: api/RequestForQuotation/5/close
        [HttpPut("{id}/close")]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> Close(int id)
        {
            var rfq = await _rfqRepo.GetByIdAsync(id);
            if (rfq == null || !rfq.IsActive)
                return NotFound(new { message = "RFQ not found" });

            if (rfq.Status == "Closed")
                return BadRequest(new { message = "RFQ is already closed" });

            rfq.Status = "Closed";
            rfq.UpdatedBy = User.Identity?.Name;
            rfq.UpdatedDate = DateTime.Now;

            _rfqRepo.Update(rfq);
            await _rfqRepo.SaveChangesAsync();

            return Ok(new { message = "RFQ closed successfully" });
        }

        // DELETE: api/RequestForQuotation/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var rfq = await _rfqRepo.GetByIdAsync(id);
            if (rfq == null || !rfq.IsActive)
                return NotFound(new { message = "RFQ not found" });

            // Only allow delete if no quotations received? (optional)
            _rfqRepo.SoftDelete(rfq);
            await _rfqRepo.SaveChangesAsync();

            return Ok(new { message = "RFQ deleted successfully" });
        }

        private async Task<RFQResponseDto> MapToResponseAsync(RequestForQuotation rfq)
        {
            var existingCS = await _csRepo.GetByRFQIdAsync(rfq.Id);
            var rfqSuppliers = await _rfqSupplierRepo.GetByRFQIdAsync(rfq.Id);
            
            return new RFQResponseDto
            {
                Id = rfq.Id,
                RFQNumber = rfq.RFQNumber,
                RFQDate = rfq.RFQDate,
                QuotationDeadline = rfq.QuotationDeadline,
                Status = rfq.Status,
                Notes = rfq.Notes,
                RequisitionId = rfq.RequisitionId,
                RequisitionNumber = rfq.Requisition?.RequisitionNumber,
                CreatedById = rfq.CreatedById,
                CreatedByEmail = rfq.CreatedByUser?.Email,
                HasCS = existingCS != null,
                CSId = existingCS?.Id,
                Suppliers = rfqSuppliers.Select(rs => new RFQSupplierDto
                {
                    Id = rs.SupplierId,
                    Name = rs.Supplier?.Name ?? "Unknown Supplier",
                    ContactPerson = rs.Supplier?.ContactPerson
                }).ToList(),
                Items = rfq.Requisition?.RequisitionItems?
                    .Where(i => i.IsActive)
                    .Select(i => new DTOs.RequestForQuotation.RFQItemDto
                    {
                        ProductId = i.ProductId,
                        ProductName = i.Product?.Name ?? "Unknown Product",
                        RequiredQuantity = i.RequiredQuantity
                    }).ToList() ?? new List<DTOs.RequestForQuotation.RFQItemDto>()
            };
        }

        private async Task<string> GenerateRFQNumber()
        {
            var year = DateTime.Now.Year;
            var all = await _rfqRepo.FindAsync(r => r.RFQNumber.StartsWith($"RFQ-{year}-"));
            int next = 1;
            if (all.Any())
            {
                var maxNum = all.Max(r => {
                    var parts = r.RFQNumber.Split('-');
                    return parts.Length == 3 && int.TryParse(parts[2], out int num) ? num : 0;
                });
                next = maxNum + 1;
            }
            return $"RFQ-{year}-{next:D3}";
        }
    }
}