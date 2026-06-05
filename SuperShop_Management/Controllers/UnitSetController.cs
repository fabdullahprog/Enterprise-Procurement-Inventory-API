using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.DTOs.UnitSet;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitSetController : ControllerBase
    {
        private readonly IUnitSetRepository _repo;

        public UnitSetController(IUnitSetRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var unitSets = await _repo.GetActiveUnitSetsAsync();
            var response = unitSets.Select(u => new UnitSetResponseDto
            {
                UnitSetId = u.UnitSetId,
                NameOfUnitSet = u.NameOfUnitSet,
                Description = u.Description,
                Remarks = u.Remarks,
                IsActive = u.IsActive,
                CreatedDate = u.CreatedDate,
                CreatedBy = u.CreatedBy
            });
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var unitSet = await _repo.GetByIdAsync(id);
            if (unitSet == null || !unitSet.IsActive)
                return NotFound(new { message = "UnitSet not found" });

            var response = new UnitSetResponseDto
            {
                UnitSetId = unitSet.UnitSetId,
                NameOfUnitSet = unitSet.NameOfUnitSet,
                Description = unitSet.Description,
                Remarks = unitSet.Remarks,
                IsActive = unitSet.IsActive,
                CreatedDate = unitSet.CreatedDate,
                CreatedBy = unitSet.CreatedBy
            };
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] UnitSetRequestDto dto)
        {
            var isDuplicate = await _repo.IsDuplicateAsync(dto.NameOfUnitSet);
            if (isDuplicate)
                return BadRequest(new { message = "UnitSet name already exists" });

            var unitSet = new UnitSet
            {
                NameOfUnitSet = dto.NameOfUnitSet,
                Description = dto.Description,
                Remarks = dto.Remarks,
                IsActive = true,
                CreatedBy = User.Identity?.Name ?? "System",
                CreatedDate = DateTime.Now
            };

            await _repo.AddAsync(unitSet);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "UnitSet created successfully", id = unitSet.UnitSetId });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UnitSetRequestDto dto)
        {
            var unitSet = await _repo.GetByIdAsync(id);
            if (unitSet == null || !unitSet.IsActive)
                return NotFound(new { message = "UnitSet not found" });

            var isDuplicate = await _repo.IsDuplicateAsync(dto.NameOfUnitSet, id);
            if (isDuplicate)
                return BadRequest(new { message = "UnitSet name already exists" });

            unitSet.NameOfUnitSet = dto.NameOfUnitSet;
            unitSet.Description = dto.Description;
            unitSet.Remarks = dto.Remarks;
            unitSet.UpdatedBy = User.Identity?.Name ?? "System";
            unitSet.UpdatedDate = DateTime.Now;

            _repo.Update(unitSet);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "UnitSet updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var unitSet = await _repo.GetByIdAsync(id);
            if (unitSet == null || !unitSet.IsActive)
                return NotFound(new { message = "UnitSet not found" });

            _repo.SoftDelete(unitSet);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "UnitSet deleted successfully" });
        }
    }
}