using System;
using System.Collections.Generic;
using System.Text;

namespace BaseBallDataScraper.Models
{
    public class BaseballModel
    {
        public BaseballModel(string battingTeam, string pitchingTeam, PitchingData pitchingData, DateTime gameDate, int gameNumber)
        {
            this.battingTeam = battingTeam;
            this.pitchingTeam = pitchingTeam;
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
