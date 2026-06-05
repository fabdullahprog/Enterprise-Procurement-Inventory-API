using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.SubCategory
{
    public class SubCategoryRequestDto
    {
        [Required]
        [StringLength(100)]
        public string SubCategoryName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        [Required]
        public int ItemCategoryId { get; set; }
    }
}