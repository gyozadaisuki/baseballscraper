using System;
using System.Collections.Generic;
using System.Text;

namespace BaseBallDataScraper.Models
{
    public class Players
    {
        public Player pitcher { get; }

        public Player batter { get; }

        public string catcherName { get; }

        public bool omoteFlg { get; }

        public Players(Player pitcher, Player batter, string catcherName, bool omoteFlg)
        {
            this.pitcher = pitcher;
            this.batter = batter;
            this.catcherName = catcherName;
            this.omoteFlg = omoteFlg;
        }

        public bool hasAllData()
        {
            return this.pitcher != null && this.batter != null && this.catcherName != null;
        }

        //objと自分自身が等価のときはtrueを返す
        public override bool Equals(object obj)
        {
            //objがnullか、型が違うときは、等価でない
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            //Numberで比較する
            Players player = (Players)obj;
            return (this.pitcher == player.pitcher) && (this.batter == player.batter) && (this.catcherName == player.catcherName);
        }

        //Equalsがtrueを返すときに同じ値を返す
        public override int GetHashCode()
        {
            return this.pitcher.GetHashCode() ^ this.batter.GetHashCode() ^ this.catcherName.GetHashCode() ;
        }
    }
}
