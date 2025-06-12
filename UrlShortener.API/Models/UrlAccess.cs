using System;
using System.ComponentModel.DataAnnotations;
using UrlShortener.Domain.Entities;

namespace UrlShortener.API.Models
{
    public class UrlAccess
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UrlId { get; set; }
        
        public Url? Url { get; set; }
        
        [Required]
        public DateTime AccessDate { get; set; }
        
        public bool IsSuccessful { get; set; }
        
        public string? ErrorMessage { get; set; }
        
        public string? UserAgent { get; set; }
        
        public string? IpAddress { get; set; }
    }
} 