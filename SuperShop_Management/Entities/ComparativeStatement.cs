using Microsoft.AspNetCore.Identity;
using SuperShop_Management.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperShop_Management.Models.Entities
{
    public class ComparativeStatement : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "CS Number")]
        public string CSNumber { get; set; } = string.Empty;

        [Display(Name = "CS Date")]
        public DateTime CSDate { get; set; } = DateTime.Now;

        [Display(Name = "Status")]
        public string Status { get; set; } = "Draft";
        // Draft, Reviewed, Approved, POCreated

        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Total BDT Amount")]
        public decimal TotalBDTAmount { get; set; }

        [StringLength(250)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Approved At")]
        public DateTime? ApprovedAt { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "RFQ is required")]
        [Display(Name = "RFQ")]
        public int RFQId { get; set; }

        [Required(ErrorMessage = "Created By is required")]
        [Display(Name = "Created By")]
        public int CreatedById { get; set; }

        [Display(Name = "Approved By")]
        public int? ApprovedById { get; set; }

        // Navigation Properties
        public virtual RequestForQuotation? RFQ { get; set; }
        public virtual IdentityUser<int>? CreatedByUser { get; set; }
        public virtual IdentityUser<int>? ApprovedBy { get; set; }
        public virtual ICollection<CSItem> CSItems { get; set; } = new List<CSItem>();
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }
}