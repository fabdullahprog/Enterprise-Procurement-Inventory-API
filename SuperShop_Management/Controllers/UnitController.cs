using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.DTOs.Unit;
using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitController : ControllerBase
    {
        private readonly IUnitRepository _repo;
        private readonly IUnitSetRepository _unitSetRepo;

        public UnitController(IUnitRepository repo, IUnitSetRepository unitSetRepo)
        {
            _repo = repo;
            _unitSetRepo = unitSetRepo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var units = await _repo.GetActiveUnitsAsync();
            var response = units.Select(u => new UnitResponseDto
            {
                UnitId = u.UnitId,
                NameOfUnit = u.NameOfUnit,
                UnitSetId = u.UnitSetId,
                UnitSetName = u.UnitSet?.NameOfUnitSet,
                UnitFactor = u.UnitFactor,
                IsBaseUnit = u.IsBaseUnit,
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
            var unit = await _repo.GetByIdAsync(id);
            if (unit == null || !unit.IsActive)
                return NotFound(new { message = "Unit not found" });

            var response = new UnitResponseDto
            {
                UnitId = unit.UnitId,
                NameOfUnit = unit.NameOfUnit,
                UnitSetId = unit.UnitSetId,
                UnitFactor = unit.UnitFactor,
                IsBaseUnit = unit.IsBaseUnit,
                Description = unit.Description,
                Remarks = unit.Remarks,
                IsActive = unit.IsActive,
                CreatedDate = unit.CreatedDate,
                CreatedBy = unit.CreatedBy
            };
            return Ok(response);
        }

        [HttpGet("byset/{unitSetId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByUnitSetId(int unitSetId)
        {
            var units = await _repo.GetByUnitSetIdAsync(unitSetId);
            var response = units.Select(u => new UnitResponseDto
            {
                UnitId = u.UnitId,
                NameOfUnit = u.NameOfUnit,
                UnitSetId = u.UnitSetId,
                UnitFactor = u.UnitFactor,
                IsBaseUnit = u.IsBaseUnit,
                IsActive = u.IsActive
            });
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, PurchaseOfficer")]
        public async Task<IActionResult> Create([FromBody] UnitRequestDto dto)
        {
            var unitSet = await _unitSetRepo.GetByIdAsync(dto.UnitSetId);
            if (unitSet == null || !unitSet.IsActive)
                return BadRequest(new { message = "Invalid UnitSet" });

            var isDuplicate = await _repo.IsDuplicateAsync(dto.NameOfUnit, dto.UnitSetId);
            if (isDuplicate)
                return BadRequest(new { message = "Unit name already exists in this UnitSet" });

            if (dto.IsBaseUnit)
            {
                var existingBase = await _repo.GetBaseUnitOfSetAsync(dto.UnitSetId);
                if (existingBase != null)
                    return BadRequest(new { message = "Base unit already exists for this UnitSet" });
            }

            var unit = new Unit
            {
                NameOfUnit = dto.NameOfUnit,
                UnitSetId = dto.UnitSetId,
                UnitFactor = dto.UnitFactor,
                IsBaseUnit = dto.IsBaseUnit,
                Description = dto.Description,
                Remarks = dto.Remarks,
                IsActive = true,
                CreatedBy = User.Identity?.Name ?? "System",
                CreatedDate = DateTime.Now
            };

            await _repo.AddAsync(unit);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "Unit created successfully", id = unit.UnitId });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, PurchaseOfficer")]
        public async Task<IActionResult> Update(int id, [FromBody] UnitRequestDto dto)
        {
            var unit = await _repo.GetByIdAsync(id);
            if (unit == null || !unit.IsActive)
                return NotFound(new { message = "Unit not found" });

            var unitSet = await _unitSetRepo.GetByIdAsync(dto.UnitSetId);
            if (unitSet == null || !unitSet.IsActive)
                return BadRequest(new { message = "Invalid UnitSet" });

            var isDuplicate = await _repo.IsDuplicateAsync(dto.NameOfUnit, dto.UnitSetId, id);
            if (isDuplicate)
                return BadRequest(new { message = "Unit name already exists in this UnitSet" });

            if (dto.IsBaseUnit && !unit.IsBaseUnit)
            {
                var existingBase = await _repo.GetBaseUnitOfSetAsync(dto.UnitSetId);
                if (existingBase != null && existingBase.UnitId != id)
                    return BadRequest(new { message = "Base unit already exists for this UnitSet" });
            }

            unit.NameOfUnit = dto.NameOfUnit;
            unit.UnitSetId = dto.UnitSetId;
            unit.UnitFactor = dto.UnitFactor;
            unit.IsBaseUnit = dto.IsBaseUnit;
            unit.Description = dto.Description;
            unit.Remarks = dto.Remarks;
            unit.UpdatedBy = User.Identity?.Name ?? "System";
            unit.UpdatedDate = DateTime.Now;

            _repo.Update(unit);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "Unit updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var unit = await _repo.GetByIdAsync(id);
            if (unit == null || !unit.IsActive)
                return NotFound(new { message = "Unit not found" });

            _repo.SoftDelete(unit);
            await _repo.SaveChangesAsync();

            return Ok(new { message = "Unit deleted successfully" });
        }
    }
}