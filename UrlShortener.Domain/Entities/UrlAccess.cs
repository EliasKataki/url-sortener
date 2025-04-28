using System;

namespace UrlShortener.Domain.Entities
{
    public class UrlAccess
    {
        public int Id { get; set; }
        public int UrlId { get; set; }
        public string? IpAddress { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Browser { get; set; }
        public string? Device { get; set; }
        public DateTime AccessDate { get; set; }
        public Url Url { get; set; }
    }
} 