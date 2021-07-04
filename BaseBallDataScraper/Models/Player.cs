using System;
using System.Collections.Generic;
using System.Text;

namespace BaseBallDataScraper.Models
{
    public class Player
    {
        public string name { get; }
        public string dominantHand { get; }

        public Player(string name, string dominantHand)
        {
            this.name = name;
            this.dominantHand = dominantHand;
        }


    }
}
