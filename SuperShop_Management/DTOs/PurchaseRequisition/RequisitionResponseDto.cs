namespace SuperShop_Management.DTOs.PurchaseRequisition
{
    public class RequisitionResponseDto
    {
        public int Id { get; set; }
        public string RequisitionNumber { get; set; } = string.Empty;
        public DateTime RequisitionDate { get; set; }
        public DateTime RequiredByDate { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ApprovedAt { get; set; }

        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }

        public int RequestedById { get; set; }
        public string? RequestedByEmail { get; set; }
        public string? RequestedByName { get; set; }

        public int? ApprovedById { get; set; }
        public string? ApprovedByEmail { get; set; }
        public string? ApprovedByName { get; set; }

        public List<RequisitionItemResponseDto> Items { get; set; } = new();

        // Helper properties for frontend
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanCancel { get; set; }
    }

    public class RequisitionItemResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int RequiredQuantity { get; set; }
        public string? Remarks { get; set; }
    }
}