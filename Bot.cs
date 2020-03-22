using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BoschBot
{
    public class Bot
    {
        private const char COMMAND_SYMBOL = '!';

        private readonly ReadOnlyDictionary<string, ICommandHandler> commandHandlers;
        private readonly DiscordSocketClient discordClient;
        private readonly CommandService commandService;

        public Bot(ReadOnlyDictionary<string, ICommandHandler> commandHandlers)
        {
            this.commandHandlers = commandHandlers;

            this.discordClient = new DiscordSocketClient();

            this.discordClient.Log += LogAsync;
            this.discordClient.Ready += ReadyAsync;
            this.discordClient.MessageReceived += MessageReceivedAsync;
        }

        public async Task StartAsync()
        {
            // TODO: Store token externally
            Console.WriteLine("Logging in");
            await discordClient.LoginAsync(TokenType.Bot, "TODO");

            Console.WriteLine("Starting");
            await discordClient.StartAsync();
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.Message);
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            Console.WriteLine($"{discordClient.CurrentUser} is connected");
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            // Ignore messages from bot
            if(message.Author.Id == discordClient.CurrentUser.Id)
                return;
            
            // Extract command
            string messageContent = message.Content ?? string.Empty;
            if(messageContent.Length < 2 || messageContent[0] != COMMAND_SYMBOL)
            {
                return;
            }

            // TODO: Better parsing
            int firstSpaceIndex = messageContent.IndexOf(' ');
            if(firstSpaceIndex == -1)
            {
                firstSpaceIndex = messageContent.Length;
            }
            string command = messageContent.Substring(1, firstSpaceIndex - 1);
            string args = messageContent.Substring(firstSpaceIndex);

            try
            {
                ICommandHandler handler;
                if(commandHandlers.TryGetValue(command, out handler))
                {
                    await handler.HandleCommandAsync(message, args, discordClient);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception while handling command: " + ex);
            }
        }
    }
}
