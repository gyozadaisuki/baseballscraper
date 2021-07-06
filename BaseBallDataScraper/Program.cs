using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseBallDataScraper.Logics;
using BaseBallDataScraper.Models;
using BaseBallDataScraper.Extensions;
using NLog;
using Microsoft.Extensions.DependencyInjection;

namespace BaseBallDataScraper
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static async Task Main(string[] args)
        {
            logger.Info($"Update Start");
            Dictionary<GameInformation, Dictionary<Players, List<Ball>>> baseballlogs = new Dictionary<GameInformation, Dictionary<Players, List<Ball>>>();

            var services = Startup.ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            int gameNumber = serviceProvider.GetService<DatabaseAccessor>().getMaxGameNumber();
            //int gameNumber = 2021000549;
            int noPageCount = 0;
            try
            {
                while (true)
                {
                    gameNumber++;
                    var scraper = new ScrapBaseballData(gameNumber);
                    logger.Info($"Update {gameNumber} start");
                    var baseBalldata = await scraper.FetchBaseBallGameDataAsync();
                    if (baseBalldata == null)
                    {
                        // 試合番号は飛ぶことがあるので、10連続で飛んだ場合に打ち切る形とする
                        if (noPageCount > 10)
                        {
                            break;
                        }
                        noPageCount++;
                        continue;
                    }
                    baseballlogs = baseballlogs.Merge(baseBalldata).ToDictionary();
                    logger.Info($"Update {gameNumber} finished");
                    break;
                }
            }
            catch(Exception e)
            {
                logger.Error($"Update {gameNumber} failed. ErrorMessage:{e.Message}");
            }
            serviceProvider.GetService<DatabaseAccessor>().insertBaseBallLogs(baseballlogs);
            logger.Info("Update Success");
        }
    }
}
