using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

namespace BoschBot.Services
{
    public class TexRenderService : ITexRenderService
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly HttpClient httpClient;

        public TexRenderService(
            IConfiguration configuration,
            ILogger<TexRenderService> logger,
            HttpClient httpClient
        )
        {
            this.configuration = configuration;
            this.logger = logger;
            this.httpClient = httpClient;
        }

        public async Task<Image> RenderExpression(string expression)
        {
            throw new NotImplementedException();
        }
    }
}
