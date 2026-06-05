using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.Data;
using SuperShop_Management.DTOs.EmployeeRequisition;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;
using System.Security.Claims;

namespace SuperShop_Management.Controllers
{
    [Route("api/requisitions")]
    [ApiController]
    [Authorize]
    public class EmployeeRequisitionController : ControllerBase
    {
        private readonly IEmployeeRequisitionRepository _requisitionRepo;
        private readonly IProductRepository _productRepo;
        private readonly IInventoryRepository _inventoryRepo;
        private readonly IDepartmentRepository _deptRepo;
        private readonly UserManager<IdentityUser<int>> _userManager;
        private readonly AppDbContext _context;

        public EmployeeRequisitionController(
            IEmployeeRequisitionRepository requisitionRepo,
            IProductRepository productRepo,
            IInventoryRepository inventoryRepo,
            IDepartmentRepository deptRepo,
            UserManager<IdentityUser<int>> userManager,
            AppDbContext context)
        {
            _requisitionRepo = requisitionRepo;
            _productRepo = productRepo;
            _inventoryRepo = inventoryRepo;
            _deptRepo = deptRepo;
            _userManager = userManager;
            _context = context;
        }

        // POST: api/requisitions
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmployeeRequisitionRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "User not authenticated" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized(new { success = false, message = "User not found" });

            // Validate department
            var department = await _deptRepo.GetByIdAsync(dto.DepartmentId);
            if (department == null || !department.IsActive)
                return BadRequest(new { success = false, message = "Invalid department" });

            // Validate items
            if (dto.Items == null || !dto.Items.Any())
                return BadRequest(new { success = false, message = "At least one item is required" });

            // Generate ONE requisition number for all items
            var requisitionNo = await GenerateRequisitionNumber();

            // Create master requisition
            var requisition = new Requisition
            {
                RequisitionNo = requisitionNo,
                RequestedBy = int.Parse(userId),
                DepartmentId = dto.DepartmentId,
                Status = "draft",
                RequiredByDate = dto.RequiredByDate,
                Notes = dto.Notes,
                IsActive = true,
                CreatedBy = user.Email ?? user.UserName,
                CreatedDate = DateTime.Now
            };

            // Add items
            foreach (var itemDto in dto.Items)
            {
                // Validate product
                var product = await _productRepo.GetByIdAsync(itemDto.ItemId);
                if (product == null || !product.IsActive)
                    return BadRequest(new { success = false, message = $"Invalid product: {itemDto.ItemName}" });

                // Auto-fetch current stock from inventory
                var inventories = await _inventoryRepo.GetByProductIdAsync(itemDto.ItemId);
                int currentStock = inventories.Sum(i => i.AvailableQuantity);

                var item = new EmployeeRequisitionItem
                {
                    ItemId = itemDto.ItemId,
                    ItemName = itemDto.ItemName,
                    RequiredQty = itemDto.RequiredQty,
                    CurrentStock = currentStock,
                    Remarks = itemDto.Remarks,
                    IsActive = true,
                    CreatedBy = user.Email ?? user.UserName,
                    CreatedDate = DateTime.Now
                };

                requisition.Items.Add(item);
            }

            await _requisitionRepo.AddAsync(requisition);
            await _requisitionRepo.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Requisition created successfully",
                data = new
                {
                    id = requisition.Id,
                    requisitionNo = requisition.RequisitionNo,
                    itemCount = requisition.Items.Count
                }
            });
        }

        // GET: api/requisitions
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var isDeptHead = User.Claims.Any(c => c.Type == "permission" && c.Value == "requisition:approve");

            IEnumerable<Requisition> requisitions;

            if (isAdmin)
            {
                requisitions = await _requisitionRepo.GetAllAsync();
            }
            else if (isDeptHead)
            {
                var deptIdClaim = User.FindFirstValue("DepartmentId");
                if (!string.IsNullOrEmpty(deptIdClaim) && int.TryParse(deptIdClaim, out var deptId))
                {
                    requisitions = await _requisitionRepo.GetByDepartmentIdAsync(deptId);
                }
                else
                {
                    requisitions = await _requisitionRepo.GetByRequestedByIdAsync(int.Parse(userId!));
                }
            }
            else
            {
                requisitions = await _requisitionRepo.GetByRequestedByIdAsync(int.Parse(userId!));
            }

            var response = requisitions.Select(r => MapToResponse(r));
            return Ok(new { success = true, data = response });
        }

        // GET: api/requisitions/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var requisition = await _requisitionRepo.GetByIdAsync(id);
            if (requisition == null || !requisition.IsActive)
                return NotFound(new { success = false, message = "Requisition not found" });

            var response = MapToResponse(requisition);
            return Ok(new { success = true, data = response });
        }

        // GET: api/requisitions/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyRequisitions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "User not authenticated" });

            var requisitions = await _requisitionRepo.GetByRequestedByIdAsync(int.Parse(userId));
            var response = requisitions.Select(r => MapToResponse(r));
            return Ok(new { success = true, data = response });
        }

        // PATCH: api/requisitions/{id}/submit
        [HttpPatch("{id}/submit")]
        public async Task<IActionResult> Submit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "User not authenticated" });

            var requisition = await _requisitionRepo.GetByIdAsync(id);
            if (requisition == null || !requisition.IsActive)
                return NotFound(new { success = false, message = "Requisition not found" });

            // Only creator can submit
            if (requisition.RequestedBy != int.Parse(userId))
                return Forbid();

            if (requisition.Status != "draft" && requisition.Status != "revised")
                return BadRequest(new { success = false, message = "Only draft or revise requisitions can be submitted" });

            requisition.Status = "pending_dept_head";
            requisition.SubmittedAt = DateTime.Now;
            requisition.UpdatedBy = User.Identity?.Name;
            requisition.UpdatedDate = DateTime.Now;

            _requisitionRepo.Update(requisition);
            await _requisitionRepo.SaveChangesAsync();

            return Ok(new { success = true, message = "Requisition submitted to department head" });
        }

        // PATCH: api/requisitions/{id}/revise
        [HttpPatch("{id}/revise")]
        public async Task<IActionResult> Revise(int id, [FromBody] EmployeeRequisitionReviseDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deptIdClaim = User.FindFirstValue("DepartmentId");
            
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(deptIdClaim))
                return Unauthorized(new { success = false, message = "User not authenticated" });

            var requisition = await _requisitionRepo.GetByIdAsync(id);
            if (requisition == null || !requisition.IsActive)
                return NotFound(new { success = false, message = "Requisition not found" });

            // Only dept_head of same department can revise
            if (!int.TryParse(deptIdClaim, out var userDeptId) || requisition.DepartmentId != userDeptId)
                return Forbid();

            if (requisition.Status != "pending_dept_head")
                return BadRequest(new { success = false, message = "Only pending requisitions can be revise" });

            // Update master fields if provided
            if (dto.RequiredByDate.HasValue)
                requisition.RequiredByDate = dto.RequiredByDate;

            if (!string.IsNullOrEmpty(dto.Notes))
                requisition.Notes = dto.Notes;

            // Update items if provided
            if (dto.Items != null && dto.Items.Any())
            {
                // Explicitly delete old items (due to DeleteBehavior.Restrict)
                var oldItems = requisition.Items.ToList();
                foreach (var oldItem in oldItems)
                {
                    _context.EmployeeRequisitionItems.Remove(oldItem);
                }
                await _context.SaveChangesAsync();

                // Add new items
                foreach (var itemDto in dto.Items)
                {
                    var product = await _productRepo.GetByIdAsync(itemDto.ItemId);
                    if (product == null || !product.IsActive)
                        return BadRequest(new { success = false, message = $"Invalid product: {itemDto.ItemName}" });

                    var inventories = await _inventoryRepo.GetByProductIdAsync(itemDto.ItemId);
                    int currentStock = inventories.Sum(i => i.AvailableQuantity);

                    requisition.Items.Add(new EmployeeRequisitionItem
                    {
                        ItemId = itemDto.ItemId,
                        ItemName = itemDto.ItemName,
                        RequiredQty = itemDto.RequiredQty,
                        CurrentStock = currentStock,
                        Remarks = itemDto.Remarks,
                        IsActive = true,
                        CreatedBy = User.Identity?.Name,
                        CreatedDate = DateTime.Now
                    });
                }
            }

            requisition.Status = "revised";
            requisition.UpdatedBy = User.Identity?.Name;
            requisition.UpdatedDate = DateTime.Now;

            _requisitionRepo.Update(requisition);
            await _requisitionRepo.SaveChangesAsync();

            return Ok(new { success = true, message = "Requisition revise successfully" });
        }

        // PATCH: api/requisitions/{id}/update-revised
        [HttpPatch("{id}/update-revised")]
        public async Task<IActionResult> UpdateRevised(int id, [FromBody] EmployeeRequisitionReviseDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "User not authenticated" });

            var requisition = await _requisitionRepo.GetByIdAsync(id);
            if (requisition == null || !requisition.IsActive)
                return NotFound(new { success = false, message = "Requisition not found" });

            // Only creator can update revised requisition
            if (requisition.RequestedBy != int.Parse(userId))
                return Forbid();

            var currentStatus = requisition.Status?.ToLowerInvariant();
            if (currentStatus != "revised")
                return BadRequest(new { success = false, message = $"Only revise requisitions can be updated. Current status: {requisition.Status}" });

            // Employee must provide at least one item while updating
            if (dto.Items == null || !dto.Items.Any())
                return BadRequest(new { success = false, message = "At least one item is required" });

            if (dto.RequiredByDate.HasValue)
                requisition.RequiredByDate = dto.RequiredByDate;

            requisition.Notes = dto.Notes;

            var oldItems = requisition.Items.ToList();
            foreach (var oldItem in oldItems)
            {
                _context.EmployeeRequisitionItems.Remove(oldItem);
            }
            await _context.SaveChangesAsync();

            foreach (var itemDto in dto.Items)
            {
                var product = await _productRepo.GetByIdAsync(itemDto.ItemId);
                if (product == null || !product.IsActive)
                    return BadRequest(new { success = false, message = $"Invalid product: {itemDto.ItemName}" });

                var inventories = await _inventoryRepo.GetByProductIdAsync(itemDto.ItemId);
                int currentStock = inventories.Sum(i => i.AvailableQuantity);

                requisition.Items.Add(new EmployeeRequisitionItem
                {
                    ItemId = itemDto.ItemId,
                    ItemName = itemDto.ItemName,
                    RequiredQty = itemDto.RequiredQty,
                    CurrentStock = currentStock,
                    Remarks = itemDto.Remarks,
                    IsActive = true,
                    CreatedBy = User.Identity?.Name,
                    CreatedDate = DateTime.Now
                });
            }

            // After employee updates a revised requisition, send it back to dept head review queue
            requisition.Status = "pending_dept_head";
            requisition.SubmittedAt = DateTime.Now;
            requisition.UpdatedBy = User.Identity?.Name;
            requisition.UpdatedDate = DateTime.Now;

            _requisitionRepo.Update(requisition);
            await _requisitionRepo.SaveChangesAsync();

            return Ok(new { success = true, message = "Requisition resubmitted to department head successfully" });
        }

        // PATCH: api/requisitions/{id}/approve
        [HttpPatch("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deptIdClaim = User.FindFirstValue("DepartmentId");
            
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(deptIdClaim))
                return Unauthorized(new { success = false, message = "User not authenticated" });

            var requisition = await _requisitionRepo.GetByIdAsync(id);
            if (requisition == null || !requisition.IsActive)
                return NotFound(new { success = false, message = "Requisition not found" });

            // Only dept_head of same department can approve
            if (!int.TryParse(deptIdClaim, out var userDeptId) || requisition.DepartmentId != userDeptId)
                return Forbid();

            if (requisition.Status != "pending_dept_head" && requisition.Status != "revised")
                return BadRequest(new { success = false, message = "Only pending or revise requisitions can be approved" });

            requisition.Status = "forwarded_to_store";
            requisition.ApprovedAt = DateTime.Now;
            requisition.ForwardedAt = DateTime.Now;
            requisition.UpdatedBy = User.Identity?.Name;
            requisition.UpdatedDate = DateTime.Now;

            _requisitionRepo.Update(requisition);
            await _requisitionRepo.SaveChangesAsync();

            return Ok(new { success = true, message = "Requisition approved and forwarded to store" });
        }

        // DELETE: api/requisitions/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "User not authenticated" });

            var requisition = await _requisitionRepo.GetByIdAsync(id);
            if (requisition == null || !requisition.IsActive)
                return NotFound(new { success = false, message = "Requisition not found" });

            var isAdmin = User.IsInRole("Admin");

            // Only creator can delete draft requisitions, admin can delete any
            if (!isAdmin && (requisition.RequestedBy != int.Parse(userId) || requisition.Status != "draft"))
                return Forbid();

            _requisitionRepo.SoftDelete(requisition);
            await _requisitionRepo.SaveChangesAsync();

            return Ok(new { success = true, message = "Requisition deleted successfully" });
        }

        private EmployeeRequisitionResponseDto MapToResponse(Requisition r)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deptIdClaim = User.FindFirstValue("DepartmentId");
            var isAdmin = User.IsInRole("Admin");
            var isDeptHead = User.Claims.Any(c => c.Type == "permission" && c.Value == "requisition:approve");

            bool canSubmit = !string.IsNullOrEmpty(userId) && r.RequestedBy == int.Parse(userId) && (r.Status == "draft" || r.Status == "revised");
            bool canRevise = isDeptHead && !string.IsNullOrEmpty(deptIdClaim) && int.TryParse(deptIdClaim, out var deptId) && r.DepartmentId == deptId && r.Status == "pending_dept_head";
            bool canApprove = isDeptHead && !string.IsNullOrEmpty(deptIdClaim) && int.TryParse(deptIdClaim, out var deptId2) && r.DepartmentId == deptId2 && (r.Status == "pending_dept_head" || r.Status == "revised");
            bool canCancel = isAdmin || (!string.IsNullOrEmpty(userId) && r.RequestedBy == int.Parse(userId) && r.Status == "draft");

            return new EmployeeRequisitionResponseDto
            {
                Id = r.Id,
                RequisitionNo = r.RequisitionNo,
                RequestedBy = r.RequestedBy,
                RequestedByName = r.RequestedByUser?.UserName ?? r.RequestedByUser?.Email,
                RequestedByEmail = r.RequestedByUser?.Email,
                DepartmentId = r.DepartmentId,
                DepartmentName = r.Department?.DepartmentName,
                Status = r.Status,
                RequiredByDate = r.RequiredByDate,
                Notes = r.Notes,
                SubmittedAt = r.SubmittedAt,
                ApprovedAt = r.ApprovedAt,
                ForwardedAt = r.ForwardedAt,
                CreatedDate = r.CreatedDate,
                Items = r.Items?.Select(i => new EmployeeRequisitionItemResponseDto
                {
                    Id = i.Id,
                    ItemId = i.ItemId,
                    ItemName = i.ItemName,
                    RequiredQty = i.RequiredQty,
                    CurrentStock = i.CurrentStock,
                    Remarks = i.Remarks
                }).ToList() ?? new List<EmployeeRequisitionItemResponseDto>(),
                CanSubmit = canSubmit,
                CanRevise = canRevise,
                CanApprove = canApprove,
                CanCancel = canCancel
            };
        }

        private async Task<string> GenerateRequisitionNumber()
        {
            var year = DateTime.Now.Year;
            var all = await _requisitionRepo.FindAsync(r => r.RequisitionNo.StartsWith($"REQ-{year}-"));
            int next = 1;
            if (all.Any())
            {
                var maxNumber = all.Max(r =>
                {
                    var parts = r.RequisitionNo.Split('-');
                    return parts.Length == 3 && int.TryParse(parts[2], out int num) ? num : 0;
                });
                next = maxNumber + 1;
            }
            return $"REQ-{year}-{next:D4}";
        }
    }
}
