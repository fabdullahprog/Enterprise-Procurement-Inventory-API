namespace SuperShop_Management.DTOs.SubCategory
{
    public class SubCategoryResponseDto
    {
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ItemCategoryId { get; set; }
        public string? ItemCategoryName { get; set; }  // For display
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}