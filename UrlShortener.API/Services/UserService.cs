using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UrlShortener.API.Helpers;
using UrlShortener.API.Models;
using UrlShortener.API.Repositories;
using UrlShortener.Domain.Entities;
using System.Linq;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace UrlShortener.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<(bool success, string message, Guid? userId)> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return (false, "Bu email adresi zaten kullanılıyor.", null);
            }

            // FirstName değerini güvenli bir şekilde alıp boşlukları koruyalım
            string firstName = request.FirstName ?? string.Empty;
            
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = firstName, // Tam ismi koruyoruz
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = PasswordHelper.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                RoleId = 3 // Standart kullanıcı rolü (User)
            };

            await _userRepository.CreateAsync(user);
            return (true, "Kayıt başarılı.", user.Id);
        }

        public async Task<(bool success, User? user)> ValidateCredentialsAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null) return (false, null);

            var hashedPassword = HashPassword(request.Password);
            if (user.PasswordHash != hashedPassword) return (false, null);

            // Kullanıcının firma eşleştirmelerini tekrar çek
            var userWithCompanies = await _userRepository.GetByIdAsync(user.Id);
            if (userWithCompanies != null)
            {
                user.CompanyIds = userWithCompanies.CompanyIds;
                Console.WriteLine($"User {request.Email} validated with company IDs: {string.Join(", ", user.CompanyIds)}");
            }

            return (true, user);
        }

        public async Task UpdateUserLoginTimeAsync(User user)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        public async Task<string> GenerateJwtTokenAsync(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
                new Claim(ClaimTypes.Role, user.RoleName),
                new Claim("role_id", user.RoleId.ToString())
            };
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: creds
            );
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task UpdateUserRoleAsync(string userId, int roleId)
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId)) ?? throw new Exception("Kullanıcı bulunamadı.");
            user.RoleId = roleId;
            await _userRepository.UpdateAsync(user);
        }

        public async Task UpdateUserStatusAsync(string userId, bool isActive)
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId)) ?? throw new Exception("Kullanıcı bulunamadı.");
            user.IsActive = isActive;
            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId)) ?? throw new Exception("Kullanıcı bulunamadı.");
            await _userRepository.DeleteAsync(user.Id);
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<User?> ValidateUserAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return null;

            var hashedPassword = HashPassword(password);
            return user.PasswordHash == hashedPassword ? user : null;
        }

        public async Task UpdateLastLoginAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public async Task UpdateUserCompaniesAsync(string userId, List<int> companyIds)
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId)) ?? throw new Exception("Kullanıcı bulunamadı.");
            user.CompanyIds = companyIds;
            await _userRepository.UpdateUserCompaniesAsync(user.Id, companyIds);
        }
    }
} 