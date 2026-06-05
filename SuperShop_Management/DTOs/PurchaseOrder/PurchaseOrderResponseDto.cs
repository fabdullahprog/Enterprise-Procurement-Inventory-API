namespace SuperShop_Management.DTOs.PurchaseOrder
{
    public class PurchaseOrderResponseDto
    {
        public int Id { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime ExpectedDeliveryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalBDTAmount { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? PaymentTerms { get; set; }
        public string? Notes { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int ComparativeStatementId { get; set; }
        public string? CSNumber { get; set; }

        public int CreatedById { get; set; }
        public string? CreatedByEmail { get; set; }
        public int? ApprovedById { get; set; }

        public List<POItemResponseDto> Items { get; set; } = new();
    }

    public class POItemResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int OrderedQuantity { get; set; }
        public decimal SupplierRate { get; set; }
        public decimal PORate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal BDTAmount { get; set; }
    }
}