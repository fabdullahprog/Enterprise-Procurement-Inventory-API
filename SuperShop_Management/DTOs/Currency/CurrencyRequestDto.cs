using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.Currency
{
    public class CurrencyRequestDto
    {
        [Required]
        [StringLength(3)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(5)]
        public string Symbol { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public decimal ExchangeRate { get; set; } = 1;
        public bool IsBaseCurrency { get; set; } = false;
    }
}