using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.StoreIssue
{
    public class StoreIssueRequestDto
    {
        [Required(ErrorMessage = "Requisition ID is required")]
        public int RequisitionId { get; set; }

        [Required(ErrorMessage = "Issued Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Issued Quantity cannot be negative")]
        public int IssuedQty { get; set; }

        [Required]
        [StringLength(20)]
        public string IssueType { get; set; } = "full"; // full, partial

        [StringLength(500)]
        public string? Remarks { get; set; }

        public int? WarehouseId { get; set; }
        public int? FloorId { get; set; }
        public int? ZoneId { get; set; }
        public int? AisleId { get; set; }
        public int? RackId { get; set; }
        public int? ShelfId { get; set; }
        public int? BinId { get; set; }
        public int? BatchId { get; set; }
    }

    public class ForwardToPurchaseDto
    {
        [Required(ErrorMessage = "Requisition ID is required")]
        public int RequisitionId { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}
