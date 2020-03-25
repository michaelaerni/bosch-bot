using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BoschBot
{
    public class CommandHandlerService
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;
        private readonly DiscordSocketClient discordClient;
        private readonly CommandService commandService;

        public CommandHandlerService(
            IConfiguration configuration,
            ILogger<CommandHandlerService> logger,
            IServiceProvider serviceProvider,
            DiscordSocketClient discordClient,
            CommandService commandService
        )
        {
            this.configuration = configuration;
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.discordClient = discordClient;
            this.commandService = commandService;
        }

        public async Task InitializeAsync()
        {
            // Add command modules
            await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);

            // Set up event handlers
            commandService.CommandExecuted += HandleCommandExecuted;
            discordClient.MessageReceived += HandleMessageReceivedAsync;
        }

        private async Task HandleMessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system and bot messages
            if(!(rawMessage is SocketUserMessage message) || message.Source != MessageSource.User)
            {
                return;
            }

            // Check for command char prefix
            char commandChar = configuration.GetValue<char>("Core:commandChar");
            int argsPosition = 0;
            if(!message.HasCharPrefix(commandChar, ref argsPosition))
            {
                return;
            }

            // Search and execute command
            var context = new SocketCommandContext(discordClient, message);

            // TODO: This does currently not support service scopes for async commands because the scope will be disposed before the command executes. The scoped needs to be created and disposed inside the actual command execution which happens deep in the library.
            await commandService.ExecuteAsync(context, argsPosition, serviceProvider);
        }

        private async Task HandleCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // Ignore command not found or executed successfull
            if(!command.IsSpecified || result.IsSuccess)
            {
                return;
            }

            // Log failure
            logger.LogError("Failed to handle command, result is {0}", result);

            // Notify user
            string errorMessage;
            switch(result.Error)
            {
                case CommandError.BadArgCount:
                case CommandError.MultipleMatches:
                case CommandError.ParseFailed:
                case CommandError.UnmetPrecondition:
                    errorMessage = result.ErrorReason;
                    break;
                default:
                    errorMessage = "Something went wrong :frowning:";
                    break;
            };
            await context.Channel.SendMessageAsync($"Sorry, I couldn't handle that. {errorMessage}");
        }
    }
}
