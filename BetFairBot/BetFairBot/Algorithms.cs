using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BFDL;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace BetFairBot
{
    public partial class MainWindow
    {
        public void BetOnAllRecentlyFoundIncrementing()
        {
            try
            {
                BettingLine betline = MainWindow.CurrentBetline;
                betline.isActive = true;
                betline.lastBetLost = false;
                betline.currencyCode = "EUR";
                if (!betline.algorithmName.Contains("All"))
                {
                    betline.Filters.maxBetsCount = 1;
                }
                //var bl = databaseContext.Betlines.Add(betline as Betline);
                var bl = betline.ToDBBetline();
                UserActive.Betlines.Add(bl);
                //MainWindow.databaseContext.Betlines.Add(betline as BFDL.Betline);
                MainWindow.databaseContext.SaveChanges();
                betline = new BettingLine(bl);
                BetOnAllRecentlyFoundIncrementing(betline);
                //Execute selected Method for betline
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}", ex.Message, ex.Source, ex.StackTrace));
                //restartTimer_Tick(this, new EventArgs());
            }
        }


        public void BetOnAllRecentlyFoundIncrementing(BettingLine betline)
        {
            betline.timer = new System.Windows.Threading.DispatcherTimer();
            betline.timer.Interval = TimeSpan.FromSeconds(10); //use smaller interval at Tests
            //We must replace these with filters
            betline.BetInstruction = new TO.PlaceInstruction();
            betline.BetInstruction.LimitOrder = new TO.LimitOrder();
            betline.BetInstruction.LimitOrder.PersistenceType = TO.PersistenceType.PERSIST;
            betline.BetInstruction.OrderType = TO.OrderType.LIMIT;
            betline.BetInstruction.Side = betline.betType == "Back" ? TO.Side.BACK : TO.Side.LAY;
            betline.FillMarketFilter();
            betline.timer.Tick += betline.timer_Tick_BetOnAllRecentlyFoundIncrementing_Async;
            betline.timer.Start();
        }
    }

    public partial class BettingLine
    {
        public async void timer_Tick_BetOnAllRecentlyFoundIncrementing_Async(object sender, EventArgs e)
        {
            this.timer.Stop();
            if (!this.TakeActiveStatusFmDB() | this.Users.userName != MainWindow.UserActive.userName)
            {
                return;
            }
            else
            {
                await Task.Run(() =>
                {
                    if (this.isActive)
                    {
                        try
                        {
                            if (this.lastPlacedBetId == null)
                            {
                                this.lastPlacedBetId = "";
                            }
                            if (this.Filters.maxBetsCount > 0 ? (this.lastPlacedBetId.Trim(';').Split(';').Count() < this.Filters.maxBetsCount) : true)
                            {
                                //extracting new marketlist from server to betline.currentMarketList 
                                CurrentMarketList = this.GetMarketsRequestFiltered();
                                // filter the markets by all filters
                                //sort markets by date ascending
                                var resultList = (from market in this.CurrentMarketList
                                                  where LiveScoreAvailable(market) & IsMarketCompatible(market)
                                                 orderby market.Description.MarketTime ascending
                                                 select market.MarketId).Take(40).ToList();
                                
                                var priceProjection = new TO.PriceProjection();
                                priceProjection.PriceData = new HashSet<TO.PriceData>() { TO.PriceData.EX_BEST_OFFERS };
                                priceProjection.RolloverStakes = true;
                                var language = "bg";
                                CurrentMarketBookList = MainWindow.Client.listMarketBook(resultList,priceProjection,TO.OrderProjection.ALL,TO.MatchProjection.ROLLED_UP_BY_PRICE,null,language).ToList();
                                foreach (var m in CurrentMarketBookList)
                                {
                                    // Gets all runners for m and looks for sutable one and Places bet on that
                                    if (this.Filters.maxBetsCount > 0 ? (this.lastPlacedBetId.Trim(';').Split(';').Count() < this.Filters.maxBetsCount) : true)
                                    {
                                        CurrentMarket = CurrentMarketList.FirstOrDefault(ma => ma.MarketId == m.MarketId);
                                        this.FindAndBetOnSutableRunner(m);
                                    }
                                }
                            }
                            if (this.lastPlacedBetId.Trim(';').Split(';').Count() > 0)
                            {
                                this.LastBetResult();
                            }
                        }
                        catch (Exception)
                        {
                            this.timer.Start();
                            //MainWindow.Button_Click_Login();
                            // write exception in the database                            
                        }
                        //catch (System.IO.IOException session)
                        //{
                        //    this.timer.Start();
                        //}
                        //System.Net.WebException
                        //login
                    }
                });
                MainWindow.BindBetlinesGrid();
                //(App.Current.MainWindow as MainWindow).BindFunds();
            }
            this.timer.Start();

        }

        private bool IsMarketCompatible(TO.MarketCatalogue market)
        {
            return (Filters.marketNames.Trim(';').Split(';').Count() > 0 & Filters.marketNames != null) ? this.Filters.marketNames.ToUpper().Contains(market.MarketName.ToUpper()) : true;
        }

        public static HtmlAgilityPack.HtmlDocument DocumentGetter(string URL)
        {
            //URL = FakeUrl(URL);
            try
            {
                WebClient wc = new WebClient();
                var doc = new HtmlAgilityPack.HtmlDocument();
                string html = "";                
                wc.Encoding = Encoding.UTF8;
                html = wc.DownloadString(URL);
                doc.LoadHtml(html);
                return doc;
            }
            catch (Exception)
            {
                return new HtmlAgilityPack.HtmlDocument();
            }
        }
        
        private void LastBetResult()
        {
            try 
	        {
                if (string.IsNullOrEmpty(MainWindow.soccerLivescore))
                {
                    MainWindow.UpdateLiveScores();
                }
                var betIdsList = this.lastPlacedBetId.Trim(';').Split(';').ToList();
                HashSet<string> betIdsSet = new HashSet<string>();
                var context = new BFBDBEntities();
                foreach (var betid in betIdsList)
                {
                    try
                    {
                        betIdsSet.Add(betid);
                    }
                    catch (Exception)
                    {
                    }
                }
                var currentOrdersList = MainWindow.Client.listCurrentOrders(betIdsSet, new HashSet<string>(), null, null, null, null, null, null);
                foreach (var order in currentOrdersList.CurrentOrders)
	            {
                    var yesterday = DateTime.Now.AddDays(-2);
                    var placedBetDB = context.PlacedBets.Where(b=>b.datePlaced>yesterday).FirstOrDefault(p=>p.betId == order.BetId);
                    if (placedBetDB == null)
                    {
                        RemoveBetId(order.BetId);
                    }
                    if (placedBetDB.resultCode == "Matched")
	                {
		                continue;
	                }
                    if(order.Status == TO.OrderStatus.EXECUTION_COMPLETE)
                    {
                        placedBetDB.averagePrice = order.AveragePriceMatched;
                        placedBetDB.sizeMatched = order.SizeMatched;
                        placedBetDB.resultCode = "Matched";
                        context.SaveChanges();
                    }
                    if (order.Status == TO.OrderStatus.EXECUTABLE)
	                {
                        if (order.PlacedDate < DateTime.Now.AddMinutes(-5))
                        {
                            CancelBetAndResetBetId(order);
                            continue;
                        }
		                placedBetDB.averagePrice = order.AveragePriceMatched;
                        placedBetDB.sizeMatched = order.SizeMatched;
                        placedBetDB.resultCode = "";
                        context.SaveChanges();
	                }
	            }
                var settledOrdersList = MainWindow.Client.listClearedOrders(TO.BetStatus.SETTLED, null, null, null, null, betIdsSet, null, null, null, true, null, null, null);
                foreach (var settled in settledOrdersList.ClearedOrders)
	            {
                    var yesterday = DateTime.Now.AddDays(-2);
                    var placedBetDB = context.PlacedBets.Where(b => b.datePlaced > yesterday).FirstOrDefault(p => p.betId == settled.BetId);
                    if (settled.Profit < 0)
                    {
                        if (this.algorithmName.ToUpper().Contains("increment".ToUpper()))
                        {
                            double size;
                            size = Math.Round((this.profitPerBet - (settled.Profit * settled.PriceMatched) - (profitPerBet == initialProfitPerBet ? profitPerBet : 0)), 2);

                            if (size > initialProfitPerBet)
                            {
                                // reducing or increasing the size of the bet for the next bet
                                this.profitPerBet = size;
                            }
                            else
                            {
                                this.profitPerBet = this.initialProfitPerBet;
                            }
                        }
                        this.lastBetLost = true;
                    }
                    else
                    {
                        this.lastBetLost = false;
                    }
                    this.ballance += (decimal)settled.Profit;
                    this.lastPlacedBetId = lastPlacedBetId.Replace(settled.BetId + ";","");

                    try
                    {
                        this.UpdateDB(placedBetDB.id);
                        if (placedBetDB != null)
                        {
                            placedBetDB.dateSettled = settled.SettledDate;
                            UpdatePlacedBetResult(placedBetDB, MainWindow.soccerLivescore);
                            placedBetDB.resultCode = "Settled";
                            context.SaveChanges();
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                var canceledOrdersList = MainWindow.Client.listClearedOrders(TO.BetStatus.CANCELLED, null, null, null, null, betIdsSet, null, null, null, true, null, null, null);
                foreach (var canceled in canceledOrdersList.ClearedOrders)
	            {
                    var yesterday = DateTime.Now.AddDays(-2);
                    var placedBetDB = context.PlacedBets.Where(b => b.datePlaced > yesterday).FirstOrDefault(p => p.betId == canceled.BetId);
                    
		            if (betType == "Back")
                    {
                        if (profitPerBet > initialProfitPerBet)
                        {
                            profitPerBet += (canceled.SizeCancelled+canceled.SizeSettled) * (canceled.PriceRequested - 1);
                        }
                        else
                        {
                            profitPerBet = (canceled.SizeCancelled+canceled.SizeSettled) * (canceled.PriceRequested - 1);
                        }
                    }
                    else
                    {
                        if (profitPerBet > initialProfitPerBet)
                        {
                            profitPerBet += (canceled.SizeCancelled+canceled.SizeSettled) * (canceled.PriceRequested - 1);
                        }
                        else
                        {
                            profitPerBet = (canceled.SizeCancelled+canceled.SizeSettled);
                        }
                    }
                    profitPerBet = Math.Round(profitPerBet, 2);
                    placedBetDB.resultCode = "Canceled";
                    placedBetDB.success = false;
                    lastPlacedBetId = lastPlacedBetId.Replace(canceled.BetId + ";","");
                    UpdateDB(placedBetDB.id);
	            }
                var voidedOrdersList = MainWindow.Client.listClearedOrders(TO.BetStatus.VOIDED, null, null, null, null, betIdsSet, null, null, null, true, null, null, null);
                foreach (var voided in voidedOrdersList.ClearedOrders)
	            {
                    VoidedOrLapsedResult(voided);
	            }
                var lapsedOrdersList = MainWindow.Client.listClearedOrders(TO.BetStatus.LAPSED, null, null, null, null, betIdsSet, null, null, null, true, null, null, null);
                foreach (var lapsed in lapsedOrdersList.ClearedOrders)
	            {
                    VoidedOrLapsedResult(lapsed);
	            }
            }
            catch (System.Net.WebException)
            {
                MainWindow.restartTimer_Tick(this, new EventArgs());
            }
            this.timer.Start();
        }

        private void VoidedOrLapsedResult(TO.ClearedOrderSummary voided)
        {
            var context = new BFBDBEntities();
            var yesterday = DateTime.Now.AddDays(-2);
            var placedBetDB = context.PlacedBets.Where(b => b.datePlaced > yesterday).FirstOrDefault(p => p.betId == voided.BetId);
                    
            if (betType == "Back")
            {
                if (profitPerBet > initialProfitPerBet)
                {
                    profitPerBet += (voided.SizeSettled + voided.SizeCancelled) * (voided.PriceRequested - 1);
                }
                else
                {
                    profitPerBet = (voided.SizeSettled + voided.SizeCancelled) * (voided.PriceRequested - 1);
                }
            }
            else
            {
                if (profitPerBet > initialProfitPerBet)
                {
                    profitPerBet += (voided.SizeSettled + voided.SizeCancelled) * (voided.PriceRequested - 1);
                }
                else
                {
                    profitPerBet = (voided.SizeSettled + voided.SizeCancelled);
                }
            }
            profitPerBet = Math.Round(profitPerBet, 2);
            lastPlacedBetId = lastPlacedBetId.Replace(voided.BetId + ";", "");
            UpdateDB(placedBetDB.id);
            if (placedBetDB != null)
            {
                placedBetDB.resultCode = "Lapsed";
            }
        }

        public static void UpdatePlacedBetResult(PlacedBets bet, string scoresLast2Days)
        {
            // \Soccer\Czech Soccer\Czech U21 League\Fixtures 08 August   \AC Sparta Praha U21 v FK Teplice U21
            string mMPseparated = bet.marketMenuPath.Replace(" v ","#");
            string[] matchNamesSplit = mMPseparated.Split('#');
            string[] firstTeamSplit = matchNamesSplit[0].Trim().Split();
            string[] secondTeamSplit = matchNamesSplit[1].Trim().Split();
            List<string> matchNameCompoundsList = new List<string>();
            matchNameCompoundsList.AddRange(firstTeamSplit);
            matchNameCompoundsList.AddRange(secondTeamSplit);
            var matchesLive = scoresLast2Days.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            if (matchesLive.Length > 0 & matchNameCompoundsList.Count > 0)
            {
                foreach (var match in matchesLive)
                {
                    decimal machingChars = 0;
                    decimal matchNameCharNumber = 0;
                    foreach (var item in matchNameCompoundsList)
                    {
                        matchNameCharNumber += item.Length;
                        if (match.Contains(item))
                        {
                            machingChars += item.Length;
                        }
                    }
                    decimal compared = Math.Round((machingChars / matchNameCharNumber), 1);
                    if ((double)compared > 0.7)
                    {
                        var matchArray = match.Trim().Split().ToList();
                        var progress = matchArray.Last();
                        matchArray.Remove(matchArray.Last());
                        var score = matchArray.Find(f => f.Contains("-") & f.Length < 8);
                        matchArray[matchArray.IndexOf(score)] = "?v?";
                        StringBuilder sb = new StringBuilder();
                        foreach (var item in matchArray)
                        {
                            sb.Append(item);
                        }
                        var teams = sb.ToString();
                        var firstTeam = teams.Split(new string[] { "?v?" }, StringSplitOptions.None).First().Trim();
                        var lastTeam = teams.Split(new string[] { "?v?" }, StringSplitOptions.None).Last().Trim();
                        var context = new BFBDBEntities();
                        var pb = context.PlacedBets.Find(bet.id);
                        pb.resultCode = progress.Trim();
                        pb.score = score.Replace('-', ':');
                        try
                        {
                            context.SaveChanges();
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        private bool CancelBetAndResetBetId(TO.CurrentOrderSummary order)
        {            
            TO.CancelInstruction ci = new TO.CancelInstruction();
            ci.BetId = order.BetId;
            var cireport = MainWindow.Client.cancelOrders(order.MarketId, new List<TO.CancelInstruction>() { ci}, "");            
            if (cireport.ErrorCode == TO.ExecutionReportErrorCode.OK & (cireport.Status == TO.ExecutionReportStatus.SUCCESS | cireport.Status == TO.ExecutionReportStatus.TIMEOUT))
            {
                return true;
            }
            return false;
        }

        private void RemoveBetId(string betId)
        {
            var context = new BFBDBEntities();
            this.lastPlacedBetId = this.lastPlacedBetId.Replace(betId + ";", "");
            context.Betlines.Find(this.BetlineId).lastPlacedBetId = this.lastPlacedBetId;
            context.SaveChanges();
        }

        private void FindAndBetOnSutableRunner(TO.MarketBook m)
        {
            List<string> sortedOrderList = null;
            var marketSOList = Filters.sortedOrders.Trim(MainWindow.MainSeparator[0]).Split(MainWindow.MainSeparator[0]).ToList();
            foreach (var marketSo in marketSOList)
            {
                if (marketSo.ToUpper().Contains(CurrentMarket.Description.MarketType.ToUpper()))
                {
                    sortedOrderList = sortedOrderList = marketSo.Trim(MainWindow.SortedOrderSeparator[0]).Split(MainWindow.SortedOrderSeparator[0]).ToList();
                    sortedOrderList.RemoveAt(0);
                }
            }
            var sortedRunnerInfoByTotalAmmountMatched = from runner in m.Runners
                                                        where IsSortedOrder(runner, CurrentMarket, sortedOrderList)
                                                        orderby runner.TotalMatched descending
                                                        select runner;

            foreach (var runner in sortedRunnerInfoByTotalAmmountMatched)
            {

                var backPrices = (from price in runner.ExchangePrices.AvailableToBack
                                  where price.Size > 0
                                  orderby price.Price descending
                                  select price).ToList();
                var layPrices = (from price in runner.ExchangePrices.AvailableToLay
                                 where price.Size > 0
                                 orderby price.Price ascending
                                 select price).ToList();
                try
                {
                    if (BackAndLayPricesInRange(backPrices, layPrices))
                    {
                        double currentPrice = 0;
                        if (this.betType == "Back")
                        {
                            currentPrice = backPrices[0].Price;
                        }
                        else
                        {
                            currentPrice = layPrices[0].Price;
                        }
                        double size = this.betType == "Back" ? Math.Round((this.profitPerBet / (currentPrice - 1)), 2) : Math.Round(this.profitPerBet, 2);
                        if (this.algorithmName.ToUpper().Contains("increment".ToUpper()))
                        {
                            double smallSize = this.betType == "Back" ? Math.Round((this.initialProfitPerBet / (currentPrice - 1)), 2) : Math.Round(this.initialProfitPerBet, 2);
                            smallSize = smallSize < 2 ? 2 : smallSize;
                            double limit = 0;
                            var maxAmmIncremented = this.Filters.maxAmmountIncremented;
                            if (maxAmmIncremented != null)
                            {
                                limit = maxAmmIncremented > 0 ? (maxAmmIncremented < 2 ? 2 : (double)maxAmmIncremented) : ((double)maxAmmIncremented < 0 ? 2 : double.MaxValue);
                            }                            
                            size = ((size >= 2) & (size <= limit)) ? size : smallSize;
                        }
                        else
                        {
                            size = size < 2 ? 2 : size;
                        }
                        BetInstruction.LimitOrder.Price = currentPrice;
                        BetInstruction.LimitOrder.Size = size;
                        BetInstruction.SelectionId = runner.SelectionId;
                        BetInstruction.Side = this.betType == "Back" ? TO.Side.BACK : TO.Side.LAY;
                        // Place Bet if succsessful do the following eitherway continue looking for sutable bet
                        if (MarketIsPlayable())
                        {
                            if (this.PlaceBet())
                            {
                                // reset the Bet Property
                                this.lastBetLost = true;
                                this.profitPerBet = initialProfitPerBet;
                                this.UpdateDB();
                                return;
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        private bool MarketIsPlayable()
        {
            try
            {
                var context = MainWindow.databaseContext;
                DateTime last2DaysDate = DateTime.Now.AddDays(-2);
                var placedBets = from placedBet in context.PlacedBets
                                    where placedBet.eventId == CurrentMarket.Event.Id & (int)placedBet.betlineId == BetlineId & placedBet.datePlaced < DateTime.Now & placedBet.datePlaced > last2DaysDate
                                    select placedBet;
                if (!(placedBets.Count() > 0) & LiveScoreAvailable(CurrentMarket as TO.MarketCatalogue))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool LiveScoreAvailable(TO.MarketCatalogue eventName)
        {
            try
            {
                // \Soccer\Czech Soccer\Czech U21 League\Fixtures 08 August   \AC Sparta Praha U21 v FK Teplice U21
                var correctedName = eventName.Event.Name.Trim().Replace(" v ", "#");
                string[] matchNamesSplit = correctedName.Split('#');
                string[] firstTeamSplit = matchNamesSplit[0].Trim().Split();
                string[] secondTeamSplit = matchNamesSplit[1].Trim().Split();
                List<string> matchNameCompoundsList = new List<string>();
                matchNameCompoundsList.AddRange(firstTeamSplit);
                matchNameCompoundsList.AddRange(secondTeamSplit);
                var matchesLive = MainWindow.soccerLivescore.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                if (matchesLive.Length > 0 & matchNameCompoundsList.Count > 0)
                {
                    foreach (var match in matchesLive)
                    {
                        decimal machingChars = 0;
                        decimal matchNameCharNumber = 0;
                        foreach (var item in matchNameCompoundsList)
                        {
                            matchNameCharNumber += item.Length;
                            if (EventContainsTeam(match, item))
                            {
                                machingChars += item.Length;
                            }
                        }
                        decimal compared = Math.Round((machingChars / matchNameCharNumber), 1);
                        if ((double)compared > 0.7)
                        {
                            if (this.Filters.goalDifferenceMin == null & this.Filters.goalDifferenceMax == null)
                            {
                                return true;
                            }
                            else
                            {
                                var matchArray = match.Trim().Split().ToList();
                                matchArray.Remove(matchArray.Last());
                                var score = matchArray.Find(f => f.Contains("-") & f.Length < 8);
                                try
                                {
                                    var goals = score.Split('-');
                                    var homeGoals = short.Parse(goals.First().Trim());
                                    var awayGoals = short.Parse(goals.Last().Trim());
                                    double powDifference = 0;
                                    try 
	                                {	        
		                                powDifference = Math.Pow((double)(homeGoals - awayGoals), 2);
	                                }
	                                catch (Exception)
	                                {
	                                } 
                                    var powDifMin = this.Filters.goalDifferenceMin == null ? 0 : Math.Pow((double)this.Filters.goalDifferenceMin, 2);
                                    var powDifMax = this.Filters.goalDifferenceMax == null ? Math.Pow((double)short.MaxValue, 2) : Math.Pow((double)this.Filters.goalDifferenceMax, 2);
                                    if (powDifference >= powDifMin & powDifference <= powDifMax)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                catch (Exception)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                /////
                if (eventName.Runners.Count>3)
                {
                    return false;
                }
                firstTeamSplit = eventName.Runners[0].RunnerName.Trim().Split();
                secondTeamSplit = eventName.Runners[1].RunnerName.Trim().Split();
                matchNameCompoundsList = new List<string>();
                matchNameCompoundsList.AddRange(firstTeamSplit);
                matchNameCompoundsList.AddRange(secondTeamSplit);
                matchesLive = MainWindow.soccerLivescore.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                if (matchesLive.Length > 0 & matchNameCompoundsList.Count > 0)
                {
                    foreach (var match in matchesLive)
                    {
                        decimal machingChars = 0;
                        decimal matchNameCharNumber = 0;
                        foreach (var item in matchNameCompoundsList)
                        {
                            matchNameCharNumber += item.Length;
                            if (EventContainsTeam(match, item))
                            {
                                machingChars += item.Length;
                            }
                        }
                        decimal compared = Math.Round((machingChars / matchNameCharNumber), 1);
                        if ((double)compared > 0.7)
                        {
                            if (this.Filters.goalDifferenceMin == null & this.Filters.goalDifferenceMax == null)
                            {
                                return true;
                            }
                            else
                            {
                                var matchArray = match.Trim().Split().ToList();
                                matchArray.Remove(matchArray.Last());
                                var score = matchArray.Find(f => f.Contains("-") & f.Length < 8);
                                try
                                {
                                    var goals = score.Split('-');
                                    var homeGoals = short.Parse(goals.First().Trim());
                                    var awayGoals = short.Parse(goals.Last().Trim());
                                    double powDifference = 0;
                                    try
                                    {
                                        powDifference = Math.Pow((double)(homeGoals - awayGoals), 2);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                    var powDifMin = this.Filters.goalDifferenceMin == null ? 0 : Math.Pow((double)this.Filters.goalDifferenceMin, 2);
                                    var powDifMax = this.Filters.goalDifferenceMax == null ? Math.Pow((double)short.MaxValue, 2) : Math.Pow((double)this.Filters.goalDifferenceMax, 2);
                                    if (powDifference >= powDifMin & powDifference <= powDifMax)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                catch (Exception)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        private bool EventContainsTeam(string eventName, string team)
        {
            try
            {
                var eventNameToUpper = eventName.ToLower().ToUpper();
                var teamToUpper = team.ToLower().ToUpper();
                if (eventNameToUpper.Contains(teamToUpper))
                {
                    return true;
                }
                else
                {
                    var teamCharsCount = team.Length;
                    for (int symsCount = 1; symsCount < 4; symsCount++)
                    {
                        for (int j = 0; j <= team.Length - (symsCount); j++)
                        {
                            StringBuilder teamEdit = new StringBuilder(teamToUpper);
                            for (int i = 0; i < symsCount; i++)
                            {
                                teamEdit.Replace(teamEdit[i + j], ' ', j, 1);
                            }
                            var charsMatched = 0;
                            var teamSplit = teamEdit.ToString().Split();
                            foreach (var part in teamSplit)
                            {
                                if (eventNameToUpper.Contains(part))
                                {
                                    charsMatched += part.Length;
                                }
                            }
                            if (((double)charsMatched / teamCharsCount) > 0.7)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool PlaceBet()
        {

            try
            {
                List<TO.PlaceInstruction> pbinstructions = new List<TO.PlaceInstruction>();
                pbinstructions.Add(BetInstruction);
                var language = "bg";
                var placeExecutionReport = MainWindow.Client.placeOrders(CurrentMarket.MarketId, "123456", pbinstructions, language);
                var lastPlacedBet = this.SaveLastPlacedBet(placeExecutionReport.InstructionReports[0]);
                if (placeExecutionReport.ErrorCode == TO.ExecutionReportErrorCode.OK)
                {
                    //saves the BetResult in PlacedBet property
                    if (lastPlacedBet!=null)
                    {
                        this.lastPlacedBetId += lastPlacedBet.betId + ";";
                        this.lastBetLost = true;
                        UpdateDB(lastPlacedBet.id);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    // log the error in the database
                    try
                    {
                        this.lastPlacedBetId += lastPlacedBet.betId + ";";
                        this.lastBetLost = true;
                        UpdateDB(lastPlacedBet.id);
                        return false;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            catch (System.Net.WebException)
            {
                MainWindow.restartTimer_Tick(this, new EventArgs());
                return false;
            }
            catch (NullReferenceException)
            {
                return true;
            }
            catch (TO.APINGException v)
            {
                var l = v.Message;
                return false;
            }
            catch (Exception e)
            {
                var s = e.Message;
                return false;
            }
        }

        private void UpdateDB()
        {
            var context = new BFBDBEntities();
            var betline = context.Betlines.Find(this.BetlineId);
            betline.isActive = this.isActive;
            betline.lastBetLost = this.lastBetLost;
            betline.lastPlacedBetId = this.lastPlacedBetId;
            betline.profitPerBet = this.profitPerBet;
            betline.ballance = this.ballance;
            try
            {
                foreach (var item in lastPlacedBetId.Trim().Trim(';').Split(';'))
                {
                    var yesterday = DateTime.Now.AddDays(-2);
                    var pB = context.PlacedBets.Where(b=>b.datePlaced>yesterday).FirstOrDefault(p=>p.betId == item);
                    if (pB != null)
                    {
                        pB.success = !this.lastBetLost;
                    }
                }
                context.SaveChanges();
            }
            catch (Exception)
            {
            }
        }

        private void UpdateDB(int id)
        {
            var context = MainWindow.databaseContext;
            var betline = context.Betlines.Find(this.BetlineId);
            betline.isActive = this.isActive;
            betline.lastBetLost = this.lastBetLost;
            betline.lastPlacedBetId = this.lastPlacedBetId;
            betline.profitPerBet = this.profitPerBet;
            betline.ballance = this.ballance;
            context.SaveChanges();
            var pB = context.PlacedBets.Find(id);
            if (pB != null)
            {
                pB.success = !this.lastBetLost;
            }
            context.SaveChanges();
        }

        private PlacedBets SaveLastPlacedBet(TO.PlaceInstructionReport placeBetsResult)
        {
            try
            {
                if (placeBetsResult.ErrorCode == TO.InstructionReportErrorCode.BET_IN_PROGRESS | placeBetsResult.ErrorCode == TO.InstructionReportErrorCode.BET_TAKEN_OR_LAPSED | placeBetsResult.ErrorCode == TO.InstructionReportErrorCode.OK)
                {
                    var context = MainWindow.databaseContext;
                    var runner = CurrentMarket.Runners.FirstOrDefault(r => r.SelectionId == placeBetsResult.Instruction.SelectionId);
                    BFDL.PlacedBets bet = new PlacedBets() { averagePrice = placeBetsResult.AveragePriceMatched, betId = placeBetsResult.BetId, betlineId = this.BetlineId, betType = this.betType, datePlaced = DateTime.Now, eventId = CurrentMarket.Event.Id, marketMenuPath = CurrentMarket.Event.Name, marketName = CurrentMarket.MarketName, resultCode = placeBetsResult.Status.ToString(), score = null, Selection = runner.RunnerName, sizeMatched = placeBetsResult.SizeMatched, sortedOrder = runner.SortPriority, success = false };
                    //bet.averagePrice = placeBetsResult.AveragePriceMatched;
                    //bet.betId = placeBetsResult.BetId;
                    //bet.datePlaced = DateTime.Now;
                    //bet.resultCode = placeBetsResult.Status.ToString();
                    //bet.sizeMatched = placeBetsResult.SizeMatched;
                    //bet.success = false;
                    //bet.betlineId = this.BetlineId;
                    ////bet.marketId = CurrentMarket.MarketId;
                    //bet.marketName = CurrentMarket.MarketName;
                    //bet.marketMenuPath = CurrentMarket.Event.Name;
                    //bet.sortedOrder = runner.SortPriority;
                    //bet.Selection = runner.RunnerName;
                    //bet.eventId = CurrentMarket.Event.Id;
                    context.PlacedBets.Add(bet);
                    context.SaveChanges();
                    return bet;
                }
                else
                {
                    this.lastBetLost = true;
                    return null;
                }
            }
            catch (Exception)
            {
                this.lastBetLost = true;
                return null;
            }            
        }

        private bool BackAndLayPricesInRange(List<TO.PriceSize> backPrice, List<TO.PriceSize> layPrice)
        {
            var difference = Math.Round((((this.stakeRangeMin + this.stakeRangeMax) / 2) - 1) / 2, 2);
            if (this.betType == "Back")
            {
                return (backPrice[0].Price <= this.stakeRangeMax & backPrice[0].Price >= this.stakeRangeMin & (layPrice[0].Price - backPrice[0].Price) < difference & ((backPrice[0].Size) < (layPrice[0].Size)));
            }
            else
            {
                return (layPrice[0].Price <= this.stakeRangeMax & layPrice[0].Price >= this.stakeRangeMin & (layPrice[0].Price - backPrice[0].Price) < difference & ((backPrice[0].Size) > (layPrice[0].Size)));
            }
        }
        
        private bool IsSortedOrder(TO.Runner runner, TO.MarketCatalogue marketCatalogue, List<string> sortedOrderList)
        {
            var sortedOrder = marketCatalogue.Runners.Find(r=>r.SelectionId == runner.SelectionId).SortPriority;
            try
            {
                return sortedOrderList.Contains(sortedOrder.ToString()) | (sortedOrderList == null & sortedOrder > 0);		 
            }
            catch (Exception)
            {
                return false;
            }	        
        }

        internal void AddToDB()
        {
            BFDL.BFBDBEntities context = MainWindow.databaseContext;
            context.Betlines.Add(MainWindow.CurrentBetline as BFDL.Betlines);
            context.SaveChanges();
        }
    }
}
