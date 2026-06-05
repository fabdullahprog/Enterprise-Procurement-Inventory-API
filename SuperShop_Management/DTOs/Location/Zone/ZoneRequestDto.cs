using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.Location
{
    public class ZoneRequestDto
    {
        [Required]
        [StringLength(50)]
        public string ZoneName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Remarks { get; set; }

        public int? FloorId { get; set; }
        public int? WarehouseId { get; set; }
    }
}