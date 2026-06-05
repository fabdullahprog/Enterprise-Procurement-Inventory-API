namespace SuperShop_Management.DTOs.Currency
{
    public class CurrencyResponseDto
    {
        public int CurrencyId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }
        public bool IsBaseCurrency { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}