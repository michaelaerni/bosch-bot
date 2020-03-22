
using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace BoschBot.CommandModules
{
    public class BoschModule : ModuleBase<SocketCommandContext>, IDisposable
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private Random random;
        private readonly Image boschImage;

        public BoschModule(IConfiguration configuration, ILogger<BoschModule> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.random = new Random();
            // TODO: This file is loaded on every request, cache it!
            this.boschImage = Image.Load("images/bosch_small.jpg"); // TODO: Use config value or handle differently
        }

        [Command("bosch", ignoreExtraArgs: true)]
        public async Task RandomSnippetAsync()
        {
            logger.LogDebug("Handling random snippet command");

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

        public void Dispose()
        {
            boschImage?.Dispose();
        }
    }
}
