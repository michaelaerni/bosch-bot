
using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BoschBot.CommandModules
{
    [RequireOwner]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly DiscordSocketClient discordClient;

        public AdminModule(
            IConfiguration configuration,
            ILogger<AdminModule> logger,
            DiscordSocketClient discordClient
        )
        {
            this.configuration = configuration;
            this.logger = logger;
            this.discordClient = discordClient;
        }

        [Command("exit", ignoreExtraArgs: true)]
        public async Task ExitAsync()
        {
            logger.LogInformation(
                "Exit has been ordered by user {0} (ID {1})",
                Context.Message.Author,
                Context.Message.Author.Id
            );

            string exitMessage = configuration.GetValue<string>("CommandModules:Admin:exitMessage");

            if(!string.IsNullOrWhiteSpace(exitMessage))
            {
                await ReplyAsync("Goodbye");
                await ReplyAsync(exitMessage);
            }

            // FIXME: This could be done in a more elegant way, e.g. via cancellation tokens

            discordClient.Disconnected += (ex) => { Environment.Exit(0); return Task.CompletedTask; };

            await discordClient.StopAsync();
        }
    }
}
