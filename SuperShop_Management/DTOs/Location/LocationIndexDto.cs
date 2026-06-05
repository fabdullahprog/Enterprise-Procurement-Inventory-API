namespace SuperShop_Management.DTOs.Location
{
    // 1. Tree Node DTO
    public class LocationTreeNodeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ColorClass { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string? ParentId { get; set; }
        public string? Remarks { get; set; }
        public string FullPath { get; set; } = string.Empty;
        public List<LocationTreeNodeDto> Children { get; set; } = new();
    }

    // 2. Tab List Item DTOs (To keep ChildCount, Path, etc.)
    public class WarehouseListItemDto
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Remarks { get; set; }
        public int ChildCount { get; set; }
    }

    public class FloorListItemDto
    {
        public int FloorId { get; set; }
        public string FloorName { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public int ChildCount { get; set; }
    }

    public class ZoneListItemDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public int ChildCount { get; set; }
    }

    public class AisleListItemDto
    {
        public int AisleId { get; set; }
        public string AisleName { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public int ChildCount { get; set; }
    }

    public class RackListItemDto
    {
        public int RackId { get; set; }
        public string RackName { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public int ChildCount { get; set; }
    }

    public class ShelfListItemDto
    {
        public int ShelfId { get; set; }
        public string ShelfName { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public int ChildCount { get; set; }
    }

    public class BinListItemDto
    {
        public int BinId { get; set; }
        public string BinName { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }

    // 3. Master Master Index DTO
    public class LocationIndexDto
    {
        public List<LocationTreeNodeDto> Tree { get; set; } = new();
        public int TotalWarehouses { get; set; }
        public int TotalFloors { get; set; }
        public int TotalZones { get; set; }
        public int TotalAisles { get; set; }
        public int TotalRacks { get; set; }
        public int TotalShelves { get; set; }
        public int TotalBins { get; set; }

        public List<WarehouseListItemDto> Warehouses { get; set; } = new();
        public List<FloorListItemDto> Floors { get; set; } = new();
        public List<ZoneListItemDto> Zones { get; set; } = new();
        public List<AisleListItemDto> Aisles { get; set; } = new();
        public List<RackListItemDto> Racks { get; set; } = new();
        public List<ShelfListItemDto> Shelves { get; set; } = new();
        public List<BinListItemDto> Bins { get; set; } = new();
    }
}