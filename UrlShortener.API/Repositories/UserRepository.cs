using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Dapper;
using UrlShortener.API.Infrastructure;
using UrlShortener.Domain.Entities;

namespace UrlShortener.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            // Önce kullanıcı bilgilerini alalım
            var user = await connection.QuerySingleOrDefaultAsync<User>(
                @"SELECT * FROM Users WHERE Id = @Id",
                new { Id = id });

            if (user != null)
            {
                // Sonra kullanıcının firma eşleştirmelerini ayrı bir sorgu ile alalım
                var companyIds = await connection.QueryAsync<int>(
                    @"SELECT CompanyId FROM UserCompanies WHERE UserId = @UserId",
                    new { UserId = user.Id });

                user.CompanyIds = companyIds.ToList();
                Console.WriteLine($"User {id} found with company IDs: {string.Join(", ", user.CompanyIds)}");
            }

            return user;
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            // Önce kullanıcı bilgilerini alalım
            var user = await connection.QuerySingleOrDefaultAsync<User>(
                @"SELECT * FROM Users WHERE Email = @Email AND IsActive = 1",
                new { Email = email });

            if (user != null)
            {
                // Sonra kullanıcının firma eşleştirmelerini ayrı bir sorgu ile alalım
                var companyIds = await connection.QueryAsync<int>(
                    @"SELECT CompanyId FROM UserCompanies WHERE UserId = @UserId",
                    new { UserId = user.Id });

                user.CompanyIds = companyIds.ToList();
                Console.WriteLine($"User {email} found with company IDs: {string.Join(", ", user.CompanyIds)}");
            }

            return user;
        }

        public async Task<Guid> CreateAsync(User user)
        {
            const string sql = @"
                INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, CreatedAt, IsActive, RoleId)
                VALUES (@Id, @Email, @PasswordHash, @FirstName, @LastName, @CreatedAt, @IsActive, @RoleId)";

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, user);
            return user.Id;
        }

        public async Task UpdateAsync(User user)
        {
            const string sql = @"
                UPDATE Users 
                SET FirstName = @FirstName,
                    LastName = @LastName,
                    Email = @Email,
                    RoleId = @RoleId,
                    IsActive = @IsActive,
                    LastLoginAt = @LastLoginAt
                WHERE Id = @Id";

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, user);
        }

        public async Task DeleteAsync(Guid id)
        {
            const string sql = "DELETE FROM Users WHERE Id = @Id";

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            
            // Önce tüm kullanıcıları alalım
            var users = await connection.QueryAsync<User>(
                @"SELECT * FROM Users ORDER BY LastLoginAt DESC");

            // Her kullanıcı için firma eşleştirmelerini alalım
            foreach (var user in users)
            {
                var companyIds = await connection.QueryAsync<int>(
                    @"SELECT CompanyId FROM UserCompanies WHERE UserId = @UserId",
                    new { UserId = user.Id });

                user.CompanyIds = companyIds.ToList();
                Console.WriteLine($"User {user.Email} found with company IDs: {string.Join(", ", user.CompanyIds)}");
            }

            return users;
        }

        public async Task UpdateUserCompaniesAsync(Guid userId, List<int> companyIds)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            
            using var transaction = connection.BeginTransaction();

            try
            {
                // Önce kullanıcı bilgilerini alalım
                var user = await connection.QuerySingleOrDefaultAsync<User>(
                    "SELECT * FROM Users WHERE Id = @UserId",
                    new { UserId = userId },
                    transaction);

                if (user == null)
                    throw new Exception("Kullanıcı bulunamadı");

                // Mevcut eşleştirmeleri sil
                await connection.ExecuteAsync(
                    "DELETE FROM UserCompanies WHERE UserId = @UserId",
                    new { UserId = userId },
                    transaction);

                // Yeni eşleştirmeleri ekle
                if (companyIds != null && companyIds.Any())
                {
                    var companies = await connection.QueryAsync<Company>(
                        "SELECT * FROM Companies WHERE Id IN @CompanyIds",
                        new { CompanyIds = companyIds },
                        transaction);

                    foreach (var company in companies)
                    {
                        await connection.ExecuteAsync(
                            @"INSERT INTO UserCompanies (UserId, CompanyId, UserName, CompanyName, CreatedAt)
                            VALUES (@UserId, @CompanyId, @UserName, @CompanyName, @CreatedAt)",
                            new
                            {
                                UserId = userId,
                                CompanyId = company.Id,
                                UserName = $"{user.FirstName} {user.LastName}",
                                CompanyName = company.Name,
                                CreatedAt = DateTime.UtcNow
                            },
                            transaction);
                    }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
} 