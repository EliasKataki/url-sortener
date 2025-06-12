using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using UrlShortener.API.Helpers;
using UrlShortener.API.Models;
using UrlShortener.API.Services;
using UrlShortener.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.Threading.Tasks;

namespace UrlShortener.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var (success, message, userId) = await _userService.RegisterAsync(request);
            if (!success)
                return BadRequest(new { success = false, message = message });

            return Ok(new { success = true, message = "Kayıt başarılı." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var (success, user) = await _userService.ValidateCredentialsAsync(request);
            if (!success || user == null)
                return Unauthorized("Email veya şifre hatalı.");

            await _userService.UpdateUserLoginTimeAsync(user);

            var token = await _userService.GenerateJwtTokenAsync(user);
            return Ok(new LoginResponse
            {
                Token = token,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                RoleName = user.RoleName
            });
        }
    }
} 