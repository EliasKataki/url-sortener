using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UrlShortener.API.Models;
using UrlShortener.Domain.Entities;

namespace UrlShortener.API.Services
{
    public interface IUserService
    {
        Task<User> GetByIdAsync(Guid id);
        Task<User> GetByEmailAsync(string email);
        Task<(bool success, string message, Guid? userId)> RegisterAsync(RegisterRequest request);
        Task<(bool success, User? user)> ValidateCredentialsAsync(LoginRequest request);
        Task UpdateUserLoginTimeAsync(User user);
        Task<string> GenerateJwtTokenAsync(User user);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> ValidateUserAsync(string email, string password);
        Task UpdateLastLoginAsync(Guid userId);
        Task UpdateUserRoleAsync(string id, int roleId);
        Task UpdateUserStatusAsync(string id, bool isActive);
        Task DeleteUserAsync(string id);
        Task UpdateUserCompaniesAsync(string id, List<int> companyIds);
    }
} 