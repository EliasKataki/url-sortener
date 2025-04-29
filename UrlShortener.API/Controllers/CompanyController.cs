using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.API.Data;
using UrlShortener.API.Models;
using System.Security.Cryptography;
using System.Text;

namespace UrlShortener.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CompanyController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCompanies()
        {
            return await _context.Companies
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
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateCompany([FromBody] CreateCompanyDto dto)
        {
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

            // Yanıt olarak daha basit bir DTO dönelim (döngüyü önlemek için)
            var resultDto = new 
            {
                Id = company.Id,
                Name = company.Name,
                CreatedAt = company.CreatedAt.ToUniversalTime().ToString("o")
                // Token'ları veya URL'leri burada dönmeye gerek yok
            };

            return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, resultDto); // company yerine resultDto döndürülüyor
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetCompany(int id)
        {
            var company = await _context.Companies
                .Include(c => c.Urls)
                .Include(c => c.Tokens)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
            {
                return NotFound();
            }

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
                    clickCount = u.ClickCount
                })
            };
        }

        // YENİ: Token kullanım hakkını güncelleme endpoint'i
        [HttpPut("token/{tokenId}/uses")]
        public async Task<IActionResult> UpdateTokenUses(int tokenId, [FromBody] UpdateTokenUsesDto dto)
        {
            var token = await _context.Tokens.FindAsync(tokenId);

            if (token == null)
            {
                return NotFound("Token bulunamadı.");
            }

            if (dto.RemainingUses < 0)
            {
                return BadRequest("Kalan kullanım hakkı negatif olamaz.");
            }

            token.RemainingUses = dto.RemainingUses;
            _context.Entry(token).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Tokens.Any(e => e.Id == tokenId))
                {
                    return NotFound("Token bulunamadı (eş zamanlılık).");
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // Başarılı güncellemede içerik dönmeye gerek yok
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _context.Companies
                .Include(c => c.Tokens)
                .Include(c => c.Urls)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
                return NotFound();

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("token/{tokenId}")]
        public async Task<IActionResult> DeleteToken(int tokenId)
        {
            var token = await _context.Tokens.FindAsync(tokenId);
            if (token == null)
                return NotFound();

            _context.Tokens.Remove(token);
            await _context.SaveChangesAsync();
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