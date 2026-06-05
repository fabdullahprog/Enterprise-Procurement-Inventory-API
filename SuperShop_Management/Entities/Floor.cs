using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    public class Floor : BaseEntity
    {
        [Key]
        public int FloorId { get; set; }

        [Required]
        [MaxLength(50)]
        public string FloorName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Remarks { get; set; }

        // Foreign Key
        public int? WarehouseId { get; set; }
        public virtual Warehouse? Warehouse { get; set; }

        // Navigation - immediate child
        public virtual ICollection<Zone> Zones { get; set; } = new List<Zone>();
    }
}