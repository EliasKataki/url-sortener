using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using UrlShortener.API.Infrastructure;
using UrlShortener.Domain.Entities;

namespace UrlShortener.API.Repositories
{
    public class UrlClickRepository : IUrlClickRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UrlClickRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<UrlClick> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<UrlClick>(
                "SELECT * FROM UrlClicks WHERE Id = @Id",
                new { Id = id });
        }

        public async Task<List<UrlClick>> GetByUrlIdsAsync(List<int> urlIds)
        {
            if (urlIds == null || urlIds.Count == 0)
            {
                return new List<UrlClick>();
            }

            using var connection = _connectionFactory.CreateConnection();
            var clicks = await connection.QueryAsync<UrlClick>(
                "SELECT * FROM UrlClicks WHERE UrlId IN @UrlIds",
                new { UrlIds = urlIds });
            return clicks.AsList();
        }

        public async Task<int> CreateAsync(UrlClick urlClick)
        {
            if (urlClick.Latitude.HasValue)
            {
                urlClick.Latitude = Math.Round(urlClick.Latitude.Value, 6);
            }
            
            if (urlClick.Longitude.HasValue)
            {
                urlClick.Longitude = Math.Round(urlClick.Longitude.Value, 6);
            }

            const string sql = @"
                INSERT INTO UrlClicks (UrlId, IpAddress, UserAgent, ClickedAt, Latitude, Longitude, MarkerType)
                VALUES (@UrlId, @IpAddress, @UserAgent, @ClickedAt, @Latitude, @Longitude, @MarkerType);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleAsync<int>(sql, urlClick);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                "DELETE FROM UrlClicks WHERE Id = @Id",
                new { Id = id });
        }
    }
} 