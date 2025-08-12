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
using System.Data.Entity;
using DBLBettingApp;
using System.Windows.Data;
using System.Windows;
using WpfBetApplication.BFExchangeService;

namespace WpfBetApplication
{   

    public class IsActiveTrueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var parInt = int.Parse(parameter.ToString());
            if (parInt == 0)
            {
                return value;
            }
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }

    public class PercentPerBudjetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!string.IsNullOrWhiteSpace(value.ToString()))
            {
                double budjet = 0;
                double newValue = 0;
                try
                {
                    
                    newValue = double.Parse(value.ToString());
                    double.TryParse((App.Current.MainWindow.FindName("_Funds") as Label).Content.ToString(), out budjet);
                }
                catch (Exception)
                {
                }
                newValue = Math.Round((newValue * MainWindow.ReduceBallance(budjet)) / 100,2);
                return newValue > 0 ? newValue.ToString() : "0";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!string.IsNullOrWhiteSpace(value.ToString()))
            {
                double budjet = 0;
                double newValue = 0;
                try
                {
                    
                    newValue = double.Parse(value.ToString());
                    double.TryParse((App.Current.MainWindow.FindName("_Funds") as Label).Content.ToString(), out budjet);
                }
                catch (Exception)
                {
                }
                newValue = Math.Round((newValue / MainWindow.ReduceBallance(budjet)) * 100, 2);
                return newValue > 0 ? newValue.ToString() : "0";
            }
            return value;
        }
    }
    public class RGBConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                int betlineId = (int)value;
                var colour = RGBConverterForeground.GetMeColour(betlineId);
                var brush = new System.Windows.Media.SolidColorBrush(colour);
                return brush;
            }
            catch (Exception)
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RGBConverterForeground : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                int betlineId = (int)value;
                var colour = GetMeColour(betlineId);
                if ((colour.G + colour.B + colour.R)>300)
                {
                    colour = System.Windows.Media.Colors.Black;
                }
                else
                {
                    colour = System.Windows.Media.Colors.White;
                }
                var brush = new System.Windows.Media.SolidColorBrush(colour);
                return brush;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static System.Windows.Media.Color GetMeColour(int betlineId)
        {
            var colour = new System.Windows.Media.Color();
            int betlineIdInt = betlineId % 64;
            int step = 8;
            int colourNumber = step * ((betlineIdInt % step) - 1) + (betlineIdInt / step);

            colour.R = (byte)((colourNumber / 16) * 85);
            colour.G = (byte)((colourNumber / 4) * 85);
            colour.B = (byte)((colourNumber % 4) * 85);
            colour.A = 255;
            return colour;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int? i;
            try
            {
                i = (int?)value;
                if (i != null & i >= 0)
                {
                    //var e = Session.databaseContext.Emails.Find(i);
                    //if (e!=null)
                    //{
                    //    e.email1 = "didoeddy";
                    //    Session.databaseContext.SaveChanges();
                    //}                    
                    return true;
                }
                //var ed = Session.databaseContext.Emails.Find(i);
                //if (ed !=null)
                //{
                //    ed.email1 = "";
                //    Session.databaseContext.Emails.Add(ed);
                //    Session.databaseContext.SaveChanges();
                //}                
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!((bool?)value == true))
            {
                return null;
            }
            return null; //Session.databaseContext.Emails.FirstOrDefault().id;
        }
    }


    public class BetlineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isChecked = (bool)(App.Current.MainWindow.FindName("_showAllCB") as CheckBox).IsChecked;
            if (isChecked)
            {
                var context = new BFBDBEntities();
                var view = context.PlacedBets.ToList().OrderByDescending(b=>b.datePlaced);
                return view;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MarketMenuPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value as String).Split('\\').ToList().Last();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SelectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var item = (value as DBLBettingApp.PlacedBet);
            if (item != null)
            {
                switch (item.marketName)
                {
                    case "Correct Score":
                        switch (item.Selection)
                        {
                            case "1":
                                return "0:0";
                            case "2":
                                return "0:1";
                            case "3":
                                return "0:2";
                            case "4":
                                return "0:3";
                            case "5":
                                return "1:0";
                            case "6":
                                return "1:1";
                            case "7":
                                return "1:2";
                            case "8":
                                return "1:3";
                            case "9":
                                return "2:0";
                            case "10":
                                return "2:1";
                            case "11":
                                return "2:2";
                            case "12":
                                return "2:3";
                            case "13":
                                return "3:0";
                            case "14":
                                return "3:1";
                            case "15":
                                return "3:2";
                            case "16":
                                return "3:3";
                            case "17":
                                return "Any Unquoted";
                            default:
                                return "Any unquoted";
                        }
                    case "Match Odds":
                        switch (item.Selection)
                        {
                            case "1":
                                return item.marketMenuPath.Split('\\').ToList().Last().Split(new[] { " v " }, StringSplitOptions.None).First();
                            case "2":
                                return item.marketMenuPath.Split('\\').ToList().Last().Split(new []{" v "}, StringSplitOptions.None).Last();
                            case "3":
                                return "X";
                            default:
                                return item.Selection;
                        }
                    default:
                        return "Any Unquoted";
                }
            }
            return "Any Unquoted";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

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
        public double ProfitPerBudjet { get; set; }

        List<DatabasePlacedBets.PlacedBet> databasePlacedBets;

        public List<DatabasePlacedBets.PlacedBet> DatabasePlacedBets
        {
            get { return databasePlacedBets; }
            set { databasePlacedBets = value; }
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
        
        public void FindAndBetOnSutableRunner()
        {
            foreach (var market in this.filter.MarketNamesArray)
            {
                List<byte> sortedOrderList = null;
                switch (market)
                {
                    case "Match Odds":
                        sortedOrderList = this.Filter.SortedOrdersMatchOddsList;
                        break;
                    case "Correct Score":
                        sortedOrderList = this.Filter.SortedOrdersCorrectScoreList;
                        break;
                    default:
                        return;
                }
                DBLBettingApp.BFBDBEntities context = new DBLBettingApp.BFBDBEntities();
                double maxStake = this.stakeRangeMax;
                double minStake = this.stakeRangeMin;
                var eTypes = this.filter.EventTypesArray;
                var mName = this.filter.MarketNamesArray;
                var databeseContext = new DatabasePlacedBets.BFDBLEntities();
                var runnersList = from r in this.databasePlacedBets
                                  where mName.Contains(r.marketName)
                                  select r;
                if (runnersList.Count() <= 0)
                {
                    return;
                }
                int i = Session.RND.Next(runnersList.Count());
                DatabasePlacedBets.PlacedBet runner = runnersList.ToList()[i];
                if (MarketIsPlayable(runner))
                {
                    if (this.PlaceBet(runner))
                    {
                        // reset the Bet Property
                        this.Bet = new PlaceBets();
                        this.ProfitPerBet = this.InitialProfitPerBet;
                        this.LastBetLost = true;
                        this.UpdateDB();
                    }
                }
            }                          
        }

        private bool FilterDatabse(DatabasePlacedBets.PlacedBet r)
        {
            var a = ((r.averagePrice > 1.2) & (r.averagePrice < 1.8));
            //var b = (int)r.eventType == 1;
            //var c = r.marketName == "Match Odds";
            return a;
        }

        private bool MarketIsPlayable(DatabasePlacedBets.PlacedBet bet)
        {
            try
            {
                DBLBettingApp.BFBDBEntities context = new DBLBettingApp.BFBDBEntities();
                DateTime last2DaysDate = Session.DateTimeNow.AddDays(-2);
                var placedMarkets = from placedBet in context.PlacedBets
                                    where (placedBet.eventId == bet.betId & placedBet.betlineId == this.BetlineId & placedBet.datePlaced < Session.DateTimeNow & placedBet.datePlaced > last2DaysDate)
                                    select placedBet.eventId;
                if (placedMarkets.Count() > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
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
                    rInfo.TotalAmountMatched = ParseDoubleFmNaNINF(fields[2]);
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
        public bool PlaceBet(DatabasePlacedBets.PlacedBet runner)
        {            
            try
            {
                if (runner.sortedOrder == 0)
                {
                    return false;
                }
                DBLBettingApp.BFBDBEntities context = new DBLBettingApp.BFBDBEntities();
                DBLBettingApp.PlacedBet newBet = new DBLBettingApp.PlacedBet();
                newBet.averagePrice = runner.averagePrice;
                newBet.betlineId = betlineId;
                newBet.datePlaced = Session.DateTimeNow;
                newBet.eventId = runner.betId;
                newBet.marketMenuPath = runner.marketMenuPath;
                newBet.resultCode = "Matched";
                newBet.Selection = runner.sortedOrder.ToString();
                double size = this.BetType == "B" ? Math.Round((this.ProfitPerBet / (runner.averagePrice - 1)), 2) : Math.Round((this.ProfitPerBet), 2);
                if (this.BetlineName.ToUpper().Contains("increment".ToUpper()))
                {
                    double smallSize = this.BetType == "B" ? Math.Round((this.initialProfitPerBet / (runner.averagePrice - 1)), 2) : Math.Round((this.initialProfitPerBet), 2);
                    smallSize = smallSize < 2 ? 2 : smallSize;
                    double limit = this.filter.MaxAmountIncremented > 0 ? (this.filter.MaxAmountIncremented < 2 ? 2 : this.filter.MaxAmountIncremented) : (this.filter.MaxAmountIncremented < 0 ? 2 : double.MaxValue);
                    size = ((size >= 2) & (size <= limit)) ? size : smallSize;
                }
                else
                {
                    size = size < 2 ? 2 : size;
                }
                newBet.sizeMatched = size;
                newBet.sortedOrder = runner.sortedOrder;
                newBet.success = this.betType.Contains(runner.betType) ? runner.success : !runner.success;
                newBet.marketName = runner.marketName;
                newBet.eventType = runner.eventType;
                TimeSpan duration = (DateTime)runner.dateSettled - runner.datePlaced;
                newBet.dateSettled = Session.DateTimeNow + duration;
                newBet.score = runner.score;
                newBet.eventType = runner.eventType;
                Ballance = Math.Round((Ballance - (betType.Contains("B") ? (decimal)newBet.sizeMatched : (decimal)((runner.averagePrice - 1) * newBet.sizeMatched))), 2);
                context.PlacedBets.Add(newBet);
                context.SaveChanges();
                this.LastPlacedBetId.Add(newBet.betId);
                this.LastBetLost = true;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        
        public void LastBetResult()
        {
            this.lastPlacedBetIds.TrimExcess();
            var lastPlacedBetsIdCopy = lastPlacedBetIds.ToArray();
            DBLBettingApp.BFBDBEntities context = new DBLBettingApp.BFBDBEntities();
            foreach (var item in lastPlacedBetsIdCopy)
            {
                try
                {                    
                    var placedBetDB = context.PlacedBets.Find(item);                    
                    if (placedBetDB.dateSettled < Session.DateTimeNow.AddMinutes(-5))
	                {
                        double profitAndLoss;	
                        if (placedBetDB.success)
                        {
                            profitAndLoss = Math.Round(placedBetDB.averagePrice* placedBetDB.sizeMatched, 2);
                            this.Ballance = Math.Round((Ballance + (decimal)(profitAndLoss)), 2);
                        }
                        else
                        {
                            profitAndLoss = Math.Round(-placedBetDB.sizeMatched * (placedBetDB.averagePrice), 2);
                        }
                        if (profitAndLoss < 0)
                        {
                            if (this.BetlineName.ToUpper().Contains("increment".ToUpper()))
                            {

                                double size;
                                size = Math.Round((this.ProfitPerBet - (profitAndLoss) - (ProfitPerBet == initialProfitPerBet ? ProfitPerBet : 0)), 2);                                
                                if (size > initialProfitPerBet)
                                {
                                    // reducing or increasing the size of the bet for the next bet
                                    this.ProfitPerBet = size;
                                }
                            }
                            else
                            {
                                this.ProfitPerBet = initialProfitPerBet;
                            }
                            this.LastBetLost = true;
                        }
                        else
                        {
                            this.LastBetLost = false;
                        }                        
                        try
                        {
                            this.lastPlacedBetIds.Remove(item);
                            this.UpdateDB(item);                            
                        }
                        catch (Exception)
                        {
                            continue;
                        }





                        //if (profitAndLoss<0)
                        //{
                        //    double size;
                        //    if (BetType == "B")
                        //    {
                        //        size = Math.Round((this.ProfitPerBet - (profitAndLoss * placedBetDB.averagePrice) - (ProfitPerBet == initialProfitPerBet ? ProfitPerBet : 0)), 2);
                        //    }
                        //    else
                        //    {
                        //        size = Math.Round((this.ProfitPerBet - (profitAndLoss * placedBetDB.averagePrice/(placedBetDB.averagePrice-1)) - (ProfitPerBet == initialProfitPerBet ? ProfitPerBet : 0)), 2);
                        //    }
                        //    if (size > initialProfitPerBet)
                        //    {
                        //        // reducing or increasing the size of the bet for the next bet                                
                        //        this.LastBetLost = profitAndLoss < 0 ? true : false;
                        //        this.ProfitPerBet = profitAndLoss < 0 ? size : ProfitPerBet;
                        //        // saves the betline to database
                        //        try
                        //        {
                        //            this.lastPlacedBetIds.Remove(item);
                        //            this.UpdateDB();
                        //        }
                        //        catch (Exception)
                        //        {
                        //        }
                        //    }
                        //    else
                        //    {
                        //        this.ProfitPerBet = profitAndLoss < 0 ? this.initialProfitPerBet : ProfitPerBet;
                        //        this.LastBetLost = profitAndLoss < 0 ? true : false;
                        //        try
                        //        {
                        //            this.lastPlacedBetIds.Remove(item);
                        //            this.UpdateDB();
                        //        }
                        //        catch (Exception)
                        //        {
                        //        }
                        //    }
                        //}
                        //else
                        //{                            
                        //    this.LastBetLost = profitAndLoss < 0 ? true : false;
                        //    // saves the betline to database
                        //    try
                        //    {
                        //        this.lastPlacedBetIds.Remove(item);
                        //        this.UpdateDB();
                        //    }
                        //    catch (Exception)
                        //    {
                        //    }
                        //}
	                }    
                }
                catch (Exception)
                {
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

        private void ResetLastplacedbetIdInDB(long betId)
        {
            try
            {
                DBLBettingApp.BFBDBEntities context = new DBLBettingApp.BFBDBEntities();
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
            result.profitPerBudjet = this.ProfitPerBudjet;
            result.budjet = this.Ballance;
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
            this.ProfitPerBudjet = 0;
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
                this.BetlineName = a.betlineName;
                this.ProfitPerBudjet = a.profitPerBudjet;
                this.Ballance = (decimal)a.budjet;
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
                DBLBettingApp.BFBDBEntities context = new DBLBettingApp.BFBDBEntities();
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
            try
            {
                DBLBettingApp.BFBDBEntities context = new DBLBettingApp.BFBDBEntities();
                DBLBettingApp.Betline betline = context.Betlines.Find(this.BetlineId);
                betline.isActive = this.IsActive;
                betline.lastBetLost = this.LastBetLost;
                betline.lastPlacedBetId = FilterMarkets.ArrayToString(this.LastPlacedBetId.ToArray());
                betline.profitPerBet = this.ProfitPerBet;
                betline.budjet = this.Ballance;
                context.SaveChanges();
            }
            catch (Exception)
            {                
            }            
        }

        private void UpdateDB(long betid)
        {
            try
            {                
                DBLBettingApp.BFBDBEntities context = new DBLBettingApp.BFBDBEntities();
                var betline = context.Betlines.Find(this.BetlineId);
                betline.isActive = this.IsActive;
                betline.lastBetLost = this.LastBetLost;
                betline.lastPlacedBetId = FilterMarkets.ArrayToString(this.LastPlacedBetId.ToArray());//??
                betline.profitPerBet = this.ProfitPerBet;
                betline.budjet = this.Ballance;
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
                DBLBettingApp.BFBDBEntities context = new DBLBettingApp.BFBDBEntities();
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
                    this.Filter.MaxAmountIncremented = (double)betline.Filter.maxAmmountIncremented;
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
                                //this.GetMarketsRequestFiltered();
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
                                    //this.FindAndBetOnSutableRunner(m);
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
                                    this.LastBetResult();
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
                    //WpfBetApplication.MainWindow.BindFunds();
                }
                catch (Exception)
                {
                }
            }
            this.timer.Start();
        }

        public async void timer_Tick_BetOnAllRecentlyFound_Async(object sender, EventArgs e)
        {
            this.timer.Stop();
            if (!this.TakeActiveStatusFmDB())
            {
                this.timer = null;
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
                            //extracting new marketlist from server to betline.currentMarketList 
                            //this.GetMarketsRequestFiltered();
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
                                //this.FindAndBetOnSutableRunner(m);
                                //this.LastBetResult();
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
                WpfBetApplication.MainWindow.BindBetlinesGrid();
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

        internal bool ContainsSortedOrder(int? nullable, string marketName)
        {
            if (marketName.Contains("Match Odds"))
            {
                return this.Filter.SortedOrdersMatchOddsList.Contains((byte)nullable);
            }
            else
            {
                if (marketName.Contains("Correct Score"))
                {
                    return this.Filter.SortedOrdersCorrectScoreList.Contains((byte)nullable);
                }
                else
                {
                    return true;
                }
            }
        }

        internal void timer_Tick_BetOnAllRecentlyFoundIncrementing_Async(object sender, EventArgs e)
        {
            if (!this.TakeActiveStatusFmDB())
            {
                return;
            }
            else
            {
                if (this.IsActive)
                {
                    try
                    {
                        if (this.filter.MaxBetsCount > 0 ? (this.LastPlacedBetId.Count < this.filter.MaxBetsCount) : true)
                        {
                            this.FindAndBetOnSutableRunner();
                        }
                        this.LastBetResult();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
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

        private double maxAmountIncremented;

        public double MaxAmountIncremented
        {
            get { return maxAmountIncremented; }
            set { maxAmountIncremented = value; }
        }

        public double MaxAmountIncrementedPerBudjet { get; set; }

        private string ListToDBString(List<byte> value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in value)
            {
                try
                {
                    if (item > 0)
                    {                        
                        sb.Append(item.ToString());
                        sb.Append(':');
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
            if (string.IsNullOrEmpty(sortedOrders))
            {
                return resultList;
            }
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
            this.maxAmountIncremented = 0;
            this.maxBetsCount = 0;
            this.MaxAmountIncrementedPerBudjet = 0;
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
            this.maxAmountIncremented = (f.maxAmmountIncremented == null | f.maxAmmountIncremented < 0) ? 2 : (int)f.maxAmmountIncremented;
            this.maxBetsCount = (f.maxBetsCount == null | f.maxBetsCount < 0) ? (byte)0 : (byte)f.maxBetsCount;
            this.MaxAmountIncrementedPerBudjet = f.maxAmmountIncrementedPerBudjet;
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
            result.maxBetsCount = (short)this.maxBetsCount;
            result.maxAmmountIncremented = this.maxAmountIncremented;
            result.maxAmmountIncrementedPerBudjet = this.MaxAmountIncrementedPerBudjet;
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

        public static string ArrayToString(byte[] array)
        {
            if (array != null)
            {
                StringBuilder result = new StringBuilder();
                for (byte i = 0; i < array.Length; i++)
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
                double totalAmountMatched;
        public double TotalAmountMatched
        {
          get { return totalAmountMatched; }
          set { totalAmountMatched = value; }
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
