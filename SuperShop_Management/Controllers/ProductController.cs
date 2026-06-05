using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.DTOs.Product;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepo;
        private readonly IItemCategoryRepository _categoryRepo;
        private readonly ISubCategoryRepository _subCategoryRepo;
        private readonly IBrandRepository _brandRepo;
        private readonly IUnitRepository _unitRepo;

        public ProductController(
            IProductRepository productRepo,
            IItemCategoryRepository categoryRepo,
            ISubCategoryRepository subCategoryRepo,
            IBrandRepository brandRepo,
            IUnitRepository unitRepo)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
            _subCategoryRepo = subCategoryRepo;
            _brandRepo = brandRepo;
            _unitRepo = unitRepo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productRepo.GetActiveProductsAsync();
            var response = products.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Barcode = p.Barcode,
                Price = p.Price,
                CurrentStock = p.CurrentStock,
                IsPerishable = p.IsPerishable,
                Description = p.Description,
                ItemCategoryId = p.ItemCategoryId,
                ItemCategoryName = p.ItemCategory?.CategoryName,
                SubCategoryId = p.SubCategoryId,
                SubCategoryName = p.SubCategory?.SubCategoryName,
                BrandId = p.BrandId,
                BrandName = p.Brand?.BrandName,
                UnitId = p.UnitId,
                UnitName = p.Unit?.NameOfUnit,
                IsActive = p.IsActive,
                CreatedDate = p.CreatedDate,
                CreatedBy = p.CreatedBy
            });
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null || !product.IsActive)
                return NotFound(new { message = "Product not found" });

            var response = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Barcode = product.Barcode,
                Price = product.Price,
                CurrentStock = product.CurrentStock,
                IsPerishable = product.IsPerishable,
                Description = product.Description,
                ItemCategoryId = product.ItemCategoryId,
                SubCategoryId = product.SubCategoryId,
                BrandId = product.BrandId,
                UnitId = product.UnitId,
                IsActive = product.IsActive,
                CreatedDate = product.CreatedDate,
                CreatedBy = product.CreatedBy
            };
            return Ok(response);
        }

        [HttpGet("barcode/{barcode}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByBarcode(string barcode)
        {
            var product = await _productRepo.GetByBarcodeAsync(barcode);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            var response = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Barcode = product.Barcode,
                Price = product.Price,
                CurrentStock = product.CurrentStock,
                IsPerishable = product.IsPerishable,
                Description = product.Description,
                ItemCategoryId = product.ItemCategoryId,
                SubCategoryId = product.SubCategoryId,
                BrandId = product.BrandId,
                UnitId = product.UnitId,
                IsActive = product.IsActive
            };
            return Ok(response);
        }

        [HttpGet("bycategory/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategoryId(int categoryId)
        {
            var products = await _productRepo.GetByCategoryIdAsync(categoryId);
            var response = products.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Barcode = p.Barcode,
                Price = p.Price,
                CurrentStock = p.CurrentStock,
                IsPerishable = p.IsPerishable,
                Description = p.Description,
                ItemCategoryId = p.ItemCategoryId,
                SubCategoryId = p.SubCategoryId,
                BrandId = p.BrandId,
                UnitId = p.UnitId,
                IsActive = p.IsActive
            });
            return Ok(response);
        }

        [HttpGet("bybrand/{brandId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByBrandId(int brandId)
        {
            var products = await _productRepo.GetByBrandIdAsync(brandId);
            var response = products.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Barcode = p.Barcode,
                Price = p.Price,
                CurrentStock = p.CurrentStock,
                IsPerishable = p.IsPerishable,
                Description = p.Description,
                ItemCategoryId = p.ItemCategoryId,
                SubCategoryId = p.SubCategoryId,
                BrandId = p.BrandId,
                UnitId = p.UnitId,
                IsActive = p.IsActive
            });
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> Create([FromBody] ProductRequestDto dto)
        {
            // Validate foreign keys
            var category = await _categoryRepo.GetByIdAsync(dto.ItemCategoryId);
            if (category == null || !category.IsActive)
                return BadRequest(new { message = "Invalid ItemCategory" });

            var subCategory = await _subCategoryRepo.GetByIdAsync(dto.SubCategoryId);
            if (subCategory == null || !subCategory.IsActive)
                return BadRequest(new { message = "Invalid SubCategory" });

            var brand = await _brandRepo.GetByIdAsync(dto.BrandId);
            if (brand == null || !brand.IsActive)
                return BadRequest(new { message = "Invalid Brand" });

            var unit = await _unitRepo.GetByIdAsync(dto.UnitId);
            if (unit == null || !unit.IsActive)
                return BadRequest(new { message = "Invalid Unit" });

            // Duplicate barcode
            var isDuplicate = await _productRepo.IsDuplicateBarcodeAsync(dto.Barcode);
            if (isDuplicate)
                return BadRequest(new { message = "Product with this barcode already exists" });

            var product = new Product
            {
                Name = dto.Name,
                Barcode = dto.Barcode,
                Price = dto.Price,
                CurrentStock = dto.CurrentStock,
                IsPerishable = dto.IsPerishable,
                Description = dto.Description,
                ItemCategoryId = dto.ItemCategoryId,
                SubCategoryId = dto.SubCategoryId,
                BrandId = dto.BrandId,
                UnitId = dto.UnitId,
                IsActive = true,
                CreatedBy = User.Identity?.Name ?? "System",
                CreatedDate = DateTime.Now
            };

            await _productRepo.AddAsync(product);
            await _productRepo.SaveChangesAsync();

            return Ok(new { message = "Product created successfully", id = product.Id });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductRequestDto dto)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null || !product.IsActive)
                return NotFound(new { message = "Product not found" });

            // Validate foreign keys
            var category = await _categoryRepo.GetByIdAsync(dto.ItemCategoryId);
            if (category == null || !category.IsActive)
                return BadRequest(new { message = "Invalid ItemCategory" });

            var subCategory = await _subCategoryRepo.GetByIdAsync(dto.SubCategoryId);
            if (subCategory == null || !subCategory.IsActive)
                return BadRequest(new { message = "Invalid SubCategory" });

            var brand = await _brandRepo.GetByIdAsync(dto.BrandId);
            if (brand == null || !brand.IsActive)
                return BadRequest(new { message = "Invalid Brand" });

            var unit = await _unitRepo.GetByIdAsync(dto.UnitId);
            if (unit == null || !unit.IsActive)
                return BadRequest(new { message = "Invalid Unit" });

            // Duplicate barcode (exclude self)
            var isDuplicate = await _productRepo.IsDuplicateBarcodeAsync(dto.Barcode, id);
            if (isDuplicate)
                return BadRequest(new { message = "Product with this barcode already exists" });

            product.Name = dto.Name;
            product.Barcode = dto.Barcode;
            product.Price = dto.Price;
            product.CurrentStock = dto.CurrentStock;
            product.IsPerishable = dto.IsPerishable;
            product.Description = dto.Description;
            product.ItemCategoryId = dto.ItemCategoryId;
            product.SubCategoryId = dto.SubCategoryId;
            product.BrandId = dto.BrandId;
            product.UnitId = dto.UnitId;
            product.UpdatedBy = User.Identity?.Name ?? "System";
            product.UpdatedDate = DateTime.Now;

            _productRepo.Update(product);
            await _productRepo.SaveChangesAsync();

            return Ok(new { message = "Product updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null || !product.IsActive)
                return NotFound(new { message = "Product not found" });

            _productRepo.SoftDelete(product);
            await _productRepo.SaveChangesAsync();

            return Ok(new { message = "Product deleted successfully" });
        }
    }
}