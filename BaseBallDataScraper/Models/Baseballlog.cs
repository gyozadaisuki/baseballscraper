using System;
using System.Collections.Generic;

#nullable disable

namespace BaseBallDataScraper.Models
{
    public partial class Baseballlog
    {
        public int Id { get; set; }
        public string Attackteam { get; set; }
        public string Pitchteam { get; set; }
        public string Batter { get; set; }
        public string Pitcher { get; set; }
        public string Catcher { get; set; }
        public string Balltype { get; set; }
        public short? Velocity { get; set; }
        public string Batterhand { get; set; }
        public string Pitcherhand { get; set; }
        public DateTime Gamedate { get; set; }
        public int Gamenumber { get; set; }
        public int Xaxis { get; set; }
        public int Yaxis { get; set; }
        public int Ballnumtotal { get; set; }
        public int Ballnumpitcher { get; set; }
    }
}
