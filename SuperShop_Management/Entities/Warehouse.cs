using SuperShop_Management.Entities;
using SuperShop_Management.Models.Entities;
//using SuperShop_Management.Models.Entities.Location;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    public class Warehouse : BaseEntity
    {
        [Key]
        public int WarehouseId { get; set; }

        [Required]
        [MaxLength(100)]
        public string WarehouseName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(200)]
        public string? Remarks { get; set; }

        // Navigation - immediate child
        public virtual ICollection<Floor> Floors { get; set; } = new List<Floor>();
    }
}