
using System;
using System.Threading.Tasks;
using BoschBot.Services;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BoschBot.CommandModules
{
    public class ScoreModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly IScoreService scoreService;

        public ScoreModule(
            IConfiguration configuration,
            ILogger<ScoreModule> logger,
            IScoreService scoreService
        )
        {
            this.configuration = configuration;
            this.logger = logger;
            this.scoreService = scoreService;
        }

        // TODO: Since scoped dependency injection is not working correctly with async commands, the commands are currently synchronous. However, as soon as scoping is fixed they should become async

        [Command("score", ignoreExtraArgs: true, RunMode = RunMode.Sync)]
        public async Task ShowScoreAsync()
        {
            logger.LogDebug("Handling score retrieve command");

            using(Context.Channel.EnterTypingState())
            {
                ulong userScore = await scoreService.ReadUserScoreAsync(Context.User.Id);
                await ReplyAsync($"{Context.User.Mention} has a score of {userScore}.");
            }
        }

        [Command("daily", ignoreExtraArgs: true, RunMode = RunMode.Sync)]
        public async Task ClaimDailyScoreAsync()
        {
            logger.LogDebug("Handling daily score claim command");

            using(Context.Channel.EnterTypingState())
            {
                try
                {
                    ulong userStreak = await scoreService.ClaimDailyScoreAsync(Context.User.Id);
                    await ReplyAsync($"{Context.User.Mention} you claimed your daily score! You are currently on a {userStreak} days streak.");
                }
                catch(ScoreAlreadyClaimedException)
                {
                    await ReplyAsync($"{Context.User.Mention} you already claimed your daily score today! Please try again tomorrow.");
                }
            }
        }
    }
}
