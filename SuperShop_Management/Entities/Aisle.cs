using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    public class Aisle : BaseEntity
    {
        [Key]
        public int AisleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AisleName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Remarks { get; set; }

        // Flexible Foreign Keys
        public int? ZoneId { get; set; }
        public virtual Zone? Zone { get; set; }

        public int? FloorId { get; set; }
        public virtual Floor? Floor { get; set; }

        public int? WarehouseId { get; set; }
        public virtual Warehouse? Warehouse { get; set; }

        // Navigation - immediate child
        public virtual ICollection<Rack> Racks { get; set; } = new List<Rack>();
    }
}