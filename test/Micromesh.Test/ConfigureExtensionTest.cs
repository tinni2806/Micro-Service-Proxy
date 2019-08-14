using System.Net;
using Micromesh.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Micromesh.Test
{
    [TestClass]
    public class ConfigureExtensionTest
    {
        public IConfiguration Configuration { get; }

        public ConfigureExtensionTest()
        {
           Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
        }

        [TestMethod]
        public void TestConfigureExtension_GetHostFromServicePool()
        {
            var result = Configuration.GetHostFromServicePool(TestConstants.PostmanService);

            Assert.IsTrue(result == TestConstants.PostmanHost);
        }


        [TestMethod]
        public void TestConfigureExtension_GetRetryCounts()
        {
            var retryCounts = Configuration.GetRetryCounts();
            Assert.IsTrue(retryCounts[HttpStatusCode.NotFound] == 1);
            Assert.IsTrue(retryCounts[HttpStatusCode.RequestTimeout] == 1);
        }


        [TestMethod]
        public void TestConfigureExtension_GetExceptionRetryCount()
        {
            var result = Configuration.GetExceptionRetryCount();
            Assert.IsTrue(result== 3);
        }
    }
}
