using System;
using System.Threading.Tasks;

namespace BoschBot.Services
{
    [System.Serializable]
    public class ScoreAlreadyClaimedException : System.Exception
    {
        public ScoreAlreadyClaimedException() { }
        public ScoreAlreadyClaimedException(string message) : base(message) { }
        public ScoreAlreadyClaimedException(string message, System.Exception inner) : base(message, inner) { }
        protected ScoreAlreadyClaimedException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public interface IScoreService
    {
        Task<ulong> ReadUserScoreAsync(ulong userID);
        Task<ulong> ClaimDailyScoreAsync(ulong userID);
    }
}
