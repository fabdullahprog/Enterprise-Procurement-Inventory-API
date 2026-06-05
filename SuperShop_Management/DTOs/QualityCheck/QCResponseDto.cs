namespace SuperShop_Management.DTOs.QualityCheck
{
    public class QCResponseDto
    {
        public int Id { get; set; }
        public string QCNumber { get; set; } = string.Empty;
        public DateTime QCDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }

        public int GRNId { get; set; }
        public string? GRNNumber { get; set; }
        public int InspectedById { get; set; }
        public string? InspectedByName { get; set; }

        public List<QCItemResponseDto> Items { get; set; } = new();
    }

    public class QCItemResponseDto
    {
        public int Id { get; set; }
        public int GRNItemId { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int ReceivedQuantity { get; set; }
        public int AcceptedQuantity { get; set; }
        public int RejectedQuantity { get; set; }
        public string? Remarks { get; set; }
    }
}