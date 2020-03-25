using System;
using System.ComponentModel.DataAnnotations;

namespace BoschBot.Models
{
    public class UserScore
    {
        public UserScore() { }

        public UserScore(ulong userID)
        {
            this.UserID = userID;
            this.Score = 0;
            this.LastDailyClaimed = null;
            this.CurrentDailyStreak = 0;
        }

        [Key]
        public ulong UserID { get; set; }
        public ulong Score { get; set; }
        public DateTime? LastDailyClaimed { get; set; }
        public ulong CurrentDailyStreak { get; set; }
    }
}
