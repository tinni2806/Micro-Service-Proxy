using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Micromesh.Handlers
{
    public class HttpClientRetryHandler : HttpClientHandler
    {
        public HttpClientRetryHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Content == null) return base.SendAsync(request, cancellationToken);

            request.Properties.TryGetValue(RequestPropertyKeys.RetryAttempt, out var retryAttemptValueObject);
            var retryAttempt = (int?)retryAttemptValueObject ?? 0;

            if (retryAttempt > 0)
            {
                request.Content = PrepareContentForRetry(request.Content);
            }

            request.Properties[RequestPropertyKeys.RetryAttempt] = retryAttempt + 1;

            return base.SendAsync(request, cancellationToken);
        }

        private static StreamContent PrepareContentForRetry(HttpContent content)
        {
            var ms = new MemoryStream();

            content
                .ReadAsStreamAsync()
                .Result
                .CopyToAsync(ms)
                .Wait();

            ms.Position = 0;

            var streamContent = new StreamContent(ms);

            CopyHeaders(content, streamContent);

            return streamContent;
        }

        private static void CopyHeaders(HttpContent from, HttpContent to)
        {
            foreach (var header in from.Headers)
            {
                to.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

    }
}
