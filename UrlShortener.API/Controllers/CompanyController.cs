using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.API.Data;
using UrlShortener.API.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace UrlShortener.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CompanyController> _logger;

        public CompanyController(ApplicationDbContext context, ILogger<CompanyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCompanies()
        {
            _logger.LogInformation("[COMPANY][GET] Tüm firmalar listeleniyor.");
            var result = await _context.Companies
                .Include(c => c.Urls)
                .Include(c => c.Tokens)
                .Select(c => new {
                    Id = c.Id,
                    Name = c.Name,
                    CreatedAt = c.CreatedAt.ToUniversalTime().ToString("o"),
                    Tokens = c.Tokens.Select(t => new {
                        t.Id,
                        t.Value,
                        t.RemainingUses,
                        t.CreatedAt,
                        t.ExpiresAt
                    }),
                    Urls = c.Urls.Select(u => new {
                        u.Id,
                        u.LongUrl,
                        u.ShortUrl,
                        u.CreatedAt,
                        u.ExpiresAt
                    })
                })
                .ToListAsync();
            _logger.LogInformation("[COMPANY][GET] {Count} firma listelendi.", result.Count);
            return result;
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateCompany([FromBody] CreateCompanyDto dto)
        {
            _logger.LogInformation("[COMPANY][CREATE] Yeni firma oluşturuluyor: {CompanyName}, Token adedi: {TokenCount}", dto.CompanyName, dto.TokenCount);
            var company = new Company
            {
                Name = dto.CompanyName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            // Token oluşturma
            for (int i = 0; i < dto.TokenCount; i++)
            {
                var token = new Token
                {
                    Value = GenerateToken(company.Name ?? "firma", i + 1),
                    RemainingUses = 1000, // Varsayılan kullanım hakkı
                    CompanyId = company.Id,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddYears(1) // 1 yıl geçerli
                };

                _context.Tokens.Add(token);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("[COMPANY][CREATE] Firma ve tokenlar başarıyla oluşturuldu. ID: {CompanyId}", company.Id);

            var resultDto = new 
            {
                Id = company.Id,
                Name = company.Name,
                CreatedAt = company.CreatedAt.ToUniversalTime().ToString("o")
            };

            return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, resultDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetCompany(int id)
        {
            _logger.LogInformation("[COMPANY][DETAIL] Firma detayları isteniyor. ID: {CompanyId}", id);
            var company = await _context.Companies
                .Include(c => c.Urls)
                    .ThenInclude(u => u.Clicks)
                .Include(c => c.Tokens)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
            {
                _logger.LogWarning("[COMPANY][DETAIL][NOTFOUND] Firma bulunamadı. ID: {CompanyId}", id);
                return NotFound();
            }

            _logger.LogInformation("[COMPANY][DETAIL] Firma detayları getirildi. ID: {CompanyId}", id);
            return new {
                Id = company.Id,
                Name = company.Name,
                CreatedAt = company.CreatedAt.ToUniversalTime().ToString("o"),
                Tokens = company.Tokens.Select(t => new {
                    t.Id,
                    t.Value,
                    t.RemainingUses,
                    t.CreatedAt,
                    t.ExpiresAt
                }),
                Urls = company.Urls.Select(u => new {
                    u.Id,
                    u.LongUrl,
                    u.ShortUrl,
                    u.CreatedAt,
                    u.ExpiresAt,
                    clickCount = u.ClickCount,
                    clicks = u.Clicks.Select(c => new {
                        c.Id,
                        c.IpAddress,
                        c.UserAgent,
                        c.ClickedAt,
                        c.Latitude,
                        c.Longitude
                    })
                })
            };
        }

        [HttpPut("token/{tokenId}/uses")]
        public async Task<IActionResult> UpdateTokenUses(int tokenId, [FromBody] UpdateTokenUsesDto dto)
        {
            _logger.LogInformation("[TOKEN][UPDATE] Token kullanım hakkı güncelleniyor. TokenID: {TokenId}, Yeni hak: {RemainingUses}", tokenId, dto.RemainingUses);
            var token = await _context.Tokens.FindAsync(tokenId);

            if (token == null)
            {
                _logger.LogWarning("[TOKEN][UPDATE][NOTFOUND] Token bulunamadı. TokenID: {TokenId}", tokenId);
                return NotFound("Token bulunamadı.");
            }

            if (dto.RemainingUses < 0)
            {
                _logger.LogWarning("[TOKEN][UPDATE][INVALID] Negatif kullanım hakkı girildi. TokenID: {TokenId}", tokenId);
                return BadRequest("Kalan kullanım hakkı negatif olamaz.");
            }

            token.RemainingUses = dto.RemainingUses;
            _context.Entry(token).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("[TOKEN][UPDATE][SUCCESS] Token güncellendi. TokenID: {TokenId}", tokenId);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Tokens.Any(e => e.Id == tokenId))
                {
                    _logger.LogWarning("[TOKEN][UPDATE][CONCURRENCY] Token bulunamadı (eş zamanlılık). TokenID: {TokenId}", tokenId);
                    return NotFound("Token bulunamadı (eş zamanlılık).");
                }
                else
                {
                    _logger.LogError("[TOKEN][UPDATE][ERROR] Eş zamanlılık hatası. TokenID: {TokenId}");
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            _logger.LogWarning("[COMPANY][DELETE] Firma siliniyor. ID: {CompanyId}", id);
            var company = await _context.Companies
                .Include(c => c.Tokens)
                .Include(c => c.Urls)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
            {
                _logger.LogWarning("[COMPANY][DELETE][NOTFOUND] Silinmek istenen firma bulunamadı. ID: {CompanyId}", id);
                return NotFound();
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            _logger.LogInformation("[COMPANY][DELETE][SUCCESS] Firma silindi. ID: {CompanyId}", id);
            return NoContent();
        }

        [HttpDelete("token/{tokenId}")]
        public async Task<IActionResult> DeleteToken(int tokenId)
        {
            _logger.LogWarning("[TOKEN][DELETE] Token siliniyor. TokenID: {TokenId}", tokenId);
            var token = await _context.Tokens.FindAsync(tokenId);
            if (token == null)
            {
                _logger.LogWarning("[TOKEN][DELETE][NOTFOUND] Silinmek istenen token bulunamadı. TokenID: {TokenId}", tokenId);
                return NotFound();
            }

            _context.Tokens.Remove(token);
            await _context.SaveChangesAsync();
            _logger.LogInformation("[TOKEN][DELETE][SUCCESS] Token silindi. TokenID: {TokenId}", tokenId);
            return NoContent();
        }

        private string GenerateToken(string baseName, int index)
        {
            // Firma adından boşlukları ve geçersiz karakterleri temizleyelim (isteğe bağlı)
            var sanitizedName = new string(baseName.Where(c => Char.IsLetterOrDigit(c)).ToArray()).ToLower();
            if (string.IsNullOrEmpty(sanitizedName)) sanitizedName = "firma"; // Boşsa varsayılan kullan
            
            return $"{sanitizedName}token{index}"; 
        }
    }

    public class CreateCompanyDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public int TokenCount { get; set; }
    }

    // YENİ: Token güncelleme için DTO
    public class UpdateTokenUsesDto
    {
        public int RemainingUses { get; set; }
    }
} 