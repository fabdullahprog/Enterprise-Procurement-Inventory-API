using SuperShop_Management.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperShop_Management.Entities.Location
{
    public class Bin : BaseEntity
    {
            [Key]
            public int BinId { get; set; }

            [Required]
            [MaxLength(50)]
            public string BinName { get; set; } = string.Empty;

            [MaxLength(200)]
            public string? Remarks { get; set; }

            // ═══════════════════════════════════════════════════════════
            // Flexible Foreign Keys (Huvuhu Position class-er moto)
            // ═══════════════════════════════════════════════════════════

            public int? ShelfId { get; set; }
            public Shelf? Shelf { get; set; }

            public int? RackId { get; set; }
            public Rack? Rack { get; set; }

            public int? AisleId { get; set; }
            public Aisle? Aisle { get; set; }

            public int? ZoneId { get; set; }
            public Zone? Zone { get; set; }

            public int? FloorId { get; set; }
            public Floor? Floor { get; set; }

            public int? WarehouseId { get; set; }
            public Warehouse? Warehouse { get; set; }

            // ═══════════════════════════════════════════════════════════
            // Navigation Properties (Inventory & Movement mapping-er jonno)
            // ═══════════════════════════════════════════════════════════

            // Ei bin-e ki ki product ache (Stock)
            public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

        // Stock Out/Transfer (Source)
       
        public virtual ICollection<StockMovement> OutgoingMovements { get; set; } = new List<StockMovement>();

        // Stock In/Transfer (Destination)
      
        public virtual ICollection<StockMovement> IncomingMovements { get; set; } = new List<StockMovement>();
    }
    }

