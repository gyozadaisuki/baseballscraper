using System;
using System.Collections.Generic;
using System.Text;

namespace BaseBallDataScraper.Models
{
    public class GameInformation
    {
        public DateTime gameDate { get; }

        public string firstTeam { get; }

        public string secondTeam { get; }

        public int gameNumber { get; }

        public GameInformation(DateTime gameDate, string attackTeam, string pitchTeam, int gameNumber)
        {
            this.gameDate = gameDate;
            this.firstTeam = attackTeam;
            this.secondTeam = pitchTeam;
            this.gameNumber = gameNumber;
        }
    }
}
