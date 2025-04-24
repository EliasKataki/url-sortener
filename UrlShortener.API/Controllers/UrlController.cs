using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.API.Data;
using UrlShortener.API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Web;

namespace UrlShortener.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private static readonly UrlList _urlList = new UrlList();

        public UrlController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task LogUrlOperation(string longUrl, string shortUrl, string operation, bool isSuccessful, string? errorMessage = null)
        {
            var log = new UrlLog
            {
                LongUrl = longUrl,
                ShortUrl = shortUrl,
                CreatedAt = DateTime.UtcNow,
                Operation = operation,
                UserAgent = Request.Headers["User-Agent"].ToString(),
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                IsSuccessful = isSuccessful,
                ErrorMessage = errorMessage
            };

            _context.UrlLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> ShortenUrl([FromBody] object requestObj)
        {
            try
            {
                // JSON isteğini dinamik olarak işle
                string? longUrl = null;
                
                if (requestObj is System.Text.Json.JsonElement jsonElement)
                {
                    if (jsonElement.TryGetProperty("url", out var urlProperty))
                    {
                        longUrl = urlProperty.GetString();
                    }
                }
                else if (requestObj is UrlRequest urlRequest && !string.IsNullOrEmpty(urlRequest.Url))
                {
                    longUrl = urlRequest.Url;
                }
                else if (requestObj is string stringValue)
                {
                    longUrl = stringValue;
                }

                if (string.IsNullOrEmpty(longUrl))
                {
                    await LogUrlOperation("", "", "CREATE", false, "URL boş olamaz");
                    return BadRequest("URL boş olamaz");
                }

                // URL'nin başında http:// veya https:// yoksa https:// ekle
                if (!longUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                    !longUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    longUrl = "https://" + longUrl;
                }

                // URL'yi parse et
                if (!Uri.TryCreate(longUrl, UriKind.Absolute, out Uri? uri))
                {
                    await LogUrlOperation(longUrl, "", "CREATE", false, "Geçersiz URL formatı");
                    return BadRequest("Geçersiz URL formatı");
                }

                // URL'yi encode et
                var encodedUrl = Uri.EscapeDataString(longUrl);

                // Benzersiz kısa URL oluştur
                string shortCode;
                do
                {
                    shortCode = GenerateShortUrl();
                } while (await _context.Urls.AnyAsync(u => u.ShortUrl == shortCode));

                var urlPair = new UrlPair
                {
                    LongUrl = encodedUrl,
                    ShortUrl = shortCode,
                    CreatedAt = DateTime.UtcNow,
                    ExpirationDate = DateTime.UtcNow.AddDays(30)
                };

                // Listeye ekle
                _urlList.Urls.Add(urlPair);

                // Veritabanına ekle
                var url = new Url
                {
                    LongUrl = encodedUrl,
                    ShortUrl = shortCode,
                    CreatedDate = urlPair.CreatedAt,
                    ExpirationDate = urlPair.ExpirationDate,
                    UrlAccesses = new List<UrlAccess>()
                };

                _context.Urls.Add(url);
                await _context.SaveChangesAsync();

                // İşlemi logla
                await LogUrlOperation(longUrl, shortCode, "CREATE", true);

                // Tam URL'yi döndür
                var shortUrl = $"http://localhost:5161/api/url/{shortCode}";
                return Ok(new { shortUrl });
            }
            catch (Exception ex)
            {
                await LogUrlOperation(requestObj?.ToString() ?? "", "", "CREATE", false, ex.Message);
                throw;
            }
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> RedirectToLongUrl(string code)
        {
            try
            {
                var url = await _context.Urls
                    .FirstOrDefaultAsync(u => u.ShortUrl == code);

                if (url == null)
                {
                    await LogUrlOperation("", code, "ACCESS", false, "URL bulunamadı");
                    return NotFound("URL bulunamadı");
                }

                if (url.ExpirationDate < DateTime.UtcNow)
                {
                    await LogUrlOperation(url.LongUrl, code, "ACCESS", false, "URL'nin süresi dolmuş");
                    return BadRequest("URL'nin süresi dolmuş");
                }

                // URL'yi decode et
                var decodedUrl = Uri.UnescapeDataString(url.LongUrl);

                // URL erişim bilgilerini kaydet
                var access = new UrlAccess
                {
                    UrlId = url.Id,
                    AccessDate = DateTime.UtcNow,
                    IsSuccessful = true,
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString()
                };

                _context.UrlAccesses.Add(access);
                await _context.SaveChangesAsync();

                // Başarılı erişimi logla
                await LogUrlOperation(decodedUrl, code, "ACCESS", true);

                return Redirect(decodedUrl);
            }
            catch (Exception ex)
            {
                await LogUrlOperation("", code, "ACCESS", false, ex.Message);
                throw;
            }
        }

        [HttpGet("stats/{*fullUrl}")]
        public async Task<IActionResult> GetUrlStats(string fullUrl)
        {
            // URL'den kısa kodu çıkar
            string code;
            try
            {
                // Eğer tam URL geldiyse (http://localhost:5161/api/url/ABC123 gibi)
                if (fullUrl.Contains("/"))
                {
                    code = fullUrl.Split('/').Last();
                }
                else
                {
                    code = fullUrl;
                }
            }
            catch
            {
                return BadRequest("Geçersiz URL formatı");
            }

            var url = await _context.Urls
                .Include(u => u.UrlAccesses)
                .FirstOrDefaultAsync(u => u.ShortUrl == code);

            if (url == null)
                return NotFound("URL bulunamadı");

            var stats = new
            {
                originalUrl = Uri.UnescapeDataString(url.LongUrl),
                shortCode = url.ShortUrl,
                shortUrl = $"http://localhost:5161/api/url/{url.ShortUrl}",
                createdDate = url.CreatedDate,
                expirationDate = url.ExpirationDate,
                totalAccesses = url.UrlAccesses.Count,
                successfulAccesses = url.UrlAccesses.Count(a => a.IsSuccessful),
                failedAccesses = url.UrlAccesses.Count(a => !a.IsSuccessful),
                lastAccess = url.UrlAccesses.OrderByDescending(a => a.AccessDate).FirstOrDefault()?.AccessDate,
                accessDetails = url.UrlAccesses.OrderByDescending(a => a.AccessDate)
                    .Select(a => new
                    {
                        a.AccessDate,
                        a.IsSuccessful,
                        a.ErrorMessage,
                        a.UserAgent,
                        a.IpAddress
                    })
            };

            return Ok(stats);
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListAllUrls()
        {
            var urls = await _context.Urls
                .Include(u => u.UrlAccesses)
                .Select(u => new
                {
                    id = u.Id,
                    originalUrl = Uri.UnescapeDataString(u.LongUrl),
                    shortCode = u.ShortUrl,
                    shortUrl = $"http://localhost:5161/api/url/{u.ShortUrl}",
                    createdDate = u.CreatedDate,
                    expirationDate = u.ExpirationDate,
                    totalAccesses = u.UrlAccesses.Count,
                    accesses = u.UrlAccesses.Select(a => new
                    {
                        a.AccessDate,
                        a.IsSuccessful,
                        a.UserAgent,
                        a.IpAddress
                    }).OrderByDescending(a => a.AccessDate).ToList()
                })
                .OrderByDescending(u => u.createdDate)
                .ToListAsync();

            return Ok(urls);
        }

        [HttpGet("list/memory")]
        public IActionResult GetUrlList()
        {
            return Ok(_urlList.Urls.OrderByDescending(u => u.CreatedAt));
        }

        [HttpGet("list/database")]
        public async Task<IActionResult> GetUrlListFromDatabase()
        {
            var urls = await _context.Urls
                .OrderByDescending(u => u.CreatedDate)
                .Select(u => new
                {
                    longUrl = Uri.UnescapeDataString(u.LongUrl),
                    shortUrl = u.ShortUrl,
                    createdAt = u.CreatedDate,
                    expirationDate = u.ExpirationDate
                })
                .ToListAsync();

            return Ok(urls);
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs()
        {
            var logs = await _context.UrlLogs
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return Ok(logs);
        }

        private string GenerateShortUrl()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
} 