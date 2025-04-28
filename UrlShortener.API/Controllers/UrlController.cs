using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.API.Data;
using UrlShortener.API.Models;
using System.Text;

namespace UrlShortener.API.Controllers
{
    [ApiController]
    [Route("")]
    public class UrlController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UrlController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<ActionResult<Url>> CreateShortUrl([FromBody] CreateUrlDto dto)
        {
            if (!Uri.TryCreate(dto.LongUrl, UriKind.Absolute, out _))
            {
                return BadRequest("Geçersiz URL formatı");
            }

            int? companyId = null; // CompanyId başlangıçta null
            Token? foundToken = null;

            // Eğer bir token gönderildiyse ve boş değilse, onu bulmaya çalış
            if (!string.IsNullOrEmpty(dto.Token))
            {
                foundToken = await _context.Tokens
                    .FirstOrDefaultAsync(t => t.Value == dto.Token && t.RemainingUses > 0);

                if (foundToken == null)
                {
                    // Token gönderildi ama geçersizse hata ver
                    return BadRequest("Geçersiz veya kullanılmış token");
                }
                companyId = foundToken.CompanyId; // CompanyId'yi bulduğun token'dan al
            }
            // Eğer token gönderilmediyse veya boşsa, companyId null kalacak (public kısaltma)

            var shortUrl = GenerateShortUrl();
            var url = new Url
            {
                LongUrl = dto.LongUrl,
                ShortUrl = shortUrl,
                CompanyId = companyId, // Nullable companyId atandı
                CreatedAt = DateTime.UtcNow
            };

            // Eğer geçerli bir token bulunduysa, kullanım hakkını azalt
            if (foundToken != null)
            {
                foundToken.RemainingUses--;
            }

            _context.Urls.Add(url);
            await _context.SaveChangesAsync();

            // Dönen yanıtta token bilgisini göndermemek daha iyi olabilir
            var resultDto = new { id = url.Id, longUrl = url.LongUrl, shortUrl = url.ShortUrl, createdAt = url.CreatedAt.ToUniversalTime().ToString("o"), companyId = url.CompanyId };
            
            return CreatedAtAction(nameof(GetUrl), new { id = url.Id }, resultDto);
        }

        [HttpGet("{shortUrl}")]
        public async Task<ActionResult> RedirectToLongUrl(string shortUrl)
        {
            var url = await _context.Urls.FirstOrDefaultAsync(u => u.ShortUrl == shortUrl);

            if (url == null)
                return NotFound();

            // expiresAt kontrolü
            if (url.ExpiresAt.HasValue && url.ExpiresAt.Value.Date < DateTime.UtcNow.Date)
                return StatusCode(410, "Bu kısa linkin süresi doldu."); // 410 Gone

            url.ClickCount++;
            var click = new UrlClick
            {
                UrlId = url.Id,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                UserAgent = HttpContext.Request.Headers.UserAgent.ToString(),
                Country = "Unknown",
                City = "Unknown"
            };

            _context.UrlClicks.Add(click);
            await _context.SaveChangesAsync();

            return Redirect(url.LongUrl);
        }

        [HttpGet("details/{id}")]
        public async Task<ActionResult<object>> GetUrl(int id)
        {
            var url = await _context.Urls
                .Include(u => u.Clicks)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (url == null)
            {
                return NotFound();
            }

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
                    c.Country,
                    c.City
                })
            };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUrl(int id)
        {
            var url = await _context.Urls
                .Include(u => u.Clicks)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (url == null)
                return NotFound();

            _context.Urls.Remove(url);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}/expires")]
        public async Task<IActionResult> UpdateUrlExpiresAt(int id, [FromBody] UpdateUrlExpiresAtDto dto)
        {
            var url = await _context.Urls.FindAsync(id);
            if (url == null)
                return NotFound();
            url.ExpiresAt = dto.ExpiresAt;
            _context.Entry(url).State = EntityState.Modified;
            await _context.SaveChangesAsync();
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