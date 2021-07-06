using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaseBallDataScraper.Logics;
using BaseBallDataScraper.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseBallDataScraper.Logics
{
    public class DatabaseAccessor
    {
        private readonly IConfiguration _configuration;

        private string _connectionString;

        DbContextOptionsBuilder<baseballhistoryContext> _optionsBuilder;

        public DatabaseAccessor(IConfiguration configuration)
        {
            this._configuration = configuration;
            _optionsBuilder = new DbContextOptionsBuilder<baseballhistoryContext>();
            _connectionString = _configuration.GetConnectionString("baseBallHistory");
            _optionsBuilder.UseSqlServer(_connectionString);

        }

        public int getMaxGameNumber()
        {
            using (var context = new baseballhistoryContext(_optionsBuilder.Options))
            {
                if (context.Baseballlogs.Count() == 0)
                {
                    return 2021000095;
                }
                return context.Baseballlogs.Select(x => x.Gamenumber).Max();
            }
        }

        public void insertBaseBallLogs(Dictionary<GameInformation, Dictionary<Players, List<Ball>>> gameBalls)
        {
            using (var context = new baseballhistoryContext(_optionsBuilder.Options))
            {
                foreach (var game in gameBalls)
                {
                    var gameInformation = game.Key;
                    foreach (var ballSituation in game.Value)
                    {
                        ballSituation.Value.ForEach(ball =>
                            context.Baseballlogs.Add(new Baseballlog
                            {
                                Attackteam = ballSituation.Key.omoteFlg ? gameInformation.firstTeam : gameInformation.secondTeam,
                                Pitchteam = ballSituation.Key.omoteFlg ? gameInformation.secondTeam : gameInformation.firstTeam,
                                Batter = ballSituation.Key.batter.name,
                                Pitcher = ballSituation.Key.pitcher.name,
                                Catcher = ballSituation.Key.catcherName,
                                Balltype = ball.balltype,
                                Velocity = ball.velocity,
                                Batterhand = ballSituation.Key.batter.dominantHand,
                                Pitcherhand = ballSituation.Key.pitcher.dominantHand,
                                Gamedate = gameInformation.gameDate,
                                Gamenumber = gameInformation.gameNumber,
                                Xaxis = ball.xaxis,
                                Yaxis = ball.yaxis,
                                Ballnumpitcher = ball.ballNumPitcher,
                                Ballnumtotal = ball.ballNumTotal
                            }));
                    }
                }
                context.SaveChanges();
            }
        }
    }
}
