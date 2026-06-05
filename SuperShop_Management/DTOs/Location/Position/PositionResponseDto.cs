using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.Location
{
    public class PositionResponseDto
    {
        public int PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public string? Remarks { get; set; }

        public int? ShelfId { get; set; }
        public string? ShelfName { get; set; }

        public int? RackId { get; set; }
        public string? RackName { get; set; }

        public int? AisleId { get; set; }
        public string? AisleName { get; set; }

        public int? ZoneId { get; set; }
        public string? ZoneName { get; set; }

        public int? FloorId { get; set; }
        public string? FloorName { get; set; }

        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }

        public int? InventoryId { get; set; }
        public bool IsOccupied => InventoryId.HasValue;

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}
