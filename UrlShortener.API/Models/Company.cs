using System.ComponentModel.DataAnnotations;

namespace UrlShortener.API.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();
        public virtual ICollection<Url> Urls { get; set; } = new List<Url>();
    }
} 