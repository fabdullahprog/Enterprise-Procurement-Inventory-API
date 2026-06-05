using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.Brand
{
    public class BrandRequestDto
    {
        [Required]
        [StringLength(100)]
        public string BrandName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        [Required]
        public int SubCategoryId { get; set; }
    }
}