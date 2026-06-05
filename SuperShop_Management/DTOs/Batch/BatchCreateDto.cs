using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.Batch
{
    public class BatchCreateDto
    {
        [Required]
        [StringLength(50)]
        public string BatchNumber { get; set; } = string.Empty;

        [Required]
        public DateTime ManufacturingDate { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ReceivedQuantity { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [Required]
        public int GRNId { get; set; }
    }

    public class BatchUpdateDto
    {
        [Required]
        [StringLength(50)]
        public string BatchNumber { get; set; } = string.Empty;

        [Required]
        public DateTime ManufacturingDate { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;
    }

    public class BatchResponseDto
    {
        public int Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ManufacturingDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int ReceivedQuantity { get; set; }
        public int RemainingQuantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int GRNId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}