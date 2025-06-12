using System;
using System.Collections.Generic;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Entities
{
    public class Url
    {
        public int Id { get; set; }
        public string LongUrl { get; set; } = string.Empty;
        public string ShortUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int ClickCount { get; set; }
        public int? CompanyId { get; set; }
        public virtual ICollection<UrlClick>? Clicks { get; set; }
        public virtual Company? Company { get; set; }
    }
} 