using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace Micromesh.Test.IntegrationTests
{
    [TestClass]
    public class ProxyRequestIntegrationTest 
    {
        private readonly HttpClient _client;

        public ProxyRequestIntegrationTest()
        {
            var testServer = new TestServer(new WebHostBuilder()
                .UseEnvironment("Development")
                .UseConfiguration(new ConfigurationBuilder()
                    .AddJsonFile("appsettings.test.json")
                    .Build()
                )
                .UseStartup<TestStartup>());
            _client = testServer.CreateClient();
        }

        [TestMethod]
        public async Task TestGetProxyRequest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/get");

            AddIntegrationTestHeaders(request);

            var response = await _client.SendAsync(request);
            Assert.IsTrue(response.IsSuccessStatusCode);
        }

        [TestMethod]
        [DataRow("POST")]
        [DataRow("PUT")]
        [DataRow("PATCH")]
        [DataRow("DELETE")]
        public async Task TestProxyRequestWithBody(string method)
        {
            const string content = "foobar echo";
            var request = new HttpRequestMessage(new HttpMethod(method), $"/{method}")
            {
                Content = new StringContent(content),
            };
            request.Content.Headers.ContentLength = content.Length;

            AddIntegrationTestHeaders(request);

            var response = await _client.SendAsync(request);
            Assert.IsTrue(response.IsSuccessStatusCode);

            var resultBody = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(resultBody.Contains("foobar echo"));
        }

        private static void AddIntegrationTestHeaders(HttpRequestMessage request)
        {
            request.Headers.Add(Headers.ForwardTo, "postman");
            request.Headers.Add(Headers.MicromeshVersion, "1.0");
        }

    }
}
