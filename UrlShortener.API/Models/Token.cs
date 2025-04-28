using System.ComponentModel.DataAnnotations;

namespace UrlShortener.API.Models
{
    public class Token
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Value { get; set; } = string.Empty;

        public int RemainingUses { get; set; }

        public int CompanyId { get; set; }
        public virtual Company Company { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
    }
} 