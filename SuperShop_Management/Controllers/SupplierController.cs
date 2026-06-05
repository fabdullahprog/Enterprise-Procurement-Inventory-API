using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.DTOs.Supplier;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierRepository _repo;
        private readonly ICurrencyRepository _currencyRepo;

        public SupplierController(ISupplierRepository repo, ICurrencyRepository currencyRepo)
        {
            _repo = repo;
            _currencyRepo = currencyRepo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var suppliers = await _repo.GetActiveSuppliersAsync();
            var response = suppliers.Select(s => new SupplierResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                ContactPerson = s.ContactPerson,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                TradeLicenseNo = s.TradeLicenseNo,
                TINNo = s.TINNo,
                BINNo = s.BINNo,
                BankName = s.BankName,
                BankAccountNo = s.BankAccountNo,
                CurrencyId = s.CurrencyId,
                CurrencyCode = s.Currency?.Code,
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
            var supplier = await _repo.GetByIdAsync(id);
            if (supplier == null || !supplier.IsActive)
                return NotFound(new { message = "Supplier not found" });

            var response = new SupplierResponseDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Phone = supplier.Phone,
                Email = supplier.Email,
                Address = supplier.Address,
                TradeLicenseNo = supplier.TradeLicenseNo,
                TINNo = supplier.TINNo,
                BINNo = supplier.BINNo,
                BankName = supplier.BankName,
                BankAccountNo = supplier.BankAccountNo,
                CurrencyId = supplier.CurrencyId,
                IsActive = supplier.IsActive,
                CreatedDate = supplier.CreatedDate,
                CreatedBy = supplier.CreatedBy
            };
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> Create([FromBody] SupplierRequestDto dto)
        {
            var isDuplicate = await _repo.IsDuplicateAsync(dto.Name);
            if (isDuplicate)
                return BadRequest(new { message = "Supplier name already exists" });

            if (dto.CurrencyId.HasValue)
            {
                var currency = await _currencyRepo.GetByIdAsync(dto.CurrencyId.Value);
                if (currency == null || !currency.IsActive)
                    return BadRequest(new { message = "Invalid Currency" });
            }

            var supplier = new Supplier
            {
                Name = dto.Name,
                ContactPerson = dto.ContactPerson,
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address,
                TradeLicenseNo = dto.TradeLicenseNo,
                TINNo = dto.TINNo,
                BINNo = dto.BINNo,
                BankName = dto.BankName,
                BankAccountNo = dto.BankAccountNo,
                CurrencyId = dto.CurrencyId,
                IsActive = true,
                CreatedBy = User.Identity?.Name ?? "System",
                CreatedDate = DateTime.Now
            };

            await _repo.AddAsync(supplier);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "Supplier created successfully", id = supplier.Id });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, PurchaseOfficer, PurchaseManager")]
        public async Task<IActionResult> Update(int id, [FromBody] SupplierRequestDto dto)
        {
            var supplier = await _repo.GetByIdAsync(id);
            if (supplier == null || !supplier.IsActive)
                return NotFound(new { message = "Supplier not found" });

            var isDuplicate = await _repo.IsDuplicateAsync(dto.Name, id);
            if (isDuplicate)
                return BadRequest(new { message = "Supplier name already exists" });

            if (dto.CurrencyId.HasValue)
            {
                var currency = await _currencyRepo.GetByIdAsync(dto.CurrencyId.Value);
                if (currency == null || !currency.IsActive)
                    return BadRequest(new { message = "Invalid Currency" });
            }

            supplier.Name = dto.Name;
            supplier.ContactPerson = dto.ContactPerson;
            supplier.Phone = dto.Phone;
            supplier.Email = dto.Email;
            supplier.Address = dto.Address;
            supplier.TradeLicenseNo = dto.TradeLicenseNo;
            supplier.TINNo = dto.TINNo;
            supplier.BINNo = dto.BINNo;
            supplier.BankName = dto.BankName;
            supplier.BankAccountNo = dto.BankAccountNo;
            supplier.CurrencyId = dto.CurrencyId;
            supplier.UpdatedBy = User.Identity?.Name ?? "System";
            supplier.UpdatedDate = DateTime.Now;

            _repo.Update(supplier);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "Supplier updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _repo.GetByIdAsync(id);
            if (supplier == null || !supplier.IsActive)
                return NotFound(new { message = "Supplier not found" });

            _repo.SoftDelete(supplier);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "Supplier deleted successfully" });
        }
    }
}