using Micromesh.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Micromesh.Test
{
    [TestClass]
    public class HttpRequestExtensionsTest
    {
        private readonly Uri _testUri;
        private readonly DefaultHttpContext _mockHttpContext;
        private readonly IPAddress _remoteIpAddress;

        public HttpRequestExtensionsTest()
        {
            _testUri = new Uri("http://localhost");

            _mockHttpContext = new DefaultHttpContext();
            _mockHttpContext.Request.Method = "GET";
            _mockHttpContext.Connection.RemoteIpAddress = _remoteIpAddress = IPAddress.Parse("127.0.0.1");
        }

        [TestMethod]
        [DataRow("GET")]
        [DataRow("POST")]
        [DataRow("PUT")]
        [DataRow("PATCH")]
        [DataRow("DELETE")]
        public void TestMethodIsSet(string data)
        {
            var target = _mockHttpContext.Request;
            target.Method = data;

            var expected = new HttpMethod(data);

            var actual = target
                .ToHttpRequestMessage(_testUri)
                .Method;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestAbsoluteUriIsSet()
        {
            var target = _mockHttpContext.Request;

            var expected = _testUri;

            var actual = target
                .ToHttpRequestMessage(_testUri)
                .RequestUri;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("Test-Header")]
        public void TestHeaderIsSet(string header)
        {
            var target = _mockHttpContext.Request;
            target.Headers.Add(header, new StringValues("test"));

            var expected = target.Headers[header];

            var actual = target
                .ToHttpRequestMessage(_testUri)
                .Headers
                .GetValues(header)
                .FirstOrDefault();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("68.14.178.9")]
        [DataRow("60.104.18.29, 10.0.1.12")]
        public void TestHeaderXForwardForIsChained(string data)
        {
            var target = _mockHttpContext.Request;
            target.Headers.Add(Headers.XForwardedFor, new StringValues(data));

            var expected = $"{data}, {_remoteIpAddress}";

            var actual = target
                .ToHttpRequestMessage(_testUri)
                .Headers
                .GetValues(Headers.XForwardedFor)
                .FirstOrDefault();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("Host")]
        [DataRow("Forward-To")]
        public void TestHeadersAreNotSet(string header)
        {
            var target = _mockHttpContext.Request;
            target.Headers.Add(header, new StringValues("Test"));

            target
                .ToHttpRequestMessage(_testUri)
                .Headers
                .TryGetValues(header, out IEnumerable<string> actual);

            Assert.IsNull(actual);
        }

        [TestMethod]
        [DataRow("plain-text")]
        [DataRow("[{ json:\"\" }]")]
        [DataRow("<xml />")]
        public void TestContentIsSet(string expected)
        {
            var target = _mockHttpContext.Request;

            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            {
                writer.Write(expected);
                writer.Flush();

                ms.Position = 0;

                target.Body = ms;
                target.ContentLength = ms.Length;

                string actual;

                using (var reader = new StreamReader(target
                    .ToHttpRequestMessage(_testUri)
                    .Content
                    .ReadAsStreamAsync()
                    .Result))
                {
                    actual = reader.ReadToEnd();
                }

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow("application/json")]
        [DataRow("application/xml")]
        [DataRow("text/plain")]
        [DataRow("text/xml")]
        public void TestContentTypeIsSet(string data)
        {
            var target = _mockHttpContext.Request;
            var expected = target.ContentType = data;

            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            {
                writer.Write(expected);
                writer.Flush();

                ms.Position = 0;

                target.Body = ms;
                target.ContentLength = ms.Length;

                var actual = target
                    .ToHttpRequestMessage(_testUri)
                    .Content
                    .Headers
                    .ContentType
                    .ToString();

                Assert.AreEqual(expected, actual);
            }
        }
    }
}
