using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BoschBot.Commands;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BoschBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new Program().Run(args);
        }

        public async Task Run(string[] args)
        {
            // FIXME: Could refactor this into a Startup class

            // Setup configuration
            IConfiguration configuration = SetupConfiguration(args);

            // Setup dependency injection
            ServiceProvider serviceProvider = RegisterServices(configuration);
            var log = serviceProvider.GetService<ILogger<Program>>();

            try
            {
                // TODO: Actually use dependency injection for bot
                log.LogInformation("Loading ressources");
                var memeFont = SixLabors.Fonts.SystemFonts.CreateFont("Liberation Sans", 42, SixLabors.Fonts.FontStyle.Bold);
                using(var matthiasImage = SixLabors.ImageSharp.Image.Load("images/matthias.jpg"))
                {
                    using(var boschImage = SixLabors.ImageSharp.Image.Load("images/bosch_small.jpg"))
                    {
                        var commandHandlers = new Dictionary<string, ICommandHandler>()
                        {
                            ["matthias"] = new MatthiasCommandHandler(memeFont, matthiasImage),
                            ["bosch"] = new BoschCommandHandler(boschImage),
                            ["vis"] = new VISCommandHandler()
                        };

                        var bot = new Bot(new ReadOnlyDictionary<string, ICommandHandler>(commandHandlers));
                        // TODO: Logging everywhere

                        log.LogInformation("Starting bot");

                        await bot.StartAsync();

                        // Delay until closed
                        await Task.Delay(-1);
                    }
                }
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Unhandled exception");
            }
            finally
            {
                await serviceProvider.DisposeAsync();
            }
        }

        private ServiceProvider RegisterServices(IConfiguration configuration)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(configuration);
            serviceCollection.AddLogging(config => config.AddConsole());
            serviceCollection.AddSingleton(new DiscordSocketClient());
            serviceCollection.AddSingleton(new CommandService());

            return serviceCollection.BuildServiceProvider();
        }

        private IConfiguration SetupConfiguration(string[] args)
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();
        }
    }
}
