using System;
using System.ComponentModel.DataAnnotations;

namespace UrlShortener.API.Models
{
    public class Url
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string LongUrl { get; set; } = string.Empty;
        
        [Required]
        public string ShortUrl { get; set; } = string.Empty;
        
        public int ClickCount { get; set; }
        
        public int? CompanyId { get; set; }
        public virtual Company? Company { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        
        public virtual ICollection<UrlClick> Clicks { get; set; } = new List<UrlClick>();
    }
} 