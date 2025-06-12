using System.Collections.Generic;
using System.Threading.Tasks;
using UrlShortener.API.Repositories;
using UrlShortener.Domain.Entities;

namespace UrlShortener.API.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyService(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<List<Company>> GetAllAsync()
        {
            return await _companyRepository.GetAllAsync();
        }

        public async Task<Company> GetByIdAsync(int id)
        {
            return await _companyRepository.GetByIdAsync(id);
        }

        public async Task<int> CreateAsync(Company company)
        {
            return await _companyRepository.CreateAsync(company);
        }

        public async Task UpdateAsync(Company company)
        {
            await _companyRepository.UpdateAsync(company);
        }

        public async Task DeleteAsync(int id)
        {
            await _companyRepository.DeleteAsync(id);
        }
    }
} 