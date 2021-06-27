using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using BaseBallDataScraper.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BaseBallDataScraper.Logics
{
    class ScrapBaseballData
    {
        string urlBase = "https://baseball.yahoo.co.jp/npb/game/";
        string yahooBaseball = "https://baseball.yahoo.co.jp";
        // イニング数×100000、先行は10000、打者数×100でURLパラメータが構成される
        int inningNum = 100000;
        int attackNum = 10000;
        int batterNum = 100;

        public async Task<List<BaseballModel>> FetchBaseBallGameDataAsync(int gameNumber)
        {
            List<BaseballModel> baseballModels = new List<BaseballModel>();
            var urlstring = urlBase + gameNumber.ToString() + "/top";

            var doc = await ScrapHtmlAsync(urlstring);
            if (doc == null)
            {
                return null;
            }
            if (doc.GetElementById("gm_brd").InnerHtml.Contains("試合中止") || doc.GetElementById("gm_brd").InnerHtml.Contains("ノーゲーム"))
            {
                return baseballModels;
            }
            var teams = doc.GetElementsByClassName("bb-gameScoreTable__team");
            string firstTeam = teams[0].InnerHtml;
            string secondTeam = teams[1].InnerHtml;

            var ballCounter = new BallCounter();

            DateTime gameDate = FetchGameDate(doc);
            bool gameEndFlg = false;
            for (int inning = 1; inning <= 9; inning++)
            {
                for (int attack = 1; attack <= 2; attack++)
                {
                    List<PitchingData> pitchingDatas = new List<PitchingData>();
                    bool threeoutflg = false;
                    int batter = 0;
                    while (!threeoutflg)
                    {
                        var parameter = inningNum * inning + attackNum * attack + batterNum * batter;
                        var scoreurlbase = urlBase + gameNumber.ToString() + "/score?index=";
                        if (batter == 0)
                        {
                            gameEndFlg = await CheckAbnormalGameEnd(scoreurlbase, parameter);
                        }
                        else
                        {
                            IHtmlDocument scoreHtml = await SearchNext(scoreurlbase, parameter);

                            if (GameEnded(scoreHtml))
                            {
                                gameEndFlg = true;
                            }
                            scoreHtml = await SearchBallResult(scoreHtml, parameter);
                            ballCounter.omoteUra(attack);
                            var pitchingdata = FetchBaseballScores(scoreHtml, parameter, ballCounter);
                            if (pitchingdata == null)
                            {
                                continue;
                            }
                            pitchingDatas.AddRange(pitchingdata);
                            threeoutflg = IsThreeOut(scoreHtml);
                        }
                        batter++;
                        if (gameEndFlg)
                        {
                            break;
                        }
                    }
                    foreach (var pitchingData in pitchingDatas)
                    {
                        string battingTeam = attack % 2 == 0 ? secondTeam : firstTeam;
                        string pitchingTeam = attack % 2 == 0 ? firstTeam : secondTeam;
                        baseballModels.Add(new BaseballModel(battingTeam, pitchingTeam, pitchingData, gameDate, gameNumber));
                    }
                    if (gameEndFlg)
                    {
                        break;
                    }
                }
                if (gameEndFlg)
                {
                    break;
                }
            }
            return baseballModels;
        }

        private bool GameEnded(IHtmlDocument doc)
        {
            var result = doc.GetElementById("result").TextContent;
            return doc.GetElementById("result").TextContent.Contains("試合終了");
        }

        private async Task<IHtmlDocument> SearchNext(string scoreURLBase, int parameter)
        {
            var url = scoreURLBase + parameter.ToString("D7");
            var doc = await ScrapHtmlAsync(url);
            if(doc == null)
            {
                return null;
            }
            var btnNext = doc.GetElementById("btn_next");
            if(btnNext == null)
            {
                return doc;
            }
            var href = btnNext.GetAttribute("href");
            var reg = Regex.Matches(href, @"[=](\d*\.?\d+)");
            var next = int.Parse(reg[0].Groups[1].Value);
            if (next/100 == parameter/100)
            {
                doc = await SearchNext(scoreURLBase, next);
            }
            return doc;
        }

        private async Task<bool> CheckAbnormalGameEnd(string scoreURLBase, int parameter)
        {
            var url = scoreURLBase + parameter.ToString("D7");
            var doc = await ScrapHtmlAsync(url);
            if (doc == null)
            {
                return false;
            }
            if (GameEnded(doc))
            {
                return true;
            }
            var btnNext = doc.GetElementById("btn_next");
            if (btnNext == null)
            {
                return false;
            }
            var href = btnNext.GetAttribute("href");
            var reg = Regex.Matches(href, @"[=](\d*\.?\d+)");
            var next = int.Parse(reg[0].Groups[1].Value);
            if (next / 100 == parameter / 100)
            {
                return await CheckAbnormalGameEnd(scoreURLBase, next);
            }
            return false;
        }

        private async Task<IHtmlDocument> SearchBallResult(IHtmlDocument scoreHtml, int parameter)
        {
            if (scoreHtml.GetElementById("gm_rslt") != null)
            {
                return scoreHtml;
            }
            else
            {
                var btnPrev = scoreHtml.GetElementById("btn_prev");
                var href = btnPrev.GetAttribute("href");
                var url = yahooBaseball + href;
                var prevHtml = await ScrapHtmlAsync(url);
                var reg = Regex.Matches(href, @"[=](\d*\.?\d+)");
                var prev = int.Parse(reg[0].Groups[1].Value);
                if (prev / 100 == parameter / 100)
                {
                    return await SearchBallResult(prevHtml, prev);
                }
                return null;
            }
        }

        private DateTime FetchGameDate(IHtmlDocument doc)
        {
            // タイトルがXXXX年YY月ZZ日の形式なので、タイトルから数値のみ抜き出せばそれぞれ年月日の値となる
            var title = doc.QuerySelector("title");
            var dateTimeGroup = Regex.Matches(doc.QuerySelector("title").TextContent, @"(\d*\.?\d+)");
            var year = int.Parse(dateTimeGroup[0].Groups[0].Value);
            var month = int.Parse(dateTimeGroup[1].Groups[0].Value);
            var date = int.Parse(dateTimeGroup[2].Groups[0].Value);
            return new DateTime(year, month, date);

        }

        private async Task<IHtmlDocument> ScrapHtmlAsync(string urlstring)
        {
            // 指定したサイトのHTMLをストリームで取得する
            var doc = default(IHtmlDocument);
            try
            {
                using (var client = new HttpClient())
                using (var stream = await client.GetStreamAsync(new Uri(urlstring)))
                {
                    // AngleSharp.Html.Parser.HtmlParserオブジェクトにHTMLをパースさせる
                    var parser = new HtmlParser();
                    doc = await parser.ParseDocumentAsync(stream);
                }
            }
            catch (Exception e)
            {
                return null;
            }
            return doc;
        }

        private List<PitchingData> FetchBaseballScores(IHtmlDocument doc, int parameter, BallCounter ballCounter)
        {
            if(doc == null)
            {
                return null;
            }
            List<PitchingData> pitchingDatas = new List<PitchingData>();

            // 投手の名前を取得する
            var pitcherArea = doc.GetElementById("pit");
            if (pitcherArea == null)
            {
                return null;
            }
            var pitcherName = pitcherArea.GetElementsByClassName("nm")[0].QuerySelector("a").InnerHtml;

            // 打者の名前を取得する
            var batterArea = doc.GetElementById("batt");
            if (batterArea == null)
            {
                return null;
            }
            var batterName = batterArea.GetElementsByClassName("nm")[0].QuerySelector("a").InnerHtml;

            // 捕手の名前を取得する
            var isOmote = (null == doc.GetElementById("pitcherR"));
            var startingmemberId = "gm_mema";
            if (isOmote)
            {
                startingmemberId = "gm_memh";
            }
            var tableRows = doc.GetElementById(startingmemberId).GetElementsByClassName("bb-splitsTable")[0].GetElementsByClassName("bb-splitsTable__row");
            string catcherName = "";
            foreach (var tableRow in tableRows)
            {
                if (tableRow.InnerHtml.Contains("捕"))
                {
                    catcherName = tableRow.QuerySelector("a").InnerHtml;
                }
            }

            // 配球を取得する
            var chartArea = doc.GetElementsByClassName("bb-allocationChart");
            var ballElements = chartArea[0].GetElementsByClassName("bb-icon__ballCircle");

            var ballTypes = doc.GetElementsByClassName("bb-splitsTable__data--ballType");
            var ballVelocities = doc.GetElementsByClassName("bb-splitsTable__data--speed");

            var batter = doc.GetElementById("batter");
            var bathand = batter.GetElementsByClassName("dominantHand")[0].InnerHtml;

            var pitcher = doc.GetElementById("pit");
            var pitchhand = pitcher.GetElementsByClassName("dominantHand")[0].InnerHtml;
            var pitchnum = int.Parse(pitcher.GetElementsByClassName("score")[0].QuerySelector("td").InnerHtml);

            var index = 0;
            var maxBall = ballElements.Length;

            foreach (var ballElement in ballElements)
            {
                ballCounter.countUpBallNum();
                int id = parameter * 100 + index;
                var outerHtml = ballElement.OuterHtml;
                var topPxGroup = Regex.Match(outerHtml, @"[t][o][p][:](-*\d*\.?\d+)[p][x]");
                int yaxis = 175 - int.Parse(topPxGroup.Groups[1].ToString());

                var leftPxGroup = Regex.Match(outerHtml, @"[l][e][f][t][:](-*\d*\.?\d+)[p][x]");
                int xaxis = int.Parse(leftPxGroup.Groups[1].ToString());
  
                var ballType = ballTypes[index].InnerHtml;
                short? velocity = null;
                var velocityGroup = Regex.Match(ballVelocities[index].InnerHtml, @"(\d*\.?\d+)");
                if (velocityGroup.Length > 0)
                {
                    velocity = short.Parse(velocityGroup.Groups[0].Value);
                }

                PitchingData pitchingData = new PitchingData(xaxis, yaxis, pitcherName, catcherName, batterName, ballType, velocity, id, pitchhand, bathand, ballCounter.giveBallNum(), pitchnum - (maxBall - index) + 1);
                pitchingDatas.Add(pitchingData);
                index++;
            }

            return pitchingDatas;
        }


        private bool IsThreeOut(IHtmlDocument doc)
        {
            // アウトカウントのhtmlに●●●があると3アウト
            var outCount = doc.GetElementsByClassName("o")[0].QuerySelector("b");
            return outCount.InnerHtml.Equals("●●●");
        }
    }
}
