using System.Collections.Generic;

namespace SuperShop_Management.DTOs.GRN
{
    public class DirectReceiveDto
    {
        public int PurchaseOrderId { get; set; }
        public int? WarehouseId { get; set; }
        public int? FloorId { get; set; }
        public int? ZoneId { get; set; }
        public int? AisleId { get; set; }
        public int? RackId { get; set; }
        public int? ShelfId { get; set; }
        public int? BinId { get; set; }
        
        public string? VehicleNumber { get; set; }
        public string? DeliveryPersonName { get; set; }
        public string? Notes { get; set; }

        public List<DirectReceiveItemDto> Items { get; set; } = new();
    }

    public class DirectReceiveItemDto
    {
        public int POItemId { get; set; }
        public int ReceivedQuantity { get; set; }
        public int AcceptedQuantity { get; set; }
        public string? Condition { get; set; }
        public string? Remarks { get; set; }
    }
}
