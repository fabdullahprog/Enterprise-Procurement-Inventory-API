using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.DTOs.StoreIssue;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;
using System.Security.Claims;

namespace SuperShop_Management.Controllers
{
    [Route("api/store")]
    [ApiController]
    [Authorize]
    public class StoreController : ControllerBase
    {
        private readonly IEmployeeRequisitionRepository _requisitionRepo;
        private readonly IStoreIssueRepository _storeIssueRepo;
        private readonly IInventoryRepository _inventoryRepo;
        private readonly IRequisitionRepository _purchaseReqRepo;
        private readonly UserManager<IdentityUser<int>> _userManager;

        public StoreController(
            IEmployeeRequisitionRepository requisitionRepo,
            IStoreIssueRepository storeIssueRepo,
            IInventoryRepository inventoryRepo,
            IRequisitionRepository purchaseReqRepo,
            UserManager<IdentityUser<int>> userManager)
        {
            _requisitionRepo = requisitionRepo;
            _storeIssueRepo = storeIssueRepo;
            _inventoryRepo = inventoryRepo;
            _purchaseReqRepo = purchaseReqRepo;
            _userManager = userManager;
        }

        // GET: api/store/requisitions
        [HttpGet("requisitions")]
        public async Task<IActionResult> GetApprovedRequisitions()
        {
            var requisitions = await _requisitionRepo.GetForwardedToStoreAsync();
            
            var response = requisitions.Select(r => new
            {
                id = r.Id,
                requisitionNo = r.RequisitionNo,
                items = r.Items.Select(i => new
                {
                    itemId = i.ItemId,
                    itemName = i.ItemName,
                    requiredQty = i.RequiredQty,
                    currentStock = i.CurrentStock,
                    remarks = i.Remarks
                }).ToList(),
                requestedBy = r.RequestedByUser?.UserName ?? r.RequestedByUser?.Email,
                department = r.Department?.DepartmentName,
                status = r.Status,
                notes = r.Notes,
                approvedAt = r.ApprovedAt,
                forwardedAt = r.ForwardedAt
            });

            return Ok(new { success = true, data = response });
        }

        // ──────────────────────────────────────────────────────────────
        // LEGACY IssueProduct endpoint — COMMENTED OUT on 2026-05-18.
        // Reason: AmbiguousMatchException — this route conflicts with the
        //         upgraded FIFO-based endpoint in StoreIssueController.cs.
        //         All store-issue traffic now routes to StoreIssueController.
        // ──────────────────────────────────────────────────────────────
        /*
        // POST: api/store/issue
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
            var requisition = await _requisitionRepo.GetByIdAsync(dto.RequisitionId);
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
            int availableStock = inventories.Sum(i => i.AvailableQuantity);

            if (dto.IssuedQty > availableStock)
                return BadRequest(new { success = false, message = $"Insufficient stock. Available: {availableStock}" });

            // Determine issue type
            string issueType = dto.IssuedQty >= firstItem.RequiredQty ? "full" : "partial";

            // Create store issue
            var storeIssue = new StoreIssue
            {
                RequisitionId = dto.RequisitionId,
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

            // Update requisition status
            if (issueType == "full")
            {
                requisition.Status = "fully_issued";
            }
            else
            {
                requisition.Status = "partially_issued";
            }
            requisition.UpdatedBy = user.Email ?? user.UserName;
            requisition.UpdatedDate = DateTime.Now;
            _requisitionRepo.Update(requisition);

            // Deduct inventory (FIFO basis)
            int remainingQty = dto.IssuedQty;
            foreach (var inventory in inventories.Where(i => i.AvailableQuantity > 0).OrderBy(i => i.CreatedDate))
            {
                if (remainingQty <= 0) break;

                int deductQty = Math.Min(inventory.AvailableQuantity, remainingQty);
                inventory.AvailableQuantity -= deductQty;
                inventory.LastUpdated = DateTime.Now;
                _inventoryRepo.Update(inventory);

                remainingQty -= deductQty;
            }

            await _storeIssueRepo.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Product issued successfully ({issueType})",
                data = new
                {
                    id = storeIssue.Id,
                    issuedQty = dto.IssuedQty,
                    issueType = issueType,
                    remainingStock = availableStock - dto.IssuedQty
                }
            });
        }
        */
    }
}
