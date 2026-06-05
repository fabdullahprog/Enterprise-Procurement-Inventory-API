namespace SuperShop_Management.DTOs.Location
{
    public class RackResponseDto
    {
        public int RackId { get; set; }
        public string RackName { get; set; } = string.Empty;
        public string? Remarks { get; set; }

        public int? AisleId { get; set; }
        public string? AisleName { get; set; }
        public int? ZoneId { get; set; }
        public string? ZoneName { get; set; }
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
                if (!string.IsNullOrEmpty(ZoneName)) parts.Add(ZoneName);
                if (!string.IsNullOrEmpty(AisleName)) parts.Add(AisleName);
                parts.Add(RackName);

                return string.Join(" → ", parts.Where(p => !string.IsNullOrEmpty(p)));
            }
        }
    }
}