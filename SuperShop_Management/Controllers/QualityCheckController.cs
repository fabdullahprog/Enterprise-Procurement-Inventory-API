using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperShop_Management.DTOs.QualityCheck;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;
using System.Security.Claims;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QualityCheckController : ControllerBase
    {
        private readonly IQualityCheckRepository _qcRepo;
        private readonly IGRNRepository _grnRepo;
        private readonly IGRNItemRepository _grnItemRepo;
        private readonly UserManager<IdentityUser<int>> _userManager;

        public QualityCheckController(
            IQualityCheckRepository qcRepo,
            IGRNRepository grnRepo,
            IGRNItemRepository grnItemRepo,
            UserManager<IdentityUser<int>> userManager)
        {
            _qcRepo = qcRepo;
            _grnRepo = grnRepo;
            _grnItemRepo = grnItemRepo;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var qcs = await _qcRepo.GetActiveQCsAsync();
            var response = qcs.Select(MapToResponse);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var qc = await _qcRepo.GetByIdAsync(id);
            if (qc == null || !qc.IsActive)
                return NotFound(new { message = "QualityCheck not found" });
            return Ok(MapToResponse(qc));
        }

        [HttpGet("bygrn/{grnId}")]
        public async Task<IActionResult> GetByGRNId(int grnId)
        {
            var qc = await _qcRepo.GetByGRNIdAsync(grnId);
            if (qc == null) return NotFound();
            return Ok(MapToResponse(qc));
        }

        [HttpPost]
        [Authorize(Roles = "Admin, WarehouseManager")]
        public async Task<IActionResult> Create([FromBody] QCCreateDto dto)
        {
            // Validate GRN
            var grn = await _grnRepo.GetByIdAsync(dto.GRNId);
            if (grn == null || !grn.IsActive)
                return BadRequest(new { message = "Invalid GRN" });

            if (grn.Status != "Pending")
                return BadRequest(new { message = "QC already performed for this GRN" });

            // Check if QC already exists
            var existingQC = await _qcRepo.GetByGRNIdAsync(dto.GRNId);
            if (existingQC != null)
                return BadRequest(new { message = "QualityCheck already exists for this GRN" });

            // Get current user
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();
            var user = await _userManager.FindByIdAsync(currentUserId);
            if (user == null) return Unauthorized();

            // Validate each item
            foreach (var itemDto in dto.Items)
            {
                var grnItem = await _grnItemRepo.GetByIdAsync(itemDto.GRNItemId);
                if (grnItem == null || !grnItem.IsActive)
                    return BadRequest(new { message = $"GRNItem {itemDto.GRNItemId} not found" });

                if (itemDto.AcceptedQuantity + itemDto.RejectedQuantity > grnItem.ReceivedQuantity)
                    return BadRequest(new { message = $"Accepted + Rejected cannot exceed Received quantity for product" });
            }

            var qcNumber = await GenerateQCNumber();

            var qc = new QualityCheck
            {
                QCNumber = qcNumber,
                QCDate = DateTime.Now,
                Status = "Pending",
                Remarks = dto.Remarks,
                GRNId = dto.GRNId,
                InspectedById = int.Parse(currentUserId),
                IsActive = true,
                CreatedBy = user.Email ?? user.UserName,
                CreatedDate = DateTime.Now
            };

            // Add QC items
            bool allAccepted = true;
            bool allRejected = true;

            foreach (var itemDto in dto.Items)
            {
                var grnItem = await _grnItemRepo.GetByIdAsync(itemDto.GRNItemId);
                var qcItem = new QCItem
                {
                    GRNItemId = itemDto.GRNItemId,
                    AcceptedQuantity = itemDto.AcceptedQuantity,
                    RejectedQuantity = itemDto.RejectedQuantity,
                    Remarks = itemDto.Remarks,
                    IsActive = true,
                    CreatedBy = user.Email ?? user.UserName,
                    CreatedDate = DateTime.Now
                };

                if (qcItem.AcceptedQuantity > 0) allRejected = false;
                if (qcItem.RejectedQuantity > 0) allAccepted = false;

                qc.QCItems.Add(qcItem);

                // Update GRNItem's accepted/rejected quantities
                grnItem.AcceptedQuantity = itemDto.AcceptedQuantity;
                grnItem.RejectedQuantity = itemDto.RejectedQuantity;
                grnItem.UpdatedBy = user.Email ?? user.UserName;
                grnItem.UpdatedDate = DateTime.Now;
                _grnItemRepo.Update(grnItem);
            }

            // Determine QC status
            if (allAccepted && !allRejected)
                qc.Status = "Accepted";
            else if (allRejected && !allAccepted)
                qc.Status = "Rejected";
            else
                qc.Status = "PartiallyAccepted";

            // Update GRN status
            if (qc.Status == "Accepted")
                grn.Status = "Accepted";
            else if (qc.Status == "Rejected")
                grn.Status = "Rejected";
            else
                grn.Status = "PartiallyAccepted";

            _grnRepo.Update(grn);

            await _qcRepo.AddAsync(qc);
            await _qcRepo.SaveChangesAsync();

            return Ok(new { message = "QualityCheck completed", id = qc.Id, number = qc.QCNumber, status = qc.Status });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var qc = await _qcRepo.GetByIdAsync(id);
            if (qc == null || !qc.IsActive)
                return NotFound(new { message = "QualityCheck not found" });

            _qcRepo.SoftDelete(qc);
            await _qcRepo.SaveChangesAsync();

            return Ok(new { message = "QualityCheck deleted" });
        }

        private QCResponseDto MapToResponse(QualityCheck qc)
        {
            return new QCResponseDto
            {
                Id = qc.Id,
                QCNumber = qc.QCNumber,
                QCDate = qc.QCDate,
                Status = qc.Status,
                Remarks = qc.Remarks,
                GRNId = qc.GRNId,
                GRNNumber = qc.GRN?.GRNNumber,  // ✅ safe
                InspectedById = qc.InspectedById,
                InspectedByName = qc.InspectedBy?.Email,  // ✅ safe
                Items = qc.QCItems.Where(i => i.IsActive).Select(i => new QCItemResponseDto
                {
                    Id = i.Id,
                    GRNItemId = i.GRNItemId,
                    ProductId = i.GRNItem?.POItem?.ProductId ?? 0,
                    ProductName = i.GRNItem?.POItem?.Product?.Name ?? string.Empty,
                    ReceivedQuantity = i.GRNItem?.ReceivedQuantity ?? 0,
                    AcceptedQuantity = i.AcceptedQuantity,
                    RejectedQuantity = i.RejectedQuantity,
                    Remarks = i.Remarks
                }).ToList()
            };
        }

        private async Task<string> GenerateQCNumber()
        {
            var year = DateTime.Now.Year;
            var all = await _qcRepo.FindAsync(q => q.QCNumber.StartsWith($"QC-{year}-"));
            int next = 1;
            if (all.Any())
            {
                var maxNum = all.Max(q => {
                    var parts = q.QCNumber.Split('-');
                    return parts.Length == 3 && int.TryParse(parts[2], out int num) ? num : 0;
                });
                next = maxNum + 1;
            }
            return $"QC-{year}-{next:D3}";
        }
    }
}