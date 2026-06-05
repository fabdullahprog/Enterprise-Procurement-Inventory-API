namespace SuperShop_Management.DTOs.Brand
{
    public class BrandResponseDto
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Country { get; set; }
        public string? Website { get; set; }
        public int SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}