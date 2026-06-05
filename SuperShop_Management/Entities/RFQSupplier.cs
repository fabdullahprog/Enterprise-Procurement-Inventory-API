using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    /// <summary>
    /// MODULE 3: RFQ Suppliers - tracks which suppliers an RFQ was sent to
    /// </summary>
    public class RFQSupplier : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "RFQ is required")]
        [Display(Name = "RFQ")]
        public int RFQId { get; set; }

        [Required(ErrorMessage = "Supplier is required")]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [Display(Name = "Sent At")]
        public DateTime SentAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual RequestForQuotation? RFQ { get; set; }
        public virtual Supplier? Supplier { get; set; }
    }
}
