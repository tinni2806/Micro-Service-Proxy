using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Micromesh
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseApplicationInsights()
                .UseStartup<Startup>();

        public static IWebHost BuildWebHost(string[] args) =>
            CreateWebHostBuilder(args).Build();
    }
}
