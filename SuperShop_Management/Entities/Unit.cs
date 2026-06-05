using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    public class Unit : BaseEntity
    {
        [Key]
        public int UnitId { get; set; }

        [Required(ErrorMessage = "Unit Name is required")]
        [StringLength(100, ErrorMessage = "Unit Name cannot exceed 100 characters")]
        [Display(Name = "Unit Name")]
        public string NameOfUnit { get; set; } = string.Empty;

        [Required(ErrorMessage = "Unit Set is required")]
        [Display(Name = "Unit Set")]
        public int UnitSetId { get; set; }

        [Range(0.0000001, double.MaxValue, ErrorMessage = "Factor must be greater than 0")]
        [Display(Name = "Factor to Base Unit")]
        public double UnitFactor { get; set; } = 1;

        [Display(Name = "Is Base Unit")]
        public bool IsBaseUnit { get; set; } = false;

        [StringLength(250)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(250)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        // Navigation Properties
        public virtual UnitSet? UnitSet { get; set; }
        public virtual ICollection<Product> Products { get; set; }
            = new List<Product>();
    }
}
