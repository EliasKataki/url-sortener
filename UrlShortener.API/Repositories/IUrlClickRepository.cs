using System.Collections.Generic;
using System.Threading.Tasks;
using UrlShortener.Domain.Entities;

namespace UrlShortener.API.Repositories
{
    public interface IUrlClickRepository
    {
        Task<UrlClick> GetByIdAsync(int id);
        Task<List<UrlClick>> GetByUrlIdsAsync(List<int> urlIds);
        Task<int> CreateAsync(UrlClick urlClick);
        Task DeleteAsync(int id);
    }
} 