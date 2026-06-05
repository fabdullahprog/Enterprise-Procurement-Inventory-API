using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperShop_Management.Models.Entities
{
    public class POItem : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        [Display(Name = "Ordered Quantity")]
        public int OrderedQuantity { get; set; }

        [Required(ErrorMessage = "Supplier Rate is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Supplier Rate must be greater than 0")]
        [Display(Name = "Supplier Rate")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SupplierRate { get; set; }

        [Required(ErrorMessage = "PO Rate is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "PO Rate must be greater than 0")]
        [Display(Name = "PO Rate")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PORate { get; set; }

        [Display(Name = "Total Price")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "BDT Amount")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BDTAmount { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "Purchase Order is required")]
        [Display(Name = "Purchase Order")]
        public int PurchaseOrderId { get; set; }

        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        // Navigation Properties
        public virtual PurchaseOrder? PurchaseOrder { get; set; }
        public virtual Product? Product { get; set; }
    }
}