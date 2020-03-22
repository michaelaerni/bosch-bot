using System.Threading.Tasks;
using Discord.WebSocket;

namespace BoschBot
{
    public interface ICommandHandler
    {
        Task HandleCommandAsync(SocketMessage message, string args, DiscordSocketClient client);
    }
}
