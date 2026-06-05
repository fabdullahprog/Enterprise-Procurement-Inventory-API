using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.PurchaseOrder
{
    public class PurchaseOrderRequestDto
    {
        [Required]
        public int ComparativeStatementId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public DateTime ExpectedDeliveryDate { get; set; }

        [StringLength(500)]
        public string? DeliveryAddress { get; set; }

        [StringLength(100)]
        public string? PaymentTerms { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Items: এগুলো CS-এর Selected Items থেকেই আসবে, কিন্তু এখন ইউজার PORate কাস্টমাইজ করতে পারবে।
        public List<POItemRequestDto> Items { get; set; } = new();
    }

    public class POItemRequestDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int OrderedQuantity { get; set; }

        [Required]
        public decimal SupplierRate { get; set; }

        [Required]
        public decimal PORate { get; set; }
    }
}