using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseBallDataScraper.Logics;
using BaseBallDataScraper.Models;
using NLog;

namespace BaseBallDataScraper
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static async Task Main(string[] args)
        {
            logger.Info($"Update Start");
            List<BaseballModel> baseballlogs = new List<BaseballModel>();
            DatabaseAccessor accessor = DatabaseAccessor.getInstance();
            int gameNumber = accessor.getMaxGameNumber();
            int noPageCount = 0;
            try
            {
                //while (true)
                //{
                    gameNumber++;
                    var scraper = new ScrapBaseballData(gameNumber);
                    logger.Info($"Update {gameNumber} start");
                    var baseBalldata = await scraper.FetchBaseBallGameDataAsync();
                    if (baseBalldata == null)
                    {
                        //// 試合番号は飛ぶことがあるので、10連続で飛んだ場合に打ち切る形とする
                        //if(noPageCount > 10)
                        //{
                        //    break;
                        //}
                        //noPageCount++;
                        //continue;
                    }
                    baseballlogs.AddRange(baseBalldata);
                    logger.Info($"Update {gameNumber} finished");
                //}
            }
            catch(Exception e)
            {
                logger.Error($"Update {gameNumber} failed. ErrorMessage:{e.Message}");
            }
            accessor.updateBaseballLogs(baseballlogs);
            logger.Info("Update Success");
        }
    }
}
