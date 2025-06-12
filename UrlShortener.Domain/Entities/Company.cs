using System;
using System.Collections.Generic;

namespace UrlShortener.Domain.Entities
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public virtual ICollection<Url>? Urls { get; set; } = new List<Url>();
        public virtual ICollection<Token>? Tokens { get; set; } = new List<Token>();
    }
} 