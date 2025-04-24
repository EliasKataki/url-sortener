using System;
using System.Collections.Generic;

namespace UrlShortener.API.Models
{
    public class UrlList
    {
        public List<UrlPair> Urls { get; set; } = new List<UrlPair>();
    }

    public class UrlPair
    {
        public string LongUrl { get; set; } = string.Empty;
        public string ShortUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
} 