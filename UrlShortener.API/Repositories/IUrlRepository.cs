using System.Collections.Generic;
using System.Threading.Tasks;
using UrlShortener.Domain.Entities;

namespace UrlShortener.API.Repositories
{
    public interface IUrlRepository
    {
        Task<Url> GetByIdAsync(int id);
        Task<Url> GetByShortUrlAsync(string shortUrl);
        Task<List<Url>> GetByCompanyIdAsync(int companyId);
        Task<int> CreateAsync(Url url);
        Task UpdateAsync(Url url);
        Task DeleteAsync(int id);
        Task IncrementClickCountAsync(int id);
    }
} 