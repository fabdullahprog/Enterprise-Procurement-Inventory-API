using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.DTOs.Location;
using SuperShop_Management.DTOs.Location.Bin;
using SuperShop_Management.Entities.Location;
using SuperShop_Management.Models.Entities;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Controllers
{
    [Route("api/location")]
    [ApiController]
    [Authorize]
    public class LocationController : ControllerBase
    {
        private readonly IWarehouseRepository _warehouseRepo;
        private readonly IFloorRepository _floorRepo;
        private readonly IZoneRepository _zoneRepo;
        private readonly IAisleRepository _aisleRepo;
        private readonly IRackRepository _rackRepo;
        private readonly IShelfRepository _shelfRepo;
        private readonly IBinRepository _binRepo;

        public LocationController(
            IWarehouseRepository warehouseRepo,
            IFloorRepository floorRepo,
            IZoneRepository zoneRepo,
            IAisleRepository aisleRepo,
            IRackRepository rackRepo,
            IShelfRepository shelfRepo,
            IBinRepository binRepo)
        {
            _warehouseRepo = warehouseRepo;
            _floorRepo = floorRepo;
            _zoneRepo = zoneRepo;
            _aisleRepo = aisleRepo;
            _rackRepo = rackRepo;
            _shelfRepo = shelfRepo;
            _binRepo = binRepo;
        }

        // ==================== WAREHOUSES ====================

        [HttpGet("warehouses")]
        [AllowAnonymous]
        public async Task<IActionResult> GetWarehouses()
        {
            var items = await _warehouseRepo.FindAsync(w => w.IsActive);
            var result = items.Select(w => new WarehouseResponseDto
            {
                WarehouseId = w.WarehouseId,
                WarehouseName = w.WarehouseName,
                Address = w.Address,
                Remarks = w.Remarks,
                IsActive = w.IsActive,
                CreatedDate = w.CreatedDate,
                CreatedBy = w.CreatedBy
            }).ToList();
            return Ok(result);
        }

        [HttpGet("warehouses/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetWarehouse(int id)
        {
            var w = await _warehouseRepo.GetByIdAsync(id);
            if (w == null || !w.IsActive)
                return NotFound(new { message = "Warehouse not found" });
            return Ok(new WarehouseResponseDto
            {
                WarehouseId = w.WarehouseId,
                WarehouseName = w.WarehouseName,
                Address = w.Address,
                Remarks = w.Remarks,
                IsActive = w.IsActive,
                CreatedDate = w.CreatedDate,
                CreatedBy = w.CreatedBy
            });
        }

        [HttpPost("warehouses")]
        [Authorize(Roles = "Admin,StoreManager")]
        public async Task<IActionResult> CreateWarehouse([FromBody] WarehouseRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = new Warehouse
            {
                WarehouseName = dto.WarehouseName,
                Address = dto.Address,
                Remarks = dto.Remarks,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "System"
            };
            await _warehouseRepo.AddAsync(entity);
            await _warehouseRepo.SaveChangesAsync();
            return Ok(new { message = "Warehouse created successfully" });
        }

        [HttpPut("warehouses/{id}")]
        [Authorize(Roles = "Admin,StoreManager")]
        public async Task<IActionResult> UpdateWarehouse(int id, [FromBody] WarehouseRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = await _warehouseRepo.GetByIdAsync(id);
            if (entity == null || !entity.IsActive)
                return NotFound(new { message = "Warehouse not found" });
            entity.WarehouseName = dto.WarehouseName;
            entity.Address = dto.Address;
            entity.Remarks = dto.Remarks;
            _warehouseRepo.Update(entity);
            await _warehouseRepo.SaveChangesAsync();
            return Ok(new { message = "Warehouse updated successfully" });
        }

        [HttpDelete("warehouses/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            var entity = await _warehouseRepo.GetByIdAsync(id);
            if (entity == null || !entity.IsActive)
                return NotFound(new { message = "Warehouse not found" });
            _warehouseRepo.SoftDelete(entity);
            await _warehouseRepo.SaveChangesAsync();
            return Ok(new { message = "Warehouse deleted successfully" });
        }

        // ==================== FloorS ====================

        [HttpGet("floors")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFloors()
        {
            var items = await _floorRepo.GetAllWithParentsAsync();
            var result = items.Select(f => new FloorResponseDto
            {
                FloorId = f.FloorId,
                FloorName = f.FloorName,
                Remarks = f.Remarks,
                WarehouseId = f.WarehouseId,
                WarehouseName = f.Warehouse?.WarehouseName,
                IsActive = f.IsActive,
                CreatedDate = f.CreatedDate,
                CreatedBy = f.CreatedBy
            }).ToList();
            return Ok(result);
        }

        [HttpGet("floors/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFloor(int id)
        {
            var x = await _floorRepo.GetByIdAsync(id);
            if (x == null || !x.IsActive)
                return NotFound(new { message = "Floor not found" });
            return Ok(new FloorResponseDto
            {
                FloorId = x.FloorId,
                FloorName = x.FloorName,
                Remarks = x.Remarks,
                WarehouseId = x.WarehouseId,
                IsActive = x.IsActive,
                CreatedDate = x.CreatedDate,
                CreatedBy = x.CreatedBy
            });
        }

        [HttpGet("floors/by-warehouse/{warehouseId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFloorsByWarehouse(int warehouseId)
        {
            var items = await _floorRepo.FindAsync(x => x.WarehouseId == warehouseId && x.IsActive);
            var result = items.Select(x => new FloorResponseDto
            {
                FloorId = x.FloorId,
                FloorName = x.FloorName,
                Remarks = x.Remarks,
                WarehouseId = x.WarehouseId,
                IsActive = x.IsActive,
                CreatedDate = x.CreatedDate,
                CreatedBy = x.CreatedBy
            }).ToList();
            return Ok(result);
        }

        [HttpPost("floors")]
        [Authorize(Roles = "Admin,StoreManager")]
        public async Task<IActionResult> CreateFloor([FromBody] FloorRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = new Floor
            {
                FloorName = dto.FloorName,
                Remarks = dto.Remarks,
                WarehouseId = dto.WarehouseId,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "System"
            };
            await _floorRepo.AddAsync(entity);
            await _floorRepo.SaveChangesAsync();
            return Ok(new { message = "Floor created successfully" });
        }

        [HttpPut("floors/{id}")]
        [Authorize(Roles = "Admin,StoreManager")]
        public async Task<IActionResult> UpdateFloor(int id, [FromBody] FloorRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = await _floorRepo.GetByIdAsync(id);
            if (entity == null || !entity.IsActive)
                return NotFound(new { message = "Floor not found" });
            entity.FloorName = dto.FloorName;
            entity.Remarks = dto.Remarks;
            entity.WarehouseId = dto.WarehouseId;
            _floorRepo.Update(entity);
            await _floorRepo.SaveChangesAsync();
            return Ok(new { message = "Floor updated successfully" });
        }

        [HttpDelete("floors/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFloor(int id)
        {
            var entity = await _floorRepo.GetByIdAsync(id);
            if (entity == null || !entity.IsActive)
                return NotFound(new { message = "Floor not found" });
            _floorRepo.SoftDelete(entity);
            await _floorRepo.SaveChangesAsync();
            return Ok(new { message = "Floor deleted successfully" });
        }

        // ==================== ZoneS ====================

        [HttpGet("zones")]
        [AllowAnonymous]
        public async Task<IActionResult> GetZones()
        {
            var items = await _zoneRepo.GetAllWithParentsAsync();
            var result = items.Select(z => new ZoneResponseDto
            {
                ZoneId = z.ZoneId,
                ZoneName = z.ZoneName,
                Remarks = z.Remarks,
                FloorId = z.FloorId,
                FloorName = z.Floor?.FloorName,
                WarehouseId = z.WarehouseId ?? z.Floor?.WarehouseId,
                WarehouseName = z.Warehouse?.WarehouseName ?? z.Floor?.Warehouse?.WarehouseName,
                IsActive = z.IsActive,
                CreatedDate = z.CreatedDate,
                CreatedBy = z.CreatedBy
            }).ToList();
            return Ok(result);
        }

        [HttpGet("zones/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetZone(int id)
        {
            var x = await _zoneRepo.GetByIdAsync(id);
            if (x == null || !x.IsActive)
                return NotFound(new { message = "Zone not found" });
            return Ok(new ZoneResponseDto
            {
                ZoneId = x.ZoneId,
                ZoneName = x.ZoneName,
                Remarks = x.Remarks,
                FloorId = x.FloorId,
                WarehouseId = x.WarehouseId,
                IsActive = x.IsActive,
                CreatedDate = x.CreatedDate,
                CreatedBy = x.CreatedBy
            });
        }

        [HttpGet("zones/by-floor/{floorId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetZonesByFloor(int floorId)
        {
            var items = await _zoneRepo.FindAsync(x => x.FloorId == floorId && x.IsActive);
            var result = items.Select(x => new ZoneResponseDto
            {
                ZoneId = x.ZoneId,
                ZoneName = x.ZoneName,
                Remarks = x.Remarks,
                FloorId = x.FloorId,
                WarehouseId = x.WarehouseId,
                IsActive = x.IsActive,
                CreatedDate = x.CreatedDate,
                CreatedBy = x.CreatedBy
            }).ToList();
            return Ok(result);
        }

        [HttpPost("zones")]
        [Authorize(Roles = "Admin,StoreManager")]
        public async Task<IActionResult> CreateZone([FromBody] ZoneRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = new Zone
            {
                ZoneName = dto.ZoneName,
                Remarks = dto.Remarks,
                FloorId = dto.FloorId,
                WarehouseId = dto.WarehouseId,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "System"
            };
            await _zoneRepo.AddAsync(entity);
            await _zoneRepo.SaveChangesAsync();
            return Ok(new { message = "Zone created successfully" });
        }

        [HttpPut("zones/{id}")]
        [Authorize(Roles = "Admin,StoreManager")]
        public async Task<IActionResult> UpdateZone(int id, [FromBody] ZoneRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = await _zoneRepo.GetByIdAsync(id);
            if (entity == null || !entity.IsActive)
                return NotFound(new { message = "Zone not found" });
            entity.ZoneName = dto.ZoneName;
            entity.Remarks = dto.Remarks;
            entity.FloorId = dto.FloorId;
            entity.WarehouseId = dto.WarehouseId;
            _zoneRepo.Update(entity);
            await _zoneRepo.SaveChangesAsync();
            return Ok(new { message = "Zone updated successfully" });
        }

        [HttpDelete("zones/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteZone(int id)
        {
            var entity = await _zoneRepo.GetByIdAsync(id);
            if (entity == null || !entity.IsActive)
                return NotFound(new { message = "Zone not found" });
            _zoneRepo.SoftDelete(entity);
            await _zoneRepo.SaveChangesAsync();
            return Ok(new { message = "Zone deleted successfully" });
        }

        // ==================== AisleS ====================

        [HttpGet("aisles")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAisles()
        {
            var items = await _aisleRepo.GetAllWithParentsAsync();
            var result = items.Select(a => new AisleResponseDto
            {
                AisleId = a.AisleId,
                AisleName = a.AisleName,
                Remarks = a.Remarks,
                ZoneId = a.ZoneId,
                ZoneName = a.Zone?.ZoneName,
                FloorId = a.FloorId ?? a.Zone?.FloorId,
                FloorName = a.Floor?.FloorName ?? a.Zone?.Floor?.FloorName,
                WarehouseId = a.WarehouseId ?? a.Floor?.WarehouseId ?? a.Zone?.Floor?.WarehouseId,
                WarehouseName = a.Warehouse?.WarehouseName ?? a.Floor?.Warehouse?.WarehouseName ?? a.Zone?.Floor?.Warehouse?.WarehouseName,
                IsActive = a.IsActive,
                CreatedDate = a.CreatedDate,
                CreatedBy = a.CreatedBy
            }).ToList();
            return Ok(result);
        }

        [HttpGet("aisles/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAisle(int id)
        {
            var x = await _aisleRepo.GetByIdAsync(id);
            if (x == null || !x.IsActive)
                return NotFound(new { message = "Aisle not found" });
            return Ok(new AisleResponseDto
            {
                AisleId = x.AisleId,
                AisleName = x.AisleName,
                Remarks = x.Remarks,
                ZoneId = x.ZoneId,
                FloorId = x.FloorId,
                WarehouseId = x.WarehouseId,
                IsActive = x.IsActive,
                CreatedDate = x.CreatedDate,
                CreatedBy = x.CreatedBy
            });
        }

        [HttpGet("aisles/by-zone/{zoneId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAislesByZone(int zoneId)
        {
            var items = await _aisleRepo.FindAsync(x => x.ZoneId == zoneId && x.IsActive);
            var result = items.Select(x => new AisleResponseDto
            {
                AisleId = x.AisleId,
                AisleName = x.AisleName,
                Remarks = x.Remarks,
                ZoneId = x.ZoneId,
                FloorId = x.FloorId,
                WarehouseId = x.WarehouseId,
                IsActive = x.IsActive,
                CreatedDate = x.CreatedDate,
                CreatedBy = x.CreatedBy
            }).ToList();
            return Ok(result);
        }

        [HttpPost("aisles")]
        [Authorize(Roles = "Admin,StoreManager")]
        public async Task<IActionResult> CreateAisle([FromBody] AisleRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = new Aisle
            {
                AisleName = dto.AisleName,
                Remarks = dto.Remarks,
                ZoneId = dto.ZoneId,
                FloorId = dto.FloorId,
                WarehouseId = dto.WarehouseId,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "System"
            };
            await _aisleRepo.AddAsync(entity);
            await _aisleRepo.SaveChangesAsync();
            return Ok(new { message = "Aisle created successfully" });
        }

        [HttpPut("aisles/{id}")]
        [Authorize(Roles = "Admin,StoreManager")]
        public async Task<IActionResult> UpdateAisle(int id, [FromBody] AisleRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = await _aisleRepo.GetByIdAsync(id);
            if (entity == null || !entity.IsActive)
                return NotFound(new { message = "Aisle not found" });
            entity.AisleName = dto.AisleName;
            entity.Remarks = dto.Remarks;
            entity.ZoneId = dto.ZoneId;
            entity.FloorId = dto.FloorId;
            entity.WarehouseId = dto.WarehouseId;
            _aisleRepo.Update(entity);
            await _aisleRepo.SaveChangesAsync();
            return Ok(new { message = "Aisle updated successfully" });
        }

        [HttpDelete("aisles/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAisle(int id)
        {
            var entity = await _aisleRepo.GetByIdAsync(id);
            if (entity == null || !entity.IsActive)
                return NotFound(new { message = "Aisle not found" });
            _aisleRepo.SoftDelete(entity);
            await _aisleRepo.SaveChangesAsync();
            return Ok(new { message = "Aisle deleted successfully" });
        }

        // ==================== RackS ====================

        [HttpGet("racks")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRacks()
        {
            var items = await _rackRepo.GetAllWithParentsAsync();
            var result = items.Select(r => new RackResponseDto
            {
                RackId = r.RackId,
                RackName = r.RackName,
                Remarks = r.Remarks,
                AisleId = r.AisleId,
                AisleName = r.Aisle?.AisleName,
                ZoneId = r.ZoneId ?? r.Aisle?.ZoneId,
                ZoneName = r.Zone?.ZoneName ?? r.Aisle?.Zone?.ZoneName,
                FloorId = r.FloorId ?? r.Zone?.FloorId ?? r.Aisle?.Zone?.FloorId,
                FloorName = r.Floor?.FloorName ?? r.Zone?.Floor?.FloorName ?? r.Aisle?.Zone?.Floor?.FloorName,
                WarehouseId = r.WarehouseId ?? r.Floor?.WarehouseId ?? r.Zone?.Floor?.WarehouseId ?? r.Aisle?.Zone?.Floor?.WarehouseId,
                WarehouseName = r.Warehouse?.WarehouseName ?? r.Floor?.Warehouse?.WarehouseName ?? r.Zone?.Floor?.Warehouse?.WarehouseName ?? r.Aisle?.Zone?.Floor?.Warehouse?.WarehouseName,
                IsActive = r.IsActive,
                CreatedDate = r.CreatedDate,
                CreatedBy = r.CreatedBy
            }).ToList();
            return Ok(result);
        }

        [HttpGet("racks/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRack(int id)
        {
            var x = await _rackRepo.GetByIdAsync(id);
            if (x == null || !x.IsActive)
                return NotFound(new { message = "Rack not found" });
            return Ok(new RackResponseDto
            {
                RackId = x.RackId,
                RackName = x.RackName,
                Remarks = x.Remarks,
                AisleId = x.AisleId,
                ZoneId = x.ZoneId,
                FloorId = x.FloorId,
                WarehouseId = x.WarehouseId,
                IsActive = x.IsActive,
                CreatedDate = x.CreatedDate,
                CreatedBy = x.CreatedBy
            });
        }

        [HttpGet("racks/by-aisle/{aisleId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRacksByAisle(int aisleId)
        {
            var items = await _rackRepo.FindAsync(x => x.AisleId == aisleId && x.IsActive);
            var result = items.Select(x => new RackResponseDto
            {
                RackId = x.RackId,
                RackName = x.RackName,
                Remarks = x.Remarks,
                AisleId = x.AisleId,
                ZoneId = x.ZoneId,
                FloorId = x.FloorId,
                WarehouseId = x.WarehouseId,
                IsActive = x.IsActive,
                CreatedDate = x.CreatedDate,
                CreatedBy = x.CreatedBy
            }).ToList();
            return Ok(result);
        }

        [HttpPost("racks")]
        [Authorize(Roles = "Admin,StoreManager")]
        public async Task<IActionResult> CreateRack([FromBody] RackRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = new Rack
            {
                RackName = dto.RackName,
                Remarks = dto.Remarks,
                AisleId = dto.AisleId,
                ZoneId = dto.ZoneId,
                FloorId = dto.FloorId,
                WarehouseId = dto.WarehouseId,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "System"
            };
            await _rackRepo.AddAsync(entity);
            await _rackRepo.SaveChangesAsync();
            return Ok(new { message = "Rack created successfully" });
        }

        [HttpPut("racks/{id}")]
        [Authorize(Roles = "Admin,StoreManager")]
        public async Task<IActionResult> UpdateRack(int id, [FromBody] RackRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = await _rackRepo.GetByIdAsync(id);
            if (entity == null || !entity.IsActive)
                return NotFound(new { message = "Rack not found" });
            entity.RackName = dto.RackName;
            entity.Remarks = dto.Remarks;
            entity.AisleId = dto.AisleId;
            entity.ZoneId = dto.ZoneId;
            entity.FloorId = dto.FloorId;
            entity.WarehouseId = dto.WarehouseId;
            _rackRepo.Update(entity);
            await _rackRepo.SaveChangesAsync();
            return Ok(new { message = "Rack updated successfully" });
        }

        [HttpDelete("racks/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRack(int id)
        {
            var entity = await _rackRepo.GetByIdAsync(id);
            if (entity == null || !entity.IsActive)
                return NotFound(new { message = "Rack not found" });
            _rackRepo.SoftDelete(entity);
            await _rackRepo.SaveChangesAsync();
            return Ok(new { message = "Rack deleted successfully" });
        }

        // ==================== ShelfS ====================

        [HttpGet("shelfs")]
        [AllowAnonymous]
        public async Task<IActionResult> GetShelfs()
        {
            var items = await _shelfRepo.GetAllWithParentsAsync();
            var result = items.Select(s => new ShelfResponseDto
            {
                ShelfId = s.ShelfId,
                ShelfName = s.ShelfName,
                Remarks = s.Remarks,
                RackId = s.RackId,
                RackName = s.Rack?.RackName,
                AisleId = s.AisleId ?? s.Rack?.AisleId,
                AisleName = s.Aisle?.AisleName ?? s.Rack?.Aisle?.AisleName,
                ZoneId = s.ZoneId ?? s.Aisle?.ZoneId ?? s.Rack?.Aisle?.ZoneId,
                ZoneName = s.Zone?.ZoneName ?? s.Aisle?.Zone?.ZoneName ?? s.Rack?.Aisle?.Zone?.ZoneName,
                FloorId = s.FloorId ?? s.Zone?.FloorId ?? s.Aisle?.Zone?.FloorId ?? s.Rack?.Aisle?.Zone?.FloorId,
                FloorName = s.Floor?.FloorName ?? s.Zone?.Floor?.FloorName ?? s.Aisle?.Zone?.Floor?.FloorName ?? s.Rack?.Aisle?.Zone?.Floor?.FloorName,
                WarehouseId = s.WarehouseId ?? s.Floor?.WarehouseId ?? s.Zone?.Floor?.WarehouseId ?? s.Aisle?.Zone?.Floor?.WarehouseId ?? s.Rack?.Aisle?.Zone?.Floor?.WarehouseId,
                WarehouseName = s.Warehouse?.WarehouseName ?? s.Floor?.Warehouse?.WarehouseName ?? s.Zone?.Floor?.Warehouse?.WarehouseName ?? s.Aisle?.Zone?.Floor?.Warehouse?.WarehouseName ?? s.Rack?.Aisle?.Zone?.Floor?.Warehouse?.WarehouseName,
                IsActive = s.IsActive,
                CreatedDate = s.CreatedDate,
                CreatedBy = s.CreatedBy
            }).ToList();
            return Ok(result);
        }

        [HttpGet("shelfs/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetShelf(int id)
        {
            var x = await _shelfRepo.GetByIdAsync(id);
            if (x == null || !x.IsActive)
                return NotFound(new { message = "Shelf not found" });
            return Ok(new ShelfResponseDto
            {
                ShelfId = x.ShelfId,
                ShelfName = x.ShelfName,
                Remarks = x.Remarks,
                RackId = x.RackId,
                AisleId = x.AisleId,
                ZoneId = x.ZoneId,
                FloorId = x.FloorId,
                WarehouseId = x.WarehouseId,
                IsActive = x.IsActive,
                CreatedDate = x.CreatedDate,
                CreatedBy = x.CreatedBy
            });
        }

        [HttpGet("shelfs/by-rack/{rackId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetShelfsByRack(int rackId)
        {
            var items = await _shelfRepo.FindAsync(x => x.RackId == rackId && x.IsActive);
            var result = items.Select(x => new ShelfResponseDto
            {
                ShelfId = x.ShelfId,
                ShelfName = x.ShelfName,
                Remarks = x.Remarks,
                RackId = x.RackId,
                AisleId = x.AisleId,
                ZoneId = x.ZoneId,
                FloorId = x.FloorId,
                WarehouseId = x.WarehouseId,
                IsActive = x.IsActive,
                CreatedDate = x.CreatedDate,
                CreatedBy = x.CreatedBy
            }).ToList();
            return Ok(result);
        }

        [HttpPost("shelfs")]
        [Authorize(Roles = "Admin,StoreManager")]
        public async Task<IActionResult> CreateShelf([FromBody] ShelfRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = new Shelf
            {
                ShelfName = dto.ShelfName,
                Remarks = dto.Remarks,
                RackId = dto.RackId,
                AisleId = dto.AisleId,
                ZoneId = dto.ZoneId,
                FloorId = dto.FloorId,
                WarehouseId = dto.WarehouseId,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "System"
            };
            await _shelfRepo.AddAsync(entity);
            await _shelfRepo.SaveChangesAsync();
            return Ok(new { message = "Shelf created successfully" });
        }

        [HttpPut("shelfs/{id}")]
        [Authorize(Roles = "Admin,StoreManager")]
        public async Task<IActionResult> UpdateShelf(int id, [FromBody] ShelfRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = await _shelfRepo.GetByIdAsync(id);
            if (entity == null || !entity.IsActive)
                return NotFound(new { message = "Shelf not found" });
            entity.ShelfName = dto.ShelfName;
            entity.Remarks = dto.Remarks;
            entity.RackId = dto.RackId;
            entity.AisleId = dto.AisleId;
            entity.ZoneId = dto.ZoneId;
            entity.FloorId = dto.FloorId;
            entity.WarehouseId = dto.WarehouseId;
            _shelfRepo.Update(entity);
            await _shelfRepo.SaveChangesAsync();
            return Ok(new { message = "Shelf updated successfully" });
        }

        [HttpDelete("shelfs/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteShelf(int id)
        {
            var entity = await _shelfRepo.GetByIdAsync(id);
            if (entity == null || !entity.IsActive)
                return NotFound(new { message = "Shelf not found" });
            _shelfRepo.SoftDelete(entity);
            await _shelfRepo.SaveChangesAsync();
            return Ok(new { message = "Shelf deleted successfully" });
        }

        // ==================== BinS ====================

        [HttpGet("bins")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBins()
        {
            var items = await _binRepo.GetAllWithParentsAsync();
            var result = items.Select(b => new BinResponseDto
            {
                BinId = b.BinId,
                BinName = b.BinName,
                Remarks = b.Remarks,
                ShelfId = b.ShelfId,
                ShelfName = b.Shelf?.ShelfName,
                RackId = b.RackId ?? b.Shelf?.RackId,
                RackName = b.Rack?.RackName ?? b.Shelf?.Rack?.RackName,
                AisleId = b.AisleId ?? b.Rack?.AisleId ?? b.Shelf?.Rack?.AisleId,
                AisleName = b.Aisle?.AisleName ?? b.Rack?.Aisle?.AisleName ?? b.Shelf?.Rack?.Aisle?.AisleName,
                ZoneId = b.ZoneId ?? b.Aisle?.ZoneId ?? b.Rack?.Aisle?.ZoneId ?? b.Shelf?.Rack?.Aisle?.ZoneId,
                ZoneName = b.Zone?.ZoneName ?? b.Aisle?.Zone?.ZoneName ?? b.Rack?.Aisle?.Zone?.ZoneName ?? b.Shelf?.Rack?.Aisle?.Zone?.ZoneName,
                FloorId = b.FloorId ?? b.Zone?.FloorId ?? b.Aisle?.Zone?.FloorId ?? b.Rack?.Aisle?.Zone?.FloorId ?? b.Shelf?.Rack?.Aisle?.Zone?.FloorId,
                FloorName = b.Floor?.FloorName ?? b.Zone?.Floor?.FloorName ?? b.Aisle?.Zone?.Floor?.FloorName ?? b.Rack?.Aisle?.Zone?.Floor?.FloorName ?? b.Shelf?.Rack?.Aisle?.Zone?.Floor?.FloorName,
                WarehouseId = b.WarehouseId ?? b.Floor?.WarehouseId ?? b.Zone?.Floor?.WarehouseId ?? b.Aisle?.Zone?.Floor?.WarehouseId ?? b.Rack?.Aisle?.Zone?.Floor?.WarehouseId ?? b.Shelf?.Rack?.Aisle?.Zone?.Floor?.WarehouseId,
                WarehouseName = b.Warehouse?.WarehouseName ?? b.Floor?.Warehouse?.WarehouseName ?? b.Zone?.Floor?.Warehouse?.WarehouseName ?? b.Aisle?.Zone?.Floor?.Warehouse?.WarehouseName ?? b.Rack?.Aisle?.Zone?.Floor?.Warehouse?.WarehouseName ?? b.Shelf?.Rack?.Aisle?.Zone?.Floor?.Warehouse?.WarehouseName,
                IsActive = b.IsActive,
                CreatedDate = b.CreatedDate,
                CreatedBy = b.CreatedBy
            }).ToList();
            return Ok(result);
        }

        [HttpGet("bins/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBin(int id)
        {
            var x = await _binRepo.GetByIdAsync(id);
            if (x == null || !x.IsActive)
                return NotFound(new { message = "Bin not found" });
            return Ok(new BinResponseDto
            {
                BinId = x.BinId,
                BinName = x.BinName,
                Remarks = x.Remarks,
                ShelfId = x.ShelfId,
                RackId = x.RackId,
                AisleId = x.AisleId,
                ZoneId = x.ZoneId,
                FloorId = x.FloorId,
                WarehouseId = x.WarehouseId,
                IsActive = x.IsActive,
                CreatedDate = x.CreatedDate,
                CreatedBy = x.CreatedBy
            });
        }

        [HttpGet("bins/by-shelf/{shelfId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBinsByShelf(int shelfId)
        {
            var items = await _binRepo.FindAsync(x => x.ShelfId == shelfId && x.IsActive);
            var result = items.Select(x => new BinResponseDto
            {
                BinId = x.BinId,
                BinName = x.BinName,
                Remarks = x.Remarks,
                ShelfId = x.ShelfId,
                RackId = x.RackId,
                AisleId = x.AisleId,
                ZoneId = x.ZoneId,
                FloorId = x.FloorId,
                WarehouseId = x.WarehouseId,
                IsActive = x.IsActive,
                CreatedDate = x.CreatedDate,
                CreatedBy = x.CreatedBy
            }).ToList();
            return Ok(result);
        }

        [HttpPost("bins")]
        [Authorize(Roles = "Admin,StoreManager")]
        public async Task<IActionResult> CreateBin([FromBody] BinRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = new Bin
            {
                BinName = dto.BinName,
                Remarks = dto.Remarks,
                ShelfId = dto.ShelfId,
                RackId = dto.RackId,
                AisleId = dto.AisleId,
                ZoneId = dto.ZoneId,
                FloorId = dto.FloorId,
                WarehouseId = dto.WarehouseId,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "System"
            };
            await _binRepo.AddAsync(entity);
            await _binRepo.SaveChangesAsync();
            return Ok(new { message = "Bin created successfully" });
        }

        [HttpPut("bins/{id}")]
        [Authorize(Roles = "Admin,StoreManager")]
        public async Task<IActionResult> UpdateBin(int id, [FromBody] BinRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = await _binRepo.GetByIdAsync(id);
            if (entity == null || !entity.IsActive)
                return NotFound(new { message = "Bin not found" });
            entity.BinName = dto.BinName;
            entity.Remarks = dto.Remarks;
            entity.ShelfId = dto.ShelfId;
            entity.RackId = dto.RackId;
            entity.AisleId = dto.AisleId;
            entity.ZoneId = dto.ZoneId;
            entity.FloorId = dto.FloorId;
            entity.WarehouseId = dto.WarehouseId;
            _binRepo.Update(entity);
            await _binRepo.SaveChangesAsync();
            return Ok(new { message = "Bin updated successfully" });
        }

        [HttpDelete("bins/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBin(int id)
        {
            var entity = await _binRepo.GetByIdAsync(id);
            if (entity == null || !entity.IsActive)
                return NotFound(new { message = "Bin not found" });
            _binRepo.SoftDelete(entity);
            await _binRepo.SaveChangesAsync();
            return Ok(new { message = "Bin deleted successfully" });
        }

        // ==================== SPECIAL ENDPOINTS ====================

        [HttpGet("index")]
        [AllowAnonymous]
        public async Task<IActionResult> GetIndex()
        {
            var warehouses = (await _warehouseRepo.FindAsync(w => w.IsActive)).ToList();
            var floors = (await _floorRepo.FindAsync(f => f.IsActive)).ToList();
            var zones = (await _zoneRepo.FindAsync(z => z.IsActive)).ToList();
            var aisles = (await _aisleRepo.FindAsync(a => a.IsActive)).ToList();
            var racks = (await _rackRepo.FindAsync(r => r.IsActive)).ToList();
            var shelves = (await _shelfRepo.FindAsync(s => s.IsActive)).ToList();
            var bins = (await _binRepo.FindAsync(b => b.IsActive)).ToList();

            var tree = BuildTree(warehouses, floors, zones, aisles, racks, shelves, bins);

            var result = new LocationIndexDto
            {
                Tree = tree,
                TotalWarehouses = warehouses.Count,
                TotalFloors = floors.Count,
                TotalZones = zones.Count,
                TotalAisles = aisles.Count,
                TotalRacks = racks.Count,
                TotalShelves = shelves.Count,
                TotalBins = bins.Count,
                Warehouses = warehouses.Select(w => new WarehouseListItemDto
                {
                    WarehouseId = w.WarehouseId,
                    WarehouseName = w.WarehouseName,
                    Address = w.Address,
                    Remarks = w.Remarks,
                    ChildCount = floors.Count(f => f.WarehouseId == w.WarehouseId)
                }).ToList(),
                Floors = floors.Select(f => new FloorListItemDto
                {
                    FloorId = f.FloorId,
                    FloorName = f.FloorName,
                    Remarks = f.Remarks,
                    ParentName = warehouses.FirstOrDefault(w => w.WarehouseId == f.WarehouseId)?.WarehouseName ?? "",
                    Path = $"{warehouses.FirstOrDefault(w => w.WarehouseId == f.WarehouseId)?.WarehouseName} > {f.FloorName}",
                    ChildCount = zones.Count(z => z.FloorId == f.FloorId)
                }).ToList(),
                Zones = zones.Select(z => new ZoneListItemDto
                {
                    ZoneId = z.ZoneId,
                    ZoneName = z.ZoneName,
                    Remarks = z.Remarks,
                    ParentName = floors.FirstOrDefault(f => f.FloorId == z.FloorId)?.FloorName ?? "",
                    Path = $"{warehouses.FirstOrDefault(w => w.WarehouseId == z.WarehouseId)?.WarehouseName} > {floors.FirstOrDefault(f => f.FloorId == z.FloorId)?.FloorName} > {z.ZoneName}",
                    ChildCount = aisles.Count(a => a.ZoneId == z.ZoneId)
                }).ToList(),
                Aisles = aisles.Select(a => new AisleListItemDto
                {
                    AisleId = a.AisleId,
                    AisleName = a.AisleName,
                    Remarks = a.Remarks,
                    ParentName = zones.FirstOrDefault(z => z.ZoneId == a.ZoneId)?.ZoneName ?? "",
                    Path = $"{warehouses.FirstOrDefault(w => w.WarehouseId == a.WarehouseId)?.WarehouseName} > {floors.FirstOrDefault(f => f.FloorId == a.FloorId)?.FloorName} > {zones.FirstOrDefault(z => z.ZoneId == a.ZoneId)?.ZoneName} > {a.AisleName}",
                    ChildCount = racks.Count(r => r.AisleId == a.AisleId)
                }).ToList(),
                Racks = racks.Select(r => new RackListItemDto
                {
                    RackId = r.RackId,
                    RackName = r.RackName,
                    Remarks = r.Remarks,
                    ParentName = aisles.FirstOrDefault(a => a.AisleId == r.AisleId)?.AisleName ?? "",
                    Path = $"{warehouses.FirstOrDefault(w => w.WarehouseId == r.WarehouseId)?.WarehouseName} > {floors.FirstOrDefault(f => f.FloorId == r.FloorId)?.FloorName} > {zones.FirstOrDefault(z => z.ZoneId == r.ZoneId)?.ZoneName} > {aisles.FirstOrDefault(a => a.AisleId == r.AisleId)?.AisleName} > {r.RackName}",
                    ChildCount = shelves.Count(s => s.RackId == r.RackId)
                }).ToList(),
                Shelves = shelves.Select(s => new ShelfListItemDto
                {
                    ShelfId = s.ShelfId,
                    ShelfName = s.ShelfName,
                    Remarks = s.Remarks,
                    ParentName = racks.FirstOrDefault(r => r.RackId == s.RackId)?.RackName ?? "",
                    Path = $"{warehouses.FirstOrDefault(w => w.WarehouseId == s.WarehouseId)?.WarehouseName} > {floors.FirstOrDefault(f => f.FloorId == s.FloorId)?.FloorName} > {zones.FirstOrDefault(z => z.ZoneId == s.ZoneId)?.ZoneName} > {aisles.FirstOrDefault(a => a.AisleId == s.AisleId)?.AisleName} > {racks.FirstOrDefault(r => r.RackId == s.RackId)?.RackName} > {s.ShelfName}",
                    ChildCount = bins.Count(b => b.ShelfId == s.ShelfId)
                }).ToList(),
                Bins = bins.Select(b => new BinListItemDto
                {
                    BinId = b.BinId,
                    BinName = b.BinName,
                    Remarks = b.Remarks,
                    ParentName = shelves.FirstOrDefault(s => s.ShelfId == b.ShelfId)?.ShelfName ?? "",
                    Path = $"{warehouses.FirstOrDefault(w => w.WarehouseId == b.WarehouseId)?.WarehouseName} > {floors.FirstOrDefault(f => f.FloorId == b.FloorId)?.FloorName} > {zones.FirstOrDefault(z => z.ZoneId == b.ZoneId)?.ZoneName} > {aisles.FirstOrDefault(a => a.AisleId == b.AisleId)?.AisleName} > {racks.FirstOrDefault(r => r.RackId == b.RackId)?.RackName} > {shelves.FirstOrDefault(s => s.ShelfId == b.ShelfId)?.ShelfName} > {b.BinName}"
                }).ToList()
            };
            return Ok(result);
        }

        [HttpGet("tree")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTree()
        {
            var warehouses = (await _warehouseRepo.FindAsync(w => w.IsActive)).ToList();
            var floors = (await _floorRepo.FindAsync(f => f.IsActive)).ToList();
            var zones = (await _zoneRepo.FindAsync(z => z.IsActive)).ToList();
            var aisles = (await _aisleRepo.FindAsync(a => a.IsActive)).ToList();
            var racks = (await _rackRepo.FindAsync(r => r.IsActive)).ToList();
            var shelves = (await _shelfRepo.FindAsync(s => s.IsActive)).ToList();
            var bins = (await _binRepo.FindAsync(b => b.IsActive)).ToList();
            return Ok(BuildTree(warehouses, floors, zones, aisles, racks, shelves, bins));
        }

        [HttpGet("dropdowns")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDropdowns(
            int? warehouseId = null,
            int? floorId = null,
            int? zoneId = null,
            int? aisleId = null,
            int? rackId = null,
            int? shelfId = null)
        {
            var warehouses = (await _warehouseRepo.FindAsync(w => w.IsActive)).ToList();
            var floors = (await _floorRepo.FindAsync(f => f.IsActive)).ToList();
            var zones = (await _zoneRepo.FindAsync(z => z.IsActive)).ToList();
            var aisles = (await _aisleRepo.FindAsync(a => a.IsActive)).ToList();
            var racks = (await _rackRepo.FindAsync(r => r.IsActive)).ToList();
            var shelves = (await _shelfRepo.FindAsync(s => s.IsActive)).ToList();
            var bins = (await _binRepo.FindAsync(b => b.IsActive)).ToList();

            // Apply cascading filters
            if (warehouseId.HasValue)
            {
                floors = floors.Where(f => f.WarehouseId == warehouseId.Value).ToList();
                zones = zones.Where(z => z.WarehouseId == warehouseId.Value || 
                                        (z.FloorId.HasValue && floors.Any(f => f.FloorId == z.FloorId))).ToList();
                aisles = aisles.Where(a => a.WarehouseId == warehouseId.Value || 
                                          (a.FloorId.HasValue && floors.Any(f => f.FloorId == a.FloorId)) ||
                                          (a.ZoneId.HasValue && zones.Any(z => z.ZoneId == a.ZoneId))).ToList();
                racks = racks.Where(r => r.WarehouseId == warehouseId.Value || 
                                        (r.FloorId.HasValue && floors.Any(f => f.FloorId == r.FloorId)) ||
                                        (r.ZoneId.HasValue && zones.Any(z => z.ZoneId == r.ZoneId)) ||
                                        (r.AisleId.HasValue && aisles.Any(a => a.AisleId == r.AisleId))).ToList();
                shelves = shelves.Where(s => s.WarehouseId == warehouseId.Value || 
                                            (s.FloorId.HasValue && floors.Any(f => f.FloorId == s.FloorId)) ||
                                            (s.ZoneId.HasValue && zones.Any(z => z.ZoneId == s.ZoneId)) ||
                                            (s.AisleId.HasValue && aisles.Any(a => a.AisleId == s.AisleId)) ||
                                            (s.RackId.HasValue && racks.Any(r => r.RackId == s.RackId))).ToList();
                bins = bins.Where(b => b.WarehouseId == warehouseId.Value || 
                                      (b.FloorId.HasValue && floors.Any(f => f.FloorId == b.FloorId)) ||
                                      (b.ZoneId.HasValue && zones.Any(z => z.ZoneId == b.ZoneId)) ||
                                      (b.AisleId.HasValue && aisles.Any(a => a.AisleId == b.AisleId)) ||
                                      (b.RackId.HasValue && racks.Any(r => r.RackId == b.RackId)) ||
                                      (b.ShelfId.HasValue && shelves.Any(s => s.ShelfId == b.ShelfId))).ToList();
            }

            if (floorId.HasValue)
            {
                zones = zones.Where(z => z.FloorId == floorId.Value).ToList();
                aisles = aisles.Where(a => a.FloorId == floorId.Value || 
                                          (a.ZoneId.HasValue && zones.Any(z => z.ZoneId == a.ZoneId))).ToList();
                racks = racks.Where(r => r.FloorId == floorId.Value || 
                                        (r.ZoneId.HasValue && zones.Any(z => z.ZoneId == r.ZoneId)) ||
                                        (r.AisleId.HasValue && aisles.Any(a => a.AisleId == r.AisleId))).ToList();
                shelves = shelves.Where(s => s.FloorId == floorId.Value || 
                                            (s.ZoneId.HasValue && zones.Any(z => z.ZoneId == s.ZoneId)) ||
                                            (s.AisleId.HasValue && aisles.Any(a => a.AisleId == s.AisleId)) ||
                                            (s.RackId.HasValue && racks.Any(r => r.RackId == s.RackId))).ToList();
                bins = bins.Where(b => b.FloorId == floorId.Value || 
                                      (b.ZoneId.HasValue && zones.Any(z => z.ZoneId == b.ZoneId)) ||
                                      (b.AisleId.HasValue && aisles.Any(a => a.AisleId == b.AisleId)) ||
                                      (b.RackId.HasValue && racks.Any(r => r.RackId == b.RackId)) ||
                                      (b.ShelfId.HasValue && shelves.Any(s => s.ShelfId == b.ShelfId))).ToList();
            }

            if (zoneId.HasValue)
            {
                aisles = aisles.Where(a => a.ZoneId == zoneId.Value).ToList();
                racks = racks.Where(r => r.ZoneId == zoneId.Value || 
                                        (r.AisleId.HasValue && aisles.Any(a => a.AisleId == r.AisleId))).ToList();
                shelves = shelves.Where(s => s.ZoneId == zoneId.Value || 
                                            (s.AisleId.HasValue && aisles.Any(a => a.AisleId == s.AisleId)) ||
                                            (s.RackId.HasValue && racks.Any(r => r.RackId == s.RackId))).ToList();
                bins = bins.Where(b => b.ZoneId == zoneId.Value || 
                                      (b.AisleId.HasValue && aisles.Any(a => a.AisleId == b.AisleId)) ||
                                      (b.RackId.HasValue && racks.Any(r => r.RackId == b.RackId)) ||
                                      (b.ShelfId.HasValue && shelves.Any(s => s.ShelfId == b.ShelfId))).ToList();
            }

            if (aisleId.HasValue)
            {
                racks = racks.Where(r => r.AisleId == aisleId.Value).ToList();
                shelves = shelves.Where(s => s.AisleId == aisleId.Value || 
                                            (s.RackId.HasValue && racks.Any(r => r.RackId == s.RackId))).ToList();
                bins = bins.Where(b => b.AisleId == aisleId.Value || 
                                      (b.RackId.HasValue && racks.Any(r => r.RackId == b.RackId)) ||
                                      (b.ShelfId.HasValue && shelves.Any(s => s.ShelfId == b.ShelfId))).ToList();
            }

            if (rackId.HasValue)
            {
                shelves = shelves.Where(s => s.RackId == rackId.Value).ToList();
                bins = bins.Where(b => b.RackId == rackId.Value || 
                                      (b.ShelfId.HasValue && shelves.Any(s => s.ShelfId == b.ShelfId))).ToList();
            }

            if (shelfId.HasValue)
            {
                bins = bins.Where(b => b.ShelfId == shelfId.Value).ToList();
            }

            return Ok(new
            {
                warehouses = warehouses.Select(w => new { id = w.WarehouseId, name = w.WarehouseName }),
                floors = floors.Select(f => new { id = f.FloorId, name = f.FloorName, warehouseId = f.WarehouseId }),
                zones = zones.Select(z => new { id = z.ZoneId, name = z.ZoneName, floorId = z.FloorId, warehouseId = z.WarehouseId }),
                aisles = aisles.Select(a => new { id = a.AisleId, name = a.AisleName, zoneId = a.ZoneId, floorId = a.FloorId, warehouseId = a.WarehouseId }),
                racks = racks.Select(r => new { id = r.RackId, name = r.RackName, aisleId = r.AisleId, zoneId = r.ZoneId, floorId = r.FloorId, warehouseId = r.WarehouseId }),
                shelves = shelves.Select(s => new { id = s.ShelfId, name = s.ShelfName, rackId = s.RackId, aisleId = s.AisleId, zoneId = s.ZoneId, floorId = s.FloorId, warehouseId = s.WarehouseId }),
                bins = bins.Select(b => new { id = b.BinId, name = b.BinName, shelfId = b.ShelfId, rackId = b.RackId, aisleId = b.AisleId, zoneId = b.ZoneId, floorId = b.FloorId, warehouseId = b.WarehouseId })
            });
        }

        // ==================== PRIVATE HELPERS ====================

        private List<LocationTreeNodeDto> BuildTree(
            List<Warehouse> warehouses,
            List<Floor> floors,
            List<Zone> zones,
            List<Aisle> aisles,
            List<Rack> racks,
            List<Shelf> shelves,
            List<Bin> bins)
        {
            var tree = new List<LocationTreeNodeDto>();
            
            foreach (var w in warehouses)
            {
                var wNode = new LocationTreeNodeDto
                {
                    Id = $"W-{w.WarehouseId}",
                    Label = w.WarehouseName,
                    Type = "warehouse",
                    Icon = "🏭",
                    ColorClass = "text-blue-600",
                    EntityId = w.WarehouseId,
                    Remarks = w.Remarks,
                    FullPath = w.WarehouseName,
                    Children = new List<LocationTreeNodeDto>()
                };

                // Add floors under warehouse
                foreach (var f in floors.Where(f => f.WarehouseId == w.WarehouseId))
                {
                    var fNode = BuildFloorNode(w, f, zones, aisles, racks, shelves, bins);
                    wNode.Children.Add(fNode);
                }

                // Add zones directly under warehouse (skipping floor)
                foreach (var z in zones.Where(z => z.WarehouseId == w.WarehouseId && !z.FloorId.HasValue))
                {
                    var zNode = BuildZoneNode(w.WarehouseName, null, z, aisles, racks, shelves, bins);
                    wNode.Children.Add(zNode);
                }

                tree.Add(wNode);
            }

            return tree;
        }

        private LocationTreeNodeDto BuildFloorNode(
            Warehouse warehouse,
            Floor floor,
            List<Zone> zones,
            List<Aisle> aisles,
            List<Rack> racks,
            List<Shelf> shelves,
            List<Bin> bins)
        {
            var fNode = new LocationTreeNodeDto
            {
                Id = $"F-{floor.FloorId}",
                Label = floor.FloorName,
                Type = "floor",
                Icon = "🏢",
                ColorClass = "text-green-600",
                EntityId = floor.FloorId,
                ParentId = $"W-{warehouse.WarehouseId}",
                Remarks = floor.Remarks,
                FullPath = $"{warehouse.WarehouseName} > {floor.FloorName}",
                Children = new List<LocationTreeNodeDto>()
            };

            // Add zones under floor
            foreach (var z in zones.Where(z => z.FloorId == floor.FloorId))
            {
                var zNode = BuildZoneNode(warehouse.WarehouseName, floor.FloorName, z, aisles, racks, shelves, bins);
                fNode.Children.Add(zNode);
            }

            // Add aisles directly under floor (skipping zone)
            foreach (var a in aisles.Where(a => a.FloorId == floor.FloorId && !a.ZoneId.HasValue))
            {
                var aNode = BuildAisleNode($"{warehouse.WarehouseName} > {floor.FloorName}", $"F-{floor.FloorId}", a, racks, shelves, bins);
                fNode.Children.Add(aNode);
            }

            return fNode;
        }

        private LocationTreeNodeDto BuildZoneNode(
            string warehouseName,
            string? floorName,
            Zone zone,
            List<Aisle> aisles,
            List<Rack> racks,
            List<Shelf> shelves,
            List<Bin> bins)
        {
            var parentPath = floorName != null ? $"{warehouseName} > {floorName}" : warehouseName;
            var parentId = zone.FloorId.HasValue ? $"F-{zone.FloorId}" : $"W-{zone.WarehouseId}";

            var zNode = new LocationTreeNodeDto
            {
                Id = $"Z-{zone.ZoneId}",
                Label = zone.ZoneName,
                Type = "zone",
                Icon = "📦",
                ColorClass = "text-yellow-600",
                EntityId = zone.ZoneId,
                ParentId = parentId,
                Remarks = zone.Remarks,
                FullPath = $"{parentPath} > {zone.ZoneName}",
                Children = new List<LocationTreeNodeDto>()
            };

            // Add aisles under zone
            foreach (var a in aisles.Where(a => a.ZoneId == zone.ZoneId))
            {
                var aNode = BuildAisleNode($"{parentPath} > {zone.ZoneName}", $"Z-{zone.ZoneId}", a, racks, shelves, bins);
                zNode.Children.Add(aNode);
            }

            // Add racks directly under zone (skipping aisle)
            foreach (var r in racks.Where(r => r.ZoneId == zone.ZoneId && !r.AisleId.HasValue))
            {
                var rNode = BuildRackNode($"{parentPath} > {zone.ZoneName}", $"Z-{zone.ZoneId}", r, shelves, bins);
                zNode.Children.Add(rNode);
            }

            return zNode;
        }

        private LocationTreeNodeDto BuildAisleNode(
            string parentPath,
            string parentId,
            Aisle aisle,
            List<Rack> racks,
            List<Shelf> shelves,
            List<Bin> bins)
        {
            var aNode = new LocationTreeNodeDto
            {
                Id = $"A-{aisle.AisleId}",
                Label = aisle.AisleName,
                Type = "aisle",
                Icon = "🚶",
                ColorClass = "text-orange-600",
                EntityId = aisle.AisleId,
                ParentId = parentId,
                Remarks = aisle.Remarks,
                FullPath = $"{parentPath} > {aisle.AisleName}",
                Children = new List<LocationTreeNodeDto>()
            };

            // Add racks under aisle
            foreach (var r in racks.Where(r => r.AisleId == aisle.AisleId))
            {
                var rNode = BuildRackNode($"{parentPath} > {aisle.AisleName}", $"A-{aisle.AisleId}", r, shelves, bins);
                aNode.Children.Add(rNode);
            }

            // Add shelves directly under aisle (skipping rack)
            foreach (var s in shelves.Where(s => s.AisleId == aisle.AisleId && !s.RackId.HasValue))
            {
                var sNode = BuildShelfNode($"{parentPath} > {aisle.AisleName}", $"A-{aisle.AisleId}", s, bins);
                aNode.Children.Add(sNode);
            }

            return aNode;
        }

        private LocationTreeNodeDto BuildRackNode(
            string parentPath,
            string parentId,
            Rack rack,
            List<Shelf> shelves,
            List<Bin> bins)
        {
            var rNode = new LocationTreeNodeDto
            {
                Id = $"R-{rack.RackId}",
                Label = rack.RackName,
                Type = "rack",
                Icon = "🗄️",
                ColorClass = "text-red-600",
                EntityId = rack.RackId,
                ParentId = parentId,
                Remarks = rack.Remarks,
                FullPath = $"{parentPath} > {rack.RackName}",
                Children = new List<LocationTreeNodeDto>()
            };

            // Add shelves under rack
            foreach (var s in shelves.Where(s => s.RackId == rack.RackId))
            {
                var sNode = BuildShelfNode($"{parentPath} > {rack.RackName}", $"R-{rack.RackId}", s, bins);
                rNode.Children.Add(sNode);
            }

            // Add bins directly under rack (skipping shelf)
            foreach (var b in bins.Where(b => b.RackId == rack.RackId && !b.ShelfId.HasValue))
            {
                var bNode = BuildBinNode($"{parentPath} > {rack.RackName}", $"R-{rack.RackId}", b);
                rNode.Children.Add(bNode);
            }

            return rNode;
        }

        private LocationTreeNodeDto BuildShelfNode(
            string parentPath,
            string parentId,
            Shelf shelf,
            List<Bin> bins)
        {
            var sNode = new LocationTreeNodeDto
            {
                Id = $"S-{shelf.ShelfId}",
                Label = shelf.ShelfName,
                Type = "shelf",
                Icon = "📚",
                ColorClass = "text-purple-600",
                EntityId = shelf.ShelfId,
                ParentId = parentId,
                Remarks = shelf.Remarks,
                FullPath = $"{parentPath} > {shelf.ShelfName}",
                Children = new List<LocationTreeNodeDto>()
            };

            // Add bins under shelf
            foreach (var b in bins.Where(b => b.ShelfId == shelf.ShelfId))
            {
                var bNode = BuildBinNode($"{parentPath} > {shelf.ShelfName}", $"S-{shelf.ShelfId}", b);
                sNode.Children.Add(bNode);
            }

            return sNode;
        }

        private LocationTreeNodeDto BuildBinNode(
            string parentPath,
            string parentId,
            Bin bin)
        {
            return new LocationTreeNodeDto
            {
                Id = $"B-{bin.BinId}",
                Label = bin.BinName,
                Type = "bin",
                Icon = "📫",
                ColorClass = "text-pink-600",
                EntityId = bin.BinId,
                ParentId = parentId,
                Remarks = bin.Remarks,
                FullPath = $"{parentPath} > {bin.BinName}",
                Children = new List<LocationTreeNodeDto>()
            };
        }
    }
}