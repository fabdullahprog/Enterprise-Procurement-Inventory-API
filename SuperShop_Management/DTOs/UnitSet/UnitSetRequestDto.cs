using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.UnitSet
{
    public class UnitSetRequestDto
    {
        [Required]
        [StringLength(100)]
        public string NameOfUnitSet { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        [StringLength(250)]
        public string? Remarks { get; set; }
    }
}