using Polly;
using System;
using System.Net;
using System.Net.Http;

namespace Micromesh.Factories
{
    public class RetryPolicyFactory
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(HttpStatusCode statusCode, int retryCount)
        {
            return Policy<HttpResponseMessage>
                .HandleResult(msg => msg.StatusCode == statusCode)
                .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy<TException>(int retryCount) where TException : Exception
        {
            return Policy<HttpResponseMessage>
                .Handle<TException>()
                .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
