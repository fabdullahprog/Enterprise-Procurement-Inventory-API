using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.DTOs.Currency;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyRepository _repo;

        public CurrencyController(ICurrencyRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var currencies = await _repo.GetActiveCurrenciesAsync();
            var response = currencies.Select(c => new CurrencyResponseDto
            {
                CurrencyId = c.CurrencyId,
                Code = c.Code,
                Symbol = c.Symbol,
                Name = c.Name,
                ExchangeRate = c.ExchangeRate,
                IsBaseCurrency = c.IsBaseCurrency,
                IsActive = c.IsActive,
                CreatedDate = c.CreatedDate,
                CreatedBy = c.CreatedBy
            });
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var currency = await _repo.GetByIdAsync(id);
            if (currency == null || !currency.IsActive)
                return NotFound(new { message = "Currency not found" });

            var response = new CurrencyResponseDto
            {
                CurrencyId = currency.CurrencyId,
                Code = currency.Code,
                Symbol = currency.Symbol,
                Name = currency.Name,
                ExchangeRate = currency.ExchangeRate,
                IsBaseCurrency = currency.IsBaseCurrency,
                IsActive = currency.IsActive,
                CreatedDate = currency.CreatedDate,
                CreatedBy = currency.CreatedBy
            };
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CurrencyRequestDto dto)
        {
            var isDuplicate = await _repo.IsDuplicateCodeAsync(dto.Code);
            if (isDuplicate)
                return BadRequest(new { message = "Currency code already exists" });

            if (dto.IsBaseCurrency)
            {
                var existingBase = await _repo.GetBaseCurrencyAsync();
                if (existingBase != null)
                    return BadRequest(new { message = "Base currency already exists" });
            }

            var currency = new Currency
            {
                Code = dto.Code.ToUpper(),
                Symbol = dto.Symbol,
                Name = dto.Name,
                ExchangeRate = dto.ExchangeRate,
                IsBaseCurrency = dto.IsBaseCurrency,
                IsActive = true,
                CreatedBy = User.Identity?.Name ?? "System",
                CreatedDate = DateTime.Now
            };

            await _repo.AddAsync(currency);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "Currency created successfully", id = currency.CurrencyId });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CurrencyRequestDto dto)
        {
            var currency = await _repo.GetByIdAsync(id);
            if (currency == null || !currency.IsActive)
                return NotFound(new { message = "Currency not found" });

            var isDuplicate = await _repo.IsDuplicateCodeAsync(dto.Code, id);
            if (isDuplicate)
                return BadRequest(new { message = "Currency code already exists" });

            if (dto.IsBaseCurrency && !currency.IsBaseCurrency)
            {
                var existingBase = await _repo.GetBaseCurrencyAsync();
                if (existingBase != null)
                    return BadRequest(new { message = "Base currency already exists" });
            }

            currency.Code = dto.Code.ToUpper();
            currency.Symbol = dto.Symbol;
            currency.Name = dto.Name;
            currency.ExchangeRate = dto.ExchangeRate;
            currency.IsBaseCurrency = dto.IsBaseCurrency;
            currency.UpdatedBy = User.Identity?.Name ?? "System";
            currency.UpdatedDate = DateTime.Now;

            _repo.Update(currency);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "Currency updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var currency = await _repo.GetByIdAsync(id);
            if (currency == null || !currency.IsActive)
                return NotFound(new { message = "Currency not found" });

            _repo.SoftDelete(currency);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "Currency deleted successfully" });
        }
    }
}