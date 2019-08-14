using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net.Http;

namespace Micromesh.Extensions
{
    public static class HttpRequestExtensions
    {
        public static HttpRequestMessage ToHttpRequestMessage(this HttpRequest request, Uri requestUri) 
            => new HttpRequestMessage()
            .SetMethod(request)
            .SetAbsoluteUri(requestUri)
            .SetHeaders(request)
            .SetContent(request)
            .SetContentType(request);

        private static void ConcatXForwardedFor(this HttpRequest request)
        {
            var remoteIpAddress = request.HttpContext.Connection.RemoteIpAddress;

            if (remoteIpAddress == null) return;

            var currentChain = request.Headers.SingleOrDefault(h => h.Key.Equals(Headers.XForwardedFor, StringComparison.InvariantCultureIgnoreCase)).Value;
            var concatenatedChain = (currentChain.Any())
                ? string.Join(',', currentChain.ToArray()) + $", {remoteIpAddress}"
                : remoteIpAddress.ToString();

            request.Headers[Headers.XForwardedFor] = concatenatedChain;
        }

        private static HttpRequestMessage Set(this HttpRequestMessage message, Action<HttpRequestMessage> config, bool applyIf = true)
        {
            if (!applyIf) return message;
            config.Invoke(message);
            return message;
        }

        private static HttpRequestMessage SetAbsoluteUri(this HttpRequestMessage message, Uri uri)
            => message.Set(m => m.RequestUri = uri);

        private static HttpRequestMessage SetContent(this HttpRequestMessage message, HttpRequest request)
            => message.Set(m => m.Content = new StreamContent(request.Body), applyIf: request.ContentLength > 0); 

        private static HttpRequestMessage SetContentType(this HttpRequestMessage message, HttpRequest request)
            => message.Set(m => m.Content.Headers.Add(Headers.ContentType, request.ContentType), applyIf: request.Headers.ContainsKey(Headers.ContentType));

        private static HttpRequestMessage SetHeaders(this HttpRequestMessage message, HttpRequest request)
        {
            request.ConcatXForwardedFor();

            request.Headers
                .Where(h => !h.Key.Equals(Headers.Host, StringComparison.InvariantCultureIgnoreCase)
                    && !h.Key.Equals(Headers.ForwardTo, StringComparison.InvariantCultureIgnoreCase))
                .Aggregate(message, (acc, h) => acc.Set(m => m.Headers.TryAddWithoutValidation(h.Key, h.Value.AsEnumerable())));

            return message;
        }

        private static HttpRequestMessage SetMethod(this HttpRequestMessage message, HttpRequest request)
            => message.Set(m => m.Method = new HttpMethod(request.Method));
    }
}
