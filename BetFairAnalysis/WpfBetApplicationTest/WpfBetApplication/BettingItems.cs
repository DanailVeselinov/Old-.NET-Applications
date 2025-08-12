using mshtml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Linq;
using WpfBetApplication.WebExchangeService;

namespace WpfBetApplication
{
    public class BettingLine
    {
        int betlineId;
        public int BetlineId
        {
            get { return betlineId; }
            set { betlineId = value; }
        }
        private PlaceBets bet;
        public PlaceBets Bet
        {
            get { return bet; }
            set { bet = value; }
        }
        List<Int64> lastPlacedBetIds;
        public List<Int64> LastPlacedBetId
        {
            get { return lastPlacedBetIds; }
            set { lastPlacedBetIds = value; }
        }
        List<Market> currentMarketList;
        public List<Market> CurrentMarketList
        {
            get { return currentMarketList; }
            set { currentMarketList = value; }
        }
        int eventId;
        public int EventId
        {
            get { return eventId; }
            set { eventId = value; }
        }
        string placeBetMarketMenuPath;
        public string PlaceBetMarketMenuPath
        {
            get { return placeBetMarketMenuPath; }
            set { placeBetMarketMenuPath = value; }
        }
        int eventType;
        public int EventType
        {
            get { return eventType; }
            set { eventType = value; }
        }
        string marketName;
        public string MarketName
        {
            get { return marketName; }
            set { marketName = value; }
        }
        FilterMarkets filter;
        public FilterMarkets Filter
        {
            get { return filter; }
            set { filter = value; }
        }
        double stakeRangeMin;
        public double StakeRangeMin
        {
            get { return stakeRangeMin; }
            set { stakeRangeMin = value; }
        }
        double stakeRangeMax;
        public double StakeRangeMax
        {
            get { return stakeRangeMax; }
            set { stakeRangeMax = value; }
        }
        double initialProfitPerBet;
        public double InitialProfitPerBet
        {
            get { return initialProfitPerBet; }
            set { initialProfitPerBet = value; }
        }
        double profitPerBet;
        public double ProfitPerBet
        {
            get { return profitPerBet; }
            set { profitPerBet = value; }
        }
        bool lastBetLost;
        public bool LastBetLost
        {
            get { return lastBetLost; }
            set { lastBetLost = value; }
        }
        bool isActive;
        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }        
        string currencyCode;
        public string CurrencyCode
        {
            get { return currencyCode; }
            set { currencyCode = value; }
        }
        string algorithmName;
        public string AlgorithmName
        {
            get { return algorithmName; }
            set { algorithmName = value; }
        }
        string betlineName;
        public string BetlineName
        {
            get { return betlineName; }
            set { betlineName = value; }
        }
        string betType;
        public string BetType
        {
            get { return betType; }
            set { betType = value; }
        }
        public decimal Ballance { get; set; }
        public DispatcherTimer timer;



        public static List<Market> FilterMarketsByAllFilters(List<Market> marketsList, FilterMarkets filter)
        {            
            if (filter.MarketTypesArray != null)
            {
                marketsList = filter.FilterMarketsByType(marketsList, filter.MarketTypesArray);
            }           
            if (filter.IsBSP != null)
            {
                marketsList = filter.FilterMarketsByIsBSP(marketsList, (bool)filter.IsBSP);
            }
            if (filter.ExchangeId != null)
            {
                marketsList = filter.FilterMarketsByExcahgeId(marketsList, (byte)filter.ExchangeId);
            }           
            if (filter.IsActive != null)
            {
                marketsList = filter.FilterMarketsByStatus(marketsList, (bool)filter.IsActive);
            }
            if (filter.RunnersMin != int.MinValue || filter.RunnersMax != int.MaxValue)
            {
                marketsList = filter.FilterMarketsByNumberOfRunners(marketsList, (int)filter.RunnersMin, (int)filter.RunnersMax);
            }
            if (filter.WinnersMin != int.MinValue || filter.WinnersMax != int.MaxValue)
            {
                marketsList = filter.FilterMarketsByNumberOfWinners(marketsList, (int)filter.WinnersMin, (int)filter.WinnersMax);
            }
            return marketsList;
        }
        //Call this method by getMarketsButton
        public void GetMarketsRequestFiltered() 
        {
            try
            {
                WebExchangeService.GetAllMarketsReq request = new WebExchangeService.GetAllMarketsReq();
                request.header = new APIRequestHeader();
                request.header.sessionToken = Session.globalHeader.sessionToken;
                request.eventTypeIds = filter.EventTypesArray;
                request.fromDate = filter.FromEventDate;
                request.toDate = filter.ToEventDate == null ? DateTime.UtcNow : filter.ToEventDate;
                request.countries = filter.CountryCodeArray;
                try
                {
                    WebExchangeService.GetAllMarketsResp responce = Session.exchangeService.getAllMarkets(request);
                    Session.checkHeader(responce.header);
                    if (responce.errorCode == GetAllMarketsErrorEnum.OK)
                    {
                        this.CurrentMarketList = Session.AllMarketsInList(responce.marketData);
                    }
                    else
                    {
                        //Log error
                        this.CurrentMarketList = new List<Market>();
                    }
                }
                catch (System.Net.WebException)
                {
                }
                
            }
            catch (Exception)
            {
                //login
                this.CurrentMarketList = new List<Market>();
                //WpfBetApplication.MainWindow.Button_Click_Login();
            }
            //catch
            //System.Net.WebException            
            //Session.exchangeService.getAllMarketsAsync(request, new Guid().ToString());
            //Session.exchangeService.getAllMarketsCompleted += exchangeService_getAllMarketsCompleted;                        
        }

        void exchangeService_getAllMarketsCompleted(object sender, getAllMarketsCompletedEventArgs e)
        {
            Session.checkHeader(e.Result.header);
            if (e.Result.errorCode == GetAllMarketsErrorEnum.OK)
            {
                this.CurrentMarketList = Session.AllMarketsInList(e.Result.marketData);
            }
            else
            {
                //Log error
                this.CurrentMarketList = new List<Market>();
            }
        }

        public void FindAndBetOnSutableRunnerIncrementing(Market m)
        {
            GetCompleteMarketPricesCompressedReq PricesCompressedReq = new GetCompleteMarketPricesCompressedReq();
            PricesCompressedReq.header = new APIRequestHeader();
            PricesCompressedReq.header.sessionToken = Session.globalHeader.sessionToken;
            PricesCompressedReq.marketId = m.marketId;
            //?? make this form database
            PricesCompressedReq.currencyCode = "EUR";
            try
            {
                WebExchangeService.GetCompleteMarketPricesCompressedResp responcePricesCompressed = Session.exchangeService.getCompleteMarketPricesCompressed(PricesCompressedReq);
                Session.checkHeader(responcePricesCompressed.header);
                if (responcePricesCompressed.errorCode == GetCompleteMarketPricesErrorEnum.OK)
                {
                    List<RunnerInfoType> marketsRunnersInfo = UnpackCompleteMarketPrices(responcePricesCompressed.completeMarketPrices);
                    // saves the sutable runnerId in the betline.sutableRunnerId and sutable Price in betline.sutablePrice
                    this.FilterSutableRunnerIdIncrementing(marketsRunnersInfo, m.name);
                }
            }
            catch (System.Net.WebException)
            {
            }            
            
        }

        private void FilterSutableRunnerId(List<RunnerInfoType> RunnersInfoList, string marketName)
        {
            List<byte> sortedOrderList = null;
            switch (marketName)
	        {
                case "Match Odds":
                    sortedOrderList = this.Filter.SortedOrdersMatchOddsList;
                    break;
                case "Half Time":
                    sortedOrderList = this.Filter.SortedOrdersMatchOddsList;
                    break;
                case "Correct Score":
                    sortedOrderList = this.Filter.SortedOrdersCorrectScoreList;
                    break;
		        default:
                break;
	        }
            var sortedRunnerInfoByTotalAmmountMatched = from runner in RunnersInfoList
                                                        where (sortedOrderList.Contains((byte)runner.SortOrdered) | (sortedOrderList == null & runner.SortOrdered>0))
                                                        orderby runner.TotalAmmountMatched descending
                                                        select runner;

            foreach (RunnerInfoType runner in sortedRunnerInfoByTotalAmmountMatched)
            {
                var backPrices = (from price in runner.Prices
                                 where price.BackAmount > 0
                                 orderby price.Price descending
                                 select price).ToList();
                var layPrices = (from price in runner.Prices
                                where price.LayAmount > 0
                                orderby price.Price ascending
                                select price).ToList();
                try
                {
                    if (!(backPrices.Count>1 & layPrices.Count>1))
                    {
                        continue;
                    }
                    if (BackAndLayPricesInRange(backPrices, layPrices))
                    {
                        double currentPrice = 0;
                        if (this.betType == "B")
                        {
                            currentPrice = backPrices[0].Price;
                        }
                        else
                        {
                            currentPrice = layPrices[0].Price;
                        }
                        double size = this.BetType == "B" ? Math.Round((this.ProfitPerBet / (currentPrice - 1)), 2) : Math.Round((this.ProfitPerBet * (currentPrice - 1)), 2);
                        if (this.AlgorithmName.ToUpper().Contains("increment".ToUpper()))
                        {
                            double smallSize = this.BetType == "B" ? Math.Round((this.initialProfitPerBet / (currentPrice - 1)), 2) : Math.Round((this.initialProfitPerBet * (currentPrice - 1)), 2);
                            smallSize = smallSize < 2 ? 2 : smallSize;
                            double limit = this.filter.MaxAmmountIncremented > 0 ? (this.filter.MaxAmmountIncremented < 2 ? 2 : this.filter.MaxAmmountIncremented) : (this.filter.MaxAmmountIncremented < 0 ? 2 : double.MaxValue);
                            size = ((size >= 2) & (size <= limit)) ? size : smallSize;
                        }
                        else
                        {
                            size = size < 2 ? 2 : size;
                        }
                        this.Bet.asianLineId = runner.AsianLineId;
                        this.Bet.selectionId = runner.SelectionId;
                        this.Bet.price = currentPrice;
                        this.Bet.bspLiability = 0.0; //May be different if needed To place BSP bets
                        this.Bet.size = (size >= 2 ? size : 2); //?? this is the minimum betsize in GBP
                        this.Bet.betType = this.BetType == "B" ? BetTypeEnum.B : BetTypeEnum.L;
                        // Place Bet if succsessful do the following eitherway continue looking for sutable bet
                        if (MarketIsPlayable())
                        {
                            if (this.PlaceBet(runner))
                            {
                                // reset the Bet Property
                                this.Bet = new PlaceBets();
                                this.LastBetLost = true;
                                this.ProfitPerBet = this.InitialProfitPerBet;
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

        private bool BackAndLayPricesInRange(List<PricesType> backPrice, List<PricesType> layPrice)
        {
            var difference = Math.Round((((this.stakeRangeMin + this.stakeRangeMax) / 2) - 1) / 2, 2);
            if (this.betType == "B")
            {
                return (backPrice[0].Price <= this.stakeRangeMax & backPrice[0].Price >= this.stakeRangeMin & (layPrice[0].Price - backPrice[0].Price) < difference & ((backPrice[0].BackAmount) < (layPrice[0].LayAmount)));
            }
            else
            {
                return (layPrice[0].Price <= this.stakeRangeMax & layPrice[0].Price >= this.stakeRangeMin & (layPrice[0].Price - backPrice[0].Price) < difference & ((backPrice[0].BackAmount) > (layPrice[0].LayAmount)));
            }
        }

        private void FilterSutableRunnerIdIncrementing(List<RunnerInfoType> RunnersInfoList, string marketName)
        {

            List<byte> sortedOrderList = null;
            switch (marketName)
            {
                case "Match Odds":
                    sortedOrderList = this.Filter.SortedOrdersMatchOddsList;
                    break;
                case "Half Time":
                    sortedOrderList = this.Filter.SortedOrdersMatchOddsList;
                    break;
                case "Correct Score":
                    sortedOrderList = this.Filter.SortedOrdersCorrectScoreList;
                    break;
                default:
                    break;
            }
            var sortedRunnerInfoByTotalAmmountMatched = from runner in RunnersInfoList
                                                        where (sortedOrderList.Contains((byte)runner.SortOrdered) | (sortedOrderList == null & runner.SortOrdered > 0))
                                                        orderby runner.TotalAmmountMatched descending
                                                        select runner;

            foreach (RunnerInfoType runner in sortedRunnerInfoByTotalAmmountMatched)
            {
                var backPrices = (from price in runner.Prices
                                  where price.BackAmount > 0
                                  orderby price.Price descending
                                  select price).ToList();
                var layPrices = (from price in runner.Prices
                                 where price.LayAmount > 0
                                 orderby price.Price ascending
                                 select price).ToList();
                try
                {
                    if (!(backPrices.Count>0 & layPrices.Count>0))
                    {
                        continue;
                    }
                    if (BackAndLayPricesInRange(backPrices, layPrices))
                    {
                        double currentPrice = 0;
                        if (this.betType == "B")
                        {
                            currentPrice = backPrices[0].Price;
                        }
                        else
                        {
                            currentPrice = layPrices[0].Price;
                        }
                        double size = this.BetType == "B" ? Math.Round((this.ProfitPerBet / (currentPrice - 1)), 2) : Math.Round(this.ProfitPerBet, 2);
                        if (this.algorithmName.ToUpper().Contains("increment".ToUpper()))
                        {
                            double smallSize = this.BetType == "B" ? Math.Round((this.initialProfitPerBet / (currentPrice - 1)), 2) : Math.Round(this.initialProfitPerBet, 2);
                            smallSize = smallSize < 2 ? 2 : smallSize;
                            double limit = (this.filter.MaxAmmountIncremented > 0) ? (this.filter.MaxAmmountIncremented < 2 ? 2 : this.filter.MaxAmmountIncremented) : (this.filter.MaxAmmountIncremented < 0 ? 2 : double.MaxValue);
                            size = ((size >= 2) & (size <= limit)) ? size : smallSize;
                        }
                        else
                        {
                            size = size < 2 ? 2 : size;
                        }
                        this.Bet.asianLineId = runner.AsianLineId;
                        this.Bet.selectionId = runner.SelectionId;
                        this.Bet.price = currentPrice;
                        this.Bet.bspLiability = 0.0; //May be different if needed To place BSP bets
                        this.Bet.size = (size >= 2 ? size : 2); //?? this is the minimum betsize in GBP
                        this.Bet.betType = this.BetType == "B" ? BetTypeEnum.B : BetTypeEnum.L;
                        // Place Bet if succsessful do the following eitherway continue looking for sutable bet
                        if (MarketIsPlayable())
                        {
                            if (this.PlaceBet(runner))
                            {
                                // reset the Bet Property
                                this.Bet = new PlaceBets();
                                this.LastBetLost = true;
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

            //List<byte> sortedOrderList = null;
            //switch (marketName)
            //{
            //    case "Match Odds":
            //        sortedOrderList = this.Filter.SortedOrdersMatchOddsList;
            //        break;
            //    case "Correct Score":
            //        sortedOrderList = this.Filter.SortedOrdersCorrectScoreList;
            //        break;
            //    default:
            //    break;
            //}
            //var sortedRunnerInfoByTotalAmmountMatched = from runner in RunnersInfoList
            //                                            where LastPriceMatchedInRange(runner) & (sortedOrderList.Contains((byte)runner.SortOrdered) | (sortedOrderList == null & runner.SortOrdered>0))
            //                                            orderby runner.TotalAmmountMatched descending
            //                                            select runner;

            //foreach (RunnerInfoType runner in sortedRunnerInfoByTotalAmmountMatched)
            //{
            //    List<PricesType> sutablePricesList = new List<PricesType>();
            //    if (this.betType == "B")
            //    {
            //        var sutablePrices = from price in runner.Prices
            //                            where price.Price <= this.StakeRangeMax & price.Price >= this.stakeRangeMin
            //                            orderby price.Price descending
            //                            select price;
            //        sutablePricesList = sutablePrices.ToList();
            //    }
            //    else
            //    {
            //        var sutablePrices = from price in runner.Prices
            //                            where price.Price <= this.StakeRangeMax & price.Price >= this.stakeRangeMin
            //                            orderby price.Price ascending
            //                            select price;
            //        sutablePricesList = sutablePrices.ToList();
            //    }
                
            //    for (int i = 0; i < sutablePricesList.Count; i++)
            //    {
            //        //calculate size for B or L differently
            //        double size = this.BetType == "B" ? Math.Round((this.ProfitPerBet / (sutablePricesList[i].Price - 1)), 2) : Math.Round(this.ProfitPerBet, 2);
            //        var ammount = this.BetType == "B" ? sutablePricesList[i].BackAmount : sutablePricesList[i].LayAmount;
            //        if (ammount > size) //In profitPerBet because it will increment when we loose money
            //        {
            //            this.Bet.asianLineId = runner.AsianLineId;
            //            this.Bet.selectionId = runner.SelectionId;
            //            this.Bet.price = sutablePricesList[i].Price;
            //            this.Bet.bspLiability = 0.0; //May be different if needed To place BSP bets
            //            this.Bet.size = (size >= 2 ? size : 2); //?? this is the minimum betsize in GBP
            //            this.Bet.betType = this.BetType == "B" ? BetTypeEnum.B : BetTypeEnum.L;                        
            //            // Place Bet if succsessful do the following eitherway continue looking for sutable bet
            //            if (MarketIsPlayable())
            //            {
            //                if (this.PlaceBet(runner))
            //                {
            //                    // reset the Bet Property
            //                    this.Bet = new PlaceBets();
            //                    this.ProfitPerBet = this.InitialProfitPerBet;
            //                    this.LastBetLost = true;
            //                    this.UpdateDB();
            //                    return;
            //                }
            //            }
            //        }
            //    }
            //}
        }
        //test test test
        private bool MarketIsPlayable()
        {
            try
            {
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                DateTime last2DaysDate = DateTime.Now.AddDays(-2);
                var placedMarkets = from placedBet in context.PlacedBets
                                    where placedBet.eventId == this.EventId & placedBet.betlineId == this.BetlineId & placedBet.success != true & placedBet.datePlaced < DateTime.Now & placedBet.datePlaced > last2DaysDate
                                    select placedBet.eventId;
                if (!(placedMarkets.Count() > 0) & LiveScoreAvailable(this.PlaceBetMarketMenuPath))
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

        private bool LiveScoreAvailable(string marketMenuPath)
        {
            try
            {
                // \Soccer\Czech Soccer\Czech U21 League\Fixtures 08 August   \AC Sparta Praha U21 v FK Teplice U21
                string[] mMPseparated = marketMenuPath.Split('\\');
                string[] matchNamesSplit = mMPseparated.Last().Split('v');
                string[] firstTeamSplit = matchNamesSplit[0].Trim().Split();
                string[] secondTeamSplit = matchNamesSplit[1].Trim().Split();
                List<string> matchNameCompoundsList = new List<string>();
                matchNameCompoundsList.AddRange(firstTeamSplit);
                matchNameCompoundsList.AddRange(secondTeamSplit);                
                var matchesLive = Session.soccerLivescore.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                if (matchesLive.Length > 0 & matchNameCompoundsList.Count > 0)
                {
                    foreach (var match in matchesLive)
                    {
                        decimal machingChars = 0;
                        decimal matchNameCharNumber = 0;
                        foreach (var item in matchNameCompoundsList)
                        {
                            matchNameCharNumber += item.Length;
                            if (EventContainsTeam(match, item)) // match.ToUpper().Contains(item.ToUpper())
                            {
                                machingChars += item.Length;
                            }
                        }
                        decimal compared = Math.Round((machingChars / matchNameCharNumber), 1);
                        if ((double)compared > 0.7)
                        {
                            if (this.filter.GoalDifferenceMin == null & this.filter.GoalDifferenceMax == null)
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
                                    var powDifference = Math.Pow((double)(homeGoals - awayGoals), 2);
                                    var powDifMin = this.filter.GoalDifferenceMin == null ? 0 : Math.Pow((double)this.filter.GoalDifferenceMin, 2);
                                    var powDifMax = this.filter.GoalDifferenceMax == null ? Math.Pow((double)short.MaxValue, 2) : Math.Pow((double)this.filter.GoalDifferenceMax, 2);
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

        private bool LastPriceMatchedInRange(RunnerInfoType runner)
        {
            return (runner.LastPriceMatched <= stakeRangeMax) & (runner.LastPriceMatched >= stakeRangeMin);
        }

        //Tested and working
        public List<RunnerInfoType> UnpackCompleteMarketPrices(string marketPricesResult)
        {
            int marketId;
            int delay;
            string removedRunners;
            List<RunnerInfoType> runnersInfoList = new List<RunnerInfoType>();
            const string colonCode = "&%^@";
            var mPrices = marketPricesResult.Replace(@"\:",colonCode).Split(':');
            var fields = mPrices[0].Replace(@"\~","-").Split('~');
            marketId = int.Parse(fields[0]);
            delay = int.Parse(fields[1]); 
            // we can dispatch removed Runners if needed delimited by , then by ;
            removedRunners = fields[2].Replace(colonCode,":");
            for (int i = 0; i < mPrices.Length-1; i++)
	        {
		        var parts = mPrices[i+1].Split('|');
                fields = parts[0].Split('~');
                RunnerInfoType rInfo = new RunnerInfoType();
                try
	            {
                    rInfo.SelectionId = int.Parse(fields[0]);
                    rInfo.SortOrdered = int.Parse(fields[1]);
                    rInfo.TotalAmmountMatched = ParseDoubleFmNaNINF(fields[2]);
                    rInfo.LastPriceMatched = ParseDoubleFmNaNINF(fields[3]);
                    rInfo.Handicap = ParseDoubleFmNaNINF(fields[4]);
                    rInfo.ReductionFactor = ParseDoubleFmNaNINF(fields[5]);
                    rInfo.Vacant = (fields[6].ToLower() == "true");
                    rInfo.AsianLineId = fields[7] == "" ? 0 : int.Parse(fields[7]);
                    rInfo.FarBSP = ParseDoubleFmNaNINF(fields[8]);
                    rInfo.NearBSP = ParseDoubleFmNaNINF(fields[9]);
                    rInfo.ActualBSP = ParseDoubleFmNaNINF(fields[10]);
                    fields = parts[1].Split('~');
                    rInfo.Prices = new List<PricesType>();
                    for (int j = 0; j < (fields.Length/5); j++)
			        {                        
                        PricesType prices = new PricesType();
                        prices.Price = ParseDoubleFmNaNINF(fields[(j * 5) + 0]);
                        prices.BackAmount = ParseDoubleFmNaNINF(fields[(j * 5) + 1]);
                        prices.LayAmount = ParseDoubleFmNaNINF(fields[(j * 5) + 2]);
                        prices.TotalBSPBackAmount = ParseDoubleFmNaNINF(fields[(j * 5) + 3]);
                        prices.TotalBSPLayAmount = ParseDoubleFmNaNINF(fields[(j * 5) + 4]);
                        rInfo.Prices.Add(prices);			            		        
			        }
                    runnersInfoList.Add(rInfo);
	            }
	            catch (Exception)
	            {
                    //Log Error to database
	            }   
	        }
            return runnersInfoList;
        }

        public static double ParseDoubleFmNaNINF(string dbl)
        {
            switch (dbl)
            {
                case "":
                    return 0;
                case "NaN":
                    return 0;
                case "INF":
                    return 0;
                case "-INF":
                    return 0;
                default:
                    return double.Parse(dbl);
            }
        }
        //places bet
        public bool PlaceBet(RunnerInfoType runner)
        {            
            try
            {
                if (runner.SortOrdered == 0)
                {
                    return false;
                }
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                DBLBettingApp.PlacedBet newBet = new DBLBettingApp.PlacedBet();
                newBet.averagePrice = Bet.price;
                newBet.betlineId = BetlineId;
                newBet.datePlaced = DateTime.Now;
                newBet.eventId = EventId;
                newBet.marketId = this.Bet.marketId;
                newBet.marketMenuPath = PlaceBetMarketMenuPath;
                newBet.resultCode = "Matched";
                newBet.Selection = runner.SortOrdered.ToString();
                newBet.sizeMatched = (double)Bet.size;
                newBet.sortedOrder = runner.SortOrdered;
                newBet.success = false;
                newBet.marketName = this.MarketName;
                newBet.eventType = this.EventType;
                newBet.betType = this.betType;
                context.PlacedBets.Add(newBet);
                context.SaveChanges();
                DateTime lastMinute = DateTime.Now.AddMinutes(-1);
                var lastBetsSelection = from b in context.PlacedBets
                              where b.datePlaced > lastMinute & b.betlineId == this.BetlineId
                              orderby b.betId descending
                              select b.betId;                
                //saves the BetResult in PlacedBet property
                long lastPlacedBet = (long)lastBetsSelection.ToList()[0];
                if (lastPlacedBet > 0)
                {
                    this.LastPlacedBetId.Add(lastPlacedBet);
                    this.LastBetLost = true;
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
        //saves the result in tha database and returns the bet's Id
        public Int64 SaveLastPlacedBet(PlaceBetsResult placeBetsResult)
        {
            //??
            if (placeBetsResult.resultCode == PlaceBetsResultEnum.OK)
            {
                DBLBettingApp.PlacedBet bet = new DBLBettingApp.PlacedBet();
                bet.averagePrice = placeBetsResult.averagePriceMatched;
                bet.betId = placeBetsResult.betId;
                bet.datePlaced = DateTime.Now;
                bet.resultCode = placeBetsResult.resultCode.ToString();
                bet.sizeMatched = placeBetsResult.sizeMatched;
                bet.success = false;
                bet.betlineId = this.BetlineId;
                bet.marketId = this.EventId;
                bet.marketMenuPath = this.PlaceBetMarketMenuPath;
                Session.databaseContext.PlacedBets.Add(bet);
                Session.databaseContext.SaveChanges();
                GetBetReq getBetReq = new GetBetReq();
                getBetReq.header = new APIRequestHeader();
                getBetReq.header.sessionToken = Session.globalHeader.sessionToken;
                getBetReq.betId = placeBetsResult.betId;
                try
                {
                    WebExchangeService.GetBetResp resp = Session.exchangeService.getBet(getBetReq);
                    Session.checkHeader(resp.header);
                    switch (resp.errorCode)
                    {
                        case GetBetErrorEnum.OK:
                            bet.marketMenuPath = resp.bet.fullMarketName;
                            bet.Selection = resp.bet.selectionName + (resp.bet.handicap == 0 ? "" : resp.bet.handicap.ToString());
                            break;
                        case GetBetErrorEnum.MARKET_TYPE_NOT_SUPPORTED:
                            break;
                        case GetBetErrorEnum.BET_ID_INVALID:
                            break;
                        case GetBetErrorEnum.NO_RESULTS:
                            break;
                        case GetBetErrorEnum.API_ERROR:
                            break;
                        case GetBetErrorEnum.INVALID_LOCALE_DEFAULTING_TO_ENGLISH:
                            bet.marketMenuPath = resp.bet.fullMarketName;
                            bet.Selection = resp.bet.selectionName + (resp.bet.handicap == 0 ? "" : resp.bet.handicap.ToString());
                            break;
                        default:
                            break;
                    }
                }
                catch (System.Net.WebException)
                {
                }
                
                try
                {
                    var pBet = Session.databaseContext.PlacedBets.Find(placeBetsResult.betId);
                    if (pBet != null)
                    {
                        pBet.marketMenuPath = bet.marketMenuPath;
                        pBet.Selection = bet.Selection;
                        Session.databaseContext.SaveChanges();
                    }
                    else
                    {
                        Session.databaseContext.PlacedBets.Add(bet);
                        Session.databaseContext.SaveChanges();
                    }
                    
                }
                catch (Exception)
                {
                    return placeBetsResult.betId;
                }
                return placeBetsResult.betId;
            }
            else
            {
                this.lastBetLost = true;
                return placeBetsResult.betId;                
            }                                    
        }
        
        

        public void LastBetResultIncrementing()
        {
            int sortedOrder = -1;
            this.lastPlacedBetIds.TrimExcess();
            var lastPlacedBetsIdCopy = lastPlacedBetIds.ToArray();
            foreach (var item in lastPlacedBetsIdCopy)
            {
                try
                {                    
                    DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                    var placedBetDB = context.PlacedBets.Find(item);
                    
                    if (Session.soccerLivescore=="")
                    {
                        MainWindow.UpdateLiveScores();                        
                    }                                        
                    UpdatePlacedBetResult(placedBetDB, Session.soccerLivescore);
                    if (placedBetDB.resultCode == "Settled")
                    {
                        this.lastPlacedBetIds.Remove(item);
                        this.UpdateDB();
                        continue;
                    }
                    if (placedBetDB.resultCode == "FT" | placedBetDB.resultCode.Contains("Extra") | placedBetDB.resultCode.Contains("HT"))
	                {
		                switch (placedBetDB.eventType)
                        {
                            case 1:
                                {
                                    switch (placedBetDB.marketName)
                                    {
                                        case "Match Odds":
                                            {
                                                try
                                                {
                                                    var score = placedBetDB.score.Split(':');
                                                    int homeGoals = int.Parse(score[0]);
                                                    int awayGoals = int.Parse(score[1]);
                                                    int difference = (homeGoals - awayGoals);
                                                    if (difference > 0)
                                                    {
                                                        sortedOrder = 1;
                                                    }
                                                    else
                                                    {
                                                        if (difference < 0)
                                                        {
                                                            sortedOrder = 2;
                                                        }
                                                        else
                                                        {
                                                            sortedOrder = 3;
                                                        }
                                                    }
                                                }
                                                catch (Exception)
                                                {
                                                    placedBetDB.sortedOrder = -1;
                                                }                                                
                                            }
                                            if (placedBetDB.resultCode == "FT" | placedBetDB.resultCode.Contains("Extra") | placedBetDB.resultCode.Contains("Settled"))
                                            {
                                                SettleBet(sortedOrder, item, placedBetDB);                                                
                                            }
                                            break;
                                        case "Half Time":
                                            {
                                                try
                                                {
                                                    var score = placedBetDB.score.Split(':');
                                                    int homeGoals = int.Parse(score[0]);
                                                    int awayGoals = int.Parse(score[1]);
                                                    int difference = (homeGoals - awayGoals);
                                                    if (difference > 0)
                                                    {
                                                        sortedOrder = 1;
                                                    }
                                                    else
                                                    {
                                                        if (difference < 0)
                                                        {
                                                            sortedOrder = 2;
                                                        }
                                                        else
                                                        {
                                                            sortedOrder = 3;
                                                        }
                                                    }
                                                }
                                                catch (Exception)
                                                {
                                                    placedBetDB.sortedOrder = -1;
                                                }
                                            }
                                            if (placedBetDB.resultCode.Contains("HT") | placedBetDB.resultCode.Contains("FT") | placedBetDB.resultCode.Contains("Settled"))
                                            {
                                                SettleBet(sortedOrder, item, placedBetDB);
                                            }
                                            break;
                                        case "Correct Score":
                                            {
                                                try
                                                {
                                                    switch (placedBetDB.score.Trim())
                                                    {
                                                        case "0:0":
                                                            sortedOrder = 1;
                                                            break;
                                                        case "0:1":
                                                            sortedOrder = 2;
                                                            break;
                                                        case "0:2":
                                                            sortedOrder = 3;
                                                            break;
                                                        case "0:3":
                                                            sortedOrder = 4;
                                                            break;
                                                        case "1:0":
                                                            sortedOrder = 5;
                                                            break;
                                                        case "1:1":
                                                            sortedOrder = 6;
                                                            break;
                                                        case "1:2":
                                                            sortedOrder = 7;
                                                            break;
                                                        case "1:3":
                                                            sortedOrder = 8;
                                                            break;
                                                        case "2:0":
                                                            sortedOrder = 9;
                                                            break;
                                                        case "2:1":
                                                            sortedOrder = 10;
                                                            break;
                                                        case "2:2":
                                                            sortedOrder = 11;
                                                            break;
                                                        case "2:3":
                                                            sortedOrder = 12;
                                                            break;
                                                        case "3:0":
                                                            sortedOrder = 13;
                                                            break;
                                                        case "3:1":
                                                            sortedOrder = 14;
                                                            break;
                                                        case "3:2":
                                                            sortedOrder = 15;
                                                            break;
                                                        case "3:3":
                                                            sortedOrder = 16;
                                                            break;
                                                        default:
                                                            sortedOrder = 17;
                                                            break;
                                                    }
                                                }
                                                catch (Exception)
                                                {
                                                    sortedOrder = -1;
                                                }
                                            }
                                            if (placedBetDB.resultCode == "FT" | placedBetDB.resultCode.Contains("Extra") | placedBetDB.resultCode.Contains("Settled"))
                                            {
                                                SettleBet(sortedOrder, item, placedBetDB);
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                break;
                                // This is for tennis
                            case 2: //Tennis
                                break;
                            default:
                                break;
                        }
	                }
                    context.SaveChanges();
                }
                catch (Exception)
                {
                    this.timer.Start();
                }
            }
            this.timer.Start();
        }

        private void SettleBet(int sortedOrder, long item, DBLBettingApp.PlacedBet placedBetDB)
        {
            double profitAndLoss;
            if (sortedOrder == placedBetDB.sortedOrder)
            {
                if (BetType == "B")
                {
                    profitAndLoss = Math.Round((placedBetDB.averagePrice - 1) * placedBetDB.sizeMatched, 2);
                }
                else
                {
                    profitAndLoss = Math.Round((placedBetDB.averagePrice - 1) * -placedBetDB.sizeMatched, 2);
                }
            }
            else
            {
                if (BetType == "B")
                {
                    profitAndLoss = -placedBetDB.sizeMatched;
                }
                else
                {
                    profitAndLoss = placedBetDB.sizeMatched;
                }
            }
            if (profitAndLoss < 0)
            {
                if (this.algorithmName.ToUpper().Contains("increment".ToUpper()))
                {
                    double size;
                    size = Math.Round((this.ProfitPerBet - (profitAndLoss * placedBetDB.averagePrice) - (ProfitPerBet == initialProfitPerBet ? ProfitPerBet : 0)), 2);
                    
                    if (size > initialProfitPerBet)
                    {
                        // reducing or increasing the size of the bet for the next bet
                        this.ProfitPerBet = size;
                    }
                    else
                    {
                        this.ProfitPerBet = this.InitialProfitPerBet;
                    }
                }
                this.LastBetLost = true;
            }
            else
            {
                this.LastBetLost = false;
            }
            this.lastPlacedBetIds.Remove(item);
            this.Ballance += (decimal)profitAndLoss;
            try
            {                
                if (placedBetDB != null)
                {
                    placedBetDB.dateSettled = DateTime.Now;
                    UpdatePlacedBetResult(placedBetDB, Session.soccerLivescore);
                    placedBetDB.resultCode = "Settled";
                }
                this.UpdateDB(item);
            }
            catch (Exception)
            {
            }
        }

        public static void UpdatePlacedBetResult(DBLBettingApp.PlacedBet bet, string scoresLast2Days)
        {
            // \Soccer\Czech Soccer\Czech U21 League\Fixtures 08 August   \AC Sparta Praha U21 v FK Teplice U21
            string[] mMPseparated = bet.marketMenuPath.Split('\\');
            string[] matchNamesSplit = mMPseparated.Last().Split('v');
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
                        var firstTeam = teams.Split(new string[] {"?v?"},StringSplitOptions.None).First().Trim();
                        var lastTeam = teams.Split(new string[] { "?v?" }, StringSplitOptions.None).Last().Trim();
                        DBLBettingApp.BFDBLEntities1 context1 = new DBLBettingApp.BFDBLEntities1();
                        var pb = context1.PlacedBets.Find(bet.betId);
                        pb.resultCode = progress.Trim();
                        pb.score = score.Replace('-',':');
                        try
                        {
                            context1.SaveChanges();
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }                        
        }
        
        private static int EvaluateSortedOrder(int sortedOrder, int team1Equals, int team2Equals)
        {
            if (team1Equals == 0)
            {
                sortedOrder = 1;
            }
            else
            {
                if (team2Equals == 0)
                {
                    sortedOrder = 2;
                }
                else
                {
                    sortedOrder = 3;
                }
            }
            return sortedOrder;
        }

        private void CancelBetAndResetBetId(long betId)
        {
            CancelBetsReq cancelReq = new CancelBetsReq();
            cancelReq.header = new APIRequestHeader();
            cancelReq.header.sessionToken = Session.globalHeader.sessionToken;
            List<CancelBets> listBets = new List<CancelBets>(1);
            CancelBets bet = new CancelBets();
            bet.betId = betId;
            listBets.Add(bet);
            cancelReq.bets = listBets.ToArray();
            try
            {
                CancelBetsResp cancelResp = Session.exchangeService.cancelBets(cancelReq);
                Session.checkHeader(cancelResp.header);
                if (cancelResp.errorCode == CancelBetsErrorEnum.OK & (cancelResp.betResults[0].success | cancelResp.betResults[0].resultCode == CancelBetsResultEnum.TAKEN_OR_LAPSED))
                {
                    this.lastPlacedBetIds.Remove(betId);
                    ResetLastplacedbetIdInDB(betId);
                }
            }
            catch (System.Net.WebException)
            {
            }
            
        }

        private void ResetLastplacedbetIdInDB(long betId)
        {
            try
            {
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                DBLBettingApp.Betline betline = context.Betlines.Find(this.betlineId);
                var lastBets = betline.lastPlacedBetId.Trim(':').Split(':').ToList();
                lastBets.Remove(betId.ToString());
                var array = lastBets.ToArray();
                betline.lastPlacedBetId = FilterMarkets.ArrayToString(array);
                betline.profitPerBet = this.ProfitPerBet;
                context.SaveChanges();
            }
            catch (Exception)
            {
            }
        }
        

        //test test test
        public DBLBettingApp.Betline ToDBBetLine()
        {
            DBLBettingApp.Betline result = new DBLBettingApp.Betline();
            result.BetlineId = this.BetlineId;
            result.currencyCode = this.CurrencyCode;
            result.Filter = this.Filter.ToDBFilter();
            result.lastPlacedBetId = FilterMarkets.ArrayToString(this.LastPlacedBetId.ToArray());
            result.initialProfitPerBet = this.InitialProfitPerBet;
            result.isActive = this.IsActive;
            result.lastBetLost = this.LastBetLost;
            result.profitPerBet = this.ProfitPerBet;
            result.stakeRangeMax = this.StakeRangeMax;
            result.stakeRangeMin = this.StakeRangeMin;
            result.algorithmName = this.AlgorithmName;
            result.betlineName = this.BetlineName;
            result.betType = this.BetType;
            result.ballance = this.Ballance;
            return result;
        }

        private static DBLBettingApp.PlacedBet ToDBPlacedBet(PlaceBetsResult placeBetsResult, Market market)
        {
            DBLBettingApp.PlacedBet bet = new DBLBettingApp.PlacedBet();
            bet.averagePrice = placeBetsResult.averagePriceMatched;
            bet.betId = placeBetsResult.betId;
            bet.resultCode = placeBetsResult.resultCode.ToString();
            bet.sizeMatched = placeBetsResult.sizeMatched;
            bet.success = placeBetsResult.success;
            bet.marketId = market.marketId;
            return bet;
        }
        public BettingLine()
        {
            this.Filter = new FilterMarkets();
            this.CurrentMarketList = new List<Market>();
            this.Bet = new PlaceBets();
            this.LastPlacedBetId = new List<long>();
            this.lastBetLost = true;
            this.CurrencyCode = "";
            this.InitialProfitPerBet = 0.0;
            this.IsActive = true;
            this.ProfitPerBet = 0.0;
            this.StakeRangeMax = double.MaxValue;
            this.StakeRangeMin = 0;
            this.timer = new DispatcherTimer();
            this.AlgorithmName = "";
            this.BetType = "";
            this.Ballance = 0;
        }
        public BettingLine(DBLBettingApp.Betline a)
        {
            if (a!=null)
            {
                this.BetlineId = a.BetlineId;
                this.CurrencyCode = a.currencyCode;
                this.Filter = new FilterMarkets(a.Filter);
                this.Filter.CountryCodeArray = a.Filter.countryCodes=="" ? null : a.Filter.countryCodes.Trim(':').Split(':');
                this.filter.MarketTypesArray = a.Filter.marketTypes == "" ? null : a.Filter.marketTypes.Trim(':').Split(':');
                this.filter.MarketNamesArray = a.Filter.marketNames == "" ? null : a.Filter.marketNames.Trim(':').Split(':');
                if (a.Filter.eventTypes != "")
                {
                    string[] strArray = a.Filter.eventTypes.Trim(':').Split(':');
                    int?[] eventTypesIntArray = new int?[strArray.Length];
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        eventTypesIntArray[i] = int.Parse(strArray[i]);
                    }
                    this.filter.EventTypesArray = eventTypesIntArray;                                    
                }
                else
                {
                    this.filter.EventTypesArray = null;
                }
                this.InitialProfitPerBet = a.initialProfitPerBet;
                this.IsActive = a.isActive;
                this.LastBetLost = a.lastBetLost;
                this.LastPlacedBetId = StringToListLong(a.lastPlacedBetId);
                this.ProfitPerBet = a.profitPerBet;
                this.StakeRangeMax = a.stakeRangeMax;
                this.StakeRangeMin = a.stakeRangeMin;
                this.AlgorithmName = a.algorithmName;
                this.BetType = a.betType;
                this.Ballance = a.ballance == null ? 0 : (decimal)a.ballance;
                this.timer = new DispatcherTimer();
            }
        }

        private static List<long> StringToListLong(string lastString)
        {
            var lastArray = lastString.Trim(':').Split(':').ToList();
            var LastPlacedBetId = new List<long>();
            foreach (var item in lastArray)
            {
                if (item != "")
                {
                    try
                    {
                        LastPlacedBetId.Add(long.Parse(item));
                    }
                    catch (System.FormatException)
                    {
                    }
                }                
            }
            return LastPlacedBetId;
        }
        //working
        public void AddToDB()
        {
            try
            {                
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                DBLBettingApp.Betline betline= this.ToDBBetLine();
                context.Filters.Add(betline.Filter);
                context.Betlines.Add(betline);
                context.SaveChanges();
                var betlineArray = context.Betlines.ToArray();
                this.betlineId = betlineArray.LastOrDefault().BetlineId;
            }
            catch (Exception)
            {
            }
        }
        //working
        public void UpdateDB()
        {
            DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
            DBLBettingApp.Betline betline = context.Betlines.Find(this.BetlineId);
            betline.isActive = this.IsActive;
            betline.lastBetLost = this.LastBetLost;
            betline.lastPlacedBetId = FilterMarkets.ArrayToString(this.LastPlacedBetId.ToArray());
            betline.profitPerBet = this.ProfitPerBet;
            betline.ballance = this.Ballance;            
            context.SaveChanges();
        }

        private void UpdateDB(long betid)
        {
            try
            {                
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                var betline = context.Betlines.Find(this.BetlineId);
                betline.isActive = this.IsActive;
                betline.lastBetLost = this.LastBetLost;
                betline.lastPlacedBetId = FilterMarkets.ArrayToString(this.LastPlacedBetId.ToArray());//??
                betline.profitPerBet = this.ProfitPerBet;
                betline.ballance = this.Ballance;
                try
                {                    
                    var pB = context.PlacedBets.Find(betid);
                    if (pB != null)
                    {
                        pB.success = !this.lastBetLost;
                    }
                }
                catch (Exception)
                {                    
                }
                context.SaveChanges();
            }
            catch (Exception)
            {
            }
        }

        internal bool TakeActiveStatusFmDB()
        {
            try
            {
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                DBLBettingApp.Betline betline = context.Betlines.Find(this.betlineId);
                if (betline != null)
                {
                    this.isActive = betline.isActive;
                    this.LastPlacedBetId = StringToListLong(betline.lastPlacedBetId);
                    this.InitialProfitPerBet = betline.initialProfitPerBet;
                    this.LastBetLost = betline.lastBetLost;
                    this.ProfitPerBet = betline.profitPerBet;
                    this.StakeRangeMax = betline.stakeRangeMax;
                    this.StakeRangeMin = betline.stakeRangeMin;
                    this.BetType = betline.betType;
                    return true;
                }
                else
                {
                    return false;
                }                
            }
            catch (Exception)
            {
                //log error
                this.timer.Start();
                return false;
            }
        }
        //this method is Async
        public async void Betline_timer_TickAsync(object sender, EventArgs e)
        {
            //Action a = new Action(() =>{});
            //Task task = new Task(a);
            this.timer.Stop();
            if (!this.TakeActiveStatusFmDB())
            {
                return;
            }
            else
            {
                await Task.Run(() =>
                {

                    if (this.IsActive | this.LastBetLost)
                    {
                        try
                        {
                            if (FilterMarkets.LongArrayIsEmpty(this.LastPlacedBetId.ToArray()))
                            {
                                //extracting new marketlist from server to betline.currentMarketList 
                                this.GetMarketsRequestFiltered();
                                // filter the markets by all filters
                                //sort markets by date ascending
                                var resultList = from market in this.CurrentMarketList
                                                 where IsMarketCompatible(market)
                                                 orderby market.marketTime ascending
                                                 select market;
                                foreach (Market m in resultList.ToList<Market>())
                                {
                                    try
                                    {

                                        var index = m.eventHierarchy.Count() - 2;
                                        this.EventId = (int)m.eventHierarchy[index]; // use this for checking if event is being played yet
                                        this.EventType = (int)m.eventHierarchy[0];
                                        this.Bet.marketId = m.marketId; // use this for rss.betfair.com results function
                                        this.PlaceBetMarketMenuPath = m.menuPath;
                                        this.MarketName = m.name;
                                        
                                    }
                                    catch (Exception)
                                    {
                                        this.timer.Start();
                                        return;
                                    }
                                    // Gets all runners for m and looks for sutable one and Places bet on that
                                    this.FindAndBetOnSutableRunnerIncrementing(m);
                                    if (!FilterMarkets.LongArrayIsEmpty(this.LastPlacedBetId.ToArray()))
                                    {
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    this.LastBetResultIncrementing();
                                }
                                catch (Exception)
                                {                                    
                                    //log error
                                    this.timer.Start();
                                }
                                //catch (System.IO.IOException session)
                                //{
                                //    this.timer.Start();
                                //}
                                //System.Net.WebException
                                //login
                            }
                        }
                        catch (Exception)
                        {
                            this.timer.Start();
                            //MainWindow.Button_Click_Login();
                            // write exception in the database
                            
                        }
                        //catch
                        // System.IO.IOException
                        //login
                    }
                });
                WpfBetApplication.MainWindow.BindBetlinesGrid();
                try
                {
                    WpfBetApplication.MainWindow.BindFunds();
                }
                catch (Exception)
                {
                }
            }
            this.timer.Start();
        }

        

        public async void timer_Tick_BetOnAllRecentlyFoundIncrementing_Async(object sender, EventArgs e)
        {
            this.timer.Stop();
            if (!this.TakeActiveStatusFmDB())
            {
                return;
            }
            else
            {
                await Task.Run(() =>
                {
                    if (this.IsActive)
                    {
                        try
                        {
                            if (this.filter.MaxBetsCount > 0 ? (this.LastPlacedBetId.Count < this.filter.MaxBetsCount) : true)
                            {
                                //extracting new marketlist from server to betline.currentMarketList 
                                this.GetMarketsRequestFiltered();
                                // filter the markets by all filters
                                //sort markets by date ascending
                                var resultList = from market in this.CurrentMarketList
                                                 where IsMarketCompatible(market)
                                                 orderby market.marketTime ascending
                                                 select market;
                                foreach (Market m in resultList.ToList<Market>())
                                {
                                    try
                                    {
                                        
                                    var index = m.eventHierarchy.Count()-2;
                                    this.EventId = (int)m.eventHierarchy[index];
                                    this.EventType = (int)m.eventHierarchy[0];
                                    this.Bet.marketId = m.marketId;
                                    this.PlaceBetMarketMenuPath = m.menuPath;
                                    this.MarketName = m.name;
                                    }
                                    catch (Exception)
                                    {
                                        this.timer.Start();
                                        return;
                                    }
                                    // Gets all runners for m and looks for sutable one and Places bet on that
                                    if (this.filter.MaxBetsCount > 0 ? (this.LastPlacedBetId.Count < this.filter.MaxBetsCount) : true)
                                    {
                                        this.FindAndBetOnSutableRunnerIncrementing(m);
                                    }                                
                                }
                            }
                            this.LastBetResultIncrementing();
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
                WpfBetApplication.MainWindow.BindBetlinesGrid();
                try
                {
                    WpfBetApplication.MainWindow.BindFunds();
                }
                catch (Exception)
                {
                }
            }
            this.timer.Start();
        }

        private bool IsMarketCompatible(Market market)
        {
            bool isBspCompatible = this.filter.IsBSP == null ? true : (market.bspMarket == (bool)this.filter.IsBSP);
            bool isExchangeCompatible = this.filter.ExchangeId == null ? true : (int)market.licenceId == (int)this.filter.ExchangeId;
            bool isMarketNameCompatible = !FilterMarkets.StringArrayIsEmpty(filter.MarketNamesArray) & filter.MarketNamesArray != null ?  this.filter.MarketNamesArray.Contains(market.name) : true;
            bool isWinnersMaxCompatible = market.numberOfWinners < this.filter.WinnersMax;
            bool isWinnersMinCompatible = market.numberOfWinners > this.filter.WinnersMin;
            bool isRunnersMaxNumberCompatible = market.runners == null ? true : (market.runners.Length < this.filter.RunnersMax);
            bool isRunnersMinNumberCompatible = market.runners == null ? true : (market.runners.Length > this.filter.RunnersMin);
            bool result = (isBspCompatible & isExchangeCompatible & isMarketNameCompatible & isWinnersMaxCompatible & isWinnersMinCompatible & isRunnersMaxNumberCompatible & isRunnersMinNumberCompatible);
            return result;
        }
    }

    public class FilterMarkets
    {
        private int?[]  eventTypesArray;
        public int?[] EventTypesArray
        {
            get { return eventTypesArray; }
            set { eventTypesArray = value; }
        }        
        private DateTime? fromEventDate; //May occur error when sending request because our default value is DateTime.MinValue and betfair default value is 1970
        public DateTime? FromEventDate
        {
            get { return fromEventDate; }
            set { fromEventDate = value; }
        }
        private DateTime? toEventDate;
        public DateTime? ToEventDate
        {
            get { return toEventDate; }
            set { toEventDate = value; }
        }
        private bool isInPlay;
        public bool IsInPlay
        {
            get { return isInPlay; }
            set { isInPlay = value; }
        }
        private bool? isBSP;
        public bool? IsBSP
        {
            get { return isBSP; }
            set { isBSP = value; }
        }
        private byte? exchangeId;
        public byte? ExchangeId
        {
            get { return exchangeId; }
            set { exchangeId = value; }
        }
        private string[] countryCodeArray;
        public string[] CountryCodeArray
        {
            get { return countryCodeArray; }
            set { countryCodeArray = value; }
        }
        
        private bool? isActive;
        public bool? IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }
        private string[] marketTypesArray;
        public string[] MarketTypesArray
        {
            get { return marketTypesArray; }
            set { marketTypesArray = value; }
        }
        private string[] marketNamesArray;
        public string[] MarketNamesArray
        {
            get { return marketNamesArray; }
            set { marketNamesArray = value; }
        }
        private int runnersMin;
        public int RunnersMin
        {
            get { return runnersMin; }
            set { runnersMin = value; }
        }
        private int runnersMax;
        public int RunnersMax
        {
            get { return runnersMax; }
            set { runnersMax = value; }
        }
        private int winnersMin;
        public int WinnersMin
        {
            get { return winnersMin; }
            set { winnersMin = value; }
        }
        private int winnersMax;
        public int WinnersMax
        {
            get { return winnersMax; }
            set { winnersMax = value; }
        }
        private double? totalAmountMatchedMin;
        public double? TotalAmountMatchedMin
        {
            get { return totalAmountMatchedMin; }
            set { totalAmountMatchedMin = value; }
        }
        private double? totalAmountMatchedMax;
        public double? TotalAmountMatchedMax
        {
            get { return totalAmountMatchedMax; }
            set { totalAmountMatchedMax = value; }
        }


        private string sortedOrdersMatchOdds;

        public List<byte> SortedOrdersMatchOddsList
        {
            get { return StringToByteList(sortedOrdersMatchOdds); }
            set { sortedOrdersMatchOdds = ListToDBString(value); }
        }

        private string sortedOrdersCorrectScore;

        public List<byte> SortedOrdersCorrectScoreList
        {
            get { return StringToByteList(sortedOrdersCorrectScore); }
            set { sortedOrdersCorrectScore = ListToDBString(value); }
        }

        private byte maxBetsCount;

        public byte MaxBetsCount
        {
            get { return maxBetsCount; }
            set { maxBetsCount = value; }
        }

        private int maxAmmountIncremented;

        public int MaxAmmountIncremented
        {
            get { return maxAmmountIncremented; }
            set { maxAmmountIncremented = value; }
        }

        private Nullable<short> goalDifferenceMin;

        public Nullable<short> GoalDifferenceMin
        {
            get { return goalDifferenceMin; }
            set { goalDifferenceMin = value; }
        }

        private Nullable<short> goalDifferenceMax;

        public Nullable<short> GoalDifferenceMax
        {
            get { return goalDifferenceMax; }
            set { goalDifferenceMax = value; }
        }

        private string ListToDBString(List<byte> value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in value)
            {
                try
                {
                    if (item > 0)
                    {
                        sb.Append(':');
                        sb.Append(item.ToString());
                    }
                }
                catch (Exception)
                {
                }
            }
            return sb.ToString();
        }

        private List<byte> StringToByteList(string sortedOrders)
        {
            List<byte> resultList = new List<byte>();
            if (!(sortedOrders == null))
            {
                var arr = sortedOrders.Trim(':').Split(':');
                foreach (var item in arr)
                {
                    try
                    {
                        if (!String.IsNullOrEmpty(item))
                        {
                            resultList.Add(byte.Parse(item));
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }            
            return resultList;
        }


        public FilterMarkets()
        {
            this.EventTypesArray = null;
            this.CountryCodeArray = null;
            this.ExchangeId = null;
            this.FromEventDate = DateTime.MinValue;
            this.ToEventDate = DateTime.MaxValue;
            this.IsActive = null;
            this.IsBSP = null;
            this.IsInPlay = false;
            this.MarketTypesArray = null;
            this.marketNamesArray = null;
            this.RunnersMax = int.MaxValue;
            this.RunnersMin = int.MinValue;
            this.TotalAmountMatchedMax = double.MaxValue;
            this.TotalAmountMatchedMin = double.MinValue;
            this.WinnersMax = int.MaxValue;
            this.WinnersMin = int.MinValue;
            this.sortedOrdersMatchOdds = "";
            this.sortedOrdersCorrectScore = "";
            this.maxAmmountIncremented = 0;
            this.maxBetsCount = 0;
            this.goalDifferenceMin = null;
            this.goalDifferenceMax = null;
        }
 
        public FilterMarkets(DBLBettingApp.Filter f)
        {
            var r = f.eventTypes.Trim(':').Split(':');
            int?[] rInt = new int?[r.Length];
            for (int i = 0; i < r.Length; i++)
            {
                rInt[i] = int.Parse(r[i]);
            }
            this.EventTypesArray = rInt;            
            this.CountryCodeArray = f.countryCodes.Trim(':').Split(':');
            this.ExchangeId = (byte?)f.exchangeId;
            this.FromEventDate = f.fromEventDate;
            this.ToEventDate = f.toEventDate;
            this.IsActive = f.isActive;
            this.IsBSP = f.isBSP;
            this.IsInPlay = f.isInPlay;
            this.MarketTypesArray = f.marketTypes.Trim(':').Split(':');
            this.MarketNamesArray = f.marketNames.Trim(':').Split(':');
            this.RunnersMax = f.runnersMax;
            this.RunnersMin = f.runnersMin;
            this.TotalAmountMatchedMax = f.totalAmountMatchedMax;
            this.TotalAmountMatchedMin = f.totalAmountMatchedMin;
            this.WinnersMax = f.winnersMax;
            this.WinnersMin = f.winnersMin;
            this.sortedOrdersCorrectScore = f.sortedOrdersCorrectScore;
            this.sortedOrdersMatchOdds = f.sortedOrdersMatchOdds;
            this.maxAmmountIncremented = (f.maxAmmountIncremented == null | f.maxAmmountIncremented < 0) ? 0 : (int)f.maxAmmountIncremented;
            this.maxBetsCount = (f.maxBetsCount == null | f.maxBetsCount < 0) ? (byte)0 : (byte)f.maxBetsCount;
            this.goalDifferenceMin = f.goalDifferenceMin;
            this.goalDifferenceMax = f.goalDifferenceMax;
        }

        public DBLBettingApp.Filter ToDBFilter()
        {
            DBLBettingApp.Filter result = new DBLBettingApp.Filter();
            result.countryCodes = ArrayToString(this.CountryCodeArray);
            result.eventTypes = ArrayToString(this.EventTypesArray);
            result.exchangeId = this.ExchangeId;
            result.fromEventDate = this.FromEventDate;
            result.toEventDate = this.ToEventDate;
            result.isActive = this.IsActive;
            result.isBSP = this.IsBSP;
            result.isInPlay = this.IsInPlay;
            result.marketTypes = ArrayToString(this.MarketTypesArray);
            result.marketNames = ArrayToString(this.MarketNamesArray);
            result.runnersMax = this.RunnersMax;
            result.runnersMin = this.RunnersMin;
            result.totalAmountMatchedMax = this.TotalAmountMatchedMax;
            result.totalAmountMatchedMin = this.TotalAmountMatchedMin;
            result.winnersMax = this.WinnersMax;
            result.winnersMin = this.WinnersMin;
            result.sortedOrdersCorrectScore = this.sortedOrdersCorrectScore;
            result.sortedOrdersMatchOdds = this.sortedOrdersMatchOdds;
            result.maxBetsCount = this.maxBetsCount;
            result.maxAmmountIncremented = this.maxAmmountIncremented;
            result.goalDifferenceMin = this.goalDifferenceMin;
            result.goalDifferenceMax = this.goalDifferenceMax;
            return result;
        }

        internal List<Market> FilterMarketsByEventDate(List<Market> marketsList, DateTime fromDate, DateTime toDate)
        {
            var resultList = from market in marketsList
                             where market.marketTime > fromDate & market.marketTime < toDate
                             select market;
            return resultList.ToList<Market>();
        }
        internal List<Market> FilterMarketsByIsBSP(List<Market> marketsList, bool IsBSP)
        {
            var resultList = from market in marketsList
                             where market.bspMarket == IsBSP
                             select market;
            return resultList.ToList<Market>();
        }
        internal List<Market> FilterMarketsByExcahgeId(List<Market> marketsList, byte exhangeId)
        {
            var resultList = from market in marketsList
                             where market.licenceId == exhangeId
                             select market;
            return resultList.ToList<Market>();
        }
        internal List<Market> FilterMarketsByCountryCodeArray(List<Market> marketsList, string[] countryCodeArray)
        {
            if (!FilterMarkets.StringArrayIsEmpty(countryCodeArray))
            {
                var resultList = from market in marketsList
                                 where countryCodeArray.Contains<string>(market.countryISO3)
                                 select market;
                return resultList.ToList<Market>();
            }
            else
            {
                return marketsList;
            }
            
        }

        
        internal List<Market> FilterMarketsByStatus(List<Market> marketsList, bool IsActive)
        {
            var resultList = from market in marketsList
                             where market.marketStatus.ToString() == (IsActive ? "ACTIVE" : "SUSPENDED")
                             select market;
            return resultList.ToList<Market>();
        }
        internal List<Market> FilterMarketsByType(List<Market> marketsList, string[] marketTypeArray)
        {
            if (!FilterMarkets.StringArrayIsEmpty(marketTypeArray))
            {
                var resultList = from market in marketsList
                                 where marketTypeArray.Contains<string>(market.marketType.ToString())
                                 select market;
                return resultList.ToList<Market>();
            }
            else
            {
                return marketsList;
            }
        }
        internal List<Market> FilterMarketsByName(List<Market> marketsList, string[] marketNamesArray)
        {
            if (!FilterMarkets.StringArrayIsEmpty(MarketNamesArray))
            {
                var resultList = from market in marketsList
                                 where marketNamesArray.Contains<string>(market.name.ToString())
                                 select market;
                return resultList.ToList<Market>();
            }
            else
            {
                return marketsList;
            }
        }
        internal List<Market> FilterMarketsByNumberOfRunners(List<Market> marketsList, int min, int max)
        {
            var resultList = from market in marketsList
                             where market.runners.Length > min & market.runners.Length < max
                             select market;
            return resultList.ToList<Market>();
        }
        internal List<Market> FilterMarketsByNumberOfWinners(List<Market> marketsList, int min, int max)
        {
            var resultList = from market in marketsList
                             where market.numberOfWinners > min & market.numberOfWinners < max
                             select market;
            return resultList.ToList<Market>();
        }

        public static string ArrayToString(string[] array)
        {
            if (array!=null)
            {
                StringBuilder result = new StringBuilder();
                for (int i = 0; i < array.Length; i++)
                {
                    if ((array[i] !="") & (array[i]!=null))
                    {
                        result.Append(array[i] + ":");                        
                    }
                }
                return result.ToString();
            }
            else
            {
                return "";
            }
            
        }

        public static string ArrayToString(long[] array)
        {
            if (array != null)
            {
                StringBuilder result = new StringBuilder();
                for (int i = 0; i < array.Length; i++)
                {
                    if (!(array[i] < 0))
                    {
                        result.Append(array[i].ToString() + ":");
                    }
                }
                return result.ToString();
            }
            else
            {
                return "";
            }

        }

        public static string ArrayToString(int?[] array)
        {
            if (array!=null)
            {
                StringBuilder result = new StringBuilder();
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i]!=null & !(array[i]<0))
                    {
                        result.Append(array[i].ToString() + ":");                        
                    }
                }
                return result.ToString();
            }
            else
            {
                return "";
            }
            
        }

        public static bool LongArrayIsEmpty(long[] a)
        {
            foreach (var item in a)
            {
                if (item > 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool StringArrayIsEmpty(string[] a)
        {
            foreach (var item in a)
            {
                if ((item != "") & (item != null))
                {
                    return false;
                }
            }
            return true;
        }
        //test test test
        
    }

    public class RunnerInfoType
    {
        int selectionId;

        public int SelectionId
        {
          get { return selectionId; }
          set { selectionId = value; }
        }
                int sortOrdered;
        public int SortOrdered
        {
          get { return sortOrdered; }
          set { sortOrdered = value; }
        }
                double totalAmmountMatched;
        public double TotalAmmountMatched
        {
          get { return totalAmmountMatched; }
          set { totalAmmountMatched = value; }
        }
                double lastPriceMatched;
        public double LastPriceMatched
        {
          get { return lastPriceMatched; }
          set { lastPriceMatched = value; }
        }
                double handicap;
        public double Handicap
        {
          get { return handicap; }
          set { handicap = value; }
        }
                double reductionFactor;
        public double ReductionFactor
        {
          get { return reductionFactor; }
          set { reductionFactor = value; }
        }
                bool vacant;
        public bool Vacant
        {
          get { return vacant; }
          set { vacant = value; }
        }
                int asianLineId;
        public int AsianLineId
        {
          get { return asianLineId; }
          set { asianLineId = value; }
        }
                double farBSP;
        public double FarBSP
        {
          get { return farBSP; }
          set { farBSP = value; }
        }
                double nearBSP;
        public double NearBSP
        {
          get { return nearBSP; }
          set { nearBSP = value; }
        }
                double actualBSP;
        public double ActualBSP
        {
          get { return actualBSP; }
          set { actualBSP = value; }
        }
                List<PricesType> prices;
        public List<PricesType> Prices
        {
          get { return prices; }
          set { prices = value; }
        }
    }
    public class PricesType
    {
        double price;
        public double Price
        {
          get { return price; }
          set { price = value; }
        }
                double backAmount;
        public double BackAmount
        {
          get { return backAmount; }
          set { backAmount = value; }
        }
                double layAmount;
        public double LayAmount
        {
          get { return layAmount; }
          set { layAmount = value; }
        }
                double totalBSPBackAmount;
        public double TotalBSPBackAmount
        {
          get { return totalBSPBackAmount; }
          set { totalBSPBackAmount = value; }
        }
                double totalBSPLayAmount;
        public double TotalBSPLayAmount
        {
          get { return totalBSPLayAmount; }
          set { totalBSPLayAmount = value; }
        }
    }
    public class CountryCode 
    {
        string countryCode;

        public string CountryCode1
        {
            get { return countryCode; }
            set { countryCode = value; }
        }
        string countryName;

        public string CountryName
        {
            get { return countryName; }
            set { countryName = value; }
        }
        public CountryCode(string code, string country)
        {
            this.countryCode = code;
            this.countryName = country;
        }
    }
    
}
