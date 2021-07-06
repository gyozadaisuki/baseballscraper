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
        private string urlBase = "https://baseball.yahoo.co.jp/npb/game/";
        private string yahooBaseball = "https://baseball.yahoo.co.jp";
        // イニング数×100000、先行は10000、打者数×100でURLパラメータが構成される
        private int _inningNum = 100000;
        private int _attackNum = 10000;
        private int _batterNum = 100;
        private int _gameNumber;

        public ScrapBaseballData(int gameNumber)
        {
            this._gameNumber = gameNumber;
        }

        /// <summary>
        /// クラスの保持する試合番号を元に、その試合の全投球を取得する
        /// </summary>
        /// <returns>試合情報、投手・打者の組み合わせ、その投手の投げたボールのDictionary</returns>
        public async Task<Dictionary<GameInformation, Dictionary<Players, List<Ball>>>> FetchBaseBallGameDataAsync()
        {
            var urlstring = urlBase + _gameNumber.ToString() + "/top";

            var doc = await ScrapHtmlAsync(urlstring);
            if (doc == null || isNoGame(doc))
            {
                return null;
            }

            var teams = doc.GetElementsByClassName("bb-gameScoreTable__team");
            string firstTeam = teams[0].InnerHtml;
            string secondTeam = teams[1].InnerHtml;

            var ballCounter = new BallCounter(firstTeam, secondTeam);

            DateTime gameDate = FetchGameDate(doc);

            GameInformation info = new GameInformation(gameDate, firstTeam, secondTeam, _gameNumber);
            var scoreurlbase = urlBase + _gameNumber.ToString() + "/score?index=";
            Dictionary<GameInformation, Dictionary<Players, List<Ball>>> dict = new Dictionary<GameInformation, Dictionary<Players, List<Ball>>>();
            return await FetchOneGameballs(ballCounter, info, scoreurlbase, dict);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ballCounter"></param>
        /// <param name="info"></param>
        /// <param name="scoreurlbase"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        private async Task<Dictionary<GameInformation, Dictionary<Players, List<Ball>>>> FetchOneGameballs(BallCounter ballCounter, GameInformation info, string scoreurlbase, Dictionary<GameInformation, Dictionary<Players, List<Ball>>> dict)
        {
            bool gameEndFlg = false;
            for (int inning = 1; inning <= 9; inning++)
            {
                for (int attack = 1; attack <= 2; attack++)
                {
                    int batter = 1;
                    ballCounter.OmoteUra(attack);
                    var resultHtml = default(IHtmlDocument);
                    Dictionary<Players, List<Ball>> playerBalls = new Dictionary<Players, List<Ball>>();
                    if(await CheckAbnormalGameEnd(scoreurlbase, _inningNum * inning + _attackNum * attack))
                    {
                        return dict;
                    }
                    do
                    {
                        var parameter = _inningNum * inning + _attackNum * attack + _batterNum * batter;
                        // 雨天中止等の場合に、0人目ifの打者で試合終了している場合があるのでそのチェック
                        resultHtml = await GetResultHtml(scoreurlbase, parameter);
                        var pitchingDatas = FetchBaseballScores(resultHtml, parameter, ballCounter);
                        playerBalls = MergeBallResults(playerBalls, pitchingDatas);
                        gameEndFlg = await GameEnded(scoreurlbase, parameter);
                        batter++;
                    } while (!gameEndFlg && !IsThreeOut(resultHtml));
                    // TODO : dictが空の場合に、dict[info]がnullとなるのでもしなければ新規作成してあれば追加するロジックを書くこと
                    dict[info] = MergeBallResults(dict.ContainsKey(info) ? dict[info] : new Dictionary<Players, List<Ball>>(), playerBalls);
                    if (gameEndFlg)
                    {
                        return dict;
                    }
                }
            }
            return dict;
        }

        private static Dictionary<Players, List<Ball>> MergeBallResults(Dictionary<Players, List<Ball>> playerBalls, Dictionary<Players, List<Ball>> newDatas)
        {
            if(newDatas == null)
            {
                return playerBalls;
            }
            foreach (var newdata in newDatas)
            {
                if (playerBalls.ContainsKey(newdata.Key))
                {
                    playerBalls[newdata.Key].AddRange(newdata.Value);
                }
                else
                {
                    playerBalls.Add(newdata.Key, newdata.Value);
                }
            }
            return playerBalls;
        }

        private async Task<IHtmlDocument> GetResultHtml(string scoreurlbase, int parameter)
        {
            // その打者の最後のHTMLを探す
            IHtmlDocument scoreHtml = await SearchLastResult(scoreurlbase, parameter);

            // 最後からさかのぼり、結果がある個所を探す
            return await SearchBallResult(scoreHtml, parameter);
        }

        private async Task<bool> GameEnded(string scoreurlbase, int parameter)
        {
            var url = scoreurlbase + parameter.ToString("D7");
            bool isGameEnd = false;
            var doc = await ScrapHtmlAsync(url);
            if (doc == null)
            {
                return isGameEnd;
            }
            var result = doc.GetElementById("result").TextContent;
            isGameEnd = doc.GetElementById("result").TextContent.Contains("試合終了");
            if (!isGameEnd) {
                var next = GetNextParam(doc);
                if (next / 100 == parameter / 100)
                {
                    isGameEnd = await GameEnded(scoreurlbase, next);
                }
            }
            return isGameEnd;
        }

        // その打者で結果が出るまで次のページへ進む
        private async Task<IHtmlDocument> SearchLastResult(string scoreURLBase, int parameter)
        {
            var url = scoreURLBase + parameter.ToString("D7");
            var doc = await ScrapHtmlAsync(url);
            if (doc == null)
            {
                return null;
            }
            var next = GetNextParam(doc);
            if (next / 100 == parameter / 100)
            {
                doc = await SearchLastResult(scoreURLBase, next);
            }
            return doc;
        }

        private static int GetNextParam(IHtmlDocument doc)
        {
            var btnNext = doc.GetElementById("btn_next");
            if (btnNext == null)
            {
                return -1;
            }
            var href = btnNext.GetAttribute("href");
            var reg = Regex.Matches(href, @"[=](\d*\.?\d+)");
            return int.Parse(reg[0].Groups[1].Value);
        }

        private async Task<bool> CheckAbnormalGameEnd(string scoreURLBase, int parameter)
        {
            var url = scoreURLBase + parameter.ToString("D7");
            var doc = await ScrapHtmlAsync(url);
            if (doc == null)
            {
                return false;
            }
            if (await GameEnded(scoreURLBase, parameter))
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

        private Dictionary<Players, List<Ball>> FetchBaseballScores(IHtmlDocument doc, int parameter, BallCounter ballCounter)
        {
            if(doc == null)
            {
                return null;
            }
            List<PitchingData> pitchingDatas = new List<PitchingData>();

            // 配球を取得する
            var chartArea = doc.GetElementsByClassName("bb-allocationChart");
            var ballElements = chartArea[0].GetElementsByClassName("bb-icon__ballCircle");

            var ballTypes = doc.GetElementsByClassName("bb-splitsTable__data--ballType");
            var ballVelocities = doc.GetElementsByClassName("bb-splitsTable__data--speed");

            var pitchnum = int.Parse(doc.GetElementById("pit").GetElementsByClassName("score")[0].QuerySelector("td").InnerHtml);

            var index = 0;
            var maxBall = ballElements.Length;
            Dictionary<Players, List<Ball>> playerBalls = new Dictionary<Players, List<Ball>>();
            Players players = new Players(GetPlayerData(doc, "pit"), GetPlayerData(doc, "batt"), GetCatcherName(doc, ballCounter),ballCounter.IsOmote());
            List<Ball> balls = new List<Ball>();

            foreach (var ballElement in ballElements)
            {
                ballCounter.CountUpBallNum();

                var ballDetail = ballElement.OuterHtml;
                int yaxis = GetYAxis(ballDetail);

                int xaxis = GetXAxis(ballDetail);

                var ballType = ballTypes[index].InnerHtml;

                short? velocity = GetVelocity(ballVelocities, index);

                balls.Add(new Ball(xaxis, yaxis, ballType, velocity, pitchnum - (maxBall - index) + 1, ballCounter.GiveBallNum()));

                index++;
            }
            playerBalls.Add(players, balls);
            return playerBalls;
        }

        private static short? GetVelocity(AngleSharp.Dom.IHtmlCollection<AngleSharp.Dom.IElement> ballVelocities, int index)
        {
            short? velocity = null;
            var velocityGroup = Regex.Match(ballVelocities[index].InnerHtml, @"(\d*\.?\d+)");
            if (velocityGroup.Length > 0)
            {
                velocity = short.Parse(velocityGroup.Groups[0].Value);
            }

            return velocity;
        }

        private static int GetXAxis(string ballDetail)
        {
            var leftPxGroup = Regex.Match(ballDetail, @"[l][e][f][t][:](-*\d*\.?\d+)[p][x]");
            int xaxis = int.Parse(leftPxGroup.Groups[1].ToString());
            return xaxis;
        }

        private static int GetYAxis(string ballDetail)
        {
            var topPxGroup = Regex.Match(ballDetail, @"[t][o][p][:](-*\d*\.?\d+)[p][x]");
            int yaxis = 175 - int.Parse(topPxGroup.Groups[1].ToString());
            return yaxis;
        }

        private Player GetPlayerData(IHtmlDocument doc, string type)
        {
            // 投手の名前を取得する
            var playerArea = doc.GetElementById(type);
            if (playerArea == null)
            {
                return null;
            }
            var playerName =  playerArea.GetElementsByClassName("nm")[0].QuerySelector("a").InnerHtml;
            var dominantHand = playerArea.GetElementsByClassName("dominantHand")[0].InnerHtml;

            return new Player(playerName, dominantHand);
        }

        private bool IsThreeOut(IHtmlDocument doc)
        {
            // アウトカウントのhtmlに●●●があると3アウト
            var outCount = doc.GetElementsByClassName("o")[0].QuerySelector("b");
            return outCount.InnerHtml.Equals("●●●");
        }

        private bool isNoGame(IHtmlDocument doc)
        {
            var gameBoard = doc.GetElementById("gm_brd").InnerHtml;
            if (gameBoard.Contains("試合中止") || gameBoard.Contains("ノーゲーム"))
            {
                return true;
            }
            return false;
        }

        private string GetCatcherName(IHtmlDocument doc, BallCounter ballCounter)

        {   // 捕手の名前を取得する
            var startingmemberId = ballCounter.IsOmote() ? "gm_memh" : "gm_mema";
            var tableRows = doc.GetElementById(startingmemberId).
                GetElementsByClassName("bb-splitsTable")[0].
                GetElementsByClassName("bb-splitsTable__row");

            foreach (var tableRow in tableRows)
            {
                if (tableRow.InnerHtml.Contains("捕"))
                {
                   return tableRow.QuerySelector("a").InnerHtml;
                }
            }
            return null;
        }
    }
}
