using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.EmployeeRequisition
{
    // Item DTO for request
    public class EmployeeRequisitionItemDto
    {
        [Required(ErrorMessage = "Item ID is required")]
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Item Name is required")]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int RequiredQty { get; set; }

        public int CurrentStock { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    // Create Requisition Request (Master-Details)
    public class EmployeeRequisitionRequestDto
    {
        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }

        public DateTime? RequiredByDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "At least one item is required")]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<EmployeeRequisitionItemDto> Items { get; set; } = new();
    }

    // Revise DTO (for Dept Head)
    public class EmployeeRequisitionReviseDto
    {
        public DateTime? RequiredByDate { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }

        public List<EmployeeRequisitionItemDto>? Items { get; set; }
    }
}
