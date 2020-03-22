using System;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace BoschBot.Commands
{
    public class MatthiasCommandHandler : ICommandHandler
    {
        private readonly Font memeFont;
        private readonly Image matthiasImage;

        public MatthiasCommandHandler(Font memeFont, Image matthiasImage)
        {
            this.memeFont = memeFont;
            this.matthiasImage = matthiasImage;
        }

        public async Task HandleCommandAsync(SocketMessage message, string args, DiscordSocketClient client)
        {
            Console.WriteLine("Handling matthias command");

            string strippedArgs = args.Trim();
            // TODO: Remove all double newlines and generally sanitize args
            if(strippedArgs.Length == 0)
            {
                await message.Channel.SendMessageAsync("Dude, I need a meme caption!");
            }
            else if(strippedArgs.Length > 400)
            {
                await message.Channel.SendMessageAsync("Sorry but at most 400 characters.");
            }
            else
            {
                // TODO: Refactor this mess

                var textGraphicOptions = new TextGraphicsOptions(true)
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    WrapTextWidth = 550
                };

                // Determine text block height
                var textBounds = TextMeasurer.Measure(
                    strippedArgs,
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
                    .DrawText(textGraphicOptions, strippedArgs, memeFont, SixLabors.ImageSharp.Color.Black, new PointF(75, 25))
                    .Resize(finalResizeOptions)
                );

                // Save image to stream
                using(var stream = new MemoryStream())
                {
                    memeImage.SaveAsJpeg(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    await message.Channel.SendFileAsync(
                        stream: stream,
                        filename: "meme.jpg",
                        text: "Enjoy!"
                    );
                }
            }
        }
    }
}
