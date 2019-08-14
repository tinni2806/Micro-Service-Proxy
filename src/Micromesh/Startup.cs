using System;
using System.Linq;
using Micromesh.Extensions;
using Micromesh.Factories;
using Micromesh.Handlers;
using Micromesh.MiddleWare;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;

namespace Micromesh
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddApiVersioning(opt =>
            {
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.DefaultApiVersion = new ApiVersion(1, 0);
                opt.ReportApiVersions = true;
                opt.ApiVersionReader = new HeaderApiVersionReader(Headers.MicromeshVersion);
            });

            var retryCounts = Configuration.GetRetryCounts();
            var exceptionRetryCount = Configuration.GetExceptionRetryCount();

            services
                .AddHttpClient(HttpClients.ResilientClient)
                .ConfigureHttpMessageHandlerBuilder(h =>
                {
                    h.PrimaryHandler = new HttpClientRetryHandler();
                })
                .AddPolicyHandler(RetryPolicyFactory.GetRetryPolicy<HttpRequestException>(exceptionRetryCount))
                .AddPolicyHandler(RetryPolicyFactory.GetRetryPolicy(HttpStatusCode.NotFound, retryCounts[HttpStatusCode.NotFound]))
                .AddPolicyHandler(RetryPolicyFactory.GetRetryPolicy(HttpStatusCode.RequestTimeout, retryCounts[HttpStatusCode.RequestTimeout]))
                .AddPolicyHandler(RetryPolicyFactory.GetRetryPolicy(HttpStatusCode.InternalServerError, retryCounts[HttpStatusCode.InternalServerError]))
                .AddPolicyHandler(RetryPolicyFactory.GetRetryPolicy(HttpStatusCode.ServiceUnavailable, retryCounts[HttpStatusCode.ServiceUnavailable]));

            ConfigureAuth(services);
        }

        protected virtual void ConfigureAuth(IServiceCollection services)
        {
            services
                .AddAuthorization()
                .AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = Configuration.GetValue<string>("Security:Authority");
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "micromesh";
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var origins = (Configuration.GetValue<string>("CorsOrigins") ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim())
                .ToArray();

            app.UseCors(builder => builder
                .WithOrigins(origins)
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod());

            app.UseAuthentication();
            app.UseApplicationInsightsLogger();
            app.UseMiddleware<RequestResponseLogger>();
            app.UseMvc();
        }
    }
}
