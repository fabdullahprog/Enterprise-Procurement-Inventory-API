using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SuperShop_Management.DTOs.Auth;
using SuperShop_Management.Repositories.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SuperShop_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser<int>> _userManager;
        private readonly SignInManager<IdentityUser<int>> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IDepartmentRepository _departmentRepo;

        public AuthController(
            UserManager<IdentityUser<int>> userManager,
            SignInManager<IdentityUser<int>> signInManager,
            RoleManager<IdentityRole<int>> roleManager,
            IConfiguration configuration,
            IDepartmentRepository departmentRepo)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _departmentRepo = departmentRepo;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid data" });

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "User with this email already exists" });

            var user = new IdentityUser<int>
            {
                UserName = dto.Email,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(new { message = "Registration failed", errors = result.Errors.Select(e => e.Description) });

            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new { message = "User registered successfully", email = user.Email });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized(new { message = "Invalid email or password" });

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var deptIdClaim = claims.FirstOrDefault(c => c.Type == "DepartmentId")?.Value;
            int? departmentId = null;
            string? departmentName = null;
            if (int.TryParse(deptIdClaim, out var parsed))
            {
                departmentId = parsed;
                var dept = await _departmentRepo.GetByIdAsync(parsed);
                departmentName = dept?.DepartmentName;
            }

            var permissions = await GetPermissionsForRolesAsync(roles);
            var token = GenerateJwtToken(user, roles, permissions, departmentId);

            return Ok(new
            {
                message = "Login successful",
                token = token,
                userId = user.Id,
                email = user.Email,
                roles = roles,
                permissions = permissions,
                departmentId,
                departmentName
            });
        }

        private string GenerateJwtToken(IdentityUser<int> user, IList<string> roles, IList<string> permissions, int? departmentId)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secret = jwtSettings["Secret"] ?? "YourSuperSecretKeyHereMin32CharactersLong123!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.Email ?? "")
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            claims.AddRange((permissions ?? new List<string>()).Select(p => new Claim("permission", p)));
            if (departmentId.HasValue)
            {
                claims.Add(new Claim("DepartmentId", departmentId.Value.ToString()));
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"] ?? "480")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<IList<string>> GetPermissionsForRolesAsync(IList<string> roles)
        {
            var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var roleName in roles ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(roleName)) continue;
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) continue;

                var claims = await _roleManager.GetClaimsAsync(role);
                foreach (var c in claims)
                {
                    if (string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(c.Value))
                        permissions.Add(c.Value.Trim());
                }
            }
            return permissions.OrderBy(x => x).ToList();
        }

        // POST: api/Auth/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully" });
        }

        // GET: api/Auth/me
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Not authenticated" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var claims = await _userManager.GetClaimsAsync(user);
            var deptIdClaim = claims.FirstOrDefault(c => c.Type == "DepartmentId")?.Value;
            int? departmentId = null;
            string? departmentName = null;
            if (int.TryParse(deptIdClaim, out var parsed))
            {
                departmentId = parsed;
                var dept = await _departmentRepo.GetByIdAsync(parsed);
                departmentName = dept?.DepartmentName;
            }

            var response = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.Email ?? string.Empty,
                CreatedDate = DateTime.Now,
                DepartmentId = departmentId,
                DepartmentName = departmentName
            };

            return Ok(response);
        }
    }
}