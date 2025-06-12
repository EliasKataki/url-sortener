using System.Threading.Tasks;
using UrlShortener.Domain.Entities;

namespace UrlShortener.API.Services
{
    public interface IUrlClickService
    {
        Task<UrlClick> GetByIdAsync(int id);
        Task<int> CreateAsync(UrlClick urlClick);
        Task DeleteAsync(int id);
    }
} 