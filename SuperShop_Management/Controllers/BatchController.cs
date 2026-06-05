using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.DTOs.Batch;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BatchController : ControllerBase
    {
        private readonly IBatchRepository _batchRepo;
        private readonly IProductRepository _productRepo;
        private readonly ISupplierRepository _supplierRepo;

        public BatchController(IBatchRepository batchRepo, IProductRepository productRepo, ISupplierRepository supplierRepo)
        {
            _batchRepo = batchRepo;
            _productRepo = productRepo;
            _supplierRepo = supplierRepo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var batches = await _batchRepo.GetActiveBatchesAsync();
            var response = batches.Select(b => new BatchResponseDto
            {
                Id = b.Id,
                BatchNumber = b.BatchNumber,
                ManufacturingDate = b.ManufacturingDate,
                ExpiryDate = b.ExpiryDate,
                ReceivedQuantity = b.ReceivedQuantity,
                RemainingQuantity = b.RemainingQuantity,
                Status = b.Status,
                ProductId = b.ProductId,
                ProductName = b.Product?.Name,
                SupplierId = b.SupplierId,
                SupplierName = b.Supplier?.Name,
                GRNId = b.GRNId,
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
            var batch = await _batchRepo.GetByIdAsync(id);
            if (batch == null || !batch.IsActive)
                return NotFound(new { message = "Batch not found" });

            var response = new BatchResponseDto
            {
                Id = batch.Id,
                BatchNumber = batch.BatchNumber,
                ManufacturingDate = batch.ManufacturingDate,
                ExpiryDate = batch.ExpiryDate,
                ReceivedQuantity = batch.ReceivedQuantity,
                RemainingQuantity = batch.RemainingQuantity,
                Status = batch.Status,
                ProductId = batch.ProductId,
                ProductName = batch.Product?.Name,
                SupplierId = batch.SupplierId,
                SupplierName = batch.Supplier?.Name,
                GRNId = batch.GRNId,
                IsActive = batch.IsActive,
                CreatedDate = batch.CreatedDate,
                CreatedBy = batch.CreatedBy
            };
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> Create([FromBody] BatchCreateDto dto)
        {
            var product = await _productRepo.GetByIdAsync(dto.ProductId);
            if (product == null || !product.IsActive)
                return BadRequest(new { message = "Invalid Product" });

            var supplier = await _supplierRepo.GetByIdAsync(dto.SupplierId);
            if (supplier == null || !supplier.IsActive)
                return BadRequest(new { message = "Invalid Supplier" });

            var batch = new Batch
            {
                BatchNumber = dto.BatchNumber,
                ManufacturingDate = dto.ManufacturingDate,
                ExpiryDate = dto.ExpiryDate,
                ReceivedQuantity = dto.ReceivedQuantity,
                RemainingQuantity = dto.ReceivedQuantity,
                Status = "Active",
                ProductId = dto.ProductId,
                SupplierId = dto.SupplierId,
                GRNId = dto.GRNId,
                IsActive = true,
                CreatedBy = User.Identity?.Name ?? "System",
                CreatedDate = DateTime.Now
            };

            await _batchRepo.AddAsync(batch);
            await _batchRepo.SaveChangesAsync();

            return Ok(new { message = "Batch created successfully", id = batch.Id });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> Update(int id, [FromBody] BatchUpdateDto dto)
        {
            var batch = await _batchRepo.GetByIdAsync(id);
            if (batch == null || !batch.IsActive)
                return NotFound(new { message = "Batch not found" });

            batch.BatchNumber = dto.BatchNumber;
            batch.ManufacturingDate = dto.ManufacturingDate;
            batch.ExpiryDate = dto.ExpiryDate;
            batch.Status = dto.Status;
            batch.UpdatedBy = User.Identity?.Name ?? "System";
            batch.UpdatedDate = DateTime.Now;

            _batchRepo.Update(batch);
            await _batchRepo.SaveChangesAsync();

            return Ok(new { message = "Batch updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var batch = await _batchRepo.GetByIdAsync(id);
            if (batch == null || !batch.IsActive)
                return NotFound(new { message = "Batch not found" });

            _batchRepo.SoftDelete(batch);
            await _batchRepo.SaveChangesAsync();

            return Ok(new { message = "Batch deleted successfully" });
        }
    }
}