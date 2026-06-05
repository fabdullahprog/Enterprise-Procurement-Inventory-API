using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.DTOs.Brand;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly IBrandRepository _repo;
        private readonly ISubCategoryRepository _subCategoryRepo;

        public BrandController(IBrandRepository repo, ISubCategoryRepository subCategoryRepo)
        {
            _repo = repo;
            _subCategoryRepo = subCategoryRepo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var brands = await _repo.GetActiveBrandsAsync();
            var response = brands.Select(b => new BrandResponseDto
            {
                BrandId = b.BrandId,
                BrandName = b.BrandName,
                Description = b.Description,
                Country = b.Country,
                Website = b.Website,
                SubCategoryId = b.SubCategoryId,
                SubCategoryName = b.SubCategory?.SubCategoryName,
                IsActive = b.IsActive,
                CreatedDate = b.CreatedDate,
                CreatedBy = b.CreatedBy
            });
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var brand = await _repo.GetByIdAsync(id);
            if (brand == null || !brand.IsActive)
                return NotFound(new { message = "Brand not found" });

            var response = new BrandResponseDto
            {
                BrandId = brand.BrandId,
                BrandName = brand.BrandName,
                Description = brand.Description,
                Country = brand.Country,
                Website = brand.Website,
                SubCategoryId = brand.SubCategoryId,
                IsActive = brand.IsActive,
                CreatedDate = brand.CreatedDate,
                CreatedBy = brand.CreatedBy
            };
            return Ok(response);
        }

        [HttpGet("bysubcategory/{subCategoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBySubCategoryId(int subCategoryId)
        {
            var brands = await _repo.GetBySubCategoryIdAsync(subCategoryId);
            var response = brands.Select(b => new BrandResponseDto
            {
                BrandId = b.BrandId,
                BrandName = b.BrandName,
                Description = b.Description,
                Country = b.Country,
                Website = b.Website,
                SubCategoryId = b.SubCategoryId,
                IsActive = b.IsActive
            });
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> Create([FromBody] BrandRequestDto dto)
        {
            var isDuplicate = await _repo.IsDuplicateAsync(dto.BrandName);
            if (isDuplicate)
                return BadRequest(new { message = "Brand name already exists" });

            var subCategory = await _subCategoryRepo.GetByIdAsync(dto.SubCategoryId);
            if (subCategory == null || !subCategory.IsActive)
                return BadRequest(new { message = "Invalid SubCategory" });

            var brand = new Brand
            {
                BrandName = dto.BrandName,
                Description = dto.Description,
                Country = dto.Country,
                Website = dto.Website,
                SubCategoryId = dto.SubCategoryId,
                IsActive = true,
                CreatedBy = User.Identity?.Name ?? "System",
                CreatedDate = DateTime.Now
            };

            await _repo.AddAsync(brand);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "Brand created successfully", id = brand.BrandId });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> Update(int id, [FromBody] BrandRequestDto dto)
        {
            var brand = await _repo.GetByIdAsync(id);
            if (brand == null || !brand.IsActive)
                return NotFound(new { message = "Brand not found" });

            var isDuplicate = await _repo.IsDuplicateAsync(dto.BrandName, id);
            if (isDuplicate)
                return BadRequest(new { message = "Brand name already exists" });

            var subCategory = await _subCategoryRepo.GetByIdAsync(dto.SubCategoryId);
            if (subCategory == null || !subCategory.IsActive)
                return BadRequest(new { message = "Invalid SubCategory" });

            brand.BrandName = dto.BrandName;
            brand.Description = dto.Description;
            brand.Country = dto.Country;
            brand.Website = dto.Website;
            brand.SubCategoryId = dto.SubCategoryId;
            brand.UpdatedBy = User.Identity?.Name ?? "System";
            brand.UpdatedDate = DateTime.Now;

            _repo.Update(brand);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "Brand updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _repo.GetByIdAsync(id);
            if (brand == null || !brand.IsActive)
                return NotFound(new { message = "Brand not found" });

            _repo.SoftDelete(brand);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "Brand deleted successfully" });
        }
    }
}