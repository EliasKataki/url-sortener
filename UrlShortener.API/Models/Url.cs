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
        
        [Required]
        public DateTime CreatedDate { get; set; }
        
        [Required]
        public DateTime ExpirationDate { get; set; }
        
        public ICollection<UrlAccess> UrlAccesses { get; set; } = new List<UrlAccess>();
    }
} 