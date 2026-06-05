using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperShop_Management.Models.Entities
{
   
    public class CSSupplierRow : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "CS is required")]
        [Display(Name = "Comparative Statement")]
        public int CSId { get; set; }

        [Required(ErrorMessage = "CS Item is required")]
        [Display(Name = "CS Item")]
        public int CSItemId { get; set; }

        [Required(ErrorMessage = "Supplier is required")]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Quotation Item is required")]
        [Display(Name = "Quotation Item")]
        public int QuotationItemId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Unit")]
        public string Unit { get; set; } = string.Empty;

        [Required(ErrorMessage = "Unit Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit Price must be greater than 0")]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        [Display(Name = "Quantity")]
        public int Qty { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Total Price")]
        public decimal TotalPrice { get; set; }
        // Auto-calculated: UnitPrice × Qty

        [Required]
        [StringLength(20)]
        [Display(Name = "Rating")]
        public string Rating { get; set; } = "average";
        // excellent, good, average, poor

        [Display(Name = "Is Selected")]
        public bool IsSelected { get; set; } = false;

        [StringLength(500)]
        [Display(Name = "Selected Reason")]
        public string? SelectedReason { get; set; }

        // Navigation Properties
        public virtual ComparativeStatement? CS { get; set; }
        public virtual CSItem? CSItem { get; set; }
        public virtual Supplier? Supplier { get; set; }
        public virtual QuotationItem? QuotationItem { get; set; }
    }
}
