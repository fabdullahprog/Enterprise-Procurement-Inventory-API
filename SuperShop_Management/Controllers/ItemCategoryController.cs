using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SuperShop_Management.DTOs.ItemCategory;
using SuperShop_Management.Entities;

using SuperShop_Management.Repositories.Interfaces;
using SuperShop_Management.Attributes;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemCategoryController : ControllerBase
    {
        private readonly IItemCategoryRepository _repo;

        public ItemCategoryController(IItemCategoryRepository repo)
        {
            _repo = repo;
        }

        // GET: api/ItemCategory - সবাই পারে (Login লাগবে না)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _repo.GetActiveCategoriesAsync();
            return Ok(categories);
        }

        // GET: api/ItemCategory/5 - শুধু Login করা User পারে
        [HttpGet("{id}")]
        [AuthorizeRoles("Admin", "PurchaseOfficer")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _repo.GetByIdAsync(id);

            if (category == null || !category.IsActive)
                return NotFound(new { message = "Category not found" });

            return Ok(category);
        }

        // POST: api/ItemCategory - শুধু Admin এবং PurchaseOfficer পারে
        [HttpPost]
        [AuthorizeRoles("Admin", "PurchaseOfficer")]
        public async Task<IActionResult> Create([FromBody] ItemCategoryRequestDto dto)
        {
            // Duplicate check
            var isDuplicate = await _repo.IsDuplicateAsync(dto.CategoryName);
            if (isDuplicate)
                return BadRequest(new { message = "Category name already exists" });

            var category = new ItemCategory
            {
                CategoryName = dto.CategoryName,
                CategoryDescription = dto.CategoryDescription,
                IsActive = true,
                CreatedBy = User.Identity?.Name ?? "System",
                CreatedDate = DateTime.Now
            };

            await _repo.AddAsync(category);
            await _repo.SaveChangesAsync();

            return Ok(new
            {
                message = "Category created successfully",
                data = category
            });
        }

        // PUT: api/ItemCategory/5 - শুধু Admin পারে
        [HttpPut("{id}")]
        [AuthorizeRoles("Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] ItemCategoryRequestDto dto)
        {
            var category = await _repo.GetByIdAsync(id);

            if (category == null || !category.IsActive)
                return NotFound(new { message = "Category not found" });

            // Duplicate check (excluding current)
            var isDuplicate = await _repo.IsDuplicateAsync(dto.CategoryName, id);
            if (isDuplicate)
                return BadRequest(new { message = "Category name already exists" });

            category.CategoryName = dto.CategoryName;
            category.CategoryDescription = dto.CategoryDescription;
            category.UpdatedBy = User.Identity?.Name ?? "System";
            category.UpdatedDate = DateTime.Now;

            _repo.Update(category);
            await _repo.SaveChangesAsync();

            return Ok(new
            {
                message = "Category updated successfully",
                data = category
            });
        }

        // DELETE: api/ItemCategory/5 (Soft Delete) - শুধু Admin পারে
        //[HttpDelete("{id}")]
        //[AuthorizeRoles("Admin")]
        //public async Task<IActionResult> SoftDelete(int id)
        //{
        //    var category = await _repo.GetByIdAsync(id);

        //    if (category == null || !category.IsActive)
        //        return NotFound(new { message = "Category not found" });

        //    _repo.SoftDelete(category);
        //    await _repo.SaveChangesAsync();

        //    return Ok(new { message = "Category soft deleted successfully" });
        //}

        // DELETE: api/ItemCategory/5 (Hard Delete) - শুধু Admin পারে


        [HttpDelete("{id}")]
        [AuthorizeRoles("Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _repo.GetByIdAsync(id);

            if (category == null)
                return NotFound(new { message = "Category not found" });

            _repo.HardDelete(category);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully" });
        }
    }
}