namespace SuperShop_Management.DTOs.Admin
{
    public class RolePermissionsDto
    {
        public string Role { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
        public List<string> AllPermissions { get; set; } = new();
    }

    public class SetRolePermissionsDto
    {
        public List<string> Permissions { get; set; } = new();
    }
}

