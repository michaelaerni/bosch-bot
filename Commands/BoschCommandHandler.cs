using System;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace BoschBot.Commands
{
    public class BoschCommandHandler : ICommandHandler
    {
        private readonly Random random;
        private readonly Image boschImage;
        private const int BOSCH_SIZE = 450;

        public BoschCommandHandler(Image boschImage)
        {
            this.random = new Random();
            this.boschImage = boschImage;
        }

        public async Task HandleCommandAsync(SocketMessage message, string args, DiscordSocketClient client)
        {
            Console.WriteLine("Handling bosch command");

            int xPosition = random.Next(boschImage.Width - BOSCH_SIZE);
            int yPosition = random.Next(boschImage.Height - BOSCH_SIZE);

            var cropRectangle = new Rectangle(xPosition, yPosition, BOSCH_SIZE, BOSCH_SIZE);
            var imagePatch = boschImage.Clone(ctx => ctx.Crop(cropRectangle));

            // Save image to stream
            using(var stream = new MemoryStream())
            {
                imagePatch.SaveAsJpeg(stream);
                stream.Seek(0, SeekOrigin.Begin);

                await message.Channel.SendFileAsync(
                    stream: stream,
                    filename: "bosch.jpg",
                    text: "Oh my bosch!"
                );
            }
        }
    }
}