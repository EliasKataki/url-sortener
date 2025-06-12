using System.Collections.Generic;
using System.Threading.Tasks;
using UrlShortener.API.Repositories;
using UrlShortener.Domain.Entities;

namespace UrlShortener.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly ITokenRepository _tokenRepository;

        public TokenService(ITokenRepository tokenRepository)
        {
            _tokenRepository = tokenRepository;
        }

        public async Task<Token> GetByIdAsync(int id)
        {
            return await _tokenRepository.GetByIdAsync(id);
        }

        public async Task<Token> GetByValueAsync(string value)
        {
            return await _tokenRepository.GetByValueAsync(value);
        }

        public async Task<List<Token>> GetByCompanyIdAsync(int companyId)
        {
            return await _tokenRepository.GetByCompanyIdAsync(companyId);
        }

        public async Task<int> CreateAsync(Token token)
        {
            return await _tokenRepository.CreateAsync(token);
        }

        public async Task UpdateAsync(Token token)
        {
            await _tokenRepository.UpdateAsync(token);
        }

        public async Task DeleteAsync(int id)
        {
            await _tokenRepository.DeleteAsync(id);
        }

        public async Task DecreaseRemainingUsesAsync(int id)
        {
            await _tokenRepository.DecreaseRemainingUsesAsync(id);
        }
    }
} 