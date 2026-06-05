using SuperShop_Management.Entities;
using SuperShop_Management.Entities.Location;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperShop_Management.Models.Entities
{
    public class Inventory : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Available Quantity cannot be negative")]
        [Display(Name = "Available Quantity")]
        public int AvailableQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "GRN Quantity cannot be negative")]
        [Display(Name = "GRN Quantity (Original Received)")]
        public int GrnQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Reserved Quantity cannot be negative")]
        [Display(Name = "Reserved Quantity")]
        public int ReservedQuantity { get; set; } = 0; // Picked কিন্তু এখনো issue হয়নি

        [Range(0, int.MaxValue, ErrorMessage = "Min Quantity cannot be negative")]
        [Display(Name = "Min Quantity (Reorder Level)")]
        public int MinQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Max Quantity cannot be negative")]
        [Display(Name = "Max Quantity (Overstock Limit)")]
        public int MaxQuantity { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // ═══════════════════════════════════════════════════════════
        // Required Foreign Keys
        // ═══════════════════════════════════════════════════════════

        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Batch is required")]
        [Display(Name = "Batch")]
        public int BatchId { get; set; }

        [Required(ErrorMessage = "GRN is required")]
        [Display(Name = "GRN")]
        public int GRNId { get; set; }

        // ═══════════════════════════════════════════════════════════
        // Flexible Location Chain (Warehouse → Floor → Zone → Aisle → Rack → Shelf → Bin)
        // যেকোনো একটা set করলেই হবে, সবগুলো nullable
        // ═══════════════════════════════════════════════════════════

        public int? WarehouseId { get; set; }
        public int? FloorId { get; set; }
        public int? ZoneId { get; set; }
        public int? AisleId { get; set; }
        public int? RackId { get; set; }
        public int? ShelfId { get; set; }
        public int? BinId { get; set; }

        // ═══════════════════════════════════════════════════════════
        // Computed: কোন Level এ আছে (Bin > Shelf > Rack > Aisle > Zone > Floor > Warehouse)
        // ═══════════════════════════════════════════════════════════

        [NotMapped]
        public string LocationLevel
        {
            get
            {
                if (BinId.HasValue) return "Bin";
                if (ShelfId.HasValue) return "Shelf";
                if (RackId.HasValue) return "Rack";
                if (AisleId.HasValue) return "Aisle";
                if (ZoneId.HasValue) return "Zone";
                if (FloorId.HasValue) return "Floor";
                if (WarehouseId.HasValue) return "Warehouse";
                return "Unassigned";
            }
        }

        [NotMapped]
        public int? ActiveLocationId
        {
            get
            {
                return BinId ?? ShelfId ?? RackId ?? AisleId ?? ZoneId ?? FloorId ?? WarehouseId;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Navigation Properties
        // ═══════════════════════════════════════════════════════════

        public virtual Product? Product { get; set; }
        public virtual Batch? Batch { get; set; }
        public virtual GRN? GRN { get; set; }

        // Location Navigation
        public virtual Warehouse? Warehouse { get; set; }
        public virtual Floor? Floor { get; set; }
        public virtual Zone? Zone { get; set; }
        public virtual Aisle? Aisle { get; set; }
        public virtual Rack? Rack { get; set; }
        public virtual Shelf? Shelf { get; set; }
        public virtual Bin? Bin { get; set; }

        public virtual ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();
    }
}
