using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace BoschBot.Commands
{
    public class VISCommandHandler : ICommandHandler
    {
        private readonly Random random;
        private const int DELAY_MIN = 300;
        private const int DELAY_MAX = 5000;

        public VISCommandHandler()
        {
            this.random = new Random();
        }

        public async Task HandleCommandAsync(SocketMessage message, string args, DiscordSocketClient client)
        {
            Console.WriteLine("Handling VIS command");

            using(message.Channel.EnterTypingState())
            {
                await Task.Delay(random.Next(DELAY_MIN, DELAY_MAX));
                await message.Channel.SendMessageAsync("There are currently no planned VIS events.");
            }
        }
    }
}
