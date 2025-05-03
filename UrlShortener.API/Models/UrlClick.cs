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
        public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? MarkerType { get; set; } // 'gps' veya 'ip'
    }
} 