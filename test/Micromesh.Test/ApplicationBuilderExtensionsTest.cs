using Micromesh.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace Micromesh.Test
{
    [TestClass]
    public class ApplicationBuilderExtensionsTest
    {
        private readonly IDictionary<string, object> _appProperties;

        public ApplicationBuilderExtensionsTest()
        {
            _appProperties = new Dictionary<string, object>();
        }

        [TestMethod]
        [DataRow("Debug", LogLevel.Debug)]
        [DataRow("Information", LogLevel.Information)]
        [DataRow(null, LogLevel.Information)]
        public void TestUseApplicationInsightsLogger_SetsLogLevel(string returnValue, LogLevel expected)
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(m => m.GetSection(It.Is<string>(s => s.Contains("Logging:LogLevel:Default"))).Value).Returns(returnValue);

            var target = CreateApplicationBuilder(mockConfig.Object);
            target.UseApplicationInsightsLogger();

            var actual = target.Properties["LogLevel"];

            Assert.AreEqual(expected, actual);
        }

        private IApplicationBuilder CreateApplicationBuilder(IConfiguration configuration)
        {
            var mockAppBuilder = new Mock<IApplicationBuilder>();
            mockAppBuilder.SetupGet(p => p.Properties).Returns(_appProperties);
            mockAppBuilder.SetupGet(m => m.ApplicationServices).Returns(
                (new ServiceCollection()
                .AddLogging()
                .AddSingleton(configuration))
                .BuildServiceProvider()
            );
            return mockAppBuilder.Object;
        }
    }
}
