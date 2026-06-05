using Microsoft.AspNetCore.Identity;
using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    public class RequestForQuotation : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "RFQ Number")]
        public string RFQNumber { get; set; } = string.Empty;

        [Display(Name = "RFQ Date")]
        public DateTime RFQDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Quotation Deadline is required")]
        [Display(Name = "Quotation Deadline")]
        public DateTime QuotationDeadline { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Sent";
        // Status: Sent, QuotationReceived, Closed

        [StringLength(250)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "Requisition is required")]
        [Display(Name = "Requisition")]
        public int RequisitionId { get; set; }

        [Required(ErrorMessage = "Created By is required")]
        [Display(Name = "Created By User")]
        public int CreatedById { get; set; }

        // Navigation Properties
        public virtual PurchaseRequisition? Requisition { get; set; }
        public virtual IdentityUser<int>? CreatedByUser { get; set; }
        public virtual ICollection<SupplierQuotation> SupplierQuotations { get; set; } = new List<SupplierQuotation>();
        //public virtual User? CreatedByUser { get; set; }
        //public virtual ICollection<SupplierQuotation> SupplierQuotations { get; set; }
        //    = new List<SupplierQuotation>();
    }
}
