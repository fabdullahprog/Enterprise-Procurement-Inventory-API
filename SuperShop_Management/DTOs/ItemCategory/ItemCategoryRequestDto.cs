using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.ItemCategory
{
    public class ItemCategoryRequestDto
    {
        [Required(ErrorMessage = "Category Name is required")]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? CategoryDescription { get; set; }
    }
}
