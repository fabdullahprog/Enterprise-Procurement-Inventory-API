using Microsoft.AspNetCore.Identity;
using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    public class QualityCheck : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "QC Number")]
        public string QCNumber { get; set; } = string.Empty;

        [Display(Name = "QC Date")]
        public DateTime QCDate { get; set; } = DateTime.Now;

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";
        // Status: Pending, Accepted, PartiallyAccepted, Rejected

        [Required(ErrorMessage = "Received Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        [Display(Name = "Received Quantity")]
        public int ReceivedQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Accepted Quantity cannot be negative")]
        [Display(Name = "Accepted Quantity")]
        public int AcceptedQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Rejected Quantity cannot be negative")]
        [Display(Name = "Rejected Quantity")]
        public int RejectedQuantity { get; set; }

        [StringLength(50)]
        [Display(Name = "Rejection Reason")]
        public string? RejectionReason { get; set; }
        // RejectionReason: Damaged, Expired, WrongProduct, QualityIssue

        [StringLength(250)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "GRN is required")]
        [Display(Name = "GRN")]
        public int GRNId { get; set; }

        [Required(ErrorMessage = "Inspected By is required")]
        [Display(Name = "Inspected By")]
        public int InspectedById { get; set; }

        // Navigation Properties
        public virtual GRN? GRN { get; set; }
        public virtual IdentityUser<int>? InspectedBy { get; set; }
        public virtual ICollection<QCItem> QCItems { get; set; } = new List<QCItem>();
    }
}
