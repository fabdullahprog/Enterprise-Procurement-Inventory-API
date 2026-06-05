using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    public class UnitSet : BaseEntity
    {
        [Key]
        public int UnitSetId { get; set; }

        [Required(ErrorMessage = "Unit Set Name is required")]
        [StringLength(100, ErrorMessage = "Unit Set Name cannot exceed 100 characters")]
        [Display(Name = "Unit Set Name")]
        public string NameOfUnitSet { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(250)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        // Navigation Properties
        public virtual ICollection<Unit> Units { get; set; }
            = new List<Unit>();
    }
}
