
using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace BoschBot.CommandModules
{
    public class BoschModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly IMemoryCache cache;
        private readonly Random random;

        public BoschModule(
            IConfiguration configuration,
            ILogger<BoschModule> logger,
            IMemoryCache cache
        )
        {
            this.configuration = configuration;
            this.logger = logger;
            this.cache = cache;
            this.random = new Random();
        }

        [Command("bosch", ignoreExtraArgs: true)]
        public async Task RandomSnippetAsync()
        {
            logger.LogDebug("Handling random snippet command");

            using(Context.Channel.EnterTypingState())
            {
                var boschImage = await LoadBoschImage();

                int patchSize = configuration.GetValue<int>("CommandModules:Bosch:patchSize");

                int xPosition = random.Next(boschImage.Width - patchSize);
                int yPosition = random.Next(boschImage.Height - patchSize);

                var cropRectangle = new Rectangle(xPosition, yPosition, patchSize, patchSize);
                var imagePatch = boschImage.Clone(ctx => ctx.Crop(cropRectangle));

                // Save image to stream
                using(var stream = new MemoryStream())
                {
                    imagePatch.SaveAsJpeg(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    await Context.Channel.SendFileAsync(
                        stream: stream,
                        filename: "bosch.jpg",
                        text: "Oh my bosch!"
                    );
                }
            }
        }

        private enum CacheKeys
        {
            BoschImage
        }

        private async Task<Image> LoadBoschImage()
        {
            // TODO: Use config value for path or handle differently
            return await cache.GetOrCreateAsync<Image>(
                CacheKeys.BoschImage,
                entry =>
                {
                    entry.Priority = CacheItemPriority.NeverRemove;
                    return Task.FromResult(Image.Load("images/bosch_small.jpg"));
                }
            );
        }
    }
}
