using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.Attributes;
using SuperShop_Management.DTOs.PurchaseRequisition;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;
using System.Security.Claims;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PurchaseRequisitionController : ControllerBase
    {
        private const string PermissionClaimType = "permission";
        private const string CancelRequisitionPermission = "requisition:cancel";

        private readonly IRequisitionRepository _requisitionRepo;
        private readonly IEmployeeRequisitionRepository _empReqRepo;
        private readonly IDepartmentRepository _deptRepo;
        private readonly IProductRepository _productRepo;
        private readonly UserManager<IdentityUser<int>> _userManager;

        public PurchaseRequisitionController(
            IRequisitionRepository requisitionRepo,
            IEmployeeRequisitionRepository empReqRepo,
            IDepartmentRepository deptRepo,
            IProductRepository productRepo,
            UserManager<IdentityUser<int>> userManager)
        {
            _requisitionRepo = requisitionRepo;
            _empReqRepo = empReqRepo;
            _deptRepo = deptRepo;
            _productRepo = productRepo;
            _userManager = userManager;
        }

        // GET: api/PurchaseRequisition
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var hasManagePermission = User.Claims.Any(c =>
                c.Type == "permission" &&
                c.Value.Equals("requisition:manage", StringComparison.OrdinalIgnoreCase));

            var requisitions = await _requisitionRepo.GetActiveRequisitionsAsync();

            // Non-admin users without manage permission can only see their own requisitions
            // or requisitions from their department if they have approve permission
            if (!isAdmin && !hasManagePermission)
            {
                var deptIdClaim = User.FindFirstValue("DepartmentId");
                var hasApprovePermission = User.Claims.Any(c =>
                    c.Type == "permission" &&
                    c.Value.Equals("requisition:approve", StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(userIdClaim) && int.TryParse(userIdClaim, out var userId))
                {
                    if (hasApprovePermission && !string.IsNullOrWhiteSpace(deptIdClaim) && int.TryParse(deptIdClaim, out var deptId))
                    {
                        // Show own requisitions + department requisitions
                        requisitions = requisitions.Where(r => r.RequestedById == userId || r.DepartmentId == deptId).ToList();
                    }
                    else
                    {
                        // Show only own requisitions
                        requisitions = requisitions.Where(r => r.RequestedById == userId).ToList();
                    }
                }
            }

            var response = await MapToResponseListAsync(requisitions);
            return Ok(response);
        }

        // GET: api/PurchaseRequisition/pending
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingRequisitions()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var hasManagePermission = User.Claims.Any(c =>
                c.Type == "permission" &&
                c.Value.Equals("requisition:manage", StringComparison.OrdinalIgnoreCase));

            var requisitions = await _requisitionRepo.GetActiveRequisitionsAsync();
            var pending = requisitions.Where(r => r.Status == "Pending").ToList();

            // Filter based on user permissions
            if (!isAdmin && !hasManagePermission)
            {
                var deptIdClaim = User.FindFirstValue("DepartmentId");
                var hasApprovePermission = User.Claims.Any(c =>
                    c.Type == "permission" &&
                    c.Value.Equals("requisition:approve", StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(userIdClaim) && int.TryParse(userIdClaim, out var userId))
                {
                    if (hasApprovePermission && !string.IsNullOrWhiteSpace(deptIdClaim) && int.TryParse(deptIdClaim, out var deptId))
                    {
                        // Show own pending + department pending
                        pending = pending.Where(r => r.RequestedById == userId || r.DepartmentId == deptId).ToList();
                    }
                    else
                    {
                        // Show only own pending
                        pending = pending.Where(r => r.RequestedById == userId).ToList();
                    }
                }
            }

            var response = await MapToResponseListAsync(pending);
            return Ok(response);
        }

        // GET: api/PurchaseRequisition/my-department-pending
        [HttpGet("my-department-pending")]
        [RequirePermission("requisition:approve")]
        public async Task<IActionResult> GetMyDepartmentPendingRequisitions()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "User not authenticated" });

            var deptIdClaim = User.FindFirstValue("DepartmentId");
            if (string.IsNullOrWhiteSpace(deptIdClaim) || !int.TryParse(deptIdClaim, out var deptId))
                return BadRequest(new { message = "User not assigned to any department" });

            var requisitions = await _requisitionRepo.GetActiveRequisitionsAsync();
            var myDeptPending = requisitions
                .Where(r => r.Status == "Pending" && r.DepartmentId == deptId)
                .ToList();

            var response = await MapToResponseListAsync(myDeptPending);
            return Ok(response);
        }

        // GET: api/PurchaseRequisition/for-approval
        [HttpGet("for-approval")]
        public async Task<IActionResult> GetRequisitionsForApproval()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "User not authenticated" });

            // Authorization check: Admin OR PurchaseManager OR specific permission
            var isGlobalApprover = User.IsInRole("Admin") || User.IsInRole("PurchaseManager");
            var hasApprovePermission = User.Claims.Any(c => 
                c.Type == "permission" && 
                c.Value.Equals("requisition:approve", StringComparison.OrdinalIgnoreCase));

            if (!isGlobalApprover && !hasApprovePermission)
                return Forbid();

            var requisitions = await _requisitionRepo.GetActiveRequisitionsAsync();
            var pending = requisitions.Where(r => r.Status == "Pending").ToList();

            // Global approver can approve all pending requisitions
            if (!isGlobalApprover)
            {
                var deptIdClaim = User.FindFirstValue("DepartmentId");
                if (string.IsNullOrWhiteSpace(deptIdClaim) || !int.TryParse(deptIdClaim, out var deptId))
                    return BadRequest(new { message = "User not assigned to any department" });

                // Department Head can only approve requisitions from their department
                pending = pending.Where(r => r.DepartmentId == deptId).ToList();
            }

            var response = await MapToResponseListAsync(pending);
            return Ok(response);
        }

        // GET: api/PurchaseRequisition/my-requisitions
        [HttpGet("my-requisitions")]
        public async Task<IActionResult> GetMyRequisitions()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "User not authenticated" });

            var requisitions = await _requisitionRepo.GetByRequestedByIdAsync(userId);
            var response = await MapToResponseListAsync(requisitions);
            return Ok(response);
        }

        // GET: api/PurchaseRequisition/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var requisition = await _requisitionRepo.GetByIdAsync(id);
            if (requisition == null || !requisition.IsActive)
                return NotFound(new { message = "Requisition not found" });
            
            var response = await MapToResponseAsync(requisition);
            return Ok(response);
        }

        // GET: api/PurchaseRequisition/5/status
        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetRequisitionStatus(int id)
        {
            var requisition = await _requisitionRepo.GetByIdAsync(id);
            if (requisition == null || !requisition.IsActive)
                return NotFound(new { message = "Requisition not found" });

            var response = new
            {
                requisitionId = requisition.Id,
                requisitionNumber = requisition.RequisitionNumber,
                status = requisition.Status,
                requisitionDate = requisition.RequisitionDate,
                requiredByDate = requisition.RequiredByDate,
                requestedBy = new
                {
                    id = requisition.RequestedById,
                    email = requisition.RequestedBy?.Email,
                    name = requisition.RequestedBy?.UserName ?? requisition.RequestedBy?.Email
                },
                department = new
                {
                    id = requisition.DepartmentId,
                    name = requisition.Department?.DepartmentName
                },
                approval = requisition.ApprovedById.HasValue ? new
                {
                    approvedById = requisition.ApprovedById,
                    approvedByEmail = requisition.ApprovedBy?.Email,
                    approvedByName = requisition.ApprovedBy?.UserName ?? requisition.ApprovedBy?.Email,
                    approvedAt = requisition.ApprovedAt
                } : null,
                notes = requisition.Notes,
                itemCount = requisition.RequisitionItems.Count(i => i.IsActive)
            };

            return Ok(response);
        }

        // GET: api/PurchaseRequisition/5/items
        [HttpGet("{id}/items")]
        public async Task<IActionResult> GetItemsByRequisitionId(int id)
        {
            var requisition = await _requisitionRepo.GetByIdAsync(id);
            if (requisition == null || !requisition.IsActive)
                return NotFound(new { message = "Requisition not found" });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && currentUserId != requisition.RequestedById.ToString())
                return Forbid();

            var items = await _requisitionRepo.GetItemsByRequisitionIdAsync(id);
            var response = items.Select(i => new
            {
                id = i.Id,
                requiredQuantity = i.RequiredQuantity,
                remarks = i.Remarks,
                requisitionId = i.RequisitionId,
                productId = i.ProductId,
                product = i.Product == null ? null : new
                {
                    id = i.Product.Id,
                    name = i.Product.Name,
                    barcode = i.Product.Barcode
                },
                isActive = i.IsActive
            });

            return Ok(response);
        }

        // POST: api/PurchaseRequisition
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RequisitionRequestDto dto)
        {
            // Get current logged in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized(new { message = "User not found" });

            // Validate department
            var department = await _deptRepo.GetByIdAsync(dto.DepartmentId);
            if (department == null || !department.IsActive)
                return BadRequest(new { message = "Invalid Department" });

            // Check if department can request items (optional validation)
            // if (!department.CanRequestItem) return BadRequest(...);

            // Validate products
            foreach (var itemDto in dto.Items)
            {
                var product = await _productRepo.GetByIdAsync(itemDto.ProductId);
                if (product == null || !product.IsActive)
                    return BadRequest(new { message = $"Product with id {itemDto.ProductId} not found" });
            }

            // Generate requisition number
            var requisitionNumber = await GenerateRequisitionNumber();

            var requisition = new PurchaseRequisition
            {
                RequisitionNumber = requisitionNumber,
                RequisitionDate = DateTime.Now,
                RequiredByDate = dto.RequiredByDate,
                Notes = dto.Notes,
                Status = "Pending",
                DepartmentId = dto.DepartmentId,
                RequestedById = int.Parse(userId),
                IsActive = true,
                CreatedBy = user.Email ?? user.UserName,
                CreatedDate = DateTime.Now
            };

            // Add items
            foreach (var itemDto in dto.Items)
            {
                requisition.RequisitionItems.Add(new RequisitionItem
                {
                    ProductId = itemDto.ProductId,
                    RequiredQuantity = itemDto.RequiredQuantity,
                    Remarks = itemDto.Remarks,
                    IsActive = true,
                    CreatedBy = user.Email ?? user.UserName,
                    CreatedDate = DateTime.Now
                });
            }

            await _requisitionRepo.AddAsync(requisition);
            await _requisitionRepo.SaveChangesAsync();

            // Update source employee requisitions if any
            if (dto.SourceRequisitionIds != null && dto.SourceRequisitionIds.Any())
            {
                foreach (var sourceId in dto.SourceRequisitionIds)
                {
                    var sourceReq = await _empReqRepo.GetByIdAsync(sourceId);
                    if (sourceReq != null && sourceReq.IsActive)
                    {
                        sourceReq.Status = "purchase_requisition_created";
                        sourceReq.UpdatedBy = user.Email ?? user.UserName;
                        sourceReq.UpdatedDate = DateTime.Now;
                        _empReqRepo.Update(sourceReq);
                    }
                }
                await _empReqRepo.SaveChangesAsync();
            }

            return Ok(new { message = "Requisition created successfully", id = requisition.Id, number = requisition.RequisitionNumber });
        }

        // PUT: api/PurchaseRequisition/5
        [HttpPut("{id}")]
        [RequirePermission("requisition:approve")]
        public async Task<IActionResult> Update(int id, [FromBody] RequisitionRequestDto dto)
        {
            var requisition = await _requisitionRepo.GetByIdAsync(id);
            if (requisition == null || !requisition.IsActive)
                return NotFound(new { message = "Requisition not found" });

            if (requisition.Status != "Pending")
                return BadRequest(new { message = "Only pending requisitions can be updated" });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(currentUserId) || !int.TryParse(currentUserId, out var userId))
                return Unauthorized(new { message = "User not authenticated" });

            // Check if user is from the same department (unless Admin)
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin)
            {
                var deptIdClaim = User.FindFirstValue("DepartmentId");
                if (string.IsNullOrWhiteSpace(deptIdClaim) || !int.TryParse(deptIdClaim, out var userDeptId))
                    return BadRequest(new { message = "User not assigned to any department" });

                if (requisition.DepartmentId != userDeptId)
                    return Forbid();
            }

            var user = await _userManager.FindByIdAsync(currentUserId);
            if (user == null)
                return Unauthorized(new { message = "User not found" });

            // Validate products
            foreach (var itemDto in dto.Items)
            {
                var product = await _productRepo.GetByIdAsync(itemDto.ProductId);
                if (product == null || !product.IsActive)
                    return BadRequest(new { message = $"Product with id {itemDto.ProductId} not found" });
            }

            // Update requisition fields
            requisition.RequiredByDate = dto.RequiredByDate;
            requisition.Notes = dto.Notes;
            requisition.UpdatedBy = user.Email ?? user.UserName;
            requisition.UpdatedDate = DateTime.Now;

            // Clear existing items collection and soft delete them
            var itemsToDelete = requisition.RequisitionItems.Where(i => i.IsActive).ToList();
            foreach (var item in itemsToDelete)
            {
                item.IsActive = false;
                item.UpdatedBy = user.Email ?? user.UserName;
                item.UpdatedDate = DateTime.Now;
            }

            // Add new items to the collection
            var newItems = dto.Items.Select(itemDto => new RequisitionItem
            {
                RequisitionId = requisition.Id,
                ProductId = itemDto.ProductId,
                RequiredQuantity = itemDto.RequiredQuantity,
                Remarks = itemDto.Remarks,
                IsActive = true,
                CreatedBy = user.Email ?? user.UserName,
                CreatedDate = DateTime.Now
            }).ToList();

            foreach (var newItem in newItems)
            {
                requisition.RequisitionItems.Add(newItem);
            }

            // Save all changes
            _requisitionRepo.Update(requisition);
            await _requisitionRepo.SaveChangesAsync();

            return Ok(new { 
                message = "Requisition updated successfully", 
                requisitionNumber = requisition.RequisitionNumber,
                itemsUpdated = newItems.Count
            });
        }

        // PUT: api/PurchaseRequisition/5/approve
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var requisition = await _requisitionRepo.GetByIdAsync(id);
            if (requisition == null || !requisition.IsActive)
                return NotFound(new { message = "Requisition not found" });

            if (requisition.Status != "Pending")
                return BadRequest(new { message = "Only pending requisitions can be approved" });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(currentUserId) || !int.TryParse(currentUserId, out var userId))
                return Unauthorized(new { message = "User not authenticated" });

            // Authorization check: Admin OR PurchaseManager OR specific permission
            var isGlobalApprover = User.IsInRole("Admin") || User.IsInRole("PurchaseManager");
            var hasApprovePermission = User.Claims.Any(c => 
                c.Type == "permission" && 
                c.Value.Equals("requisition:approve", StringComparison.OrdinalIgnoreCase));

            if (!isGlobalApprover && !hasApprovePermission)
                return Forbid();

            // Check if user is from the same department (unless global approver)
            if (!isGlobalApprover)
            {
                var deptIdClaim = User.FindFirstValue("DepartmentId");
                if (string.IsNullOrWhiteSpace(deptIdClaim) || !int.TryParse(deptIdClaim, out var userDeptId))
                    return BadRequest(new { message = "User not assigned to any department" });

                if (requisition.DepartmentId != userDeptId)
                    return Forbid();
            }

            requisition.Status = "Approved";
            requisition.ApprovedById = userId;
            requisition.ApprovedAt = DateTime.Now;
            requisition.UpdatedBy = User.Identity?.Name;
            requisition.UpdatedDate = DateTime.Now;

            _requisitionRepo.Update(requisition);
            await _requisitionRepo.SaveChangesAsync();

            return Ok(new { message = "Requisition approved successfully", requisitionNumber = requisition.RequisitionNumber });
        }

        // PUT: api/PurchaseRequisition/5/reject
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectReasonDto? dto)
        {
            var requisition = await _requisitionRepo.GetByIdAsync(id);
            if (requisition == null || !requisition.IsActive)
                return NotFound(new { message = "Requisition not found" });

            if (requisition.Status != "Pending")
                return BadRequest(new { message = "Only pending requisitions can be rejected" });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(currentUserId) || !int.TryParse(currentUserId, out var userId))
                return Unauthorized(new { message = "User not authenticated" });

            // Authorization check: Admin OR PurchaseManager OR specific permission
            var isGlobalApprover = User.IsInRole("Admin") || User.IsInRole("PurchaseManager");
            var hasApprovePermission = User.Claims.Any(c => 
                c.Type == "permission" && 
                c.Value.Equals("requisition:approve", StringComparison.OrdinalIgnoreCase));

            if (!isGlobalApprover && !hasApprovePermission)
                return Forbid();

            // Check if user is from the same department (unless global approver)
            if (!isGlobalApprover)
            {
                var deptIdClaim = User.FindFirstValue("DepartmentId");
                if (string.IsNullOrWhiteSpace(deptIdClaim) || !int.TryParse(deptIdClaim, out var userDeptId))
                    return BadRequest(new { message = "User not assigned to any department" });

                if (requisition.DepartmentId != userDeptId)
                    return Forbid();
            }

            requisition.Status = "Rejected";
            requisition.ApprovedById = userId;
            requisition.ApprovedAt = DateTime.Now;
            
            var reason = dto?.Reason?.Trim();
            if (!string.IsNullOrWhiteSpace(reason))
                requisition.Notes = (requisition.Notes + " | Rejection reason: " + reason).Trim();
            
            requisition.UpdatedBy = User.Identity?.Name;
            requisition.UpdatedDate = DateTime.Now;

            _requisitionRepo.Update(requisition);
            await _requisitionRepo.SaveChangesAsync();

            return Ok(new { message = "Requisition rejected", requisitionNumber = requisition.RequisitionNumber });
        }

        // DELETE: api/PurchaseRequisition/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var requisition = await _requisitionRepo.GetByIdAsync(id);
            if (requisition == null || !requisition.IsActive)
                return NotFound(new { message = "Requisition not found" });

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var currentUserId))
                return Unauthorized(new { message = "User not authenticated" });

            var isAdmin = User.IsInRole("Admin");
            var hasCancelPermission = User.Claims.Any(c =>
                string.Equals(c.Type, PermissionClaimType, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(c.Value, CancelRequisitionPermission, StringComparison.OrdinalIgnoreCase));

            // Admin can cancel any active requisition.
            // Non-admin must have explicit permission and can cancel only own pending requisition.
            if (!isAdmin)
            {
                if (!hasCancelPermission)
                    return Forbid();

                if (requisition.RequestedById != currentUserId)
                    return Forbid();

                if (!string.Equals(requisition.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "Only pending requisitions can be cancelled" });
            }

            _requisitionRepo.SoftDelete(requisition);
            await _requisitionRepo.SaveChangesAsync();

            return Ok(new { message = "Requisition deleted successfully" });
        }

        private async Task<RequisitionResponseDto> MapToResponseAsync(PurchaseRequisition r)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserDeptId = User.FindFirstValue("DepartmentId");
            var hasApprovePermission = User.Claims.Any(c =>
                c.Type == "permission" &&
                c.Value.Equals("requisition:approve", StringComparison.OrdinalIgnoreCase));
            var hasCancelPermission = User.Claims.Any(c =>
                c.Type == "permission" &&
                c.Value.Equals("requisition:cancel", StringComparison.OrdinalIgnoreCase));
            var isGlobalApprover = User.IsInRole("Admin") || User.IsInRole("PurchaseManager");

            // Determine if current user can approve/reject this requisition
            bool canApprove = false;
            bool canReject = false;
            if (r.Status == "Pending" && (hasApprovePermission || isGlobalApprover))
            {
                if (isGlobalApprover)
                {
                    canApprove = true;
                    canReject = true;
                }
                else if (!string.IsNullOrWhiteSpace(currentUserDeptId) &&
                         int.TryParse(currentUserDeptId, out var deptId) &&
                         r.DepartmentId == deptId)
                {
                    canApprove = true;
                    canReject = true;
                }
            }

            // Determine if current user can cancel this requisition
            bool canCancel = false;
            if (User.IsInRole("Admin"))
            {
                canCancel = r.IsActive;
            }
            else if (hasCancelPermission &&
                     !string.IsNullOrWhiteSpace(currentUserId) &&
                     int.TryParse(currentUserId, out var userId) &&
                     r.RequestedById == userId &&
                     r.Status == "Pending")
            {
                canCancel = true;
            }

            // Get ApprovedBy user information
            string? approvedByEmail = null;
            string? approvedByName = null;
            if (r.ApprovedById.HasValue && r.ApprovedBy != null)
            {
                approvedByEmail = r.ApprovedBy.Email;
                approvedByName = r.ApprovedBy.UserName ?? r.ApprovedBy.Email;
            }

            return new RequisitionResponseDto
            {
                Id = r.Id,
                RequisitionNumber = r.RequisitionNumber,
                RequisitionDate = r.RequisitionDate,
                RequiredByDate = r.RequiredByDate,
                Notes = r.Notes,
                Status = r.Status,
                ApprovedAt = r.ApprovedAt,
                DepartmentId = r.DepartmentId,
                DepartmentName = r.Department?.DepartmentName,
                RequestedById = r.RequestedById,
                RequestedByEmail = r.RequestedBy?.Email,
                RequestedByName = r.RequestedBy?.UserName ?? r.RequestedBy?.Email,
                ApprovedById = r.ApprovedById,
                ApprovedByEmail = approvedByEmail,
                ApprovedByName = approvedByName,
                Items = r.RequisitionItems.Where(i => i.IsActive).Select(i => new RequisitionItemResponseDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name,
                    RequiredQuantity = i.RequiredQuantity,
                    Remarks = i.Remarks
                }).ToList(),
                CanApprove = canApprove,
                CanReject = canReject,
                CanCancel = canCancel
            };
        }

        private async Task<List<RequisitionResponseDto>> MapToResponseListAsync(IEnumerable<PurchaseRequisition> requisitions)
        {
            var result = new List<RequisitionResponseDto>();
            foreach (var r in requisitions)
            {
                result.Add(await MapToResponseAsync(r));
            }
            return result;
        }

        private RequisitionResponseDto MapToResponse(PurchaseRequisition r)
        {
            // Legacy method - kept for backward compatibility
            // Use MapToResponseAsync for new code
            return new RequisitionResponseDto
            {
                Id = r.Id,
                RequisitionNumber = r.RequisitionNumber,
                RequisitionDate = r.RequisitionDate,
                RequiredByDate = r.RequiredByDate,
                Notes = r.Notes,
                Status = r.Status,
                ApprovedAt = r.ApprovedAt,
                DepartmentId = r.DepartmentId,
                DepartmentName = r.Department?.DepartmentName,
                RequestedById = r.RequestedById,
                RequestedByEmail = r.RequestedBy?.Email,
                RequestedByName = r.RequestedBy?.UserName ?? r.RequestedBy?.Email,
                ApprovedById = r.ApprovedById,
                ApprovedByEmail = r.ApprovedBy?.Email,
                ApprovedByName = r.ApprovedBy?.UserName ?? r.ApprovedBy?.Email,
                Items = r.RequisitionItems.Where(i => i.IsActive).Select(i => new RequisitionItemResponseDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name,
                    RequiredQuantity = i.RequiredQuantity,
                    Remarks = i.Remarks
                }).ToList()
            };
        }

        private Task<string> GenerateRequisitionNumber()
        {
            var year = DateTime.Now.Year;
            var last = _requisitionRepo.FindAsync(r => r.RequisitionNumber.StartsWith($"PR-{year}-")).Result;
            int next = 1;
            if (last.Any())
            {
                var maxNumber = last.Max(r =>
                {
                    var parts = r.RequisitionNumber.Split('-');
                    return parts.Length == 3 && int.TryParse(parts[2], out int num) ? num : 0;
                });
                next = maxNumber + 1;
            }
            return Task.FromResult($"PR-{year}-{next:D3}");
        }
    }
}