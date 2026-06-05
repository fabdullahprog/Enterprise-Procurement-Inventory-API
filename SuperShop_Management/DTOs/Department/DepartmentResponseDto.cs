namespace SuperShop_Management.DTOs.Department
{
    public class DepartmentResponseDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? DepartmentEmail { get; set; }
        public string? DepartmentPhone { get; set; }
        public string? Location { get; set; }
        public bool CanRequestItem { get; set; }
        public bool CanIssueItem { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}