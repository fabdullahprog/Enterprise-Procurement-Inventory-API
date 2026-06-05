using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.StockMovement
{
    public class StockMovementDto
    {
        public int Id { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Reason { get; set; }
        public int? RelatedDocumentId { get; set; }
        public string? RelatedDocumentType { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }

        public int? FromWarehouseId { get; set; }
        public string? FromWarehouseName { get; set; }
        public int? FromFloorId { get; set; }
        public string? FromFloorName { get; set; }
        public int? FromZoneId { get; set; }
        public string? FromZoneName { get; set; }
        public int? FromAisleId { get; set; }
        public string? FromAisleName { get; set; }
        public int? FromRackId { get; set; }
        public string? FromRackName { get; set; }
        public int? FromShelfId { get; set; }
        public string? FromShelfName { get; set; }
        public int? FromBinId { get; set; }
        public string? FromBinName { get; set; }
        public string? FromLocationLevel { get; set; }

        public int? ToWarehouseId { get; set; }
        public string? ToWarehouseName { get; set; }
        public int? ToFloorId { get; set; }
        public string? ToFloorName { get; set; }
        public int? ToZoneId { get; set; }
        public string? ToZoneName { get; set; }
        public int? ToAisleId { get; set; }
        public string? ToAisleName { get; set; }
        public int? ToRackId { get; set; }
        public string? ToRackName { get; set; }
        public int? ToShelfId { get; set; }
        public string? ToShelfName { get; set; }
        public int? ToBinId { get; set; }
        public string? ToBinName { get; set; }
        public string? ToLocationLevel { get; set; }
    }

    public class CreateStockMovementDto
    {
        [Required]
        public int InventoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string MovementType { get; set; } = string.Empty;

        [Required]
        [StringLength(3)]
        public string Direction { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        public int? RelatedDocumentId { get; set; }
        public string? RelatedDocumentType { get; set; }

        public int? FromWarehouseId { get; set; }
        public int? FromFloorId { get; set; }
        public int? FromZoneId { get; set; }
        public int? FromAisleId { get; set; }
        public int? FromRackId { get; set; }
        public int? FromShelfId { get; set; }
        public int? FromBinId { get; set; }

        public int? ToWarehouseId { get; set; }
        public int? ToFloorId { get; set; }
        public int? ToZoneId { get; set; }
        public int? ToAisleId { get; set; }
        public int? ToRackId { get; set; }
        public int? ToShelfId { get; set; }
        public int? ToBinId { get; set; }
    }
}