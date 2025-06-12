using Microsoft.AspNetCore.Mvc;
using UrlShortener.API.Models;
using UrlShortener.Domain.Entities;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using UrlShortener.API.Services;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace UrlShortener.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly ITokenService _tokenService;
        private readonly IUrlService _urlService;
        private readonly IUserService _userService;
        private readonly ILogger<CompanyController> _logger;

        public CompanyController(
            ICompanyService companyService, 
            ITokenService tokenService, 
            IUrlService urlService,
            IUserService userService,
            ILogger<CompanyController> logger)
        {
            _companyService = companyService;
            _tokenService = tokenService;
            _urlService = urlService;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCompanies()
        {
            try
            {
                _logger.LogInformation("[COMPANY][GET] Tüm firmalar listeleniyor.");

                // Kullanıcı bilgilerini al
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var roleId = int.Parse(User.FindFirst("role_id")?.Value ?? "3");

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Tüm firmaları çek
                var allCompanies = await _companyService.GetAllAsync();
                var companies = new List<Company>();

                // Süper admin tüm firmaları görebilir
                if (roleId == 1) // SuperAdmin
                {
                    companies = allCompanies.ToList();
                }
                else // Admin veya User
                {
                    // Kullanıcının eşleştirildiği firmaları al
                    var user = await _userService.GetByIdAsync(Guid.Parse(userId));
                    if (user != null && user.CompanyIds != null)
                    {
                        companies = allCompanies.Where(c => user.CompanyIds.Contains(c.Id)).ToList();
                    }
                }

                var result = new List<object>();
                foreach (var company in companies)
                {
                    var tokens = await _tokenService.GetByCompanyIdAsync(company.Id);
                    var urls = await _urlService.GetByCompanyIdAsync(company.Id);

                    result.Add(new {
                        Id = company.Id,
                        Name = company.Name,
                        CreatedAt = company.CreatedAt.ToUniversalTime().ToString("o"),
                        Tokens = tokens.Select(t => new {
                            t.Id,
                            t.Value,
                            t.RemainingUses,
                            t.CreatedAt,
                            t.ExpiresAt
                        }),
                        Urls = urls.Select(u => new {
                            u.Id,
                            u.LongUrl,
                            u.ShortUrl,
                            u.CreatedAt,
                            u.ExpiresAt
                        })
                    });
                }

                _logger.LogInformation($"[COMPANY][GET] {result.Count} firma listelendi. User: {userId}, Role: {roleId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[COMPANY][GET] Firmalar listelenirken hata oluştu.");
                throw;
            }
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

            var companyId = await _companyService.CreateAsync(company);
            company.Id = companyId;

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

                await _tokenService.CreateAsync(token);
            }

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
            var company = await _companyService.GetByIdAsync(id);

            if (company == null)
            {
                _logger.LogWarning("[COMPANY][DETAIL][NOTFOUND] Firma bulunamadı. ID: {CompanyId}", id);
                return NotFound();
            }

            // İlişkili verileri al
            var tokens = await _tokenService.GetByCompanyIdAsync(id);
            var urls = await _urlService.GetByCompanyIdAsync(id);
            
            // URL ile ilişkili tıklama verilerini al - Dapper ile bunu ayrı bir sorgu olarak getirmek gerekecek
            var urlIds = urls.Select(u => u.Id).ToList();
            var allClicks = await _urlService.GetAllClicksByUrlIdsAsync(urlIds);

            _logger.LogInformation("[COMPANY][DETAIL] Firma detayları getirildi. ID: {CompanyId}", id);
            return new {
                Id = company.Id,
                Name = company.Name,
                CreatedAt = company.CreatedAt.ToUniversalTime().ToString("o"),
                Tokens = tokens.Select(t => new {
                    t.Id,
                    t.Value,
                    t.RemainingUses,
                    t.CreatedAt,
                    t.ExpiresAt
                }),
                Urls = urls.Select(u => new {
                    u.Id,
                    u.LongUrl,
                    u.ShortUrl,
                    u.CreatedAt,
                    u.ExpiresAt,
                    clickCount = u.ClickCount,
                    clicks = allClicks.Where(c => c.UrlId == u.Id).Select(c => new {
                        c.Id,
                        c.IpAddress,
                        c.UserAgent,
                        c.ClickedAt,
                        c.Latitude,
                        c.Longitude,
                        c.MarkerType
                    })
                })
            };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            _logger.LogWarning("[COMPANY][DELETE] Firma siliniyor. ID: {CompanyId}", id);
            var company = await _companyService.GetByIdAsync(id);

            if (company == null)
            {
                _logger.LogWarning("[COMPANY][DELETE][NOTFOUND] Silinmek istenen firma bulunamadı. ID: {CompanyId}", id);
                return NotFound();
            }

            await _companyService.DeleteAsync(id);
            _logger.LogInformation("[COMPANY][DELETE][SUCCESS] Firma silindi. ID: {CompanyId}", id);

            return NoContent();
        }

        [HttpPut("token/{tokenId}/uses")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateTokenUses(int tokenId, [FromBody] UpdateTokenUsesDto dto)
        {
            _logger.LogInformation("[TOKEN][UPDATE] Token kullanım hakkı güncelleniyor. TokenID: {TokenId}, Yeni hak: {RemainingUses}", tokenId, dto.RemainingUses);
            var token = await _tokenService.GetByIdAsync(tokenId);

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
            await _tokenService.UpdateAsync(token);
            _logger.LogInformation("[TOKEN][UPDATE][SUCCESS] Token güncellendi. TokenID: {TokenId}", tokenId);

            return NoContent();
        }

        [HttpDelete("token/{tokenId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteToken(int tokenId)
        {
            _logger.LogWarning("[TOKEN][DELETE] Token siliniyor. ID: {TokenId}", tokenId);
            var token = await _tokenService.GetByIdAsync(tokenId);

            if (token == null)
            {
                _logger.LogWarning("[TOKEN][DELETE][NOTFOUND] Silinmek istenen token bulunamadı. ID: {TokenId}", tokenId);
                return NotFound();
            }

            await _tokenService.DeleteAsync(tokenId);
            _logger.LogInformation("[TOKEN][DELETE][SUCCESS] Token silindi. ID: {TokenId}", tokenId);

            return NoContent();
        }

        private string GenerateToken(string baseName, int index)
        {
            return $"{baseName}-token-{index}";
        }
    }
    
    public class CreateCompanyDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public int TokenCount { get; set; }
    }

    public class UpdateTokenUsesDto
    {
        public int RemainingUses { get; set; }
    }
} 