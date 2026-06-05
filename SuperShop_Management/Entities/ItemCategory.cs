using SuperShop_Management.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Entities
{
    public class ItemCategory:BaseEntity
    {
        [Key]
        public int ItemCategoryId { get; set; }

        [Required(ErrorMessage = "Category Name is required")]
        [StringLength(100, ErrorMessage = "Category Name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Description")]
        public string? CategoryDescription { get; set; }
        public virtual ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    }
}
