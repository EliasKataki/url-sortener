using UrlShortener.API.Models;
using Microsoft.Extensions.Logging;

namespace UrlShortener.API.Helpers
{
    public static class LogHelper
    {
        public static void LogUrlOperation(ILogger logger, string operation, string message, string? url = null, int? urlId = null)
        {
            logger.LogInformation("[URL-{Operation}] {Message} | URL: {Url} | URL ID: {UrlId}", 
                operation, message, url ?? "N/A", urlId?.ToString() ?? "N/A");
        }

        public static void LogCompanyOperation(ILogger logger, string operation, string message, int? companyId = null)
        {
            logger.LogInformation("[COMPANY-{Operation}] {Message} | Company ID: {CompanyId}", 
                operation, message, companyId?.ToString() ?? "N/A");
        }

        public static void LogTokenOperation(ILogger logger, string operation, string message, string? token = null, int? companyId = null)
        {
            logger.LogInformation("[TOKEN-{Operation}] {Message} | Token: {Token} | Company ID: {CompanyId}", 
                operation, message, token ?? "N/A", companyId?.ToString() ?? "N/A");
        }

        public static void LogMapOperation(ILogger logger, string operation, string message, double? latitude = null, double? longitude = null, int? urlId = null)
        {
            logger.LogInformation("[MAP-{Operation}] {Message} | Location: ({Latitude}, {Longitude}) | URL ID: {UrlId}", 
                operation, message, latitude?.ToString() ?? "N/A", longitude?.ToString() ?? "N/A", urlId?.ToString() ?? "N/A");
        }

        public static void LogAuthOperation(ILogger logger, string operation, string message, string? ip = null)
        {
            logger.LogInformation("[AUTH-{Operation}] {Message} | IP: {Ip}", 
                operation, message, ip ?? "N/A");
        }

        public static void LogSystemOperation(ILogger logger, string operation, string message)
        {
            logger.LogInformation("[SYSTEM-{Operation}] {Message}", operation, message);
        }

        public static void LogDatabaseOperation(ILogger logger, string operation, string message, string? query = null)
        {
            logger.LogInformation("[DB-{Operation}] {Message} | Query: {Query}", 
                operation, message, query ?? "N/A");
        }

        public static void LogError(ILogger logger, LogCategory category, string operation, string message, Exception? exception = null)
        {
            logger.LogError("[{Category}-{Operation}] {Message} | Exception: {Exception}", 
                category.ToString(), operation, message, exception?.ToString() ?? "N/A");
        }

        public static void LogWarning(ILogger logger, LogCategory category, string operation, string message)
        {
            logger.LogWarning("[{Category}-{Operation}] {Message}", 
                category.ToString(), operation, message);
        }
    }
} 