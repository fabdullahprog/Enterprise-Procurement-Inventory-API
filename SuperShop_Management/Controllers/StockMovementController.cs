using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SuperShop_Management.DTOs.StockMovement;
using SuperShop_Management.Entities;
using SuperShop_Management.Repositories.Interfaces;
using System.Security.Claims;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StockMovementController : ControllerBase
    {
        private readonly IStockMovementRepository _movementRepo;
        private readonly IInventoryRepository _inventoryRepo;
        private readonly UserManager<IdentityUser<int>> _userManager;

        public StockMovementController(
            IStockMovementRepository movementRepo,
            IInventoryRepository inventoryRepo,
            UserManager<IdentityUser<int>> userManager)
        {
            _movementRepo = movementRepo;
            _inventoryRepo = inventoryRepo;
            _userManager = userManager;
        }

        [HttpGet("by-inventory/{inventoryId}")]
        public async Task<IActionResult> GetByInventoryId(int inventoryId)
        {
            var movements = await _movementRepo.GetByInventoryIdAsync(inventoryId);
            var response = movements.Select(m => new StockMovementDto
            {
                Id = m.Id,
                MovementType = m.MovementType,
                Direction = m.Direction,
                Quantity = m.Quantity,
                Reason = m.Reason,
                RelatedDocumentId = m.RelatedDocumentId,
                RelatedDocumentType = m.RelatedDocumentType,
                CreatedDate = m.CreatedDate,
                CreatedBy = m.CreatedByUser?.Email ?? m.CreatedBy,
                // FROM Location
                FromWarehouseId = m.FromWarehouseId,
                FromWarehouseName = m.FromWarehouse?.WarehouseName,
                FromFloorId = m.FromFloorId,
                FromFloorName = m.FromFloor?.FloorName,
                FromZoneId = m.FromZoneId,
                FromZoneName = m.FromZone?.ZoneName,
                FromAisleId = m.FromAisleId,
                FromAisleName = m.FromAisle?.AisleName,
                FromRackId = m.FromRackId,
                FromRackName = m.FromRack?.RackName,
                FromShelfId = m.FromShelfId,
                FromShelfName = m.FromShelf?.ShelfName,
                FromBinId = m.FromBinId,
                FromBinName = m.FromBin?.BinName,
                FromLocationLevel = m.FromLocationLevel,
                // TO Location
                ToWarehouseId = m.ToWarehouseId,
                ToWarehouseName = m.ToWarehouse?.WarehouseName,
                ToFloorId = m.ToFloorId,
                ToFloorName = m.ToFloor?.FloorName,
                ToZoneId = m.ToZoneId,
                ToZoneName = m.ToZone?.ZoneName,
                ToAisleId = m.ToAisleId,
                ToAisleName = m.ToAisle?.AisleName,
                ToRackId = m.ToRackId,
                ToRackName = m.ToRack?.RackName,
                ToShelfId = m.ToShelfId,
                ToShelfName = m.ToShelf?.ShelfName,
                ToBinId = m.ToBinId,
                ToBinName = m.ToBin?.BinName,
                ToLocationLevel = m.ToLocationLevel
            });
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, StoreManager")]
        public async Task<IActionResult> Create([FromBody] CreateStockMovementDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var inventory = await _inventoryRepo.GetByIdAsync(dto.InventoryId);
            if (inventory == null || !inventory.IsActive)
                return BadRequest(new { message = "Invalid inventory record" });

            if (dto.Direction == "OUT" && dto.Quantity > inventory.AvailableQuantity)
                return BadRequest(new { message = "Insufficient stock" });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(currentUserId);
            if (user == null) return Unauthorized();

            var movement = new StockMovement
            {
                InventoryId = dto.InventoryId,
                MovementType = dto.MovementType,
                Direction = dto.Direction,
                Quantity = dto.Quantity,
                Reason = dto.Reason,
                RelatedDocumentId = dto.RelatedDocumentId,
                RelatedDocumentType = dto.RelatedDocumentType,
                CreatedById = int.Parse(currentUserId),
                CreatedByUser = user,
                CreatedBy = user.Email ?? user.UserName,
                IsActive = true,
                CreatedDate = DateTime.Now,
                // FROM Location
                FromWarehouseId = dto.FromWarehouseId,
                FromFloorId = dto.FromFloorId,
                FromZoneId = dto.FromZoneId,
                FromAisleId = dto.FromAisleId,
                FromRackId = dto.FromRackId,
                FromShelfId = dto.FromShelfId,
                FromBinId = dto.FromBinId,
                // TO Location
                ToWarehouseId = dto.ToWarehouseId,
                ToFloorId = dto.ToFloorId,
                ToZoneId = dto.ToZoneId,
                ToAisleId = dto.ToAisleId,
                ToRackId = dto.ToRackId,
                ToShelfId = dto.ToShelfId,
                ToBinId = dto.ToBinId
            };

            // Update inventory quantity
            if (dto.Direction == "IN")
                inventory.AvailableQuantity += dto.Quantity;
            else
                inventory.AvailableQuantity -= dto.Quantity;

            inventory.LastUpdated = DateTime.Now;

            _inventoryRepo.Update(inventory);
            await _movementRepo.AddAsync(movement);
            await _movementRepo.SaveChangesAsync();

            return Ok(new { message = "Stock movement recorded", newQuantity = inventory.AvailableQuantity });
        }
    }
}