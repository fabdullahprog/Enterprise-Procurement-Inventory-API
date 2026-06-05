using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.SupplierQuotation
{
    public class SupplierQuotationRequestDto
    {
        [Required]
        public int RFQId { get; set; }

        [Required]
        public int SupplierId { get; set; }

        public int? CurrencyId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int DeliveryDays { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        [MinLength(1)]
        public List<QuotationItemDto> Items { get; set; } = new();
    }

    public class QuotationItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int OfferedQuantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }
}