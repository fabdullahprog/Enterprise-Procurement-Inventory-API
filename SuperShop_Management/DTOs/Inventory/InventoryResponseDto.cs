namespace SuperShop_Management.DTOs.Inventory
{
    public class InventoryResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int BatchId { get; set; }
        public string? BatchNumber { get; set; }
        public int AvailableQuantity { get; set; }
        public int GrnQuantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int MinQuantity { get; set; }
        public int MaxQuantity { get; set; }
        public DateTime LastUpdated { get; set; }
        
        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public int? FloorId { get; set; }
        public string? FloorName { get; set; }
        public int? ZoneId { get; set; }
        public string? ZoneName { get; set; }
        public int? AisleId { get; set; }
        public string? AisleName { get; set; }
        public int? RackId { get; set; }
        public string? RackName { get; set; }
        public int? ShelfId { get; set; }
        public string? ShelfName { get; set; }
        public int? BinId { get; set; }
        public string? BinName { get; set; }
        public string? LocationLevel { get; set; }
    }
}