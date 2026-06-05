using SuperShop_Management.Entities;
using SuperShop_Management.Entities.Location;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    public class Shelf : BaseEntity
    {
        [Key]
        public int ShelfId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ShelfName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Remarks { get; set; }

        // Flexible Foreign Keys
        public int? RackId { get; set; }
        public virtual Rack? Rack { get; set; }

        public int? AisleId { get; set; }
        public virtual Aisle? Aisle { get; set; }

        public int? ZoneId { get; set; }
        public virtual Zone? Zone { get; set; }

        public int? FloorId { get; set; }
        public virtual Floor? Floor { get; set; }

        public int? WarehouseId { get; set; }
        public virtual Warehouse? Warehouse { get; set; }

        // Navigation - immediate child
        public virtual ICollection<Bin> Bins { get; set; } = new List<Bin>();
    }
}