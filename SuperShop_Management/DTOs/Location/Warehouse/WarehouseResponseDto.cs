using System.ComponentModel.DataAnnotations;
namespace SuperShop_Management.DTOs.Location
{
    public class WarehouseResponseDto
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }

        public string LocationPath
        {
            get
            {
                // ওয়ারহাউস নিজেই রুট, তাই এর নামই তার পাথ
                return WarehouseName;
            }
        }
    }
}
