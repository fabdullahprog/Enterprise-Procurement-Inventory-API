using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.DTOs.SubCategory;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubCategoryController : ControllerBase
    {
        private readonly ISubCategoryRepository _repo;
        private readonly IItemCategoryRepository _categoryRepo;

        public SubCategoryController(ISubCategoryRepository repo, IItemCategoryRepository categoryRepo)
        {
            _repo = repo;
            _categoryRepo = categoryRepo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var subCategories = await _repo.GetActiveSubCategoriesAsync();
            var response = subCategories.Select(s => new SubCategoryResponseDto
            {
                SubCategoryId = s.SubCategoryId,
                SubCategoryName = s.SubCategoryName,
                Description = s.Description,
                ItemCategoryId = s.ItemCategoryId,
                ItemCategoryName = s.ItemCategory?.CategoryName,
                IsActive = s.IsActive,
                CreatedDate = s.CreatedDate,
                CreatedBy = s.CreatedBy
            });
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var subCategory = await _repo.GetByIdAsync(id);
            if (subCategory == null || !subCategory.IsActive)
                return NotFound(new { message = "SubCategory not found" });

            var response = new SubCategoryResponseDto
            {
                SubCategoryId = subCategory.SubCategoryId,
                SubCategoryName = subCategory.SubCategoryName,
                Description = subCategory.Description,
                ItemCategoryId = subCategory.ItemCategoryId,
                IsActive = subCategory.IsActive,
                CreatedDate = subCategory.CreatedDate,
                CreatedBy = subCategory.CreatedBy
            };
            return Ok(response);
        }

        [HttpGet("bycategory/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategoryId(int categoryId)
        {
            var subCategories = await _repo.GetByCategoryIdAsync(categoryId);
            var response = subCategories.Select(s => new SubCategoryResponseDto
            {
                SubCategoryId = s.SubCategoryId,
                SubCategoryName = s.SubCategoryName,
                Description = s.Description,
                ItemCategoryId = s.ItemCategoryId,
                IsActive = s.IsActive
            });
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, PurchaseOfficer")]
        public async Task<IActionResult> Create([FromBody] SubCategoryRequestDto dto)
        {
            var isDuplicate = await _repo.IsDuplicateAsync(dto.SubCategoryName);
            if (isDuplicate)
                return BadRequest(new { message = "SubCategory name already exists" });

            var category = await _categoryRepo.GetByIdAsync(dto.ItemCategoryId);
            if (category == null || !category.IsActive)
                return BadRequest(new { message = "Invalid ItemCategory" });

            var subCategory = new SubCategory
            {
                SubCategoryName = dto.SubCategoryName,
                Description = dto.Description,
                ItemCategoryId = dto.ItemCategoryId,
                IsActive = true,
                CreatedBy = User.Identity?.Name ?? "System",
                CreatedDate = DateTime.Now
            };

            await _repo.AddAsync(subCategory);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "SubCategory created successfully", id = subCategory.SubCategoryId });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, PurchaseOfficer")]
        public async Task<IActionResult> Update(int id, [FromBody] SubCategoryRequestDto dto)
        {
            var subCategory = await _repo.GetByIdAsync(id);
            if (subCategory == null || !subCategory.IsActive)
                return NotFound(new { message = "SubCategory not found" });

            var isDuplicate = await _repo.IsDuplicateAsync(dto.SubCategoryName, id);
            if (isDuplicate)
                return BadRequest(new { message = "SubCategory name already exists" });

            var category = await _categoryRepo.GetByIdAsync(dto.ItemCategoryId);
            if (category == null || !category.IsActive)
                return BadRequest(new { message = "Invalid ItemCategory" });

            subCategory.SubCategoryName = dto.SubCategoryName;
            subCategory.Description = dto.Description;
            subCategory.ItemCategoryId = dto.ItemCategoryId;
            subCategory.UpdatedBy = User.Identity?.Name ?? "System";
            subCategory.UpdatedDate = DateTime.Now;

            _repo.Update(subCategory);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "SubCategory updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var subCategory = await _repo.GetByIdAsync(id);
            if (subCategory == null || !subCategory.IsActive)
                return NotFound(new { message = "SubCategory not found" });

            _repo.SoftDelete(subCategory);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "SubCategory deleted successfully" });
        }
    }
}