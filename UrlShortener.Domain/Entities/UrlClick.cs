using System;

namespace UrlShortener.Domain.Entities
{
    public class UrlClick
    {
        public int Id { get; set; }
        public int UrlId { get; set; }
        public virtual Url? Url { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? MarkerType { get; set; }
    }
} 