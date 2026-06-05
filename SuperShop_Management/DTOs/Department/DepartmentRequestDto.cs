using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.Department
{
    public class DepartmentRequestDto
    {
        [Required(ErrorMessage = "Department Code is required")]
        [StringLength(10)]
        public string DepartmentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department Name is required")]
        [StringLength(100)]
        public string DepartmentName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? DepartmentEmail { get; set; }

        [Phone]
        [StringLength(20)]
        public string? DepartmentPhone { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public bool CanRequestItem { get; set; } = false;
        public bool CanIssueItem { get; set; } = false;
    }
}