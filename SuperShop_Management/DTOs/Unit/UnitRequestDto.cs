using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.Unit
{
    public class UnitRequestDto
    {
        [Required]
        [StringLength(100)]
        public string NameOfUnit { get; set; } = string.Empty;

        [Required]
        public int UnitSetId { get; set; }

        public double UnitFactor { get; set; } = 1;
        public bool IsBaseUnit { get; set; } = false;

        [StringLength(250)]
        public string? Description { get; set; }

        [StringLength(250)]
        public string? Remarks { get; set; }
    }
}