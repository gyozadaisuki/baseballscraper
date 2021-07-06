using System;
using System.Collections.Generic;
using System.Text;

namespace BaseBallDataScraper.Models
{
    public class PitchingData
    {
        public PitchingData
            (Ball ball, Player pitcher, string catcherName, Player batter)
        {
            this.xAxis = ball.xaxis;
            this.yAxis = ball.yaxis;
            this.pitcher = pitcher.name;
            this.catcher = catcherName;
            this.batter = batter.name;
            this.kyushu = ball.balltype;
            this.velocity = ball.velocity;
            this.pitchhand = pitcher.dominantHand;
            this.bathand = batter.dominantHand;
            this.teamBallNum = ball.ballNumTotal;
            this.pitcherBallNum = ball.ballNumPitcher;
        }
        public int xAxis { get; }
        public int yAxis { get; }

        public string pitcher { get; }

        public string catcher { get; }

        public string batter { get; }

        public string kyushu { get; }

        public short? velocity { get; }

        public string pitchhand { get; }

        public string bathand { get; }

        public int teamBallNum { get; }

        public int pitcherBallNum { get; }
    }
}
