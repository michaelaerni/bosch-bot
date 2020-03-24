using System;
using System.Threading.Tasks;
using Discord;
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
            // FIXME: Try options instead of configuration at some point
            // FIXME: Error handling

            // Setup configuration
            IConfiguration configuration = SetupConfiguration(args);

            // Setup dependency injection
            using(ServiceProvider rootProvider = RegisterServices(configuration))
            {
                using(var serviceScope = rootProvider.CreateScope())
                {
                    var services = serviceScope.ServiceProvider;

                    var logger = services.GetRequiredService<ILogger<Program>>();

                    // Configure command handler
                    var commandHandlerService = services.GetRequiredService<CommandHandlerService>();
                    await commandHandlerService.InitializeAsync();

                    var discordClient = services.GetRequiredService<DiscordSocketClient>();

                    // Configure logging
                    // TODO: Improve this to make sure log levels etc are respected and the correct targets are logged. This is very crude here
                    discordClient.Log += message => { logger.LogInformation(message.ToString()); return Task.CompletedTask; };
                    services.GetRequiredService<CommandService>().Log += message => { logger.LogInformation(message.ToString()); return Task.CompletedTask; };

                    // Configure initialization handlers
                    discordClient.Ready += async () => await discordClient.SetGameAsync("with fire");

                    // Connect and start client
                    logger.LogInformation("Connecting to Discord");
                    await discordClient.LoginAsync(TokenType.Bot, configuration.GetValue<string>("Core:loginToken"));
                    logger.LogInformation("Starting client");
                    await discordClient.StartAsync();

                    // Run until closed
                    await Task.Delay(-1);
                }
            }
        }

        private ServiceProvider RegisterServices(IConfiguration configuration)
        {
            return new ServiceCollection()
                .AddSingleton(configuration)
                .AddLogging(config => config.AddConsole())
                .AddMemoryCache()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlerService>()
                .BuildServiceProvider();
        }

        private IConfiguration SetupConfiguration(string[] args)
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();
        }
    }
}
