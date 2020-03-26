using System;
using System.Threading.Tasks;
using BoschBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BoschBot.Services
{
    public class ScoreService : IScoreService
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly BotDbContext dbContext;

        public ScoreService(
            IConfiguration configuration,
            ILogger<ScoreService> logger,
            BotDbContext dbContext
        )
        {
            this.configuration = configuration;
            this.logger = logger;
            this.dbContext = dbContext;
        }

        // TODO: Add functionality to transfer scores between users

        public async Task<ulong> ReadUserScoreAsync(ulong userID)
        {
            var user = await FindOrCreateUser(userID);
            return user.Score;
        }

        public async Task<ulong> ClaimDailyScoreAsync(ulong userID)
        {
            DateTime now = DateTime.UtcNow;

            await using(var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                var user = await FindOrCreateUser(userID);

                // FIXME: Can combine the two cases to be more similar, i.e. handle no streak and never claimed equally
                // FIXME: Make base score multiplier configurable
                const ulong BASE_SCORE_MULTIPLIER = 100;

                if(user.LastDailyClaimed == null)
                {
                    // First time claim
                    user.LastDailyClaimed = now;
                    user.Score = BASE_SCORE_MULTIPLIER;
                    user.CurrentDailyStreak = 1;
                }
                else
                {
                    // Check whether last claim was yesterday or earlier
                    if(now.Date == user.LastDailyClaimed?.Date)
                    {
                        throw new ScoreAlreadyClaimedException();
                    }

                    // Calculate additional points based on streak
                    if(now.Date.Subtract(user.LastDailyClaimed.Value.Date).TotalDays <= 1)
                    {
                        // Streak going
                        ++user.CurrentDailyStreak;
                    }
                    else
                    {
                        // Streak reset
                        user.CurrentDailyStreak = 1;
                    }

                    // Update score based on streak
                    user.Score += (ulong)Math.Floor(1 + Math.Log2(user.CurrentDailyStreak)) * BASE_SCORE_MULTIPLIER;
                    user.LastDailyClaimed = now;
                }

                // Optimistic concurrency: If score was updated in the meantime (i.e. already claimed) transaction fails
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return user.CurrentDailyStreak;
            }
        }

        private async Task<UserScore> FindOrCreateUser(ulong userID)
        {
            var existingUser = await dbContext.UserScores.FindAsync(userID);
            if(existingUser == null)
            {
                // Optimistic concurrency: Concurrent creations result in exception since user ID is unique
                await dbContext.AddAsync(new UserScore(userID));
                await dbContext.SaveChangesAsync();

                // Reload new user from database to be in correct context
                existingUser = await dbContext.UserScores.FindAsync(userID);
                if(existingUser == null)
                {
                    throw new Exception($"User with ID {userID} should have been created but database returned null");
                }
            }

            return existingUser;
        }
    }
}
