using System.ComponentModel.DataAnnotations;

namespace UrlShortener.API.Models
{
    public class CreateCompanyDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [Range(1, 100)]
        public int TokenCount { get; set; }
    }
} 