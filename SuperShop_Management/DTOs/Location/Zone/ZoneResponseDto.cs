namespace SuperShop_Management.DTOs.Location
{
    public class ZoneResponseDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string? Remarks { get; set; }

        public int? FloorId { get; set; }
        public string? FloorName { get; set; }

        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }

        public string LocationPath
        {
            get
            {
                var parts = new List<string?>();
                if (!string.IsNullOrEmpty(WarehouseName)) parts.Add(WarehouseName);
                if (!string.IsNullOrEmpty(FloorName)) parts.Add(FloorName);
                parts.Add(ZoneName);

                return string.Join(" → ", parts.Where(p => !string.IsNullOrEmpty(p)));
            }
        }
    }
}