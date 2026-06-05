using Microsoft.AspNetCore.Identity;
using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
   
    public class Requisition : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Requisition Number")]
        public string RequisitionNo { get; set; } = string.Empty;
        // Format: REQ-YYYY-XXXX

        [Required(ErrorMessage = "Requested By is required")]
        [Display(Name = "Requested By (Employee)")]
        public int RequestedBy { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "draft";
        // Status: draft, submitted, approved, forwarded, rejected

        [Display(Name = "Required By Date")]
        public DateTime? RequiredByDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Submitted At")]
        public DateTime? SubmittedAt { get; set; }

        [Display(Name = "Approved At")]
        public DateTime? ApprovedAt { get; set; }

        [Display(Name = "Forwarded At")]
        public DateTime? ForwardedAt { get; set; }

        // Navigation Properties
        public virtual IdentityUser<int>? RequestedByUser { get; set; }
        public virtual Department? Department { get; set; }

        // Items Collection (Master-Details)
        public virtual ICollection<EmployeeRequisitionItem> Items { get; set; } = new List<EmployeeRequisitionItem>();

        // Store Issues
        public virtual ICollection<StoreIssue> StoreIssues { get; set; } = new List<StoreIssue>();
    }
}
