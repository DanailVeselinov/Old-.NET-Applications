using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BFDL;
using System.Windows.Threading;

namespace BetFairBot
{
    public partial class BettingLine : Betlines
    {
        public DispatcherTimer timer { get; set; }
        public TO.PlaceInstruction BetInstruction { get; set; }
        public TO.MarketFilter MarketFilter { get; set; }
        public TO.MarketCatalogue CurrentMarket { get; set; }
        public List<TO.MarketCatalogue> CurrentMarketList { get; set; }
        public List<TO.MarketBook> CurrentMarketBookList { get; set; }
        public BettingLine() { }
        public BettingLine(Betlines b)
        {
            this.algorithmName = b.algorithmName;
            this.ballance = b.ballance;
            this.BetlineId = b.BetlineId;
            this.betlineName = b.betlineName;
            this.betType = b.betType;
            this.currencyCode = b.currencyCode;
            this.Filter_FilterId = b.Filter_FilterId;
            this.Filters = b.Filters;
            this.initialProfitPerBet = b.initialProfitPerBet;
            this.isActive = b.isActive;
            this.lastBetLost = b.lastBetLost;
            this.lastPlacedBetId = b.lastPlacedBetId;
            this.ownersUserName = b.ownersUserName;
            this.profitPerBet = b.profitPerBet;
            this.profitPerBudjet = b.profitPerBudjet;
            this.stakeRangeMax = b.stakeRangeMax;
            this.stakeRangeMin = b.stakeRangeMin;
            this.Users = b.Users;
            FillMarketFilter();
        }

        public Betlines ToDBBetline()
        {
            Betlines result = new Betlines();
            result.algorithmName = this.algorithmName;
            result.ballance = this.ballance;
            result.BetlineId = this.BetlineId;
            result.betlineName = this.betlineName;
            result.betType = this.betType;
            result.currencyCode = this.currencyCode;
            result.Filter_FilterId = this.Filter_FilterId;
            result.initialProfitPerBet = this.initialProfitPerBet;
            result.isActive = this.isActive;
            result.lastBetLost = this.lastBetLost;
            result.lastPlacedBetId = this.lastPlacedBetId;
            result.ownersUserName = this.ownersUserName;
            result.profitPerBet = this.profitPerBet;
            result.profitPerBudjet = this.profitPerBudjet;
            result.stakeRangeMax = this.stakeRangeMax;
            result.stakeRangeMin = this.stakeRangeMin;
            result.Filters = this.Filters;
            return result;
        }
        public void FillMarketFilter()
        {
            this.MarketFilter = new TO.MarketFilter();
            this.MarketFilter.BspOnly = this.Filters.bspOnly;
            this.MarketFilter.CompetitionIds = StringToHashSet(this.Filters.compeitionIds, MainWindow.MainSeparator);
            this.MarketFilter.EventTypeIds = StringToHashSet(this.Filters.eventTypeIds, MainWindow.MainSeparator);
            this.MarketFilter.ExchangeIds = StringToHashSet(this.Filters.exchangeIds, MainWindow.MainSeparator);
            this.MarketFilter.InPlayOnly = this.Filters.inPlayOnly;
            this.MarketFilter.TurnInPlayEnabled = this.Filters.inPlayOnly;
            this.MarketFilter.MarketBettingTypes = StringToMBTypes(this.Filters.marketBettingTypes, MainWindow.MainSeparator);
            this.MarketFilter.MarketTypeCodes = StringToHashSet(this.Filters.marketNames.ToUpper().Replace(" ", "_"), MainWindow.MainSeparator);
            this.MarketFilter.MarketCountries = StringToHashSet(this.Filters.marketCountries, MainWindow.MainSeparator);
            if (this.Filters.marketTimeFrom != null) { this.MarketFilter.MarketStartTime.From = (DateTime)this.Filters.marketTimeFrom; }
            if (this.Filters.marketTimeTo != null) { this.MarketFilter.MarketStartTime.To = (DateTime)this.Filters.marketTimeTo; }
            // this.MarketFilter.MarketTypeCodes ????
            if (string.IsNullOrEmpty(this.Filters.textQuery)) { this.MarketFilter.TextQuery = this.Filters.textQuery; }
        }

        public ISet<TO.MarketBettingType> StringToMBTypes(string value, string separator)
        {
            HashSet<TO.MarketBettingType> result = new HashSet<TO.MarketBettingType>();
            if (string.IsNullOrEmpty(value))
            {
                return result;
            }
            var items = value.Trim(separator[0]).Split(separator[0]).ToList();
            foreach (string item in items)
            {
                switch (item)
                {
                    case "LINE":
                        result.Add(TO.MarketBettingType.LINE);
                        break;
                    case "ODDS":
                        result.Add(TO.MarketBettingType.ODDS);
                        break;
                    case "FIXED_ODDS":
                        result.Add(TO.MarketBettingType.FIXED_ODDS);
                        break;
                    case "RANGE":
                        result.Add(TO.MarketBettingType.RANGE);
                        break;
                    case "ASIAN_HANDICAP_SINGLE_LINE":
                        result.Add(TO.MarketBettingType.ASIAN_HANDICAP_SINGLE_LINE);
                        break;
                    case "ASIAN_HANDICAP_DOUBLE_LINE":
                        result.Add(TO.MarketBettingType.ASIAN_HANDICAP_DOUBLE_LINE);
                        break;
                    default:
                        break;
                }                
            }
            return result;
        }

        public HashSet<string> StringToHashSet(string value, string separator)
        {
            HashSet<string> result = new HashSet<string>();
            if (string.IsNullOrEmpty(value))
            {
                return result;
            }
            var items = value.Trim(separator[0]).Split(separator[0]).ToList();
            foreach (string item in items)
            {
                result.Add(item);
            }
            return result;
        }

        private bool TakeActiveStatusFmDB()
        {
            try
            {
                var context = new BFBDBEntities();
                var user = context.Users.FirstOrDefault(u => u.userName == Users.userName);
                Betlines betline = context.Betlines.Find(this.BetlineId);
                if (betline != null)
                {
                    this.isActive = betline.isActive;
                    this.lastPlacedBetId = betline.lastPlacedBetId;
                    this.initialProfitPerBet = betline.initialProfitPerBet;
                    this.lastBetLost = betline.lastBetLost;
                    this.profitPerBet = betline.profitPerBet;
                    this.stakeRangeMax = betline.stakeRangeMax;
                    this.stakeRangeMin = betline.stakeRangeMin;
                    this.betType = betline.betType;
                }
                return isActive;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private List<TO.MarketCatalogue> GetMarketsRequestFiltered()
        {
            try
            {
                var language = "bg";
                var marketProjectionHashSet = new HashSet<TO.MarketProjection> {TO.MarketProjection.EVENT, TO.MarketProjection.EVENT_TYPE, TO.MarketProjection.MARKET_DESCRIPTION,TO.MarketProjection.RUNNER_DESCRIPTION};
                return MainWindow.Client.listMarketCatalogue(this.MarketFilter, marketProjectionHashSet, TO.MarketSort.FIRST_TO_START, "200", language).ToList();                
            }
            catch (Exception)
            {
                //login
                return new List<TO.MarketCatalogue>();
                //WpfBetApplication.MainWindow.Button_Click_Login();
            }
        }
    }
}
