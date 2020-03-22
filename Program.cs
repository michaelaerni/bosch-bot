using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BoschBot.Commands;

namespace BoschBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new Program().Run();
        }

        public async Task Run()
        {
            Console.WriteLine("Loading font");
            var memeFont = SixLabors.Fonts.SystemFonts.CreateFont("Liberation Sans", 42, SixLabors.Fonts.FontStyle.Bold);

            Console.WriteLine("Loading matthias image");
            using(var matthiasImage = SixLabors.ImageSharp.Image.Load("matthias.jpg"))
            {
                Console.WriteLine("Loading bosch image");
                using(var boschImage = SixLabors.ImageSharp.Image.Load("bosch_small.jpg"))
                {
                    var commandHandlers = new Dictionary<string, ICommandHandler>()
                    {
                        ["matthias"] = new MatthiasCommandHandler(memeFont, matthiasImage),
                        ["bosch"] = new BoschCommandHandler(boschImage),
                        ["vis"] = new VISCommandHandler()
                    };

                    var bot = new Bot(new ReadOnlyDictionary<string, ICommandHandler>(commandHandlers));
                    
                    Console.WriteLine("Starting bot");
                    // TODO: Logging

                    await bot.StartAsync();

                    // Delay until closed
                    await Task.Delay(-1);
                }
            }
        }
    }
}
