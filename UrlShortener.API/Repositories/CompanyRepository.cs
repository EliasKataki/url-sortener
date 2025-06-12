using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using UrlShortener.API.Infrastructure;
using UrlShortener.Domain.Entities;

namespace UrlShortener.API.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CompanyRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<Company>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var companies = await connection.QueryAsync<Company>("SELECT * FROM Companies");
            return companies.AsList();
        }

        public async Task<Company> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<Company>(
                "SELECT * FROM Companies WHERE Id = @Id",
                new { Id = id });
        }

        public async Task<int> CreateAsync(Company company)
        {
            const string sql = @"
                INSERT INTO Companies (Name, CreatedAt)
                VALUES (@Name, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleAsync<int>(sql, company);
        }

        public async Task UpdateAsync(Company company)
        {
            const string sql = @"
                UPDATE Companies 
                SET Name = @Name
                WHERE Id = @Id";

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, company);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                "DELETE FROM Companies WHERE Id = @Id",
                new { Id = id });
        }
    }
} 