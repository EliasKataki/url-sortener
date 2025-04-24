using UrlShortener.API.Models;

namespace UrlShortener.API.Services;

public interface IUrlLogService
{
    Task LogUrlOperationAsync(string longUrl, string shortUrl, string operation, string? userAgent, string? ipAddress, bool isSuccessful, string? errorMessage = null);
    Task<IEnumerable<UrlLog>> GetUrlLogsAsync();
    Task<IEnumerable<UrlLog>> GetUrlLogsByShortUrlAsync(string shortUrl);
} 