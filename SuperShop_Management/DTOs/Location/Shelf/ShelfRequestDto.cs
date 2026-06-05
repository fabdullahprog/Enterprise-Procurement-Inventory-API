using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.Location
{
    public class ShelfRequestDto
    {
        [Required]
        [StringLength(50)]
        public string ShelfName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Remarks { get; set; }

        public int? RackId { get; set; }
        public int? AisleId { get; set; }
        public int? ZoneId { get; set; }
        public int? FloorId { get; set; }
        public int? WarehouseId { get; set; }
    }
}