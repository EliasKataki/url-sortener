using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.API.Data;
using UrlShortener.API.Models;
using UrlShortener.API.Helpers;
using System.Text;
using System.Net.Http;
using Serilog;

namespace UrlShortener.API.Controllers
{
    [ApiController]
    [Route("")]
    public class UrlController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UrlController> _logger;

        public UrlController(ApplicationDbContext context, IConfiguration configuration, ILogger<UrlController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<Url>> CreateShortUrl([FromBody] CreateUrlDto dto)
        {
            LogHelper.LogUrlOperation(_logger, "CREATE", "Yeni URL kısaltma isteği alındı", dto.LongUrl);

            if (!Uri.TryCreate(dto.LongUrl, UriKind.Absolute, out _))
            {
                LogHelper.LogWarning(_logger, LogCategory.URL, "CREATE", $"Geçersiz URL formatı: {dto.LongUrl}");
                return BadRequest("Geçersiz URL formatı");
            }

            int? companyId = null;
            Token? foundToken = null;

            if (!string.IsNullOrEmpty(dto.Token))
            {
                LogHelper.LogTokenOperation(_logger, "VALIDATE", "Token doğrulama isteği alındı", dto.Token);
                foundToken = await _context.Tokens
                    .FirstOrDefaultAsync(t => t.Value == dto.Token && t.RemainingUses > 0);

                if (foundToken == null)
                {
                    LogHelper.LogWarning(_logger, LogCategory.TOKEN, "VALIDATE", $"Geçersiz veya kullanılmış token: {dto.Token}");
                    return BadRequest("Geçersiz veya kullanılmış token");
                }
                companyId = foundToken.CompanyId;
                LogHelper.LogTokenOperation(_logger, "VALIDATE", "Token başarıyla doğrulandı", dto.Token, companyId);
            }

            var shortUrl = GenerateShortUrl();
            var url = new Url
            {
                LongUrl = dto.LongUrl,
                ShortUrl = shortUrl,
                CompanyId = companyId,
                CreatedAt = DateTime.UtcNow
            };

            if (foundToken != null)
            {
                foundToken.RemainingUses--;
                LogHelper.LogTokenOperation(_logger, "UPDATE", $"Token kullanım hakkı güncellendi. Kalan hak: {foundToken.RemainingUses}", dto.Token, companyId);
            }

            _context.Urls.Add(url);
            await _context.SaveChangesAsync();

            LogHelper.LogUrlOperation(_logger, "CREATE", "URL başarıyla oluşturuldu", shortUrl, url.Id);
            var resultDto = new { id = url.Id, longUrl = url.LongUrl, shortUrl = url.ShortUrl, createdAt = url.CreatedAt.ToUniversalTime().ToString("o"), companyId = url.CompanyId };
            
            return CreatedAtAction(nameof(GetUrl), new { id = url.Id }, resultDto);
        }

        [HttpGet("{shortUrl}")]
        public async Task<ActionResult> RedirectToLongUrl(string shortUrl, [FromQuery] double? latitude = null, [FromQuery] double? longitude = null)
        {
            LogHelper.LogUrlOperation(_logger, "REDIRECT", "URL yönlendirme isteği alındı", shortUrl);

            var url = await _context.Urls.FirstOrDefaultAsync(u => u.ShortUrl == shortUrl);

            if (url == null)
            {
                LogHelper.LogWarning(_logger, LogCategory.URL, "REDIRECT", $"Bulunamayan URL: {shortUrl}");
                return NotFound();
            }

            if (url.ExpiresAt.HasValue && url.ExpiresAt.Value.Date < DateTime.UtcNow.Date)
            {
                LogHelper.LogWarning(_logger, LogCategory.URL, "REDIRECT", $"Süresi dolmuş URL: {shortUrl}");
                return StatusCode(410, "Bu kısa linkin süresi doldu.");
            }

            url.ClickCount++;
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
                ClickedAt = DateTime.UtcNow
            };

            _context.UrlClicks.Add(click);
            await _context.SaveChangesAsync();

            LogHelper.LogUrlOperation(_logger, "REDIRECT", "URL başarıyla yönlendirildi", shortUrl, url.Id);
            return Redirect(url.LongUrl);
        }

        [HttpGet("details/{id}")]
        public async Task<ActionResult<object>> GetUrl(int id)
        {
            _logger.LogInformation("URL detay isteği alındı: {Id}", id);

            var url = await _context.Urls
                .Include(u => u.Clicks)
                .FirstOrDefaultAsync(u => u.Id == id);

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
                clickCount = url.ClickCount,
                clicks = url.Clicks.Select(c => new {
                    c.Id,
                    c.IpAddress,
                    c.UserAgent,
                    c.ClickedAt,
                    c.Latitude,
                    c.Longitude
                })
            };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUrl(int id)
        {
            _logger.LogInformation("URL silme isteği alındı: {Id}", id);

            var url = await _context.Urls.FindAsync(id);
            if (url == null)
            {
                _logger.LogWarning("Silinmek istenen URL bulunamadı: {Id}", id);
                return NotFound();
            }

            _context.Urls.Remove(url);
            await _context.SaveChangesAsync();

            _logger.LogInformation("URL başarıyla silindi: {Id}", id);
            return NoContent();
        }

        [HttpPut("{id}/expires")]
        public async Task<IActionResult> UpdateUrlExpiresAt(int id, [FromBody] UpdateUrlExpiresAtDto dto)
        {
            _logger.LogInformation("URL süre güncelleme isteği alındı: {Id}, Yeni süre: {ExpiresAt}", id, dto.ExpiresAt);

            var url = await _context.Urls.FindAsync(id);
            if (url == null)
            {
                _logger.LogWarning("Süresi güncellenmek istenen URL bulunamadı: {Id}", id);
                return NotFound();
            }

            url.ExpiresAt = dto.ExpiresAt;
            await _context.SaveChangesAsync();

            _logger.LogInformation("URL süresi başarıyla güncellendi: {Id}", id);
            return NoContent();
        }

        private string GenerateShortUrl()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var shortUrl = new StringBuilder(6);

            for (int i = 0; i < 6; i++)
            {
                shortUrl.Append(chars[random.Next(chars.Length)]);
            }

            return shortUrl.ToString();
        }
    }

    public class CreateUrlDto
    {
        public string LongUrl { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class UpdateUrlExpiresAtDto
    {
        public DateTime? ExpiresAt { get; set; }
    }
} 