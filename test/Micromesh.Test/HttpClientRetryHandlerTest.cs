using Micromesh.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace Micromesh.Test.IntegrationTests
{
    [TestClass]
    public class HttpClientRetryHandlerTest
    {
        [TestMethod]
        public void TestRetryHandlerIncreasesRetryAttempt()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://postman-echo.com/post")
            {
                Content = new StringContent(string.Empty)
            };

            new HttpMessageInvoker(new HttpClientRetryHandler())
                .SendAsync(request, new CancellationToken())
                .Wait();

            Assert.IsTrue(request.Properties.ContainsKey(RequestPropertyKeys.RetryAttempt));
        }

        [TestMethod]
        public void TestRetryHandlerResetsStreamPosition()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://postman-echo.com/post")
            {
                Content = new StringContent("test")
            };
            request.Properties[RequestPropertyKeys.RetryAttempt] = 1;

            new StreamReader(request.Content.ReadAsStreamAsync().Result)
                .ReadToEnd();

            new HttpMessageInvoker(new HttpClientRetryHandler())
                .SendAsync(request, new CancellationToken())
                .Wait();

            const long expected = 0;
            var actual = new StreamReader(request.Content.ReadAsStreamAsync().Result)
                .BaseStream
                .Position;

            Assert.AreEqual(expected, actual);
        }
    }
}
