namespace SuperShop_Management.DTOs.Supplier
{
    public class SupplierResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? TradeLicenseNo { get; set; }
        public string? TINNo { get; set; }
        public string? BINNo { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNo { get; set; }
        public int? CurrencyId { get; set; }
        public string? CurrencyCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}