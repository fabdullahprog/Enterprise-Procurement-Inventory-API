using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.Product
{
    public class ProductRequestDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Barcode { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        public int CurrentStock { get; set; } = 0;
        public bool IsPerishable { get; set; } = false;

        [StringLength(250)]
        public string? Description { get; set; }

        [Required]
        public int ItemCategoryId { get; set; }

        [Required]
        public int SubCategoryId { get; set; }

        [Required]
        public int BrandId { get; set; }

        [Required]
        public int UnitId { get; set; }
    }
}