using System;
using System.Collections.Generic;
using System.Text;

namespace BaseBallDataScraper.Models
{
    public class PitchingData
    {
        public PitchingData
            (int xAxis, int yAxis, string pitcher, string catcher, string batter, string kyushu, short? velocity, int id, string pitchhand, string bathand,int teamBallNum, int pitcherBallNum)
        {
            this.id = id;
            this.xAxis = xAxis;
            this.yAxis = yAxis;
            this.pitcher = pitcher;
            this.catcher = catcher;
            this.batter = batter;
            this.kyushu = kyushu;
            this.velocity = velocity;
            this.pitchhand = pitchhand;
            this.bathand = bathand;
            this.teamBallNum = teamBallNum;
            this.pitcherBallNum = pitcherBallNum;
        }
        public int id { get; }
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
