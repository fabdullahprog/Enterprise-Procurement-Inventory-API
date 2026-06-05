namespace SuperShop_Management.DTOs.StoreIssue
{
    public class StoreIssueResponseDto
    {
        public int Id { get; set; }
        public int RequisitionId { get; set; }
        public string? RequisitionNo { get; set; }
        public string? ItemName { get; set; }
        public int RequiredQty { get; set; }
        public int IssuedQty { get; set; }
        public string IssueType { get; set; } = string.Empty;
        public int IssuedById { get; set; }
        public string? IssuedByName { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public DateTime IssuedAt { get; set; }
    }
}
