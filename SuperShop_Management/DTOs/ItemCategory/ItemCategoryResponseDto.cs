namespace SuperShop_Management.DTOs.ItemCategory
{
    public class ItemCategoryResponseDto
    {
        public int ItemCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryDescription { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}
