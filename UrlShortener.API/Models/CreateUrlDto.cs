using System;

namespace UrlShortener.API.Models
{
    public class CreateUrlDto
    {
        public string LongUrl { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }
} 