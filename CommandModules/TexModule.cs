
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using BoschBot.Services;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

namespace BoschBot.CommandModules
{
    public class TexModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly ITexRenderService texRenderService;

        public TexModule(
            IConfiguration configuration,
            ILogger<TexModule> logger,
            ITexRenderService texRenderService
        )
        {
            this.configuration = configuration;
            this.logger = logger;
            this.texRenderService = texRenderService;
        }

        [Command("ttex", RunMode = RunMode.Async)]
        public async Task TestRenderAsync([Remainder]string expression)
        {
            logger.LogDebug("Handling test render command");

            using(Context.Channel.EnterTypingState())
            {
                try
                {
                    // Render LaTeX expression
                    // FIXME: Might employ caching of sorts
                    Image renderedResponse = await texRenderService.RenderExpression(expression);

                    // Reply with image
                    using(var stream = new MemoryStream())
                    {
                        renderedResponse.SaveAsPng(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        await Context.Channel.SendFileAsync(stream, "latex_expression.png");
                    }
                }
                catch(Exception ex)
                {
                    // TODO: More fine grained error handling
                    logger.LogError(ex, "Unexpected error while rendering LaTeX expression");

                    await ReplyAsync("Oops, something went wrong... Also, this is just a test command :grin:");
                }
            }
        }
    }
}
