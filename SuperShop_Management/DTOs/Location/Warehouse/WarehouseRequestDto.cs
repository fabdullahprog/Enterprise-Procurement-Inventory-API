using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.Location
{
    public class WarehouseRequestDto
    {
        [Required]
        [StringLength(100)]
        public string WarehouseName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(200)]
        public string? Remarks { get; set; }
    }
}
