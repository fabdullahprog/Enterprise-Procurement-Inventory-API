using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperShop_Management.Models.Entities
{
    /// <summary>
    /// CS Item - represents one item in a Comparative Statement
    /// NOTE: This entity supports both old and new structure for backward compatibility
    /// </summary>
    public class CSItem : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "CS is required")]
        [Display(Name = "Comparative Statement")]
        public int ComparativeStatementId { get; set; }

        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [StringLength(200)]
        [Display(Name = "Item Name")]
        public string? ItemName { get; set; }

        // ========== OLD STRUCTURE (for backward compatibility) ==========
        [Display(Name = "Selected Quotation Item")]
        public int? SelectedQuotationItemId { get; set; }
        // Will be deprecated in favor of CSSupplierRow

        [Display(Name = "Is Selected")]
        public bool IsSelected { get; set; } = false;
        // Will be deprecated in favor of CSSupplierRow.IsSelected

        [StringLength(250)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        // Navigation Properties
        public virtual ComparativeStatement? ComparativeStatement { get; set; }
        public virtual Product? Product { get; set; }
        
        // OLD: Direct link to selected quotation item
        public virtual QuotationItem? SelectedQuotationItem { get; set; }
        
        // NEW: Multiple supplier rows (one per supplier quote for this item)
        public virtual ICollection<CSSupplierRow> SupplierRows { get; set; } = new List<CSSupplierRow>();
    }
}
