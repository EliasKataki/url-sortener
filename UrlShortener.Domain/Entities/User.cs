using System;
using System.Collections.Generic;

namespace UrlShortener.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;
        public int RoleId { get; set; } = 1; // Varsayılan olarak 1 atayalım
        public List<int> CompanyIds { get; set; } = new List<int>();
        public string CompanyIdsString { get; set; } = string.Empty;

        public string RoleName => RoleId switch
        {
            1 => "SuperAdmin",
            2 => "Admin",
            3 => "User",
            _ => "Unknown"
        };
    }
} 