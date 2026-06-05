using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.QualityCheck
{
    public class QCCreateDto
    {
        [Required]
        public int GRNId { get; set; }

        [Required]
        [MinLength(1)]
        public List<QCItemDto> Items { get; set; } = new();

        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    public class QCItemDto
    {
        [Required]
        public int GRNItemId { get; set; }

        [Required]
        public int AcceptedQuantity { get; set; }

        [Required]
        public int RejectedQuantity { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}