using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperShop_Management.Models.Entities
{
    public class QuotationItem : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Unit Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit Price must be greater than 0")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Offered Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        [Display(Name = "Offered Quantity")]
        public int OfferedQuantity { get; set; }

        [Display(Name = "Total Price")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }
        // UnitPrice × OfferedQuantity

        [Display(Name = "BDT Amount")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BDTAmount { get; set; }
        // TotalPrice × ExchangeRate

        // Foreign Keys
        [Required(ErrorMessage = "Quotation is required")]
        [Display(Name = "Supplier Quotation")]
        public int SupplierQuotationId { get; set; }

        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        // Navigation Properties
        public virtual SupplierQuotation? SupplierQuotation { get; set; }
        public virtual Product? Product { get; set; }
    }
}