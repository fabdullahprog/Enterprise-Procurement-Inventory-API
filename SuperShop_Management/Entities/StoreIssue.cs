using Microsoft.AspNetCore.Identity;
using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    /// <summary>
    /// MODULE 2: Store Issue - when store_head issues products or forwards to purchase
    /// </summary>
    public class StoreIssue : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Requisition is required")]
        [Display(Name = "Requisition")]
        public int RequisitionId { get; set; }

        [Required(ErrorMessage = "Issued Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Issued Quantity cannot be negative")]
        [Display(Name = "Issued Quantity")]
        public int IssuedQty { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Issue Type")]
        public string IssueType { get; set; } = "full";
        // full, partial

        [Required(ErrorMessage = "Issued By is required")]
        [Display(Name = "Issued By (Store Head)")]
        public int IssuedById { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "issued";
        // issued, forwarded_to_purchase

        [StringLength(500)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Issued At")]
        public DateTime IssuedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual Requisition? Requisition { get; set; }
        public virtual IdentityUser<int>? IssuedBy { get; set; }
    }
}
