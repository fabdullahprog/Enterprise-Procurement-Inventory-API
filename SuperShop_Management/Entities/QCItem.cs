using SuperShop_Management.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Entities
{
    public class QCItem : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int AcceptedQuantity { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int RejectedQuantity { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        // Foreign Keys
        [Required]
        public int QualityCheckId { get; set; }

        [Required]
        public int GRNItemId { get; set; }

        // Navigation
        public virtual QualityCheck? QualityCheck { get; set; }
        public virtual GRNItem? GRNItem { get; set; }
    }
}