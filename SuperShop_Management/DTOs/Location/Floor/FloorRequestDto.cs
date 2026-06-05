using System.ComponentModel.DataAnnotations;


namespace SuperShop_Management.DTOs.Location
{
    public class FloorRequestDto
    {
        [Required]
        [StringLength(50)]
        public string FloorName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Remarks { get; set; }

        [Required]
        public int WarehouseId { get; set; }
    }
}