using System.Collections.Generic;
using System.Threading.Tasks;
using UrlShortener.Domain.Entities;

namespace UrlShortener.API.Services
{
    public interface ITokenService
    {
        Task<Token> GetByIdAsync(int id);
        Task<Token> GetByValueAsync(string value);
        Task<List<Token>> GetByCompanyIdAsync(int companyId);
        Task<int> CreateAsync(Token token);
        Task UpdateAsync(Token token);
        Task DeleteAsync(int id);
        Task DecreaseRemainingUsesAsync(int id);
    }
} 