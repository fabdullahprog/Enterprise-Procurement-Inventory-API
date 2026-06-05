namespace SuperShop_Management.DTOs.Unit
{
    public class UnitResponseDto
    {
        public int UnitId { get; set; }
        public string NameOfUnit { get; set; } = string.Empty;
        public int UnitSetId { get; set; }
        public string? UnitSetName { get; set; }
        public double UnitFactor { get; set; }
        public bool IsBaseUnit { get; set; }
        public string? Description { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}