using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace Micromesh.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseApplicationInsightsLogger(this IApplicationBuilder app)
        {
            var serviceProvider = app.ApplicationServices;
            var configuration = serviceProvider.GetService(typeof(IConfiguration)) as IConfiguration;

            var defaultLogLevel = configuration?.GetSection("Logging:LogLevel:Default")?.Value;
            var logLevel = Enum.Parse<LogLevel>(defaultLogLevel ?? "Information");

            var loggerFactory = serviceProvider.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            loggerFactory.AddApplicationInsights(serviceProvider, logLevel);

            app.Properties["LogLevel"] = logLevel;
        }
    }
}
