using System;
using System.Collections.Generic;
using BoschBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BoschBot
{
    public class BotDbContext : DbContext
    {
        private IConfiguration configuration;

        public BotDbContext(DbContextOptions<BotDbContext> options, IConfiguration configuration)
            : base(options)
        {
            this.configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite(configuration.GetConnectionString("Default"));

        public DbSet<UserScore> UserScores { get; set; }
    }

    public class BotDbContextFactory : IDesignTimeDbContextFactory<BotDbContext>
    {
        public BotDbContext CreateDbContext(string[] args)
        {
            // Use an in-memory configuration containing some connection string at design time
            // FIXME: Is there some more elegant way?
            IConfiguration designTimeConfiguration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>() { {"ConnectionStrings:Default", "Data Source=bot.db"} })
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<BotDbContext>();
            optionsBuilder.UseSqlite("Data Source=bot.db");

            return new BotDbContext(optionsBuilder.Options, designTimeConfiguration);
        }
    }
}
