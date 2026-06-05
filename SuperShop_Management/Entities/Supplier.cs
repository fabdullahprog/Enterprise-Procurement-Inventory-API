using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    public class Supplier : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Supplier Name is required")]
        [StringLength(200, ErrorMessage = "Supplier Name cannot exceed 200 characters")]
        [Display(Name = "Supplier Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact Person is required")]
        [StringLength(100)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        [Display(Name = "Phone")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(250)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [StringLength(50)]
        [Display(Name = "Trade License No")]
        public string? TradeLicenseNo { get; set; }

        [StringLength(50)]
        [Display(Name = "TIN No")]
        public string? TINNo { get; set; }

        [StringLength(50)]
        [Display(Name = "BIN No")]
        public string? BINNo { get; set; }

        [StringLength(200)]
        [Display(Name = "Bank Name")]
        public string? BankName { get; set; }

        [StringLength(50)]
        [Display(Name = "Bank Account No")]
        public string? BankAccountNo { get; set; }

        // Navigation Properties

        // Foreign Key
        [Display(Name = "Currency")]
        public int? CurrencyId { get; set; }

        // Navigation Property
        public virtual Currency? Currency { get; set; }
        //public virtual ICollection<SupplierQuotation> SupplierQuotations { get; set; }
        //    = new List<SupplierQuotation>();
        //public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; }
        //    = new List<PurchaseOrder>();
        //public virtual ICollection<AccountsPayable> AccountsPayables { get; set; }
        //    = new List<AccountsPayable>();
    }
}
