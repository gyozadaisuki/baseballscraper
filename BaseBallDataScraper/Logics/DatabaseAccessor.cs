using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaseBallDataScraper.Logics;
using BaseBallDataScraper.Models;

namespace BaseBallDataScraper.Logics
{
    public class DatabaseAccessor
    {
        private static DatabaseAccessor databaseAccessor = new DatabaseAccessor();

        private DatabaseAccessor() { }

        public static DatabaseAccessor getInstance()
        {
            return databaseAccessor;
        }

        public void updateBaseballLogs(List<BaseballModel> baseballlogs)
        {
            insertBaseBallLogs(baseballlogs);
        }

        public int getMaxGameNumber()
        {
            using (var context = new baseballhistoryContext())
            {
                if(context.Baseballlogs.Count()== 0)
                {
                    return 2021000095;
                }
                return  context.Baseballlogs.Select(x => x.Gamenumber).Max();
            }
        }

        private void insertBaseBallLogs(List<BaseballModel> baseballModels)
        {
            using (var context = new baseballhistoryContext())
            {
                foreach(var value in baseballModels)
                {
                    context.Baseballlogs.Add(new Baseballlog
                    {
                        Attackteam = value.battingTeam,
                        Pitchteam = value.pitchingTeam,
                        Batter = value.pitchingData.batter,
                        Pitcher = value.pitchingData.pitcher,
                        Catcher = value.pitchingData.catcher,
                        Balltype = value.pitchingData.kyushu,
                        Velocity = value.pitchingData.velocity,
                        Batterhand = value.pitchingData.bathand,
                        Pitcherhand = value.pitchingData.pitchhand,
                        Gamedate = value.gameDate,
                        Gamenumber = value.gameNumber,
                        Xaxis = value.pitchingData.xAxis,
                        Yaxis = value.pitchingData.yAxis,
                        Ballnumpitcher = value.pitchingData.pitcherBallNum,
                        Ballnumtotal = value.pitchingData.teamBallNum
                    });
                }
                context.SaveChanges();
            }
        }
    }
}
