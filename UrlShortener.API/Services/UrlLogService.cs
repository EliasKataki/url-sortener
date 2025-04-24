using Microsoft.EntityFrameworkCore;
using UrlShortener.API.Data;
using UrlShortener.API.Models;

namespace UrlShortener.API.Services;

public class UrlLogService : IUrlLogService
{
    private readonly ApplicationDbContext _context;

    public UrlLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogUrlOperationAsync(string longUrl, string shortUrl, string operation, string? userAgent, string? ipAddress, bool isSuccessful, string? errorMessage = null)
    {
        var urlLog = new UrlLog
        {
            LongUrl = longUrl,
            ShortUrl = shortUrl,
            CreatedAt = DateTime.UtcNow,
            Operation = operation,
            UserAgent = userAgent,
            IpAddress = ipAddress,
            IsSuccessful = isSuccessful,
            ErrorMessage = errorMessage
        };

        await _context.UrlLogs.AddAsync(urlLog);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<UrlLog>> GetUrlLogsAsync()
    {
        return await _context.UrlLogs
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UrlLog>> GetUrlLogsByShortUrlAsync(string shortUrl)
    {
        return await _context.UrlLogs
            .Where(x => x.ShortUrl == shortUrl)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
} 