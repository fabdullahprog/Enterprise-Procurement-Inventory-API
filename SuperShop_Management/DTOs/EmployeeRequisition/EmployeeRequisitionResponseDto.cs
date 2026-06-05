namespace SuperShop_Management.DTOs.EmployeeRequisition
{
    // Item Response DTO
    public class EmployeeRequisitionItemResponseDto
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int RequiredQty { get; set; }
        public int CurrentStock { get; set; }
        public string? Remarks { get; set; }
    }

    // Requisition Response DTO (Master-Details)
    public class EmployeeRequisitionResponseDto
    {
        public int Id { get; set; }
        public string RequisitionNo { get; set; } = string.Empty;
        public int RequestedBy { get; set; }
        public string? RequestedByName { get; set; }
        public string? RequestedByEmail { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? RequiredByDate { get; set; }
        public string? Notes { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ForwardedAt { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Items Collection
        public List<EmployeeRequisitionItemResponseDto> Items { get; set; } = new();
        
        // Permissions
        public bool CanSubmit { get; set; }
        public bool CanRevise { get; set; }
        public bool CanApprove { get; set; }
        public bool CanCancel { get; set; }
    }
}
