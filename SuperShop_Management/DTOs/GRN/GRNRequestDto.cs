using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.GRN
{
    public class GRNRequestDto
    {
        [Required]
        public int PurchaseOrderId { get; set; }

        [StringLength(20)]
        public string? ReceiveType { get; set; } = "full"; // full, partial

        [StringLength(50)]
        public string? VehicleNumber { get; set; }

        [StringLength(100)]
        public string? DeliveryPersonName { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        [MinLength(1)]
        public List<GRNItemDto> Items { get; set; } = new();
    }

    public class GRNItemDto
    {
        [Required]
        public int POItemId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ReceivedQuantity { get; set; }

        [StringLength(20)]
        public string? Condition { get; set; } = "good"; // good, damaged, partial

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}