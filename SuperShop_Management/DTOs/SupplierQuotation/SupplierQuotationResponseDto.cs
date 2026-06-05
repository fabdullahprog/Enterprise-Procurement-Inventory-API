namespace SuperShop_Management.DTOs.SupplierQuotation
{
    public class SupplierQuotationResponseDto
    {
        public int Id { get; set; }
        public string QuotationNumber { get; set; } = string.Empty;
        public DateTime QuotationDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalBDTAmount { get; set; }
        public int DeliveryDays { get; set; }
        public string? Notes { get; set; }

        public int RFQId { get; set; }
        public string? RFQNumber { get; set; }
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int? CurrencyId { get; set; }
        public string? CurrencyCode { get; set; }

        public List<QuotationItemResponseDto> Items { get; set; } = new();
    }

    public class QuotationItemResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int OfferedQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal BDTAmount { get; set; }
    }
}