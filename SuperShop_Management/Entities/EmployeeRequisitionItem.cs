using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    public class EmployeeRequisitionItem : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Employee Requisition")]
        public int EmployeeRequisitionId { get; set; }

        [Required(ErrorMessage = "Item is required")]
        [Display(Name = "Item")]
        public int ItemId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Item Name")]
        public string ItemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        [Display(Name = "Required Quantity")]
        public int RequiredQty { get; set; }

        [Display(Name = "Current Stock")]
        public int CurrentStock { get; set; }

        [StringLength(500)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        // Navigation Properties
        public virtual Requisition? EmployeeRequisition { get; set; }
        public virtual Product? Item { get; set; }
    }
}
