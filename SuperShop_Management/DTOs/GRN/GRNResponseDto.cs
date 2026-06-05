namespace SuperShop_Management.DTOs.GRN
{
    public class GRNResponseDto
    {
        public int Id { get; set; }
        public string GRNNumber { get; set; } = string.Empty;
        public DateTime ReceivedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ReceiveType { get; set; }
        public string? VehicleNumber { get; set; }
        public string? DeliveryPersonName { get; set; }
        public string? Notes { get; set; }

        public int PurchaseOrderId { get; set; }
        public string? PONumber { get; set; }
        public string? SupplierName { get; set; }
        public int ReceivedQuantity { get; set; }
        public int ReceivedById { get; set; }
        public string? ReceivedByName { get; set; }
        
        public int? StoreApprovedById { get; set; }
        public string? StoreApprovedByName { get; set; }
        public DateTime? StoreApprovedAt { get; set; }

        public List<GRNItemResponseDto> Items { get; set; } = new();
    }

    public class GRNItemResponseDto
    {
        public int Id { get; set; }
        public int POItemId { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int OrderedQuantity { get; set; }
        public int ReceivedQuantity { get; set; }
        public int AcceptedQuantity { get; set; }
        public int RejectedQuantity { get; set; }
        public string? Condition { get; set; }
        public string? Remarks { get; set; }
    }
}