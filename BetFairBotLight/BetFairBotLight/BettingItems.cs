using BetFairBot;
using BetFairBot.TO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace BetFairBot
{
    public partial class BettingLine : Betlines
    {
        public DispatcherTimer timer { get; set; }
        public PlaceInstruction BetInstruction { get; set; }
        public MarketFilter MarketFilter { get; set; }
        public MarketCatalogue CurrentMarket { get; set; }
        public List<MarketCatalogue> CurrentMarketList { get; set; }
        public List<MarketBook> CurrentMarketBookList { get; set; }
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
            this.MarketFilter = new MarketFilter();
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

        public ISet<MarketBettingType> StringToMBTypes(string value, string separator)
        {
            HashSet<MarketBettingType> result = new HashSet<MarketBettingType>();
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
                        result.Add(MarketBettingType.LINE);
                        break;
                    case "ODDS":
                        result.Add(MarketBettingType.ODDS);
                        break;
                    case "FIXED_ODDS":
                        result.Add(MarketBettingType.FIXED_ODDS);
                        break;
                    case "RANGE":
                        result.Add(MarketBettingType.RANGE);
                        break;
                    case "ASIAN_HANDICAP_SINGLE_LINE":
                        result.Add(MarketBettingType.ASIAN_HANDICAP_SINGLE_LINE);
                        break;
                    case "ASIAN_HANDICAP_DOUBLE_LINE":
                        result.Add(MarketBettingType.ASIAN_HANDICAP_DOUBLE_LINE);
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
                var context = new Entities();
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
            catch (System.Exception)
            {
                return false;
            }
        }
        private List<MarketCatalogue> GetMarketsRequestFiltered()
        {
            try
            {
                var language = "bg";
                var marketProjectionHashSet = new HashSet<MarketProjection> {MarketProjection.EVENT, MarketProjection.EVENT_TYPE, MarketProjection.MARKET_DESCRIPTION,MarketProjection.RUNNER_DESCRIPTION};
                return MainWindow.Client.listMarketCatalogue(this.MarketFilter, marketProjectionHashSet, MarketSort.FIRST_TO_START, "200", language).ToList();                
            }
            catch (System.Exception)
            {
                //login
                return new List<MarketCatalogue>();
                //WpfBetApplication.MainWindow.Button_Click_Login();
            }
        }
    }
}
