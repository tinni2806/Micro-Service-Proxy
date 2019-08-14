using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Micromesh.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<ContentResult> ToContentResultAsync(this HttpResponseMessage response)
        {
            if (response.Content == null)
            {
                return new ContentResult
                {
                    StatusCode = (int)response.StatusCode
                };
            }

            using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()))
            {
                return new ContentResult
                {
                    Content = await reader.ReadToEndAsync(),
                    ContentType = response.Content.Headers.ContentType?.ToString(),
                    StatusCode = (int)response.StatusCode
                };
            }
        }
    }
}
