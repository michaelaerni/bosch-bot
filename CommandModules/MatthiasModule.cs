
using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace BoschBot.CommandModules
{
    public class MatthiasModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly IMemoryCache cache;
        private readonly Random random;
        private readonly Font memeFont;
        private readonly Font superMemeFont;

        public MatthiasModule(
            IConfiguration configuration,
            ILogger<MatthiasModule> logger,
            IMemoryCache cache
        )
        {
            this.configuration = configuration;
            this.logger = logger;
            this.cache = cache;
            this.memeFont = SystemFonts.CreateFont("Liberation Sans", 42, SixLabors.Fonts.FontStyle.Bold); // TODO: Use config value or handle differently
            this.superMemeFont = SystemFonts.CreateFont("Comic Relief", 42, SixLabors.Fonts.FontStyle.Bold); // TODO: Use config value or handle differently
        }

        [Command("matthias")]
        public async Task MemeAsync([Remainder] string caption)
        {
            logger.LogDebug("Handling meme command");
            await HandleMemeRequest(caption, memeFont);
        }

        [Command("matthisans")]
        public async Task SuperMemeAsync([Remainder] string caption)
        {
            logger.LogDebug("Handling super meme command");
            await HandleMemeRequest(caption, superMemeFont);
        }

        private async Task HandleMemeRequest(string caption, Font font)
        {
            using(Context.Channel.EnterTypingState())
            {
                var matthiasImage = await LoadMatthiasImage();

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
                        new RendererOptions(font)
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
                        .DrawText(textGraphicOptions, sanitizedCaption, font, SixLabors.ImageSharp.Color.Black, new PointF(75, 25))
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
        }

        private enum CacheKeys
        {
            MatthiasImage
        }

        private async Task<Image> LoadMatthiasImage()
        {
            // TODO: Use config value for path or handle differently
            // TODO: Maybe allow image to be removed, enables hot-swapping without bot reloading
            return await cache.GetOrCreateAsync<Image>(
                CacheKeys.MatthiasImage,
                entry =>
                {
                    entry.Priority = CacheItemPriority.NeverRemove;
                    return Task.FromResult(Image.Load("images/matthias.jpg"));
                }
            );
        }
    }
}
