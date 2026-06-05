using Microsoft.AspNetCore.Identity;
using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace SuperShop_Management.Models.Entities
{
    public class PurchaseOrder : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "PO Number")]
        public string PONumber { get; set; } = string.Empty;

        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Expected Delivery Date is required")]
        [Display(Name = "Expected Delivery Date")]
        public DateTime ExpectedDeliveryDate { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Delivery Type")]
        public string DeliveryType { get; set; } = "full";
        // full, partial

        [Display(Name = "Status")]
        public string Status { get; set; } = "Draft";
        // Draft, PendingMDApproval, ApprovedByMD, Rejected, Sent,
        // PartiallyReceived, Received, Cancelled

        [Display(Name = "Total BDT Amount")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalBDTAmount { get; set; }
        // Sum of all POItems BDT amounts

        [StringLength(250)]
        [Display(Name = "Delivery Address")]
        public string? DeliveryAddress { get; set; }

        [StringLength(50)]
        [Display(Name = "Payment Terms")]
        public string? PaymentTerms { get; set; }

        [StringLength(250)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Approved At")]
        public DateTime? ApprovedAt { get; set; }

        [Display(Name = "MD Approved By")]
        public int? MDApprovedById { get; set; }

        [Display(Name = "MD Approved At")]
        public DateTime? MDApprovedAt { get; set; }

        [Display(Name = "Sent At")]
        public DateTime? SentAt { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "Supplier is required")]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Comparative Statement is required")]
        [Display(Name = "Comparative Statement")]
        public int ComparativeStatementId { get; set; }

        [Required(ErrorMessage = "Created By is required")]
        [Display(Name = "Created By")]
        public int CreatedById { get; set; }

        [Display(Name = "Approved By")]
        public int? ApprovedById { get; set; }

        // Navigation Properties
        public virtual Supplier? Supplier { get; set; }
        public virtual ComparativeStatement? ComparativeStatement { get; set; }
        public virtual IdentityUser<int>? CreatedByUser { get; set; }
        public virtual IdentityUser<int>? ApprovedBy { get; set; }
        public virtual IdentityUser<int>? MDApprovedBy { get; set; }

        public virtual ICollection<POItem> POItems { get; set; } = new List<POItem>();
        public virtual ICollection<GRN> GRNs { get; set; } = new List<GRN>();
    }
}