using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfBetApplication.WebGlobalService;
using WpfBetApplication.WebExchangeService;
using System.Windows.Threading;

namespace WpfBetApplication
{
    public static class Session 
    {
        public static string soccerLivescore = "";
        public static BFGlobalService globalService = new BFGlobalService();
        public static BFExchangeService exchangeService = new BFExchangeService();
        public static WebGlobalService.APIRequestHeader globalHeader = new WebGlobalService.APIRequestHeader();
        private static WpfBetApplication.BettingLine betline = new BettingLine();
        public static WpfBetApplication.BettingLine Betline
        {
            get { return Session.betline; }
            set { Session.betline = value; }
        }
        public static DBLBettingApp.BFDBLEntities1 databaseContext = new DBLBettingApp.BFDBLEntities1();
        public static List<BettingLine> activeBettingLines = new List<BettingLine>();

        public static DateTime baseDate = new DateTime(1970, 1, 1, 00, 00, 00, DateTimeKind.Utc);
        public static List<Market> MarketsList = new List<Market>(32);
        
        //working
        public static void checkHeader(WebGlobalService.APIResponseHeader header) 
        {
            globalHeader.sessionToken = header.sessionToken;
            
        }

        public static void checkHeader(WebExchangeService.APIResponseHeader header)
        {
            globalHeader.sessionToken = header.sessionToken;

        }
        //        
        
        public static bool FillMarketFmArray(Market market, string[] marketDataArray)
        {            
            try
            {
                market.marketId = int.Parse(marketDataArray[0]);
                market.name = marketDataArray[1];
                switch (marketDataArray[2])
	            {
                    case "A":
                        market.marketType = WebExchangeService.MarketTypeEnum.A;
                        break;
                    case "L":
                        market.marketType = WebExchangeService.MarketTypeEnum.L;
                        break;

                    case "O":
                        market.marketType = WebExchangeService.MarketTypeEnum.O;
                        break;

                    case "R":
                        market.marketType = WebExchangeService.MarketTypeEnum.R;
                        break;
		            default:
                        market.marketType = WebExchangeService.MarketTypeEnum.NOT_APPLICABLE;
                break;
	            }
                market.marketStatus = marketDataArray[3]== "ACTIVE" ? MarketStatusEnum.ACTIVE : MarketStatusEnum.SUSPENDED;
                market.marketTime = baseDate.AddMilliseconds(BettingLine.ParseDoubleFmNaNINF(marketDataArray[4]));
                market.menuPath = marketDataArray[5];
                string[] eventHierarchyString = marketDataArray[6].Trim('/').Split('/');                
                int?[] eventHierarchyInt = new int?[eventHierarchyString.Length];
                for (int i = 0; i < eventHierarchyString.Length - 1; i++)
                {                   
                     eventHierarchyInt[i] = int.Parse(eventHierarchyString[i]);                    
                }
                market.eventHierarchy = eventHierarchyInt;
                market.licenceId = int.Parse(marketDataArray[8]);
                market.countryISO3 = marketDataArray[9];
                market.numberOfWinners = int.Parse(marketDataArray[12]);
                market.bspMarket = marketDataArray[14] == "Y" ? true : false;
                return true;
            }
            catch (Exception) // Catch appropriate exception; Show Appropriate Message
            {
                return false;
            }
        } //Filling array of marketData to market(Object) and returns bool
        public static List<Market> AllMarketsInList(string marketsData) //Parsing the marketData into List of Market(Objects). 
        {
            List<Market> marketsList = new List<Market>();
            string colonCode = "&%^@";
            string[] marketsRawArray = marketsData.Replace(@"\:",colonCode).Split(':');            
            foreach (string marketRaw in marketsRawArray)
            {                
                    string[] marketDataArray = marketRaw.Replace(colonCode, ":").Replace(@"\~", "-").Split('~');
                    Market market = new Market();
                    if (marketDataArray.Length==16 && FillMarketFmArray(market , marketDataArray))
                    {
                        marketsList.Add(market);
                    }                                        
            }
            return marketsList;
        }



        internal static void BindEventsList()
        {
            GetEventTypesReq getEventTypeReq = new GetEventTypesReq();
            getEventTypeReq.header = globalHeader;
            globalService.getAllEventTypesAsync(getEventTypeReq, Guid.NewGuid().ToString());
        }
    }
}
