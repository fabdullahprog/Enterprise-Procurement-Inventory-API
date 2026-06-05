using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.ComparativeStatement
{
    public class CSRequestDto
    {
        [Required]
        public int RFQId { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    public class SelectSuppliersDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one supplier row must be selected")]
        public List<int> SelectedRowIds { get; set; } = new();

        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    public class CSItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int SelectedQuotationItemId { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}