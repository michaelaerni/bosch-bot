
using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace BoschBot.CommandModules
{
    public class MatthiasModule : ModuleBase<SocketCommandContext>, IDisposable
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private Random random;
        private readonly Image matthiasImage;
        private readonly Font memeFont;

        public MatthiasModule(IConfiguration configuration, ILogger<BoschModule> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.random = new Random();
            // TODO: This file is loaded on every request, cache it!
            this.matthiasImage = Image.Load("images/matthias.jpg"); // TODO: Use config value or handle differently
            this.memeFont = SystemFonts.CreateFont("Liberation Sans", 42, SixLabors.Fonts.FontStyle.Bold); // TODO: Use config value or handle differently
        }

        [Command("matthias")]
        public async Task MemeAsync([Remainder] string caption)
        {
            logger.LogDebug("Handling meme command");

            string sanitizedCaption = caption.Trim(); // TODO: Sanitize more/better

            if(sanitizedCaption.Length == 0)
            {
                await ReplyAsync("Dude, I need a meme caption!");
            }
            else if(sanitizedCaption.Length > 400)
            {
                await ReplyAsync("Sorry but at most 400 characters.");
            }
            else
            {
                // TODO: Refactor this mess
                // TODO: Make things configurable

                var textGraphicOptions = new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    WrapTextWidth = 550
                };

                // Determine text block height
                var textBounds = TextMeasurer.Measure(
                    sanitizedCaption,
                    new RendererOptions(memeFont)
                    {
                        WrappingWidth = textGraphicOptions.WrapTextWidth
                    }
                );

                var targetImageSize = new Size(matthiasImage.Width, matthiasImage.Height + 50 + (int)Math.Ceiling(textBounds.Height));
                var intermediateImageSize = new Size(
                    (int)Math.Max(targetImageSize.Width, (matthiasImage.Width - Math.Ceiling(textGraphicOptions.WrapTextWidth)) + textBounds.Width),
                    targetImageSize.Height
                );

                var initialResizeOptions = new ResizeOptions()
                {
                    Mode = ResizeMode.BoxPad,
                    Size = intermediateImageSize,
                    Position = AnchorPositionMode.BottomLeft
                };
                var finalResizeOptions = new ResizeOptions()
                {
                    Mode = ResizeMode.Crop,
                    Size = targetImageSize,
                    Position = AnchorPositionMode.BottomLeft
                };

                var memeImage = matthiasImage.Clone(
                    ctx => ctx
                    .Resize(initialResizeOptions)
                    .BackgroundColor(SixLabors.ImageSharp.Color.White)
                    .DrawText(textGraphicOptions, sanitizedCaption, memeFont, SixLabors.ImageSharp.Color.Black, new PointF(75, 25))
                    .Resize(finalResizeOptions)
                );

                // Save image to stream
                using(var stream = new MemoryStream())
                {
                    memeImage.SaveAsJpeg(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    await Context.Channel.SendFileAsync(
                        stream: stream,
                        filename: "meme.jpg",
                        text: "Enjoy!"
                    );
                }
            }
        }

        public void Dispose()
        {
            matthiasImage?.Dispose();
        }
    }
}
