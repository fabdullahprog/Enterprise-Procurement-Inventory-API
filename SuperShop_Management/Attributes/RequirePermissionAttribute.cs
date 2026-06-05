using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SuperShop_Management.Attributes
{
    /// <summary>
    /// Authorization attribute that checks if the user has a specific permission claim.
    /// Usage: [RequirePermission("requisition:approve")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _permission;

        public RequirePermissionAttribute(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                throw new ArgumentException("Permission cannot be null or empty", nameof(permission));

            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // Check if user is authenticated
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "User not authenticated" });
                return;
            }

            // Admin role bypasses permission checks
            if (user.IsInRole("Admin"))
            {
                return;
            }

            // Check if user has the required permission
            var hasPermission = user.Claims.Any(c =>
                string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(c.Value, _permission, StringComparison.OrdinalIgnoreCase));

            if (!hasPermission)
            {
                context.Result = new ObjectResult(new { message = $"Permission denied. Required: {_permission}" })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}
