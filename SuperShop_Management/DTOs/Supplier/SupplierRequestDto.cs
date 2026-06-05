using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.Supplier
{
    public class SupplierRequestDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? TradeLicenseNo { get; set; }

        [StringLength(50)]
        public string? TINNo { get; set; }

        [StringLength(50)]
        public string? BINNo { get; set; }

        [StringLength(200)]
        public string? BankName { get; set; }

        [StringLength(50)]
        public string? BankAccountNo { get; set; }

        public int? CurrencyId { get; set; }
    }
}