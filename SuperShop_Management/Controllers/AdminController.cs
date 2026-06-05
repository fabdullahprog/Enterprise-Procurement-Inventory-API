using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.DTOs.Admin;
using SuperShop_Management.Repositories.Interfaces;
using System.Security.Claims;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class AdminController : ControllerBase
    {
        private const string DepartmentIdClaim = "DepartmentId";
        private const string PermissionClaimType = "permission";

        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly UserManager<IdentityUser<int>> _userManager;
        private readonly IDepartmentRepository _departmentRepo;

        public AdminController(
            RoleManager<IdentityRole<int>> roleManager,
            UserManager<IdentityUser<int>> userManager,
            IDepartmentRepository departmentRepo)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _departmentRepo = departmentRepo;
        }

        // ---------------- Roles ----------------
        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles
                .Select(r => new RoleDto { Name = r.Name ?? string.Empty })
                .OrderBy(r => r.Name)
                .ToList();
            return Ok(roles);
        }

        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            var name = (dto?.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { message = "Role name is required" });

            if (await _roleManager.RoleExistsAsync(name))
                return BadRequest(new { message = "Role already exists" });

            var result = await _roleManager.CreateAsync(new IdentityRole<int>(name));
            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to create role", errors = result.Errors.Select(e => e.Description) });

            return Ok(new { message = "Role created", name });
        }

        [HttpPut("roles/{roleName}")]
        public async Task<IActionResult> RenameRole(string roleName, [FromBody] UpdateRoleDto dto)
        {
            var existing = await _roleManager.FindByNameAsync(roleName);
            if (existing == null)
                return NotFound(new { message = "Role not found" });

            var newName = (dto?.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(newName))
                return BadRequest(new { message = "New role name is required" });

            if (!string.Equals(roleName, newName, StringComparison.OrdinalIgnoreCase) && await _roleManager.RoleExistsAsync(newName))
                return BadRequest(new { message = "Role name already exists" });

            existing.Name = newName;
            var result = await _roleManager.UpdateAsync(existing);
            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to update role", errors = result.Errors.Select(e => e.Description) });

            return Ok(new { message = "Role updated", name = newName });
        }

        [HttpDelete("roles/{roleName}")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return NotFound(new { message = "Role not found" });

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to delete role", errors = result.Errors.Select(e => e.Description) });

            return Ok(new { message = "Role deleted" });
        }

        // ---------------- Permissions (Role claims) ----------------
        [HttpGet("roles/{roleName}/permissions")]
        public async Task<IActionResult> GetRolePermissions(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return NotFound(new { message = "Role not found" });

            var claims = await _roleManager.GetClaimsAsync(role);
            var current = claims.Where(c => c.Type == PermissionClaimType).Select(c => c.Value).Distinct().OrderBy(x => x).ToList();

            // Central registry of permissions for UI.
            var all = new List<string>
            {
                "requisition:create",
                "requisition:view",
                "requisition:approve",
                "requisition:cancel",
                "requisition:manage",
                "role:manage",
                "user:manage",
                "department:manage"
            };

            return Ok(new RolePermissionsDto
            {
                Role = roleName,
                Permissions = current,
                AllPermissions = all
            });
        }

        [HttpPut("roles/{roleName}/permissions")]
        public async Task<IActionResult> SetRolePermissions(string roleName, [FromBody] SetRolePermissionsDto dto)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return NotFound(new { message = "Role not found" });

            var desired = (dto?.Permissions ?? new List<string>())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var claims = await _roleManager.GetClaimsAsync(role);
            var existing = claims.Where(c => c.Type == PermissionClaimType).ToList();

            foreach (var c in existing)
                await _roleManager.RemoveClaimAsync(role, c);

            foreach (var p in desired)
                await _roleManager.AddClaimAsync(role, new Claim(PermissionClaimType, p));

            return Ok(new { message = "Permissions updated", role = roleName, count = desired.Count });
        }

        // ---------------- Users ----------------
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            // Simple list for UI; paging can be added later.
            var users = _userManager.Users.OrderBy(u => u.Email).ToList();
            var result = new List<AdminUserDto>();

            foreach (var u in users)
            {
                var roles = (await _userManager.GetRolesAsync(u)).ToList();
                var claims = await _userManager.GetClaimsAsync(u);
                var deptIdClaim = claims.FirstOrDefault(c => c.Type == DepartmentIdClaim)?.Value;
                int? deptId = null;
                if (int.TryParse(deptIdClaim, out var parsed)) deptId = parsed;

                string? deptName = null;
                if (deptId.HasValue)
                {
                    var dept = await _departmentRepo.GetByIdAsync(deptId.Value);
                    deptName = dept?.DepartmentName;
                }

                result.Add(new AdminUserDto
                {
                    Id = u.Id,
                    Email = u.Email ?? u.UserName ?? string.Empty,
                    Roles = roles,
                    DepartmentId = deptId,
                    DepartmentName = deptName
                });
            }

            return Ok(result);
        }

        [HttpPut("users/{userId}/roles")]
        public async Task<IActionResult> SetUserRoles(int userId, [FromBody] SetUserRolesDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound(new { message = "User not found" });

            var desired = (dto?.Roles ?? new List<string>())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Ensure roles exist
            foreach (var r in desired)
            {
                if (!await _roleManager.RoleExistsAsync(r))
                    return BadRequest(new { message = $"Role does not exist: {r}" });
            }

            var current = (await _userManager.GetRolesAsync(user)).ToList();
            var toRemove = current.Except(desired, StringComparer.OrdinalIgnoreCase).ToList();
            var toAdd = desired.Except(current, StringComparer.OrdinalIgnoreCase).ToList();

            if (toRemove.Any())
                await _userManager.RemoveFromRolesAsync(user, toRemove);
            if (toAdd.Any())
                await _userManager.AddToRolesAsync(user, toAdd);

            return Ok(new { message = "User roles updated" });
        }

        [HttpPut("users/{userId}/department")]
        public async Task<IActionResult> SetUserDepartment(int userId, [FromBody] SetUserDepartmentDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound(new { message = "User not found" });

            int? deptId = dto?.DepartmentId;
            if (deptId.HasValue)
            {
                var dept = await _departmentRepo.GetByIdAsync(deptId.Value);
                if (dept == null || !dept.IsActive)
                    return BadRequest(new { message = "Invalid Department" });
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var existing = claims.Where(c => c.Type == DepartmentIdClaim).ToList();
            foreach (var c in existing)
                await _userManager.RemoveClaimAsync(user, c);

            if (deptId.HasValue)
                await _userManager.AddClaimAsync(user, new Claim(DepartmentIdClaim, deptId.Value.ToString()));

            return Ok(new { message = "User department updated" });
        }
    }
}

