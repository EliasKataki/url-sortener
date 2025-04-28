using System.ComponentModel.DataAnnotations;

namespace UrlShortener.API.Models
{
    public class UrlClick
    {
        [Key]
        public int Id { get; set; }

        public int UrlId { get; set; }
        public virtual Url Url { get; set; } = null!;

        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
    }
} 