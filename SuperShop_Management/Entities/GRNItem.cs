using SuperShop_Management.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Entities
{
    public class GRNItem : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ReceivedQuantity { get; set; }

        [Range(0, int.MaxValue)]
        public int AcceptedQuantity { get; set; }   // QC-তে নির্ধারিত হবে

        [Range(0, int.MaxValue)]
        public int RejectedQuantity { get; set; }   // QC-তে নির্ধারিত হবে

        [Required]
        [StringLength(20)]
        [Display(Name = "Condition")]
        public string Condition { get; set; } = "good";
        // good, damaged, partial

        [StringLength(500)]
        public string? Remarks { get; set; }

        // Foreign Keys
        [Required]
        public int GRNId { get; set; }

        [Required]
        public int POItemId { get; set; }   // PurchaseOrder-এর আইটেমের সাথে সম্পর্ক

        // Navigation
        public virtual GRN? GRN { get; set; }
        public virtual POItem? POItem { get; set; }
    }
}