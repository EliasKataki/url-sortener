using System.Data;

namespace UrlShortener.API.Infrastructure;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
} 