using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.RequestForQuotation
{
    public class RFQRequestDto
    {
        [Required]
        public DateTime QuotationDeadline { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public int RequisitionId { get; set; }
    }

    public class SendRFQDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one supplier must be selected")]
        public List<int> SupplierIds { get; set; } = new();
    }
}