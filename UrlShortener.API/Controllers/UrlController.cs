using Microsoft.AspNetCore.Mvc;
using UrlShortener.API.Models;
using UrlShortener.API.Helpers;
using System.Text;
using System.Net.Http;
using Serilog;
using UrlShortener.Domain.Entities;
using UrlShortener.API.Services;
using System.Threading.Tasks;
using System;

namespace UrlShortener.API.Controllers
{
    [ApiController]
    [Route("")]
    public class UrlController : ControllerBase
    {
        private readonly IUrlService _urlService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UrlController> _logger;

        public UrlController(IUrlService urlService, IConfiguration configuration, ILogger<UrlController> logger)
        {
            _urlService = urlService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<Url>> CreateShortUrl([FromBody] CreateUrlDto request)
        {
            var result = await _urlService.CreateShortUrlAsync(request);
            bool success = result.success;
            string message = result.message;
            Url? url = result.url;
            
            if (!success || url == null)
            {
                return BadRequest(message);
            }

            var resultDto = new { 
                id = url.Id, 
                longUrl = url.LongUrl, 
                shortUrl = url.ShortUrl, 
                createdAt = url.CreatedAt.ToUniversalTime().ToString("o"), 
                companyId = url.CompanyId 
            };
            
            return CreatedAtAction(nameof(GetUrl), new { id = url.Id }, resultDto);
        }

        [HttpGet("{shortUrl}")]
        public async Task<ActionResult> RedirectToLongUrl(string shortUrl, [FromQuery] double? latitude = null, [FromQuery] double? longitude = null, [FromQuery] string? markerType = null)
        {
            LogHelper.LogUrlOperation(_logger, "REDIRECT", "URL yönlendirme isteği alındı", shortUrl);

            var url = await _urlService.GetByShortUrlAsync(shortUrl);

            if (url == null)
            {
                LogHelper.LogWarning(_logger, LogCategory.URL, "REDIRECT", $"Bulunamayan URL: {shortUrl}");
                return NotFound();
            }

            if (url.ExpiresAt.HasValue && url.ExpiresAt.Value.ToUniversalTime() < DateTime.UtcNow)
            {
                LogHelper.LogWarning(_logger, LogCategory.URL, "REDIRECT", $"Süresi dolmuş URL: {shortUrl}");
                return StatusCode(410, "Bu kısa linkin süresi doldu.");
            }

            if (!latitude.HasValue || !longitude.HasValue || string.IsNullOrEmpty(markerType))
            {
                return Content("Konum izni olmadan bu linke erişemezsiniz. Lütfen konum izni vererek tekrar deneyin.", "text/plain", System.Text.Encoding.UTF8);
            }

            url = await _urlService.IncrementClickCountAsync(url.Id);
            LogHelper.LogUrlOperation(_logger, "CLICK", $"URL tıklanma sayısı güncellendi: {url.ClickCount}", shortUrl, url.Id);

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            LogHelper.LogAuthOperation(_logger, "ACCESS", "URL erişim isteği", ip);

            if (latitude.HasValue && longitude.HasValue)
            {
                LogHelper.LogMapOperation(_logger, "LOCATION", "Konum bilgisi alındı", latitude, longitude, url.Id);
            }

            var click = new UrlClick
            {
                UrlId = url.Id,
                IpAddress = ip,
                UserAgent = HttpContext.Request.Headers.UserAgent.ToString(),
                Latitude = latitude,
                Longitude = longitude,
                ClickedAt = DateTime.UtcNow,
                MarkerType = markerType
            };

            await _urlService.RecordClickAsync(click);

            LogHelper.LogUrlOperation(_logger, "REDIRECT", "URL başarıyla yönlendirildi", shortUrl, url.Id);
            return Redirect(url.LongUrl);
        }

        [HttpGet("details/{id}")]
        public async Task<ActionResult<object>> GetUrl(int id)
        {
            _logger.LogInformation("URL detay isteği alındı: {Id}", id);

            var url = await _urlService.GetByIdAsync(id);

            if (url == null)
            {
                _logger.LogWarning("Bulunamayan URL detayı: {Id}", id);
                return NotFound();
            }

            _logger.LogInformation("URL detayları başarıyla getirildi: {Id}", id);
            return new {
                id = url.Id,
                longUrl = url.LongUrl,
                shortUrl = url.ShortUrl,
                createdAt = url.CreatedAt.ToUniversalTime().ToString("o"),
                expiresAt = url.ExpiresAt,
                companyId = url.CompanyId,
                clickCount = url.ClickCount
            };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUrl(int id)
        {
            _logger.LogInformation("URL silme isteği alındı: {Id}", id);

            var url = await _urlService.GetByIdAsync(id);
            if (url == null)
            {
                _logger.LogWarning("Silinmek istenen URL bulunamadı: {Id}", id);
                return NotFound();
            }

            await _urlService.DeleteUrlAsync(id);
            _logger.LogInformation("URL silindi: {Id}", id);
            return NoContent();
        }

        [HttpPut("{id}/expires")]
        public async Task<IActionResult> UpdateUrlExpiresAt(int id, [FromBody] UpdateUrlExpiresAtDto dto)
        {
            _logger.LogInformation("URL süre uzatma isteği alındı: {Id}", id);

            var success = await _urlService.UpdateUrlExpiresAtAsync(id, dto.ExpiresAt);
            if (!success)
            {
                _logger.LogWarning("URL süre uzatma işleminde URL bulunamadı: {Id}", id);
                return NotFound();
            }

            _logger.LogInformation("URL süresi uzatıldı: {Id}, Yeni Süre: {ExpiresAt}", id, dto.ExpiresAt);
            return NoContent();
        }
    }
} 