using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    public class RequisitionItem : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Required Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        [Display(Name = "Required Quantity")]
        public int RequiredQuantity { get; set; }

        [StringLength(250)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "Requisition is required")]
        [Display(Name = "Requisition")]
        public int RequisitionId { get; set; }

        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        // Navigation Properties
        public virtual PurchaseRequisition? Requisition { get; set; }
        public virtual Product? Product { get; set; }
    }
}