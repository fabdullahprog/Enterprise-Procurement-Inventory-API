using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    public class SubCategory : BaseEntity
    {
        [Key]
        public int SubCategoryId { get; set; }

        [Required(ErrorMessage = "Sub Category Name is required")]
        [StringLength(100, ErrorMessage = "Sub Category Name cannot exceed 100 characters")]
        [Display(Name = "Sub Category Name")]
        public string SubCategoryName { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        // Foreign Key
        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int ItemCategoryId { get; set; }

        // Navigation Properties
        public virtual ItemCategory? ItemCategory { get; set; }
        public virtual ICollection<Brand> Brands { get; set; }
            = new List<Brand>();
    
        //public virtual ICollection<Product> Products { get; set; }
        //    = new List<Product>();
    }
}
