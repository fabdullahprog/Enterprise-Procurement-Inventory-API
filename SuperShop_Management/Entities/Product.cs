using SuperShop_Management.Entities;
using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.Models.Entities
{
    public class Product : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(200, ErrorMessage = "Product Name cannot exceed 200 characters")]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Barcode is required")]
        [StringLength(50)]
        [Display(Name = "Barcode")]
        public string Barcode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Current Stock is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        [Display(Name = "Current Stock")]
        public int CurrentStock { get; set; }

        //[Required(ErrorMessage = "Reorder Level is required")]
        //[Range(0, int.MaxValue, ErrorMessage = "Reorder Level cannot be negative")]
        //[Display(Name = "Reorder Level")]
        //public int ReorderLevel { get; set; }

        [Display(Name = "Is Perishable")]
        public bool IsPerishable { get; set; } = false;

        [StringLength(250)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int ItemCategoryId { get; set; }

        [Required(ErrorMessage = "Sub Category is required")]
        [Display(Name = "Sub Category")]
        public int SubCategoryId { get; set; }

        [Required(ErrorMessage = "Brand is required")]
        [Display(Name = "Brand")]
        public int BrandId { get; set; }

        [Required(ErrorMessage = "Unit is required")]
        [Display(Name = "Unit")]
        public int UnitId { get; set; }

        // Navigation Properties
        public virtual ItemCategory? ItemCategory { get; set; }
        public virtual SubCategory? SubCategory { get; set; }
        public virtual Brand? Brand { get; set; }
        public virtual Unit? Unit { get; set; }
        public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();
        public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        //public virtual ICollection<PurchaseRequisition> Requisitions { get; set; }
        //    = new List<PurchaseRequisition>();
        //public virtual ICollection<SaleItem> SaleItems { get; set; }
        //    = new List<SaleItem>();

        //// Navigation Properties এ add করো:
        //public virtual ICollection<Batch> Batches { get; set; }
        //    = new List<Batch>();
        //public virtual ICollection<Inventory> Inventories { get; set; }
        //    = new List<Inventory>();
    }
}
