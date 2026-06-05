using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.Attributes;
using SuperShop_Management.DTOs.Department;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentRepository _repo;

        public DepartmentController(IDepartmentRepository repo)
        {
            _repo = repo;
        }

        // GET: api/Department
        // Used by requisition creation UI.
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var departments = await _repo.GetActiveDepartmentsAsync();
            return Ok(departments);
        }

        // GET: api/Department/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var dept = await _repo.GetByIdAsync(id);
            if (dept == null || !dept.IsActive)
                return NotFound(new { message = "Department not found" });

            return Ok(dept);
        }

        // POST: api/Department
        [HttpPost]
        [AuthorizeRoles("Admin", "PurchaseOfficer", "PurchaseManager")]
        public async Task<IActionResult> Create([FromBody] DepartmentRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isDuplicate = await _repo.IsDuplicateAsync(dto.DepartmentCode);
            if (isDuplicate)
                return BadRequest(new { message = "Department code already exists" });

            var department = new Department
            {
                DepartmentCode = dto.DepartmentCode,
                DepartmentName = dto.DepartmentName,
                Description = dto.Description,
                DepartmentEmail = dto.DepartmentEmail,
                DepartmentPhone = dto.DepartmentPhone,
                Location = dto.Location,
                CanRequestItem = dto.CanRequestItem,
                CanIssueItem = dto.CanIssueItem,
                IsActive = true,
                CreatedBy = User.Identity?.Name ?? "System",
                CreatedDate = DateTime.Now
            };

            await _repo.AddAsync(department);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "Department created successfully", id = department.DepartmentId });
        }

        // PUT: api/Department/5
        [HttpPut("{id}")]
        [AuthorizeRoles("Admin", "PurchaseOfficer", "PurchaseManager")]
        public async Task<IActionResult> Update(int id, [FromBody] DepartmentRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dept = await _repo.GetByIdAsync(id);
            if (dept == null || !dept.IsActive)
                return NotFound(new { message = "Department not found" });

            var isDuplicate = await _repo.IsDuplicateAsync(dto.DepartmentCode, id);
            if (isDuplicate)
                return BadRequest(new { message = "Department code already exists" });

            dept.DepartmentCode = dto.DepartmentCode;
            dept.DepartmentName = dto.DepartmentName;
            dept.Description = dto.Description;
            dept.DepartmentEmail = dto.DepartmentEmail;
            dept.DepartmentPhone = dto.DepartmentPhone;
            dept.Location = dto.Location;
            dept.CanRequestItem = dto.CanRequestItem;
            dept.CanIssueItem = dto.CanIssueItem;
            dept.UpdatedBy = User.Identity?.Name ?? "System";
            dept.UpdatedDate = DateTime.Now;

            _repo.Update(dept);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "Department updated successfully" });
        }

        // DELETE: api/Department/5
        [HttpDelete("{id}")]
        [AuthorizeRoles("Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var dept = await _repo.GetByIdAsync(id);
            if (dept == null || !dept.IsActive)
                return NotFound(new { message = "Department not found" });

            _repo.SoftDelete(dept);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "Department deleted successfully" });
        }
    }
}

