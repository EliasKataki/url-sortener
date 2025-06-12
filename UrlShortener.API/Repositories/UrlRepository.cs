using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using UrlShortener.API.Infrastructure;
using UrlShortener.Domain.Entities;

namespace UrlShortener.API.Repositories
{
    public class UrlRepository : IUrlRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UrlRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Url> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<Url>(
                "SELECT * FROM Urls WHERE Id = @Id",
                new { Id = id });
        }

        public async Task<Url> GetByShortUrlAsync(string shortUrl)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<Url>(
                "SELECT * FROM Urls WHERE ShortUrl = @ShortUrl",
                new { ShortUrl = shortUrl });
        }

        public async Task<List<Url>> GetByCompanyIdAsync(int companyId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var urls = await connection.QueryAsync<Url>(
                "SELECT * FROM Urls WHERE CompanyId = @CompanyId",
                new { CompanyId = companyId });
            return urls.AsList();
        }

        public async Task<int> CreateAsync(Url url)
        {
            const string sql = @"
                INSERT INTO Urls (LongUrl, ShortUrl, CreatedAt, ExpiresAt, ClickCount, CompanyId)
                VALUES (@LongUrl, @ShortUrl, @CreatedAt, @ExpiresAt, @ClickCount, @CompanyId);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleAsync<int>(sql, url);
        }

        public async Task UpdateAsync(Url url)
        {
            const string sql = @"
                UPDATE Urls 
                SET LongUrl = @LongUrl,
                    ExpiresAt = @ExpiresAt,
                    ClickCount = @ClickCount
                WHERE Id = @Id";

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, url);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                "DELETE FROM Urls WHERE Id = @Id",
                new { Id = id });
        }

        public async Task IncrementClickCountAsync(int id)
        {
            const string sql = @"
                UPDATE Urls 
                SET ClickCount = ClickCount + 1
                WHERE Id = @Id";

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
} 