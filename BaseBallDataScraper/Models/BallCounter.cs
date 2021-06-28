using System;
using System.Collections.Generic;
using System.Text;

namespace BaseBallDataScraper.Models
{
    public class BallCounter
    {
        private int firstTeam;
        private int secondTeam;
        private string firstTeamName;
        private string secondTeamName;
        private bool isOmote;

        public BallCounter(string firstTeamName, string secondTeamName) {
            this.firstTeamName = firstTeamName;
            this.secondTeamName = secondTeamName;
        }

        public int giveBallNum()
        {
            if (isOmote)
            {
                return this.secondTeam;
            }
            else
            {
                return this.firstTeam;
            }
        }

        public void countUpBallNum()
        {
            if (isOmote)
            {
                this.secondTeam++;
            }
            else
            {
                this.firstTeam++;
            }
        }

        public string getAttackTeam()
        {
            return isOmote ? firstTeamName : secondTeamName;
        }

        public string getPitchTeam()
        {
            return isOmote ? secondTeamName : firstTeamName;
        }

        public void omoteUra(int attack)
        {
            this.isOmote = attack % 2 != 0;
        }
    }
}
