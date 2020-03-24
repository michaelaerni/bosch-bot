using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SixLabors.ImageSharp;

namespace BoschBot.Services
{
    public class TexRenderService : ITexRenderService
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly HttpClient httpClient;

        private const string BASE_ADDRESS = "http://rtex.probablyaweb.site/";

        // TODO: Make this configurable (ideally even use a template file)
        // TODO: This is a very crude template and breaks probably in a thousand ways, use a "real" one
        private const string TEMPLATE_PREFIX = @"\documentclass{article}\pagenumbering{gobble}\begin{document}$";
        private const string TEMPLATE_SUFFIX = @"$\end{document}";

        public TexRenderService(
            IConfiguration configuration,
            ILogger<TexRenderService> logger,
            HttpClient httpClient
        )
        {
            this.configuration = configuration;
            this.logger = logger;
            this.httpClient = httpClient;

            // Configure HTTP client
            this.httpClient.BaseAddress = new Uri(BASE_ADDRESS);
            this.httpClient.DefaultRequestHeaders.Add("User-Agent", "BoschBot");
        }

        public async Task<Image> RenderExpression(string expression)
        {
            // FIXME: This is hacky PoC code, refactor this into something more sensible

            if(string.IsNullOrEmpty(expression))
            {
                throw new ArgumentException("Expression must be a non-empty string");
            }

            var requestDocument = TEMPLATE_PREFIX + "\n" + expression + "\n" + TEMPLATE_SUFFIX;

            // Render document
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v2")
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(
                        new RenderRequest() { Code = requestDocument, Format = "png" }
                    )
                )
            };
            var responseMessage = await httpClient.SendAsync(requestMessage);
            responseMessage.EnsureSuccessStatusCode();

            var responseString = await responseMessage.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<RenderResponse>(responseString);
            if(response.Status != "success")
            {
                logger.LogDebug("Calling LaTeX render service failed, failure log (might be null): {0}", response.Log);
                throw new Exception($"Rendering failed on service with status {response.Status}: {response.ErrorDescription}");
            }
            logger.LogDebug("Rendered LaTeX document, resulting file name is {0}", response.ImageFileName);

            // Retrieve actual file content
            // TODO: Make sure encoding is sufficient! Also, this is very hacky
            using(var imageResponseStream = await httpClient.GetStreamAsync($"/api/v2/{HttpUtility.UrlEncode(response.ImageFileName)}"))
            {
                return Image.Load(imageResponseStream);
            }
        }

        private class RenderRequest
        {
            [JsonProperty("code", Required = Required.Always)]
            public string Code { get; set; }

            [JsonProperty("format", Required = Required.Always)]
            public string Format { get; set; }
        }

        private class RenderResponse
        {
            [JsonProperty("status", Required = Required.Always)]
            public string Status { get; set; }

            [JsonProperty("log")]
            public string Log { get; set; }

            [JsonProperty("description")]
            public string ErrorDescription { get; set; }

            [JsonProperty("filename")]
            public string ImageFileName { get; set; }
        }
    }
}
