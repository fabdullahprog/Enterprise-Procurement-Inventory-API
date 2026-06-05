using Microsoft.AspNetCore.Identity;
using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    public class PurchaseRequisition : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Requisition Number")]
        public string RequisitionNumber { get; set; } = string.Empty;

        [Display(Name = "Requisition Date")]
        public DateTime RequisitionDate { get; set; } = DateTime.Now;

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";
        // Pending, Approved, RFQSent, POCreated, Rejected

        [Display(Name = "Required By Date")]
        public DateTime RequiredByDate { get; set; }

        [StringLength(250)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Approved At")]
        public DateTime? ApprovedAt { get; set; }

        [Display(Name = "Source Requisition (if forwarded from store)")]
        public int? SourceRequisitionId { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Requested By is required")]
        [Display(Name = "Requested By")]
        public int RequestedById { get; set; }

        [Display(Name = "Approved By")]
        public int? ApprovedById { get; set; }

        // Navigation Properties
        public virtual Department? Department { get; set; }
        public virtual IdentityUser<int>? RequestedBy { get; set; }
        public virtual IdentityUser<int>? ApprovedBy { get; set; }

        public virtual ICollection<RequisitionItem> RequisitionItems { get; set; } = new List<RequisitionItem>();
        public virtual ICollection<RequestForQuotation> RequestForQuotations { get; set; } = new List<RequestForQuotation>();
    }
}