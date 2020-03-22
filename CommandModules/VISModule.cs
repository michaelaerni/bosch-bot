
using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BoschBot.CommandModules
{
    public class VISModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private Random random;

        public VISModule(
            IConfiguration configuration,
            ILogger<VISModule> logger
        )
        {
            this.configuration = configuration;
            this.logger = logger;
            this.random = new Random();
        }

        [Command("vis", ignoreExtraArgs: true, RunMode = RunMode.Async)]
        public async Task EventListAsync()
        {
            logger.LogDebug("Handling event list command");

            using(Context.Channel.EnterTypingState())
            {
                int minDelay = configuration.GetValue<int>("CommandModules:VIS:minDelay", 0);
                int maxDelay = configuration.GetValue<int>("CommandModules:VIS:maxDelay", 1);

                // All events are cancelled, thus simulate looking them up to not seem fishy
                await Task.Delay(random.Next(minDelay, maxDelay));

                await ReplyAsync("There are currently no planned VIS events.");
            }
        }
    }
}
