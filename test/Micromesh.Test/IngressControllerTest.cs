using Micromesh.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Net.Http;
using System.Threading;
using Micromesh.Controllers;

namespace Micromesh.Test.UnitTests
{
    [TestClass]
    public class IngressControllerTest
    {
        /// <summary>
        /// making sure that building the Uri works as expected
        /// </summary>
        [TestMethod]
        [DataRow("http://wwww.ya.ru", "/foo", "", "http://wwww.ya.ru/foo")]
        [DataRow("http://wwww.ya.ru", "/foo", "?a=b", "http://wwww.ya.ru/foo?a=b")]
        [DataRow("http://wwww.ya.ru", null, null, "http://wwww.ya.ru/")]
        [DataRow("http://wwww.ya.ru", null, "?a=b", "http://wwww.ya.ru/?a=b")]
        public void ContructUriTest(string host, string path, string query, string result)
        {
            var uri = IngressController.BuildUri(host, path, query);
            Assert.AreEqual(result, uri.ToString());
        }
    }
}
