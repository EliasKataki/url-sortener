using System.Threading.Tasks;
using UrlShortener.API.Repositories;
using UrlShortener.Domain.Entities;

namespace UrlShortener.API.Services
{
    public class UrlClickService : IUrlClickService
    {
        private readonly IUrlClickRepository _urlClickRepository;

        public UrlClickService(IUrlClickRepository urlClickRepository)
        {
            _urlClickRepository = urlClickRepository;
        }

        public async Task<UrlClick> GetByIdAsync(int id)
        {
            return await _urlClickRepository.GetByIdAsync(id);
        }

        public async Task<int> CreateAsync(UrlClick urlClick)
        {
            return await _urlClickRepository.CreateAsync(urlClick);
        }

        public async Task DeleteAsync(int id)
        {
            await _urlClickRepository.DeleteAsync(id);
        }
    }
} 