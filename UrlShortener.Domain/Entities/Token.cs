using System;

namespace UrlShortener.Domain.Entities
{
    public class Token
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public int RemainingUses { get; set; }
        public int CompanyId { get; set; }
        public virtual Company? Company { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
    }
} 