using System;

namespace UrlShortener.API.Models
{
    public class UrlLog
    {
        public int Id { get; set; }
        public string LongUrl { get; set; } = string.Empty;
        public string ShortUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Operation { get; set; } = string.Empty; // CREATE, ACCESS, EXPIRE gibi
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
    }
} 