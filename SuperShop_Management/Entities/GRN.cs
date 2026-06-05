using Microsoft.AspNetCore.Identity;
using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    public class GRN : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "GRN Number")]
        public string GRNNumber { get; set; } = string.Empty;

        [Display(Name = "Received Date")]
        public DateTime ReceivedDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        [Display(Name = "Receive Type")]
        public string ReceiveType { get; set; } = "full";
        // full, partial

        [Display(Name = "Status")]
        public string Status { get; set; } = "Draft";
        // Status: Draft, PendingStoreApproval, Approved, Rejected

        [Required(ErrorMessage = "Received Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        [Display(Name = "Received Quantity")]
        public int ReceivedQuantity { get; set; }

        //[StringLength(50)]
        //[Display(Name = "Batch Number")]
        //public string? BatchNumber { get; set; }

        //[Display(Name = "Expiry Date")]
        //public DateTime? ExpiryDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Vehicle Number")]
        public string? VehicleNumber { get; set; }

        [StringLength(100)]
        [Display(Name = "Delivery Person Name")]
        public string? DeliveryPersonName { get; set; }

        [StringLength(250)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Store Approved By")]
        public int? StoreApprovedById { get; set; }

        [Display(Name = "Store Approved At")]
        public DateTime? StoreApprovedAt { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "Purchase Order is required")]
        [Display(Name = "Purchase Order")]
        public int PurchaseOrderId { get; set; }

        [Required(ErrorMessage = "Received By is required")]
        [Display(Name = "Received By")]
        public int ReceivedById { get; set; }

        // Navigation Properties
        public virtual PurchaseOrder? PurchaseOrder { get; set; }
        public virtual IdentityUser<int>? ReceivedBy { get; set; }
        public virtual IdentityUser<int>? StoreApprovedBy { get; set; }
        public virtual ICollection<GRNItem> GRNItems { get; set; } = new List<GRNItem>();
        public virtual QualityCheck? QualityCheck { get; set; }     // 1-1 with QualityCheck
    }
}
