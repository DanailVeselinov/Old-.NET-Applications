using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BFDL;
using System.Windows.Threading;
using System.Net.Http;
using Betfair_Non_interactive_login;
using System.Security.Cryptography;
using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Win32;
using BetFairBot.TO;

namespace BetFairBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string Url = "https://api.betfair.com/exchange/betting";
        public static BFDL.BFBDBEntities databaseContext = new BFDL.BFBDBEntities();
        public static Users UserActive = null;
        public static IClient Client = null;
        public static BettingLine CurrentBetline { get; set; }
        public static string soccerLivescore { get; set; }
        public static string MainSeparator = ";";
        public static string SortedOrderSeparator = ",";
        public static string SESSION_TOKEN = null;
        public MainWindow()
        {
            InitializeComponent();
            
            System.Net.ServicePointManager.Expect100Continue = false;
            UserActive = databaseContext.Users.FirstOrDefault(u => u.isActive);
            CurrentBetline = new BettingLine();
            CurrentBetline.Filters = new Filters();
            CurrentBetline.MarketFilter = new MarketFilter();
            if (UserActive!=null)
            {
                _username.Text = UserActive.userName;
                _password.Password = UserActive.Password;
                _appKeyTB.Text = UserActive.appKey;
                _certFileTB.Text = UserActive.certFileName;
                Button_Click_Login(this, new RoutedEventArgs());
                //if (LoginSuccessfull(UserActive.userName, UserActive.Password, UserActive.appKey, UserActive.certFileName))
                //{
                //    StartupActiveMethodsFmDatabase();
                //    BindBetlinesGrid();
                //    
                //}
            }
        }

        private bool LoginSuccessfull(string username, string password, string appKey, string certFilename)
        {
            username = "sabina8234"; //args.First();
            appKey = "vBgUxOJsNetcV5I5"; //args.ElementAt(1);
            certFilename = "EddyBetFairApp.p12";//args.ElementAt(2);
            password = "Samota8234";
            var client = new AuthClient(appKey);
            try
            {
                var resp = client.doLogin(username, password, certFilename);
                Console.WriteLine("Response Type: " + resp.LoginStatus);
                if (resp.LoginStatus == "SUCCESS")
                {
                    MainWindow.Client = new JsonRpcClient(MainWindow.Url, appKey, resp.SessionToken);
                    MainWindow.SESSION_TOKEN = resp.SessionToken;
                    KeepAliveTimer();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Could not load the certificate: " + e.Message);
                return false;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("The Betfair Login endpoint returned an HTTP Error: " + e.Message);
                return false;
            }
            catch (WebException e)
            {
                Console.WriteLine("An error occurred whilst attempting to make the request: " + e.Message);
                return false;
            }
            catch (System.Exception e)
            {
                Console.WriteLine("An Error Occurred: " + e.Message);
                return false;
            }
        }

        private void KeepAliveTimer()
        {
            DispatcherTimer keepAliveTimer = new DispatcherTimer();
            keepAliveTimer.Interval = TimeSpan.FromMinutes(15);
            keepAliveTimer.Tick += keepAliveTimer_Tick;
            keepAliveTimer.Start();
        }

        void keepAliveTimer_Tick(object sender, EventArgs e)
        {
            KeepAlive(MainWindow.SESSION_TOKEN);
            //string Url = "https://identitysso.betfair.com/api/keepAlive";
            //System.Net.HttpWebRequest request = null;
            //System.Net.WebResponse response = null;
            //string strResponseContent = "";
            //try 
            //{
            //    request = System.Net.HttpWebRequest.CreateHttp(new Uri(Url));
            //    request.Accept = "application/json"; // Tells API-NG to respond in Json not Html
            //    request.Method = "POST";
            //    request.Headers.Add(HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8");
            //    request.Headers.Add("X-Authentication", SessToken);
            //    if (!string.IsNullOrEmpty(AppKey)) request.Headers.Add("X-Application", AppKey);
            //    // Get the response using io stream
            //    using (System.Net.WebResponse wr = request.GetResponse())
            //    {
            //        using (System.IO.StreamReader sr = new System.IO.StreamReader(wr.GetResponseStream()))
            //        {
            //            strResponseContent = sr.ReadToEnd();
            //            sr.Close();
            //        }
            //        wr.Close();
            //    }
            //    Console.WriteLine(strResponseContent); 
            //} 
            //catch (System.Exception ex) 
            //{
            //    MessageBox.Show("Error: " + Environment.NewLine + ex.Message);
            //}
            // Expect to print out {"token":"4bnwldH;gf+jemndUIjmndGbCc=","product":"Your Product ID","status":"SUCCESS","error":""}

        }

        public void KeepAlive(string SessToken, string AppKey = "")
        {
            string Url = " https://identitysso.betfair.com/api/keepAlive";
            WebRequest request = null;
            WebResponse response = null;
            string strResponseStatus = "";
            try
            {
                request = WebRequest.Create(new Uri(Url));
                request.Method = "POST";
                request.ContentType = "application/json-rpc";
                request.Headers.Add(HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8");
                request.Headers.Add("X-Authentication", SessToken);
                if (!string.IsNullOrEmpty(AppKey))
                {
                    request.Headers.Add("X-Application", AppKey);
                }
                //~~> Get the response.
                response = request.GetResponse();
                //~~> Display the status below 
                strResponseStatus = "Status is " + ((HttpWebResponse)response).StatusDescription;
            }
            catch (System.Exception ex)
            {
                 MessageBox.Show("CreateRequest Error" + Environment.NewLine + ex.Message);
            }
            //~~~Clean Up
            response.Close();
        }

        private void StartupActiveMethodsFmDatabase()
        {
            var betlinesList = from b in databaseContext.Betlines
                               where b.ownersUserName == UserActive.userName
                               select b;
            foreach (var betline in betlinesList)
            {
                BettingLine bettingLine = new BettingLine(betline);
                switch (bettingLine.algorithmName)
                {
                    case "Incrementing Stake":
                        BetOnAllRecentlyFoundIncrementing(bettingLine);
                        break;
                    case "BetOnAllRecentlyFound":
                        BetOnAllRecentlyFoundIncrementing(bettingLine);
                        break;
                    case "BetOnAllRecentlyFoundIncrementing":
                        BetOnAllRecentlyFoundIncrementing(bettingLine);
                        break;
                    default:
                        break;
                }
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }


        

        public void UpdateScores()
        {
            var d = BetFairBot.BettingLine.DocumentGetter("http://www.betfair.com/exchange/inplay");
            //var d = BetFairBot.BettingLine.DocumentGetter("https://www.betfair.com/exchange/inplay?language=en");
            var rows = d.DocumentNode.Descendants("tr");
            var rs = from r in rows
                     where r.InnerHtml.Contains("class=\"name\"") & !r.InnerHtml.Contains("<tr>")
                     select r;
            StringBuilder sb = new StringBuilder();
            foreach (var row in rs)
            {
                try
                {
                    var parts = row.InnerText.Trim();
                    var p = row.Descendants("span");
                    string homeTeam = "";
                    string awayTeam = "";
                    string score = "";
                    string progress = "";
                    StringBuilder innerSb = new StringBuilder();
                    foreach (var item in p)
                    {
                        if (item.GetAttributeValue("class", "error").Contains("result") & score == "")
                        {
                            if (item.InnerText != null)
                            {
                                score = item.InnerText.Replace(" - ", "-").Trim();
                            }
                            continue;
                        }
                        if (item.GetAttributeValue("class", "error").Contains("home-team") & homeTeam == "")
                        {
                            homeTeam = item.InnerText.Replace('-', '_').Trim();
                            continue;
                        }
                        if (item.GetAttributeValue("class", "error").Contains("away-team") & awayTeam == "")
                        {
                            awayTeam = item.InnerText.Replace('-', '_').Trim();
                            continue;
                        }
                        if (item.GetAttributeValue("class", "error").Contains("dtstart time") & progress == "")
                        {
                            if (item.InnerText != null)
                            {
                                progress = item.InnerText.Trim();
                            }
                            continue;
                        }
                    }
                    innerSb.Append(homeTeam + " ");
                    innerSb.Append(score + " ");
                    innerSb.Append(awayTeam + " ");
                    innerSb.Append(progress);
                    var newParts = innerSb.ToString();
                    //var newParts = parts.Replace(" - ", "-");
                    //if (newParts.Split().Last().Contains("FT") | newParts.Split().Last().Contains("HT") | newParts.Split().Last().Contains("'"))
                    //{
                        sb.AppendLine(newParts);
                    //}
                }
                catch (System.Exception)
                {
                }
            }
            MainWindow.soccerLivescore = sb.ToString();
        }

        public static void UpdateLiveScores()
        {
            try
            {
                //XDocument liveScores = XDocument.Load("http://wap.7m.cn/en/quickLive.aspx?t=0&mt=");
                //XDocument resultScores = XDocument.Load("http://wap.7m.cn/en/quickLive.aspx?t=2&mt=");

                //Session.soccerLivescore = liveScores.ToString() + resultScores.ToString();
                //WebBrowser Browser = App.Current.MainWindow.FindName("Browser") as WebBrowser;
                //Browser.Navigate(new Uri("http://www.betfair.com/exchange/inplay/"));

                DateTime hoursBefore2 = DateTime.Now.AddHours(-2);
                var context = MainWindow.databaseContext;
                var notSettledNewBets = from bets in context.PlacedBets
                                        where bets.dateSettled == null
                                        select bets;
                foreach (var betLive in notSettledNewBets)
                {
                    BettingLine.UpdatePlacedBetResult(betLive, MainWindow.soccerLivescore);
                }
                context.SaveChanges();
                var notSettledOldBets = from bets in context.PlacedBets
                                        where bets.resultCode != "Settled" & bets.resultCode != "FT" & bets.datePlaced < hoursBefore2
                                        select bets;
                foreach (var bet in notSettledOldBets)
                {
                    context.PlacedBets.Find(bet.id).resultCode = "FT";
                }
                context.SaveChanges();
            }
            catch (System.Exception)
            {
                restartTimer_Tick(null, new EventArgs());
            }
        }

        public static void restartTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var window = (App.Current.MainWindow as MainWindow);
                //if ((bool)window._ProgramRestartCB.IsChecked)
                //{
                //    int restInterval = 3;
                //    int.TryParse(window._ProgramRestartTimeTB.Text, out restInterval);
                //    if (DateTime.Now.Hour % restInterval == 0)
                //    {
                        //Restart Application
                        try
                        {
                            App.Current.Shutdown();
                        }
                        catch (System.Exception)
                        {
                            return;
                        }
                        System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                //    }
                //}
                //if ((bool)window._ComputerRestartCB.IsChecked)
                //{
                //    int restTime = 9;
                //    int.TryParse(window._ComputerRestartTimeTB.Text, out restTime);
                //    if (DateTime.Now.Hour == restTime)
                //    {
                //        //Restart Computer
                //        System.Diagnostics.Process.Start("shutdown.exe", "-r -f -t 0");
                //    }
                //}
            }
            catch (System.Exception)
            {
            }
        }

        public static double ReduceBallance(double ballance)
        {
            try
            {
                BFBDBEntities context = new BFBDBEntities();
                var usersBetlinesCount = (from b in context.Betlines
                                          where b.isActive
                                          select b).Count();
                for (int i = 1; i < usersBetlinesCount; i++)
                {
                    ballance *= 0.8;
                }
                return ballance > 0 ? ballance : 0;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                return ballance > 0 ? ballance : 0;
            }

        }

        private void FillPrimaryFilters(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FillPrimaryFilterSuccessfuly(CurrentBetline))
                {
                    List<MarketOptions> marketsList = new List<MarketOptions>();
                    var language = "bg";
                    var marketProjectionHashSet = new HashSet<TO.MarketProjection> { TO.MarketProjection.EVENT, TO.MarketProjection.EVENT_TYPE, TO.MarketProjection.MARKET_DESCRIPTION, TO.MarketProjection.RUNNER_DESCRIPTION };
                    var currentMarkets = MainWindow.Client.listMarketCatalogue(CurrentBetline.MarketFilter, marketProjectionHashSet, TO.MarketSort.FIRST_TO_START, "200", language).ToList();
                    foreach (var item in currentMarkets)
                    {
                        var marketOptoin = new MarketOptions(item.MarketName, item.Description.MarketType, GetRunnersNames(item));
                        if (!marketsList.Contains(marketOptoin))
                        {
                            marketsList.Add(marketOptoin);
                        }
                    }
                    _marketName.DataContext = marketsList;
                    _SecondaryFilterTabItem.IsEnabled = true;
                    _SecondaryFilterTabItem.Focus();
                }
            }
            catch (System.Exception)
            {
            }
        }

        private List<string> GetRunnersNames(MarketCatalogue item)
        {
            switch (item.Description.MarketType)
            {
                case "MATCH_ODDS":
                    return new List<string>() { "Home", "Away", "The Draw" };
                case "HALF_TIME_FULL_TIME":
                    return new List<string>() { "Home/Home", "Home/Draw", "Home/Away", "Draw/Home", "Draw/Draw", "Draw/Away", "Away/Home", "Away/Draw", "Away/Away" };
                case "HALF_TIME":
                    return new List<string>() { "Home", "Away", "The Draw" };
                case "CORRECT_SCORE":
                    return new List<string>() { "0 - 0", "0 - 1", "0 - 2", "0 - 3", "1 - 0", "1 - 1", "1 - 2", "1 - 3", "2 - 0", "2 - 1", "2 - 2", "2 - 3", "3 - 0", "3 - 1", "3 - 2", "3 - 3", "Any Unquoted" };
                default:
                    return new List<string>();
            }
        }

        private bool FillPrimaryFilterSuccessfuly(BettingLine betline)
        {
            try
            {
                HashSet<string> countryList = new HashSet<string>();
                if (_countryCodes.SelectedItems.Count != 0)
                {
                    betline.Filters.marketCountries = "";
                    foreach (CountryCode item in _countryCodes.SelectedItems)
                    {
                        countryList.Add(item.CountryCode1);
                        betline.Filters.marketCountries += item.CountryCode1 + ";";
                    }
                    betline.MarketFilter.MarketCountries = countryList;
                }

                var eventTypes = new HashSet<string>();
                if (_events.SelectedItems.Count != 0)
                {
                    betline.Filters.eventTypeIds = "";
                    foreach (TO.EventTypeResult item in _events.SelectedItems)
                    {
                        eventTypes.Add(item.EventType.Id);
                        betline.Filters.eventTypeIds += item.EventType.Id + ";";
                    }
                    betline.MarketFilter.EventTypeIds = eventTypes;
                }
                if (_fromEventDatePicker.SelectedDate != null)
                {
                    betline.Filters.marketTimeFrom = _fromEventDatePicker.SelectedDate;
                    betline.MarketFilter.MarketStartTime.From = (DateTime)_fromEventDatePicker.SelectedDate;
                }
                else
                {
                    betline.Filters.marketTimeFrom = null;
                }
                if (_toEventsDatePicker.SelectedDate != null)
                {
                    betline.Filters.marketTimeTo = _toEventsDatePicker.SelectedDate;
                    betline.MarketFilter.MarketStartTime.To = (DateTime)_toEventsDatePicker.SelectedDate;
                }
                else
                {
                    betline.Filters.marketTimeTo = null;
                }
                betline.Filters.inPlayOnly = _InPlayOnlyCheckbox.IsChecked;
                betline.MarketFilter.InPlayOnly = _InPlayOnlyCheckbox.IsChecked;
                return true;
            }
            catch (System.Exception)
            {
                bool res = false;
                MessageBox.Show("Primary Filter Parsing Error! Please fill correctly Prymari filter data and Try again!");
                return res;
            }
        }
        
        private void Button_Click_NewBetLine(object sender, RoutedEventArgs e)
        {
            if (FillSecondaryFilterSuccessfuly(CurrentBetline))
            {
                _BetlineTabItem.IsEnabled = true;
                _BetlineTabItem.Focus();
                _StakeRangeMin.Focus();
            }
            else
            {
                _BetlineTabItem.IsEnabled = false;
            }
        }

        private bool FillSecondaryFilterSuccessfuly(BettingLine betline)
        {
            try
            {
                foreach (RadioButton rb in _ExchangeIDFilter.Children)
                {
                    if ((bool)rb.IsChecked)
                    {

                        switch (rb.Content.ToString())
                        {
                            case "Worldwide/UK":
                                betline.Filters.exchangeIds = "1";
                                break;
                            case "Australian":
                                betline.Filters.exchangeIds = "2";
                                break;
                            default:
                                betline.Filters.exchangeIds = null;
                                break;
                        }
                    }
                }

                foreach (RadioButton rb in _ActiveFilter.Children)
                {
                    if ((bool)rb.IsChecked)
                    {

                        switch (rb.Content.ToString())
                        {
                            case "Active":
                                betline.Filters.isActive = true;
                                break;
                            case "Non Active":
                                betline.Filters.isActive = false;
                                break;
                            default:
                                betline.Filters.isActive = null;
                                break;
                        }
                    }
                }

                foreach (RadioButton rb in _IsBSPFilter.Children)
                {
                    if ((bool)rb.IsChecked)
                    {

                        switch (rb.Content.ToString())
                        {
                            case "BSP":
                                betline.Filters.bspOnly = true;
                                break;
                            case "Non BSP":
                                betline.Filters.bspOnly = false;
                                break;
                            default:
                                betline.Filters.bspOnly = null;
                                break;
                        }
                    }
                }

                List<string> marketTypesList = new List<string>();
                foreach (CheckBox cb in _MarketTypeFilter.Children)
                {
                    if ((bool)cb.IsChecked)
                    {
                        string type = "";
                        switch (cb.Content.ToString())
                        {
                            case "Odds":
                                type = MarketBettingType.ODDS.ToString();
                                break;
                            case "Line":
                                type = MarketBettingType.LINE.ToString();
                                break;
                            case "Fixed Odds":
                                type = MarketBettingType.FIXED_ODDS.ToString();
                                break;
                            case "Range":
                                type = MarketBettingType.RANGE.ToString();
                                break;
                            case "Asian Handicap Singles":
                                type = MarketBettingType.ASIAN_HANDICAP_SINGLE_LINE.ToString();
                                break;
                            case "Asian Handicap Doubles":
                                type = MarketBettingType.ASIAN_HANDICAP_DOUBLE_LINE.ToString();
                                break;
                            default:
                                break;
                        }
                        if (!string.IsNullOrEmpty(type))
                        {
                            marketTypesList.Add(type);                            
                        }
                    }
                }                
                betline.Filters.marketBettingTypes = ListToString(marketTypesList,MainSeparator);
                List<string> marketNamesList = new List<string>();
                foreach (MarketOptions mName in _marketName.SelectedItems)
                {
                    marketNamesList.Add(mName.MarketName);
                    marketNamesList.Add(mName.MarketTypeName);
                }
                betline.Filters.marketNames = ListToString(marketNamesList,MainSeparator);
                return true;
            }
            catch (System.Exception)
            {
                bool res = false;
                MessageBox.Show("Secondary Filter Parsing Error! Please fill correctly filter data and Try again!");
                return res;
            }
        }

        private string ListToString(List<string> marketNamesList, string MainSeparator)
        {
            string result = "";
            foreach (var item in marketNamesList)
            {
                result += item + MainSeparator;
            }
            return result;
        }

        private void _algo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0].ToString().ToUpper().Contains("Incrementing".ToUpper()))
            {
                _maxAmmountStack.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                _maxAmmountStack.Visibility = System.Windows.Visibility.Collapsed;
            }
            if (e.AddedItems[0].ToString().ToUpper().Contains("AllRecentlyFound".ToUpper()))
            {
                _maxBetsCountStack.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                _maxBetsCountStack.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void Button_Click_StartNewBetLine(object sender, RoutedEventArgs e)
        {
            if (FillBetlineParameters(CurrentBetline))
            {
                //selection of betting Algorythms
                switch (_algo.SelectedIndex)
                {
                    case 0:
                        MessageBox.Show("Please select algorithm for Betting Line");
                        return;
                    case 1:
                        CurrentBetline.algorithmName = "Incrementing Stake";
                        BetOnAllRecentlyFoundIncrementing();
                        break;
                    case 2:
                        CurrentBetline.algorithmName = "BetOnAllRecentlyFound";
                        BetOnAllRecentlyFoundIncrementing();
                        break;
                    case 3:
                        CurrentBetline.algorithmName = "BetOnAllRecentlyFoundIncrementing";
                        BetOnAllRecentlyFoundIncrementing();
                        break;
                    default:
                        MessageBox.Show("Please select algorithm for Betting Line");
                        return;
                }
                _SecondaryFilterTabItem.IsEnabled = false;
                _BetlineTabItem.IsEnabled = false;
                _PrimaryFilterTabItem.Focus();
                //UserActive.Betlines.Add(CurrentBetline as Betlines);
                databaseContext.SaveChanges();
                BindBetlinesGrid();
                CurrentBetline = new BettingLine();
                CurrentBetline.MarketFilter = new MarketFilter();
            }
        }

        public bool FillBetlineParameters(BettingLine betline)
        {
            try
            {
                var ppbet = double.Parse(_profitPerBetTB.Text);
                betline.profitPerBudjet = double.Parse(_profitPerBudjetTB.Text);
                betline.initialProfitPerBet = ppbet;
                betline.profitPerBet = ppbet;
                betline.stakeRangeMin = double.Parse(_StakeRangeMin.Text);
                betline.stakeRangeMax = double.Parse(_StakeRangeMax.Text);
                betline.betType = _BetType.Text;
                
                if (!string.IsNullOrWhiteSpace(UserActive.userName))
                {
                    betline.ownersUserName = UserActive.userName;
                }
                if (!String.IsNullOrWhiteSpace(_totalAmmountMatchedMin.Text))
                {
                    betline.Filters.totalAmountMatchedMin = double.Parse(_totalAmmountMatchedMin.Text);
                }
                if (!String.IsNullOrWhiteSpace(_totalAmmountMatchedMax.Text))
                {
                    betline.Filters.totalAmountMatchedMax = double.Parse(_totalAmmountMatchedMax.Text);
                }
                try
                {
                    if (!String.IsNullOrEmpty(__maxAmmountTB.Text) | !String.IsNullOrEmpty(__maxAmmountTB.Text))
                    {
                        betline.Filters.maxAmmountIncrementedPerBudjet = double.Parse(_maxAmmountPercentTB.Text);
                        betline.Filters.maxAmmountIncremented = double.Parse(__maxAmmountTB.Text);
                    }
                    else
                    {
                        betline.Filters.maxAmmountIncremented = double.MaxValue;
                    }
                }
                catch (System.Exception)
                {
                    MessageBox.Show("Wrong Max Ammounts entered. Please enter number From 1 to 16!");
                    return false;
                }

                string maxBets = (_maxBetsCountStack.Children[1] as TextBox).Text;
                if (!String.IsNullOrEmpty(maxBets))
                {
                    try
                    {
                        betline.Filters.maxBetsCount = byte.Parse(maxBets);
                    }
                    catch (System.Exception)
                    {
                        MessageBox.Show("Wrong Max Bets Count entered try again!");
                        return false;
                    }
                }
                if (!string.IsNullOrWhiteSpace(_goalDifferenceMin.Text))
                {
                    try
                    {
                        foreach (char item in _goalDifferenceMin.Text)
                        {
                            if (!char.IsDigit(item))
                            {
                                throw new System.Exception();
                            }
                        }
                        betline.Filters.goalDifferenceMin = short.Parse(_goalDifferenceMin.Text);
                    }
                    catch (System.Exception)
                    {
                    }
                }
                if (!string.IsNullOrWhiteSpace(_goalDifferenceMax.Text))
                {
                    try
                    {
                        foreach (char item in _goalDifferenceMax.Text)
                        {
                            if (!char.IsDigit(item))
                            {
                                throw new System.Exception();
                            }
                        }
                        betline.Filters.goalDifferenceMax = short.Parse(_goalDifferenceMax.Text);
                    }
                    catch (System.Exception)
                    {
                    }
                }
                betline.ownersUserName = UserActive.userName;
                betline.betlineName = string.IsNullOrWhiteSpace(betlineName.Text) ? string.Format("{3}'s - {0} - {1} -> {2} {4} - {5}", _BetType.Text, _StakeRangeMin.Text, _StakeRangeMax.Text, UserActive.userName, betline.Filters.marketNames, _algo.Text) : betlineName.Text;
                return true;
            }
            catch (System.Exception)
            {
                MessageBox.Show("Please fill BetLine Parameters, and try again.");
                return false;
            }
        }

        private void _showAllCB_Click(object sender, RoutedEventArgs e)
        {
            BindBetlinesGrid();
        }

        private void _RunningBetLines_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void _RunningBetLines_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void ResumeBetline(object sender, RoutedEventArgs e)
        {

        }

        private void PauseBetline(object sender, RoutedEventArgs e)
        {

        }

        private void StopOnWinBetline(object sender, RoutedEventArgs e)
        {

        }

        private void Restart(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteBetline(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckConnectionTimerInit();
            _events.DataContext = new List<TO.EventTypeResult>() { new TO.EventTypeResult() { EventType = new TO.EventType() { Id = "1", Name = "Soccer" } }, new TO.EventTypeResult() { EventType = new TO.EventType() { Id = "2", Name = "Tennis" } } };
            BindCountryCodeList();
            BindAlgoComboBox();
            LivescoreUpdateTimerInitiate();
            if (UserActive!=null)
            {                
                _events.DataContext = Client.listEventTypes(new TO.MarketFilter(), null);
                GetAccountDetails();
                // loads all the active betlines from database and runs them
            }
        }

        private void GetAccountDetails()
        {
            try
            {
                
                StringBuilder sb = new StringBuilder();
                sb.Append("Mr.");
                sb.Append("Veselinov");
                sb.Append(" " + "D" + ".");
                _userFullName.Content = sb.ToString();
                _currency.Content = "EUR";
            }
            catch (System.Exception)
            {
                //restartCompTimer_Tick(this, new EventArgs());
            }
        }

        private void LivescoreUpdateTimerInitiate()
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(10);
            dt.Tick += dt_Tick;
            dt.Start();
            dt_Tick(this, new EventArgs());
        }

        void dt_Tick(object sender, EventArgs e)
        {
            UpdateScores();
        }

        internal void BindAlgoComboBox()
        {
            Dictionary<int, string> methods = new Dictionary<int, string>();
            methods.Add(0, "Select Algorithm");
            methods.Add(1, "Incrementing Stake");
            methods.Add(2, "BetOnAllRecentlyFound");
            methods.Add(3, "BetOnAllRecentlyFoundIncrementing");
            _algo.DataContext = methods;
        }

        internal void BindCountryCodeList()
        {
            List<CountryCode> countryCodesISO3 = new List<CountryCode>() { new CountryCode("AFG", "Afghanistan"), new CountryCode("AGO", "Angola"), new CountryCode("AIA", "Anguilla"), new CountryCode("ALA", "Åland Islands"), new CountryCode("ALB", "Albania"), new CountryCode("AND", "Andorra"), new CountryCode("ARE", "United Arab Emirates"), new CountryCode("ARG", "Argentina"), new CountryCode("ARM", "Armenia"), new CountryCode("ASM", "American Samoa"), new CountryCode("ATA", "Antarctica"), new CountryCode("AUS", "Australia"), new CountryCode("AUT", "Austria"), new CountryCode("BEL", "Belgium"), new CountryCode("BGR", "Bulgaria"), new CountryCode("BLR", "Belarus"), new CountryCode("BRA", "Brazil"), new CountryCode("CAN", "Canada"), new CountryCode("CHE", "Switzerland"), new CountryCode("CHL", "Chile"), new CountryCode("CHN", "China"), new CountryCode("CMR", "Cameroon"), new CountryCode("COL", "Colombia"), new CountryCode("CZE", "Czech Republic"), new CountryCode("CYP", "Cyprus"), new CountryCode("DEU", "Germany"), new CountryCode("DNK", "Denmark"), new CountryCode("ESP", "Spain"), new CountryCode("EST", "Estonia"), new CountryCode("FRA", "France"), new CountryCode("GBR", "United Kingdom"), new CountryCode("GEO", "Georgia"), new CountryCode("GRC", "Greece"), new CountryCode("HUN", "Hungary"), new CountryCode("ITA", "Italy"), new CountryCode("JPN", "Japan"), new CountryCode("ISL", "Iceland"), new CountryCode("MEX", "Mexico"), new CountryCode("MNE", "Montenegro"), new CountryCode("NOR", "Norway"), new CountryCode("PRT", "Portugal"), new CountryCode("ROU", "Romania"), new CountryCode("RUS", "Russia"), new CountryCode("SRB", "Serbia"), new CountryCode("SVK", "Slovakia"), new CountryCode("SVN", "Slovenia"), new CountryCode("SWE", "Sweden"), new CountryCode("SWZ", "Swaziland"), new CountryCode("TUR", "Turkey"), new CountryCode("UKR", "Ukraine"), new CountryCode("USA", "United States") };
            _countryCodes.DataContext = countryCodesISO3;
        }

        private void CheckConnectionTimerInit()
        {
            DispatcherTimer checkConnectionTimer = new DispatcherTimer();
            checkConnectionTimer.Interval = TimeSpan.FromMinutes(1);
            checkConnectionTimer.Start();
            checkConnectionTimer.Tick += checkConnectionTimer_Tick;
        }

        void checkConnectionTimer_Tick(object sender, EventArgs e)
        {
            if (!CheckConnection("http://www.betfair.com/exchange/inplay/"))
            {
                //restartTimer_Tick(this, new EventArgs());
            }
            //Session.UpdateLiveScores();
        }

        private bool CheckConnection(string URL)
        {
            bool kleir = false;
            using (Ping ping = new Ping())
            {
                try
                {
                    if (ping.Send(URL, 2000).Status == IPStatus.Success)
                    {
                        kleir = true;
                    }
                }
                catch (PingException)
                {
                    kleir = false;
                }
            }
            return kleir;
        }

        private void Button_Click_Login(object sender, RoutedEventArgs e)
        {
            bool loggedIn = LoginSuccessfull(_username.Text,_password.Password,_appKeyTB.Text,_certFileTB.Text);
            if (!loggedIn)
            {
                return;
            }
            var user = databaseContext.Users.FirstOrDefault(u=>u.userName ==_username.Text);
            if (user!=null)
            {
                foreach (var u in databaseContext.Users.Where(us=>us.isActive))
                {
                    if(u.userName != _username.Text)
                    {
                        u.isActive = false;
                    }
                }
                user.isActive = true;
                UserActive = user;
            }
            else
            {
                user = new Users();
                user.userName = _username.Text;
                user.Password = _password.Password;
                user.appKey = _appKeyTB.Text;
                user.certFileName = _certFileTB.Text;
                user.isActive = (bool)_rememberMe.IsChecked;
                UserActive = databaseContext.Users.Add(user);
            }
            databaseContext.SaveChanges();
            _LoginPanel.Visibility = System.Windows.Visibility.Collapsed;
            _LoggedinPanel.Visibility = System.Windows.Visibility.Visible;
            BindAccountDetails();
            StartupActiveMethodsFmDatabase();
            BindBetlinesGrid();
            CurrentBetline.Users = UserActive;
            CurrentBetline.ownersUserName = UserActive.userName;
            try
            {
                if (UserActive != null)
                {
                    _events.DataContext = Client.listEventTypes(new TO.MarketFilter(), null);
                }
            }
            catch (APINGException apiEx)
            {
                 MessageBox.Show(apiEx.ErrorCode + Environment.NewLine + apiEx.Message);
            }
            catch (System.Exception)
            {
                restartTimer_Tick(this, new EventArgs());
            }
        }

        private void BindAccountDetails()
        {
            //implement
        }

        internal static void BindBetlinesGrid()
        {
            try
            {
                BFDL.BFBDBEntities context = new BFDL.BFBDBEntities();
                var window = BetFairBot.App.Current.MainWindow as BetFairBot.MainWindow;
                int index = window._RecentBetlinesCombo.SelectedIndex;
                window.DataContext = context.Betlines.Where(b => b.ownersUserName == UserActive.userName).ToList();
                window._RecentBetlinesCombo.SelectedIndex = index < 0 ? 0 : index;
                BetFairBot.App.Current.MainWindow.SizeToContent = SizeToContent.WidthAndHeight;
            }
            catch (System.Exception)
            {
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            BindFunds();
        }

        public void BindFunds()
        {
            //try
            //{
            //    GetAccountFundsReq fundsReq = new GetAccountFundsReq();
            //    fundsReq.header = new WebExchangeService.APIRequestHeader();
            //    fundsReq.header.sessionToken = Session.globalHeader.sessionToken;
            //    GetAccountFundsResp fundsResp = Session.exchangeService.getAccountFunds(fundsReq);

            //    if (fundsResp.errorCode == GetAccountFundsErrorEnum.OK)
            //    {
            //        _FundsAvailable.Content = fundsResp.availBalance;
            //        _FundsBallance.Content = fundsResp.balance;
            //    }
            //}
            //catch (Exception)
            //{
            //}
        }

        private void UpdateAllBudjets(object sender, RoutedEventArgs e)
        {
            DateTime lastDate = DateTime.Now;
            double ballance = 0;
            try
            {
                BindFunds();
                ballance = double.Parse(_FundsBallance.Content.ToString());
                lastDate = (DateTime)TryFindResource("LastBudjetUpdateTime");
            }
            catch (NullReferenceException)
            {
                Resources.Add("LastBudjetUpdateTime", lastDate);
                UpdateDBIPPB(ballance);
            }
            if (lastDate < DateTime.Now.AddDays(-1))
            {
                Resources["LastBudjetUpdateTime"] = DateTime.Now;
                UpdateDBIPPB(ballance);
            }
        }

        private void UpdateDBIPPB(double ballance)
        {
            try
            {
                var usersBetlines = from b in databaseContext.Betlines
                                    where b.ownersUserName == UserActive.userName & b.isActive
                                    select b;
                foreach (var item in usersBetlines)
                {
                    try
                    {
                        double reducedBallance = ReduceBallance(ballance);
                        var curBetline = databaseContext.Betlines.Find(item.BetlineId);
                        curBetline.initialProfitPerBet = Math.Round(reducedBallance * curBetline.profitPerBudjet / 100, 2) < 2 ? 2 : Math.Round(reducedBallance * curBetline.profitPerBudjet / 100, 2);
                        curBetline.Filters.maxAmmountIncremented = Math.Round(item.Filters.maxAmmountIncrementedPerBudjet * reducedBallance / 100, 2) < 2 ? 2 : Math.Round(item.Filters.maxAmmountIncrementedPerBudjet * reducedBallance / 100, 2);
                        databaseContext.SaveChanges();
                    }
                    catch (System.Exception)
                    {
                    }
                }
                this.DataContext = databaseContext.Betlines;
            }
            catch (System.Exception)
            {
            }
        }

        private void Button_Click_LogOut(object sender, RoutedEventArgs e)
        {
            string Url = " https://identitysso.betfair.com/api/logout";
            WebRequest request = null;
            WebResponse response = null;
            string strResponseStatus = "";
            try
            {
                request = WebRequest.Create(new Uri(Url));
                request.Method = "POST";
                request.ContentType = "application/json-rpc";
                request.Headers.Add(HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8");
                request.Headers.Add("X-Authentication", SESSION_TOKEN);
                //~~> Get the response.
                response = request.GetResponse();
                //~~> Display the status below 
                strResponseStatus = ((HttpWebResponse)response).StatusDescription;
            }
            catch (System.Exception ex)
            {
                 MessageBox.Show("CreateRequest Error" + Environment.NewLine + ex.Message);
                 return;
            }
            //~~~Clean Up
            response.Close();
            _username.Text = "";
            _password.Password = "";                    
            databaseContext.Users.Find(UserActive.userName).isActive = false;
            databaseContext.SaveChanges();
            UserActive = new Users();
            _LoggedinPanel.Visibility = System.Windows.Visibility.Hidden;
            _LoginPanel.Visibility = System.Windows.Visibility.Visible;
        }

        private void _PrimaryFilterTabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            _SecondaryFilterTabItem.IsEnabled = false;
            _BetlineTabItem.IsEnabled = false;
        }

        private void _SecondaryFilterTabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            _BetlineTabItem.IsEnabled = false;
        }

        private void _marketName_Selected(object sender, SelectionChangedEventArgs e)
        {
            Window dialogWindow = new DialogWindow();
            dialogWindow.Owner = App.Current.MainWindow;
            MarketOptions market = null;
            MarketOptions removeMarket = null;
            try
            {
                market = (e.AddedItems[0] as MarketOptions);
            }
            catch (IndexOutOfRangeException)
            {
            }
            try
            {
                removeMarket = (e.RemovedItems[0] as MarketOptions);
            }
            catch (IndexOutOfRangeException)
            {
            }
            if (market!=null)
            {
                Label labelMarket = dialogWindow.FindName("_TitleLabel") as Label;
                Label labelMarketType = dialogWindow.FindName("_TitleLabelType") as Label;
                labelMarket.Content = market.MarketName;
                labelMarketType.Content = market.MarketTypeName;
                WrapPanel checkBoxStackPanel = dialogWindow.FindName("_CheckBoxStackPanel") as WrapPanel;

                checkBoxStackPanel.Orientation = market.MarketOptionsList.Count < 5 ? Orientation.Horizontal : Orientation.Vertical;
                for (int i = 0; i < market.MarketOptionsList.Count; i++)
                {
                    CheckBox cb = new CheckBox();
                    cb.IsChecked = true;
                    cb.Content = market.MarketOptionsList[i];
                    cb.FontSize = 15;
                    cb.Margin = new Thickness(15);
                    checkBoxStackPanel.Children.Add(cb);
                }
                dialogWindow.ShowDialog();
            }
            if (removeMarket!=null)
            {
                var filterSO = MainWindow.CurrentBetline.Filters.sortedOrders;
                var marketName = removeMarket.MarketName;                
                if (!string.IsNullOrEmpty(filterSO))
                {
                    var marketsSO = filterSO.Trim(MainWindow.MainSeparator[0]).Split(MainWindow.MainSeparator[0]).ToList();
                    var msoElement = marketsSO.FirstOrDefault(m => m.ToUpper().Contains(marketName.ToUpper()));
                    if (!string.IsNullOrWhiteSpace(msoElement))
                    {
                        marketsSO.Remove(msoElement);
                    }
                    var newFilterSO = "";
                    foreach (var so in marketsSO)
                    {
                        newFilterSO += so + MainWindow.MainSeparator;
                    }
                    MainWindow.CurrentBetline.Filters.sortedOrders = newFilterSO;
                }
            }
            
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            //dlg.Filter = "CertFile|*.crt";
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    _certFileTB.Text = dlg.FileName;
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Error loading image: " +
                    ex.Message);
                }
            }
        }

    }
}
