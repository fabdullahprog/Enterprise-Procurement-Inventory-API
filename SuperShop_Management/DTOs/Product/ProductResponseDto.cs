namespace SuperShop_Management.DTOs.Product
{
    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CurrentStock { get; set; }
        public bool IsPerishable { get; set; }
        public string? Description { get; set; }

        // Foreign Keys and related names
        public int ItemCategoryId { get; set; }
        public string? ItemCategoryName { get; set; }
        public int SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }
        public int BrandId { get; set; }
        public string? BrandName { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}