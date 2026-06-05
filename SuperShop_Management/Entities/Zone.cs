using SuperShop_Management.Entities;
//using SuperShop_Management.Models.Entities.Location;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    public class Zone : BaseEntity
    {
        [Key]
        public int ZoneId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ZoneName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Remarks { get; set; }

        // Flexible Foreign Keys
        public int? FloorId { get; set; }
        public virtual Floor? Floor { get; set; }

        public int? WarehouseId { get; set; }
        public virtual Warehouse? Warehouse { get; set; }

        // Navigation - immediate child
        public virtual ICollection<Aisle> Aisles { get; set; } = new List<Aisle>();
    }
}