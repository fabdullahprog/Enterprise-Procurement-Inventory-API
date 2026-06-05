using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    public class Brand : BaseEntity
    {
        [Key]
        public int BrandId { get; set; }

        [Required(ErrorMessage = "Brand Name is required")]
        [StringLength(100, ErrorMessage = "Brand Name cannot exceed 100 characters")]
        [Display(Name = "Brand Name")]
        public string BrandName { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(100)]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [StringLength(200)]
        [Display(Name = "Website")]
        public string? Website { get; set; }

        // Foreign Key
        [Required(ErrorMessage = "Sub Category is required")]
        [Display(Name = "Sub Category")]
        public int SubCategoryId { get; set; }

        // Navigation Properties
        public virtual SubCategory? SubCategory { get; set; }
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        //public virtual ICollection<Product> Products { get; set; }
        //    = new List<Product>();
    }
}
