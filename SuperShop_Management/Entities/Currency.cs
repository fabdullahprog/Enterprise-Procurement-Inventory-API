using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperShop_Management.Models.Entities
{
    public class Currency : BaseEntity
    {
        [Key]
        public int CurrencyId { get; set; }

        [Required(ErrorMessage = "Currency Code is required")]
        [StringLength(3)]
        [Display(Name = "ISO Code")]
        public string Code { get; set; } = string.Empty;
        // যেমন: BDT, USD, EUR

        [Required(ErrorMessage = "Symbol is required")]
        [StringLength(5)]
        [Display(Name = "Symbol")]
        public string Symbol { get; set; } = string.Empty;
        // যেমন: ৳, $, €

        [Required(ErrorMessage = "Currency Name is required")]
        [Display(Name = "Display Name")]
        public string Name { get; set; } = string.Empty;
        // যেমন: Bangladeshi Taka, US Dollar

        [Column(TypeName = "decimal(18, 6)")]
        [Display(Name = "Exchange Rate")]
        public decimal ExchangeRate { get; set; }
        // Base Currency এর সাথে rate

        [Display(Name = "Is Base Currency")]
        public bool IsBaseCurrency { get; set; } = false;
        // BDT = true হবে
    }
}
