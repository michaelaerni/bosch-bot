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

        public async Task<ulong> ReadUserScoreAsync(ulong userID)
        {
            var user = await FindOrCreateUser(userID);
            return user.Score;
        }

        public async Task<ulong> ClaimDailyScoreAsync(ulong userID)
        {
            // TODO: Implement
            throw new NotImplementedException();
        }

        private async Task<UserScore> FindOrCreateUser(ulong userID)
        {
            var existingUser = await dbContext.UserScores.FindAsync(userID);
            if(existingUser == null)
            {
                // Optimistic concurrency: Concurrent creations result in crash since user ID is unique
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
