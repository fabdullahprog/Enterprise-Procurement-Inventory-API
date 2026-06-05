using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.PurchaseRequisition
{
    public class RequisitionRequestDto
    {
        [Required]
        public DateTime RequiredByDate { get; set; }

        [StringLength(250)]
        public string? Notes { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        // Items list
        [Required]
        [MinLength(1)]
        public List<RequisitionItemDto> Items { get; set; } = new();

        // Optional list of source employee requisition IDs that were merged to create this PR
        public List<int>? SourceRequisitionIds { get; set; }
    }

    public class RequisitionItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int RequiredQuantity { get; set; }

        [StringLength(250)]
        public string? Remarks { get; set; }
    }
}