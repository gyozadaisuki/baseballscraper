using System;
using System.Collections.Generic;
using System.Text;

namespace BaseBallDataScraper.Models
{
    public class Ball
    {
        public int xaxis { get; }

        public int yaxis { get; }

        public string balltype { get; }

        public short? velocity { get; }

        public int ballNumPitcher { get; }

        public int ballNumTotal { get; }

        public Ball(int xaxis, int yaxis, string balltype, short? velocity, int ballNumPicther, int ballNumTotal)
        {
            this.xaxis = xaxis;
            this.yaxis = yaxis;
            this.balltype = balltype;
            this.velocity = velocity;
            this.ballNumPitcher = ballNumPicther;
            this.ballNumTotal = ballNumTotal;
        }
    }
}
