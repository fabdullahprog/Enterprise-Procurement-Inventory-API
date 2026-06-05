using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.DTOs.Inventory;
using SuperShop_Management.Repositories.Interfaces;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryRepository _inventoryRepo;

        public InventoryController(IInventoryRepository inventoryRepo)
        {
            _inventoryRepo = inventoryRepo;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var inventories = await _inventoryRepo.GetActiveInventoriesAsync();
            var response = inventories.Select(MapToDto);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var inventory = await _inventoryRepo.GetByIdAsync(id);
            if (inventory == null || !inventory.IsActive)
                return NotFound(new { message = "Inventory record not found" });

            return Ok(MapToDto(inventory));
        }

        [HttpGet("low-stock")]
        [Authorize(Roles = "Admin, StoreManager")]
        public async Task<IActionResult> GetLowStockItems()
        {
            var lowStockItems = await _inventoryRepo.GetLowStockItemsAsync();
            var response = lowStockItems.Select(MapToDto);
            return Ok(response);
        }

        [HttpGet("by-product/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var inventories = await _inventoryRepo.GetByProductIdAsync(productId);
            return Ok(inventories.Select(MapToDto));
        }

        // GET api/inventory/by-location?warehouseId=1&floorId=1&binId=2
        [HttpGet("by-location")]
        public async Task<IActionResult> GetByLocation(
            [FromQuery] int? warehouseId,
            [FromQuery] int? floorId,
            [FromQuery] int? zoneId,
            [FromQuery] int? aisleId,
            [FromQuery] int? rackId,
            [FromQuery] int? shelfId,
            [FromQuery] int? binId)
        {
            var inventories = await _inventoryRepo.GetByLocationAsync(
                warehouseId, floorId, zoneId, aisleId, rackId, shelfId, binId);
            return Ok(inventories.Select(MapToDto));
        }

        // ─── Helper: Entity → DTO ───────────────────────────────────────────
        private static InventoryResponseDto MapToDto(Models.Entities.Inventory i) => new()
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.Product?.Name,
            BatchId = i.BatchId,
            BatchNumber = i.Batch?.BatchNumber,
            AvailableQuantity = i.AvailableQuantity,
            GrnQuantity = i.GrnQuantity,
            ReservedQuantity = i.ReservedQuantity,
            MinQuantity = i.MinQuantity,
            MaxQuantity = i.MaxQuantity,
            LastUpdated = i.LastUpdated,
            // Location
            WarehouseId = i.WarehouseId,
            WarehouseName = i.Warehouse?.WarehouseName,
            FloorId = i.FloorId,
            FloorName = i.Floor?.FloorName,
            ZoneId = i.ZoneId,
            ZoneName = i.Zone?.ZoneName,
            AisleId = i.AisleId,
            AisleName = i.Aisle?.AisleName,
            RackId = i.RackId,
            RackName = i.Rack?.RackName,
            ShelfId = i.ShelfId,
            ShelfName = i.Shelf?.ShelfName,
            BinId = i.BinId,
            BinName = i.Bin?.BinName,
            LocationLevel = i.LocationLevel
        };
    }
}