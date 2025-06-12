using Microsoft.AspNetCore.Mvc;
using UrlShortener.API.Models;
using UrlShortener.API.Services;

namespace UrlShortener.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var (success, user) = await _userService.ValidateCredentialsAsync(request);
            if (!success || user == null)
            {
                return Unauthorized(new { message = "Geçersiz email veya şifre" });
            }

            if (!user.IsActive)
            {
                return BadRequest(new { message = "Hesabınız aktif değil" });
            }

            await _userService.UpdateUserLoginTimeAsync(user);
            var token = await _userService.GenerateJwtTokenAsync(user);

            return Ok(new
            {
                token,
                user = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    RoleId = user.RoleId,
                    IsActive = user.IsActive,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt,
                    CompanyIds = user.CompanyIds ?? new List<int>()
                }
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var (success, message, userId) = await _userService.RegisterAsync(request);
            if (!success)
            {
                return BadRequest(new { message });
            }

            return Ok(new { message, userId });
        }
    }
} 