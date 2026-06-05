using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.Location
{
    public class RackRequestDto
    {
        [Required]
        [StringLength(50)]
        public string RackName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Remarks { get; set; }

        public int? AisleId { get; set; }
        public int? ZoneId { get; set; }
        public int? FloorId { get; set; }
        public int? WarehouseId { get; set; }
    }
}