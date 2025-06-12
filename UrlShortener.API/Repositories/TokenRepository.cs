using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using UrlShortener.API.Infrastructure;
using UrlShortener.Domain.Entities;

namespace UrlShortener.API.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public TokenRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Token> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<Token>(
                "SELECT * FROM Tokens WHERE Id = @Id",
                new { Id = id });
        }

        public async Task<Token> GetByValueAsync(string value)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<Token>(
                "SELECT * FROM Tokens WHERE Value = @Value",
                new { Value = value });
        }

        public async Task<List<Token>> GetByCompanyIdAsync(int companyId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var tokens = await connection.QueryAsync<Token>(
                "SELECT * FROM Tokens WHERE CompanyId = @CompanyId",
                new { CompanyId = companyId });
            return tokens.AsList();
        }

        public async Task<int> CreateAsync(Token token)
        {
            const string sql = @"
                INSERT INTO Tokens (Value, ExpiresAt, CreatedAt, RemainingUses, CompanyId)
                VALUES (@Value, @ExpiresAt, @CreatedAt, @RemainingUses, @CompanyId);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleAsync<int>(sql, token);
        }

        public async Task UpdateAsync(Token token)
        {
            const string sql = @"
                UPDATE Tokens 
                SET ExpiresAt = @ExpiresAt,
                    RemainingUses = @RemainingUses
                WHERE Id = @Id";

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, token);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                "DELETE FROM Tokens WHERE Id = @Id",
                new { Id = id });
        }

        public async Task DecreaseRemainingUsesAsync(int id)
        {
            const string sql = @"
                UPDATE Tokens 
                SET RemainingUses = RemainingUses - 1
                WHERE Id = @Id AND RemainingUses > 0";

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
} 