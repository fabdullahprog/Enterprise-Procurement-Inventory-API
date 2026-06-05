using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    public class Batch : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Batch Number is required")]
        [StringLength(50)]
        [Display(Name = "Batch Number")]
        public string BatchNumber { get; set; } = string.Empty;
        // যেমন: BM-2026-001

        [Required(ErrorMessage = "Manufacturing Date is required")]
        [Display(Name = "Manufacturing Date")]
        public DateTime ManufacturingDate { get; set; }

        [Required(ErrorMessage = "Expiry Date is required")]
        [Display(Name = "Expiry Date")]
        public DateTime ExpiryDate { get; set; }

        [Required(ErrorMessage = "Received Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        [Display(Name = "Received Quantity")]
        public int ReceivedQuantity { get; set; }

        [Display(Name = "Remaining Quantity")]
        public int RemainingQuantity { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Active";
        // Status: Active, Expired, Recalled, Finished

        // Foreign Keys
        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Supplier is required")]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "GRN is required")]
        [Display(Name = "GRN")]
        public int GRNId { get; set; }

        // Navigation Properties
        public virtual Product? Product { get; set; }
        public virtual Supplier? Supplier { get; set; }
        public virtual GRN? GRN { get; set; }

        public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    }
}
