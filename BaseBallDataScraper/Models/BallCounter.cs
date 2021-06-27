using System;
using System.Collections.Generic;
using System.Text;

namespace BaseBallDataScraper.Models
{
    public class BallCounter
    {
        private int firstTeam;
        private int secondTeam;
        private bool isOmote;

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

        public void omoteUra(int attack)
        {
            this.isOmote = attack % 2 != 0;
        }
    }
}
