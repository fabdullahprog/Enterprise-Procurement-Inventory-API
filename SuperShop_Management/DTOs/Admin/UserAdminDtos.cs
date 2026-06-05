namespace SuperShop_Management.DTOs.Admin
{
    public class AdminUserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
    }

    public class SetUserDepartmentDto
    {
        public int? DepartmentId { get; set; }
    }

    public class SetUserRolesDto
    {
        public List<string> Roles { get; set; } = new();
    }
}

