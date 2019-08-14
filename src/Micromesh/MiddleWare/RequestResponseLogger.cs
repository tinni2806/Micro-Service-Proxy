using Micromesh.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Micromesh.MiddleWare
{
    public class RequestResponseLogger
    {

        private RequestDelegate next;
        private IConfiguration configuration { get; }
        private IHttpClientFactory httpClientFactory;
        private ILogger logger;

        public RequestResponseLogger(RequestDelegate next, IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<RequestResponseLogger> logger)
        {
            this.next = next;
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            string service = String.Empty;
            try
            {
                var request = context.Request;
                service = context.Request.Headers[Headers.ForwardTo];

                if (!configuration.IsConversationLoggingEnabled() || !configuration.IsConversationLoggedForService(service))
                {
                    //If logging is not enabled or this service is not included in logs do nothing
                    await next(context);
                    return;
                }

                string contextIdentifier = context.Request.Headers[Headers.ContextIdentifier];
                Dictionary<string, string> properties = new Dictionary<string, string>();
                if (!String.IsNullOrWhiteSpace(contextIdentifier))
                {
                    properties.Add(Headers.ContextIdentifier, contextIdentifier);
                }


                request.EnableRewind();
                var requestPayload = new MemoryStream();
                await request.Body.CopyToAsync(requestPayload);
                requestPayload.Position = 0;
                request.Body.Position = 0;
                AddToFileRepository(requestPayload, $"{service} Request Payload", "Microservice Conversation Log", properties);


                var originalBodyStream = context.Response.Body;
                //Original body stream is not readable so we temporarily replace it by a memory stream
                using (var memoryStream = new MemoryStream())
                {
                    context.Response.Body = memoryStream;
                    //Have MVC controller handle the request
                    await next(context);

                    //Read the response from the memory stream into the payload
                    memoryStream.Position = 0;
                    var responsePayload = new MemoryStream();
                    await memoryStream.CopyToAsync(responsePayload);
                    responsePayload.Position = 0;
                    AddToFileRepository(responsePayload, $"{service} Response Payload", "Microservice Conversation Log", properties);
                    
                    //retrurn the reponse stream to its original state
                    memoryStream.Position = 0;
                    await memoryStream.CopyToAsync(originalBodyStream);
                    context.Response.Body = originalBodyStream;

                }

            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to log Request/Response for Service {service} , {ex.Message}");
            }
        }

        public async Task AddToFileRepository(Stream data, string filename, string description, IEnumerable<KeyValuePair<string, string>> properties, bool encrypt= false)
        {
            try
            {
                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StringContent(description), "\"Description\"");
                    content.Add(new StringContent(encrypt.ToString()), "\"IsEncrypted\"");
                    for (int i = 0; i < properties.Count(); i++)
                    {
                        var pair = properties.ElementAt(i);
                        content.Add(new StringContent(pair.Key), $"\"Properties[{i}].Key\"");
                        content.Add(new StringContent(pair.Value), $"\"Properties[{i}].Value\"");
                    }
                    content.Add(new StreamContent(data), "\"File\"", filename);

                    var client = httpClientFactory.CreateClient(HttpClients.ResilientClient);
                    var version = GetFileRepoVersion();
                    client.BaseAddress = new Uri(configuration.GetHostFromServicePool("FileRepository"));
                    var result = await client.PostAsync($"v{version}/files", content);

                    if (!result.IsSuccessStatusCode)
                    {
                        logger.LogError($"Saving file {filename} to File Repository returned status code {result.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to add file {filename} to File Repository , {ex.Message}");
            }
        }

        private int GetFileRepoVersion()
        {
            var section = configuration.GetSection("RequestResponseLoggingMiddleWare:FileRepositoryVersion");
            if (!section.Exists())
            {
                throw new MeshException("FileRepositoryVersion has not been configured");
            }

            return section.Get<int>();
        }
    }

}
