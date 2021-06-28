using System;
using System.Collections.Generic;
using System.Text;

namespace BaseBallDataScraper.Models
{
    public class BaseballModel
    {
        public BaseballModel(BallCounter ballCounter, PitchingData pitchingData, DateTime gameDate, int gameNumber)
        {
            this.battingTeam = ballCounter.getAttackTeam();
            this.pitchingTeam = ballCounter.getPitchTeam();
            this.pitchingData = pitchingData;
            this.gameDate = gameDate;
            this.gameNumber = gameNumber;
        }
        public string battingTeam { get; }
        public string pitchingTeam { get; }

        public DateTime gameDate { get; }
        public PitchingData pitchingData { get; }

        public int gameNumber { get; }
    }
}
