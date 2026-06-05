using System.ComponentModel.DataAnnotations;

namespace SuperShop_Management.DTOs.PurchaseRequisition
{
    public class RejectReasonDto
    {
        [StringLength(500)]
        public string? Reason { get; set; }
    }
}
