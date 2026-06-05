namespace SuperShop_Management.DTOs.UnitSet
{
    public class UnitSetResponseDto
    {
        public int UnitSetId { get; set; }
        public string NameOfUnitSet { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}