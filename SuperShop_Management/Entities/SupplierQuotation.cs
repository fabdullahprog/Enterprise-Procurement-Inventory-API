using Microsoft.EntityFrameworkCore.Migrations;
using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperShop_Management.Models.Entities
{
    public class SupplierQuotation : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Quotation Number")]
        public string QuotationNumber { get; set; } = string.Empty;

        [Display(Name = "Quotation Date")]
        public DateTime QuotationDate { get; set; } = DateTime.Now;

        [Display(Name = "Validity Date")]
        public DateTime? ValidityDate { get; set; }

        [Display(Name = "Submitted At")]
        public DateTime? SubmittedAt { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "submitted";
        // submitted, shortlisted, rejected

        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Total BDT Amount")]
        public decimal TotalBDTAmount { get; set; }

        [Required(ErrorMessage = "Delivery Days is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Delivery Days must be at least 1")]
        [Display(Name = "Delivery Days")]
        public int DeliveryDays { get; set; }

        [StringLength(250)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "RFQ is required")]
        [Display(Name = "RFQ")]
        public int RFQId { get; set; }

        [Required(ErrorMessage = "Supplier is required")]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [Display(Name = "Currency")]
        public int? CurrencyId { get; set; }

        // Navigation Properties
        public virtual RequestForQuotation? RFQ { get; set; }
        public virtual Supplier? Supplier { get; set; }
        public virtual Currency? Currency { get; set; }
        public virtual ICollection<QuotationItem> QuotationItems { get; set; }
            = new List<QuotationItem>();
    }
}
