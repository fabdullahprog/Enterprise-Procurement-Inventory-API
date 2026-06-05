using Microsoft.AspNetCore.Identity;
using SuperShop_Management.Entities.Location;
using SuperShop_Management.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperShop_Management.Entities
{
    public class StockMovement : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string MovementType { get; set; } = string.Empty;
        // GRN_IN, SALE_OUT, TRANSFER, RETURN, ADJUSTMENT

        [Required]
        [StringLength(3)]
        public string Direction { get; set; } = string.Empty;
        // IN / OUT

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        // ═══════════════════════════════════════════════════════════
        // Required Foreign Keys
        // ═══════════════════════════════════════════════════════════

        [Required]
        public int InventoryId { get; set; }

        public int? RelatedDocumentId { get; set; }
        public string? RelatedDocumentType { get; set; }

        [Required]
        public int CreatedById { get; set; }

        // ═══════════════════════════════════════════════════════════
        // FROM Location Chain (Stock কোথা থেকে গেছে / Transfer source)
        // OUT movement এ set হবে
        // ═══════════════════════════════════════════════════════════

        public int? FromWarehouseId { get; set; }
        public int? FromFloorId { get; set; }
        public int? FromZoneId { get; set; }
        public int? FromAisleId { get; set; }
        public int? FromRackId { get; set; }
        public int? FromShelfId { get; set; }
        public int? FromBinId { get; set; }

        // ═══════════════════════════════════════════════════════════
        // TO Location Chain (Stock কোথায় গেছে / Transfer destination)
        // IN movement এ set হবে
        // ═══════════════════════════════════════════════════════════

        public int? ToWarehouseId { get; set; }
        public int? ToFloorId { get; set; }
        public int? ToZoneId { get; set; }
        public int? ToAisleId { get; set; }
        public int? ToRackId { get; set; }
        public int? ToShelfId { get; set; }
        public int? ToBinId { get; set; }

        // ═══════════════════════════════════════════════════════════
        // Computed: কোন Level এ ছিল / গেছে
        // ═══════════════════════════════════════════════════════════

        [NotMapped]
        public string FromLocationLevel
        {
            get
            {
                if (FromBinId.HasValue) return "Bin";
                if (FromShelfId.HasValue) return "Shelf";
                if (FromRackId.HasValue) return "Rack";
                if (FromAisleId.HasValue) return "Aisle";
                if (FromZoneId.HasValue) return "Zone";
                if (FromFloorId.HasValue) return "Floor";
                if (FromWarehouseId.HasValue) return "Warehouse";
                return "External";
            }
        }

        [NotMapped]
        public string ToLocationLevel
        {
            get
            {
                if (ToBinId.HasValue) return "Bin";
                if (ToShelfId.HasValue) return "Shelf";
                if (ToRackId.HasValue) return "Rack";
                if (ToAisleId.HasValue) return "Aisle";
                if (ToZoneId.HasValue) return "Zone";
                if (ToFloorId.HasValue) return "Floor";
                if (ToWarehouseId.HasValue) return "Warehouse";
                return "External";
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Navigation Properties
        // ═══════════════════════════════════════════════════════════

        public virtual Inventory? Inventory { get; set; }
        public virtual IdentityUser<int>? CreatedByUser { get; set; }

        // FROM Location Navigation
        public virtual Warehouse? FromWarehouse { get; set; }
        public virtual Floor? FromFloor { get; set; }
        public virtual Zone? FromZone { get; set; }
        public virtual Aisle? FromAisle { get; set; }
        public virtual Rack? FromRack { get; set; }
        public virtual Shelf? FromShelf { get; set; }
        public virtual Bin? FromBin { get; set; }

        // TO Location Navigation
        public virtual Warehouse? ToWarehouse { get; set; }
        public virtual Floor? ToFloor { get; set; }
        public virtual Zone? ToZone { get; set; }
        public virtual Aisle? ToAisle { get; set; }
        public virtual Rack? ToRack { get; set; }
        public virtual Shelf? ToShelf { get; set; }
        public virtual Bin? ToBin { get; set; }
    }
}
