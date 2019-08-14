using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Micromesh.Extensions
{
    public static class ConfigurationExtensions
    {
        private static readonly Random Random = new Random();

        public static string GetHostFromServicePool(this IConfiguration configuration, string service)
        {
            var section = configuration.GetSection($"Services:{service}");
            if (section == null)
            {
                throw new MeshException($"The {service} has not been registered in the service pool");
            }
            var servicePool = section.Get<List<string>>();
            if (servicePool == null || !servicePool.Any())
            {
                throw new MeshException($"There are no configured hosts for {service} in the service pool");
            }

            var randomHost = Random.Next() % servicePool.Count;
            return servicePool.ElementAt(randomHost);
        }

        public static Dictionary<HttpStatusCode, int> GetRetryCounts(this IConfiguration configuration)
        {
            var result = new Dictionary<HttpStatusCode, int>();

            var values = configuration.GetSection($"RetryPolicy:StatusCodeRetryCounts").GetChildren().ToList();
            if (!values.Any())
            {
                throw new MeshException("There are no configured values for RetryCounts");
            }

            foreach (var item in values)
            {
                int.TryParse(item.Key, out var statusCodeIntValue);
                var retryCountValid = int.TryParse(item.Value, out var retryCount);
                if (!Enum.IsDefined(typeof(HttpStatusCode), statusCodeIntValue) || !retryCountValid)
                {
                    throw new MeshException($"Invalid configuration for retry counts key: {item.Key}, value {item.Value}");
                }

                result[(HttpStatusCode)statusCodeIntValue] = retryCount;
            }

            return result;
        }

        public static int GetExceptionRetryCount(this IConfiguration configuration)
        {
            var setting = configuration["RetryPolicy:ExceptionRetryCount"];

            if (!int.TryParse(setting, out var retryCount))
            {
                throw new MeshException("There are no configured values for RetryCounts");
            }

            return retryCount;
        }


        public static bool IsConversationLoggedForService(this IConfiguration configuration, string service)
        {
            var services = configuration.GetConversationLoggingServices();
            return services.Contains(service);
        }

        public static bool IsConversationLoggingEnabled(this IConfiguration configuration)
        {
            var enabled = configuration.GetSection("ConversationLogging:LoggingEnabled").Get<bool>(); 
            return enabled;
        }

        private static List<string> GetConversationLoggingServices(this IConfiguration configuration)
        {
            var section = configuration.GetSection("ConversationLogging:IncludedServices");
            var includedServices = section.Get<List<string>>();

            if (includedServices == null)
                includedServices = new List<string>();

            return includedServices;
        }

    }
}
