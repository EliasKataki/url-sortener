using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UrlShortener.API.Helpers;
using UrlShortener.API.Models;
using UrlShortener.API.Repositories;
using UrlShortener.Domain.Entities;

namespace UrlShortener.API.Services
{
    public class UrlService : IUrlService
    {
        private readonly IUrlRepository _urlRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly IUrlClickRepository _urlClickRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UrlService> _logger;
        private const string ALLOWED_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private static readonly Random _random = new Random();

        public UrlService(
            IUrlRepository urlRepository,
            ITokenRepository tokenRepository,
            IUrlClickRepository urlClickRepository,
            IConfiguration configuration,
            ILogger<UrlService> logger)
        {
            _urlRepository = urlRepository;
            _tokenRepository = tokenRepository;
            _urlClickRepository = urlClickRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(bool success, string message, Url? url)> CreateShortUrlAsync(CreateUrlDto dto)
        {
            LogHelper.LogUrlOperation(_logger, "CREATE", "Yeni URL kısaltma isteği alındı", dto.LongUrl);

            if (!Uri.TryCreate(dto.LongUrl, UriKind.Absolute, out _))
            {
                LogHelper.LogWarning(_logger, LogCategory.URL, "CREATE", $"Geçersiz URL formatı: {dto.LongUrl}");
                return (false, "Geçersiz URL formatı", null);
            }

            // Eğer expiresAt null ise, varsayılan olarak 1 yıl sonrasını kullan
            if (!dto.ExpiresAt.HasValue)
            {
                dto.ExpiresAt = DateTime.UtcNow.AddYears(1);
                LogHelper.LogWarning(_logger, LogCategory.URL, "CREATE", "Bitiş tarihi (expiresAt) eksik gönderildi, varsayılan olarak 1 yıl sonrası atandı.");
            }

            int? companyId = null;
            Token? foundToken = null;

            if (!string.IsNullOrEmpty(dto.Token))
            {
                LogHelper.LogTokenOperation(_logger, "VALIDATE", "Token doğrulama isteği alındı", dto.Token);
                foundToken = await _tokenRepository.GetByValueAsync(dto.Token);

                if (foundToken == null)
                {
                    LogHelper.LogWarning(_logger, LogCategory.TOKEN, "VALIDATE", $"Geçersiz token: {dto.Token}");
                    return (false, "Geçersiz token", null);
                }

                if (foundToken.RemainingUses <= 0)
                {
                    LogHelper.LogWarning(_logger, LogCategory.TOKEN, "VALIDATE", $"Token kullanım hakkı tükenmiş: {dto.Token}");
                    return (false, $"Bu token ({foundToken.Value}) için kullanım hakkı tükenmiş. Lütfen firma yetkilisi ile iletişime geçin.", null);
                }

                if (foundToken.ExpiresAt.HasValue && foundToken.ExpiresAt.Value.ToUniversalTime() < DateTime.UtcNow)
                {
                    LogHelper.LogWarning(_logger, LogCategory.TOKEN, "VALIDATE", $"Token süresi dolmuş: {dto.Token}");
                    return (false, $"Bu token ({foundToken.Value}) süresi dolmuş. Lütfen firma yetkilisi ile iletişime geçin.", null);
                }

                companyId = foundToken.CompanyId;
                LogHelper.LogTokenOperation(_logger, "VALIDATE", "Token başarıyla doğrulandı", dto.Token, companyId);
            }

            var shortUrl = await GenerateShortUrlAsync();
            var url = new Url
            {
                LongUrl = dto.LongUrl,
                ShortUrl = shortUrl,
                CompanyId = companyId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = dto.ExpiresAt,
                ClickCount = 0
            };

            var urlId = await _urlRepository.CreateAsync(url);
            url.Id = urlId;

            if (foundToken != null)
            {
                await _tokenRepository.DecreaseRemainingUsesAsync(foundToken.Id);
                LogHelper.LogTokenOperation(_logger, "UPDATE", $"Token kullanım hakkı güncellendi. Kalan hak: {foundToken.RemainingUses - 1}", dto.Token, companyId);
            }

            LogHelper.LogUrlOperation(_logger, "CREATE", "URL başarıyla oluşturuldu", shortUrl, url.Id);
            return (true, "URL başarıyla oluşturuldu", url);
        }

        public async Task<Url> GetByIdAsync(int id)
        {
            return await _urlRepository.GetByIdAsync(id);
        }

        public async Task<List<Url>> GetByCompanyIdAsync(int companyId)
        {
            return await _urlRepository.GetByCompanyIdAsync(companyId);
        }

        public async Task<Url> GetByShortUrlAsync(string shortUrl)
        {
            return await _urlRepository.GetByShortUrlAsync(shortUrl);
        }

        public async Task<Url> IncrementClickCountAsync(int id)
        {
            await _urlRepository.IncrementClickCountAsync(id);
            return await _urlRepository.GetByIdAsync(id);
        }

        public async Task<int> RecordClickAsync(UrlClick click)
        {
            return await _urlClickRepository.CreateAsync(click);
        }

        public async Task<List<UrlClick>> GetAllClicksByUrlIdsAsync(List<int> urlIds)
        {
            return await _urlClickRepository.GetByUrlIdsAsync(urlIds);
        }

        public async Task<bool> DeleteUrlAsync(int id)
        {
            await _urlRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> UpdateUrlExpiresAtAsync(int id, DateTime? expiresAt)
        {
            var url = await _urlRepository.GetByIdAsync(id);
            if (url == null)
            {
                return false;
            }

            url.ExpiresAt = expiresAt;
            await _urlRepository.UpdateAsync(url);
            return true;
        }

        public async Task<string> GenerateShortUrlAsync()
        {
            string shortUrl;
            bool exists;

            do
            {
                shortUrl = GenerateRandomShortUrl();
                var existingUrl = await _urlRepository.GetByShortUrlAsync(shortUrl);
                exists = existingUrl != null;
            } while (exists);

            return shortUrl;
        }

        private string GenerateRandomShortUrl()
        {
            var length = 6; // Varsayılan uzunluk
            
            if (_configuration["ShortUrl:Length"] != null)
            {
                int.TryParse(_configuration["ShortUrl:Length"], out length);
            }

            var sb = new System.Text.StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(ALLOWED_CHARS[_random.Next(ALLOWED_CHARS.Length)]);
            }
            return sb.ToString();
        }
    }
} 