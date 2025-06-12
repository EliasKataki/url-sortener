using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UrlShortener.API.Models;
using UrlShortener.Domain.Entities;

namespace UrlShortener.API.Services
{
    public interface IUrlService
    {
        Task<(bool success, string message, Url? url)> CreateShortUrlAsync(CreateUrlDto dto);
        Task<Url> GetByIdAsync(int id);
        Task<List<Url>> GetByCompanyIdAsync(int companyId);
        Task<Url> GetByShortUrlAsync(string shortUrl);
        Task<Url> IncrementClickCountAsync(int id);
        Task<int> RecordClickAsync(UrlClick click);
        Task<List<UrlClick>> GetAllClicksByUrlIdsAsync(List<int> urlIds);
        Task<bool> DeleteUrlAsync(int id);
        Task<bool> UpdateUrlExpiresAtAsync(int id, DateTime? expiresAt);
        Task<string> GenerateShortUrlAsync();
    }
} 