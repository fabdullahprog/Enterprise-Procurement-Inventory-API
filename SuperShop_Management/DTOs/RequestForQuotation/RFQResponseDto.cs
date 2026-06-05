namespace SuperShop_Management.DTOs.RequestForQuotation
{
    public class RFQResponseDto
    {
        public int Id { get; set; }
        public string RFQNumber { get; set; } = string.Empty;
        public DateTime RFQDate { get; set; }
        public DateTime QuotationDeadline { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int RequisitionId { get; set; }
        public string? RequisitionNumber { get; set; }
        public int CreatedById { get; set; }
        public string? CreatedByEmail { get; set; }

        // Items from the linked requisition
        public List<RFQItemDto> Items { get; set; } = new();

        public bool HasCS { get; set; }
        public int? CSId { get; set; }
        public List<RFQSupplierDto> Suppliers { get; set; } = new();
    }

    public class RFQSupplierDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
    }

    public class RFQItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int RequiredQuantity { get; set; }
    }
}