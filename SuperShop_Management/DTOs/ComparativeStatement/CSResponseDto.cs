namespace SuperShop_Management.DTOs.ComparativeStatement
{
    public class CSResponseDto
    {
        public int Id { get; set; }
        public string CSNumber { get; set; } = string.Empty;
        public DateTime CSDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalBDTAmount { get; set; }
        public string? Remarks { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public int RFQId { get; set; }
        public string? RFQNumber { get; set; }
        public int CreatedById { get; set; }
        public string? CreatedByEmail { get; set; }
        public int? ApprovedById { get; set; }

        public List<CSItemResponseDto> Items { get; set; } = new();
        public List<CSSupplierRowDto> SupplierRows { get; set; } = new();
    }

    public class CSItemResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int SelectedQuotationItemId { get; set; }
        public decimal UnitPrice { get; set; }
        public int OfferedQuantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal BDTAmount { get; set; }
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? Remarks { get; set; }
        public bool IsSelected { get; set; }
    }

    public class CSSupplierRowDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int RowNumber { get; set; }
        public int OfferedQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal BDTAmount { get; set; }
        public int DeliveryDays { get; set; }
        public bool IsSelected { get; set; }
        public string? SelectionRemarks { get; set; }
    }
}