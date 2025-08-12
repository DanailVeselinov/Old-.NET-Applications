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
using System.Windows.Threading;
using System.Data.Entity;
using DBLBettingApp;
using WpfBetApplication.BFGlobalService;
using WpfBetApplication.BFExchangeService;
using System.Xml.Linq;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;



namespace WpfBetApplication
{
    /// <summary>
    /// // Implement Login on Account error
    /// //when checking bets see if you have more than one matched bet, if more than one then calculate the avg price and profit, Check the meaning of profitAndLoss property
    /// //Implement in-Play only
    /// // Implement Buttons for viewing all info available be methods, GetMarkets, PlaceBet, .... displayed in DataGrid or other form
    /// </summary>    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {            
            try
            {
                InitializeComponent();
                // loads all the active betlines from database and runs them
                BindEventTypes();
                BindCountryCodeList();
                BindMarketNames();
                BindAlgoComboBox();
                FromDateBind();
                TimeTimerInitiate();

                ////Email email = new Email();
                //email.email1 = "didoeddy@abv.bg";
                //email.id = 4;
                //Session.Email = email;
                //_RestartSP.DataContext = Session.Email;
                //GetResultByWebBrowser("http://www.footballgoalvideos.net/football/live#");
                this.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                //System.Threading.Thread.Sleep(new TimeSpan(0, 0, 10));
                //restartTimer_Tick(this, new EventArgs());
            }                     
        }

        private void BindMarketNames()
        {
            _marketName.ItemsSource = new List<string>() {"Match Odds","Correct Score" };
        }

        private void BindEventTypes()
        {
            List<EventType> eTypes = new List<EventType>() { new EventType(){name = "Soccer", id = 1}, new EventType(){name = "Tennis", id = 2}};
            _events.ItemsSource = eTypes;
        }

        private void TimeTimerInitiate()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10);
            timer.Tick += timer_Tick;
            timer.Start();
            Session.LastSleepDate = DateTime.Now;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                _fromDatePicker.Text = Session.DateTimeNow.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
            }
        }

        private void FromDateBind()
        {
            try
            {
                DBLBettingApp.BFDBLEntities context = new DBLBettingApp.BFDBLEntities();
                var placedBetsSortedByDateDesc = from pb in context.PlacedBets
                                                 orderby pb.datePlaced descending
                                                 select pb.datePlaced;
                _fromDatePicker.Text = placedBetsSortedByDateDesc.FirstOrDefault() == DateTime.MinValue ? DateTime.Now.ToString() : placedBetsSortedByDateDesc.FirstOrDefault().ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                _fromDatePicker.Text = DateTime.Now.ToString();
            }
            
        }

        internal static void RestartAppTimerInit()
        {
            DispatcherTimer restartTimer = new DispatcherTimer();
            restartTimer.Interval = TimeSpan.FromHours(1);
            restartTimer.Start();
            restartTimer.Tick += restartTimer_Tick;
        }

        public static void restartTimer_Tick(object sender, EventArgs e)
        {
                        
        }
        
        //fills the session betline with secondary filters and steps forward to the create betline tab i succsessfull.
        private void Button_Click_NewBetLine(object sender, RoutedEventArgs e)
        {  
            if (FillSecondaryFilterSuccessfuly(Session.Betline))
            {
                _BetlineTabItem.IsEnabled = true;
                _BetlineTabItem.Focus();
            }
            else
            {
                _BetlineTabItem.IsEnabled = false;
            }            
        }

        // Fills the Session betline with the betline parameters and starts the selected betting Algorythm
        private void Button_Click_StartNewBetLine(object sender, RoutedEventArgs e)
        {
            if (FillBetlineParameters(Session.Betline))
            {                
                //selection of betting Algorythms
                switch (_algo.SelectedIndex)
                {
                    case 0 :
                        MessageBox.Show("Please select algorithm for Betting Line");
                        break;
                    case 1 :
                        Session.Betline.AlgorithmName = "Incrementing Stake";
                        IncrementingStakeByOddBetline();
                        BindBetlinesGrid();
                        _SecondaryFilterTabItem.IsEnabled = false;
                        _BetlineTabItem.IsEnabled = false;
                        Session.Betline = new BettingLine();
                        break;
                    case 2:
                        Session.Betline.AlgorithmName = "BetOnAllRecentlyFound";
                        BetOnAllRecentlyFound();
                        BindBetlinesGrid();
                        _SecondaryFilterTabItem.IsEnabled = false;
                        _BetlineTabItem.IsEnabled = false;
                        _PrimaryFilterTabItem.Focus();
                        Session.Betline = new BettingLine();
                        break;
                    case 3:
                        Session.Betline.AlgorithmName = "BetOnAllRecentlyFoundIncrementing";
                        BetOnAllRecentlyFoundIncrementing();
                        BindBetlinesGrid();
                        _SecondaryFilterTabItem.IsEnabled = false;
                        _BetlineTabItem.IsEnabled = false;
                        _PrimaryFilterTabItem.Focus();
                        Session.Betline = new BettingLine();
                        break;
                    default:
                        Session.Betline = new BettingLine();
                        break;
                }                              
            }
        }

        public void IncrementingStakeByOddBetline()
        {
            try
            {
                BettingLine betline = Session.Betline;
                betline.IsActive = true;
                betline.LastBetLost = false;
                betline.CurrencyCode = "EUR";
                betline.timer = new DispatcherTimer();
                betline.timer.Interval = TimeSpan.FromSeconds(30); //use smaller interval at Tests            
                betline.timer.Start();
                //We can change these with filters
                betline.Bet.betCategoryType = BetCategoryTypeEnum.E;
                betline.Bet.betPersistenceType = BetPersistenceTypeEnum.NONE;
                betline.Bet.betType = _BetType.SelectedIndex == 0 ? BetTypeEnum.B : BetTypeEnum.L;
                betline.BetType = betline.Bet.betType == BetTypeEnum.B ? "B" : "L";
                // Adding betting Line to Database
                betline.AddToDB();
                //Execute selected Method for betline
                betline.timer.Tick += betline.Betline_timer_TickAsync;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                restartTimer_Tick(this, new EventArgs());
            }
                        
        }
        public void IncrementingStakeByOddBetline(BettingLine betline)
        {
            try
            {                
                betline.timer = new System.Windows.Threading.DispatcherTimer();
                betline.timer.Interval = TimeSpan.FromSeconds(30); //use smaller interval at Tests            
                betline.timer.Start();
                //We must replace these with filters
                betline.Bet = new PlaceBets();
                betline.Bet.betCategoryType = BetCategoryTypeEnum.E;
                betline.Bet.betPersistenceType = BetPersistenceTypeEnum.NONE;
                betline.Bet.betType = betline.BetType == "B" ? BetTypeEnum.B : BetTypeEnum.L ;
                // We dont Add betting Line to Database because it comes from there.
                //Execute selected Method for betline
                betline.timer.Tick += betline.Betline_timer_TickAsync;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                restartTimer_Tick(this, new EventArgs());
            }
        }

        //???
        public void BetOnAllRecentlyFound()
        {
            try
            {
                BettingLine betline = Session.Betline;
                betline.IsActive = true;
                betline.LastBetLost = false;
                betline.CurrencyCode = "EUR";
                betline.timer = new DispatcherTimer();
                betline.timer.Interval = TimeSpan.FromSeconds(30); //use smaller interval at Tests            
                betline.timer.Start();
                //We can change these with filters
                betline.Bet.betCategoryType = BetCategoryTypeEnum.E;
                betline.Bet.betPersistenceType = BetPersistenceTypeEnum.NONE;
                betline.Bet.betType = _BetType.SelectedIndex == 0 ? BetTypeEnum.B : BetTypeEnum.L;
                betline.BetType = betline.Bet.betType == BetTypeEnum.B ? "B" : "L";
                // Adding betting Line to Database
                betline.AddToDB();
                //Execute selected Method for betline
                betline.timer.Tick += betline.timer_Tick_BetOnAllRecentlyFound_Async;

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                restartTimer_Tick(this, new EventArgs());
            }
        }

        public void BetOnAllRecentlyFound(BettingLine betline)
        {
            try
            {
                betline.timer = new System.Windows.Threading.DispatcherTimer();
                betline.timer.Interval = TimeSpan.FromSeconds(30); //use smaller interval at Tests            
                betline.timer.Start();
                //We must replace these with filters
                betline.Bet = new PlaceBets();
                betline.Bet.betCategoryType = BetCategoryTypeEnum.E;
                betline.Bet.betPersistenceType = BetPersistenceTypeEnum.NONE;
                betline.Bet.betType = betline.BetType == "B" ? BetTypeEnum.B : BetTypeEnum.L;
                // We dont Add betting Line to Database because it comes from there.
                //Execute selected Method for betline
                betline.timer.Tick += betline.timer_Tick_BetOnAllRecentlyFound_Async;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                restartTimer_Tick(this, new EventArgs());
            }
        }

        public void BetOnAllRecentlyFoundIncrementing()
        {
            try
            {                
                BettingLine betline = Session.Betline;
                betline.IsActive = true;
                betline.LastBetLost = false;
                betline.CurrencyCode = "EUR";
                //We can change these with filters
                betline.Bet.betCategoryType = BetCategoryTypeEnum.E;
                betline.Bet.betPersistenceType = BetPersistenceTypeEnum.NONE;
                betline.Bet.betType = _BetType.SelectedIndex == 0 ? BetTypeEnum.B : BetTypeEnum.L;
                betline.BetType = betline.Bet.betType == BetTypeEnum.B ? "B" : "L";
                betline.timer.Tick+=timer_Tick;
                // Adding betting Line to Database
                betline.AddToDB();
                //Execute selected Method for betline
                betline.timer_Tick_BetOnAllRecentlyFoundIncrementing_Async(this,new EventArgs());
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                restartTimer_Tick(this, new EventArgs());                
            }
        }

        public void BetOnAllRecentlyFoundIncrementing(BettingLine betline)
        {
            try
            {                
                betline.timer = new System.Windows.Threading.DispatcherTimer();
                betline.timer.Interval = TimeSpan.FromSeconds(30); //use smaller interval at Tests            
                betline.timer.Start();
                //We must replace these with filters
                betline.Bet = new PlaceBets();
                betline.Bet.betCategoryType = BetCategoryTypeEnum.E;
                betline.Bet.betPersistenceType = BetPersistenceTypeEnum.NONE;
                betline.Bet.betType = betline.BetType == "B" ? BetTypeEnum.B : BetTypeEnum.L;
                // We dont Add betting Line to Database because it comes from there.
                //Execute selected Method for betline
                betline.timer.Tick += betline.timer_Tick_BetOnAllRecentlyFoundIncrementing_Async;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                restartTimer_Tick(this, new EventArgs());                
            }
        }

        private async void StartupActiveMethodsFmDatabase()
        {            
            try
            {
                Session.Running = true;
                _startButton.IsEnabled = false;
                _stopButton.IsEnabled = true;
                var betlinesList = Session.databaseContext.Betlines.ToList<DBLBettingApp.Betline>();
                List<BettingLine> bettingLinesList = new List<BettingLine>();
                DatabasePlacedBets.DatabasePlacedBetsEntities databaseDataContext = new DatabasePlacedBets.DatabasePlacedBetsEntities();
                foreach (var item in betlinesList)
                {
                    BettingLine betline = new BettingLine(item);

                    betline.DatabasePlacedBets = (from pb in databaseDataContext.PlacedBets
                                                  where pb.averagePrice <= betline.StakeRangeMax & pb.averagePrice >= betline.StakeRangeMin & pb.betType == betline.BetType// & betline.Filter.SortedOrdersMatchOddsList.Contains((byte)pb.sortedOrder) //& IsInSortedOrder(betline.Filter.SortedOrdersCorrectScoreList,betline.Filter.SortedOrdersMatchOddsList, pb)
                                                    select pb).ToList();
                    var newlist = from b in betline.DatabasePlacedBets
                                  where b.marketName == "Match Odds" ? betline.Filter.SortedOrdersMatchOddsList.Contains((byte)b.sortedOrder) : betline.Filter.SortedOrdersCorrectScoreList.Contains((byte)b.sortedOrder)
                                  select b;
                    betline.DatabasePlacedBets = newlist.ToList();
                    bettingLinesList.Add(betline);
                }

                while (Session.Running & (Session.DateTimeNow < (_toDatePicker.Text != "" ? DateTime.Parse(_toDatePicker.Text,System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat) : DateTime.MaxValue)))
                {
                    await Task.Run(() =>
                    {
                        if ((DateTime.Now - Session.LastSleepDate) > TimeSpan.FromMinutes(30))
                        {
                            System.Threading.Thread.Sleep(new TimeSpan(0, 3, 0));
                            Session.LastSleepDate = DateTime.Now;
                        }
                        foreach (BettingLine betline in bettingLinesList)
                        {
                            if (Session.DateTimeNow.Minute % 30 == 0)
                            {
                                var context = new DBLBettingApp.BFDBLEntities();
                                var MaxBetsCountFmDB = context.Betlines.Find(betline.BetlineId).Filter.maxBetsCount;
                                if (MaxBetsCountFmDB.Value>5)
                                {
                                    betline.Filter.MaxBetsCount = (byte)Session.RND.Next((int)(MaxBetsCountFmDB * 0.4), (int)MaxBetsCountFmDB);
                                }                                    
                            }
                            switch (betline.AlgorithmName)
                            {
                                case "Incrementing Stake":
                                    IncrementingStakeByOddBetline(betline);
                                    break;
                                case "BetOnAllRecentlyFound":
                                    BetOnAllRecentlyFound(betline);
                                    break;
                                case "BetOnAllRecentlyFoundIncrementing":
                                    betline.timer_Tick_BetOnAllRecentlyFoundIncrementing_Async(this,new EventArgs());
                                    break;
                                default:
                                    break;
                            }                    
                        }
                        Session.DateTimeNow += TimeSpan.FromMinutes(1);
                    });
                    (App.Current.MainWindow as WpfBetApplication.MainWindow).UpdateFunds();
                }
                _startButton.IsEnabled = true;
                _stopButton.IsEnabled = false;
                _resetDatabaseButton.IsEnabled = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}",e.Message,e.Source,e.StackTrace,e.InnerException.Message));
            }
        }

        private bool IsInSortedOrder(List<byte> list1, List<byte> list2, DatabasePlacedBets.PlacedBet pb)
        {
            try
            {                
                return pb.marketName == "Match Odds" ? list2.Contains((byte)pb.sortedOrder) : list1.Contains((byte)pb.sortedOrder);
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}",e.Message,e.Source,e.StackTrace,e.InnerException.Message));
                return false;
            }
        }


        public static bool SortedOrderContained(DatabasePlacedBets.PlacedBet pb, BettingLine betline)
        {
            bool result = false;
            if (pb.marketName == "Match Odds")
            {
                result = betline.Filter.SortedOrdersMatchOddsList.Contains((byte)pb.sortedOrder);
            }
            else
            {
                if (pb.marketName == "Correct Score")
                {
                    result = betline.Filter.SortedOrdersCorrectScoreList.Contains((byte)pb.sortedOrder);
                }
                else
                {
                    result = true;
                }
            }
            
            return result;
        }


        // Fill the Session betline with the primary filters and requested markets by these filters. Also steps forward to the secondary filters selection.
        void FillPrimaryFilters(object sender, RoutedEventArgs e)
        {
                if (FillPrimaryFilterSuccessfuly(Session.Betline))
                {
                    //Session.Betline.GetMarketsRequestFiltered();
                    BindMarketNames();
                    _SecondaryFilterTabItem.IsEnabled = true;
                    _SecondaryFilterTabItem.Focus();
                }
            
        }   

        public bool FillPrimaryFilterSuccessfuly(BettingLine betline)
        {
            try
            {
                List<string> countryList = new List<string>();
                if (_countryCodes.SelectedItems.Count!=0)
                {
                    foreach (CountryCode item in _countryCodes.SelectedItems)
                    {
                        countryList.Add(item.CountryCode1);
                    }
                    betline.Filter.CountryCodeArray = countryList.ToArray();
                }
                

                List<int?> eventTypesListNullable = new List<int?>();
                if (_events.SelectedItems.Count!=0)
                {
                    foreach (EventType item in _events.SelectedItems)
                    {
                        eventTypesListNullable.Add((int?)item.id);

                    }
                    betline.Filter.EventTypesArray = eventTypesListNullable.ToArray();                    
                }
                if (_fromEventDatePicker.SelectedDate!=null)
                {
                    betline.Filter.FromEventDate = _fromEventDatePicker.SelectedDate;                    
                }
                else
                {
                    betline.Filter.FromEventDate = null;
                }
                if (_toEventsDatePicker.SelectedDate!=null)
                {
                    betline.Filter.ToEventDate = _toEventsDatePicker.SelectedDate;                    
                }
                else
                {
                    betline.Filter.ToEventDate = null;
                }

                betline.Filter.IsInPlay = (bool)_InPlayOnlyCheckbox.IsChecked;
                return true;
            }
            catch (Exception e)
            {

                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", e.Message, e.Source, e.StackTrace, e.InnerException.Message));
                MessageBox.Show("Primary Filter Parsing Error! Please fill correctly Prymari filter data and Try again!");
                return false;
            }
            
        }

        public bool FillSecondaryFilterSuccessfuly(BettingLine betline)
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
                                betline.Filter.ExchangeId = 1;
                                break;
                            case "Australian":
                                betline.Filter.ExchangeId = 2;
                                break;
                            default:
                                betline.Filter.ExchangeId = null;
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
                                betline.Filter.IsActive = true;
                                break;
                            case "Non Active":
                                betline.Filter.IsActive = false;
                                break;
                            default:
                                betline.Filter.IsActive = null;
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
                                betline.Filter.IsBSP = true;
                                break;
                            case "Non BSP":
                                betline.Filter.IsBSP = false;
                                break;
                            default:
                                betline.Filter.IsBSP = null;
                                break;
                        }
                    }
                }
                
                List<string> marketTypesList = new List<string>();
                foreach (CheckBox cb in _MarketTypeFilter.Children)
                {
                    if ((bool)cb.IsChecked)
                    {
                        marketTypesList.Add(cb.Content.ToString()[0].ToString());
                    }
                }
                betline.Filter.MarketTypesArray = marketTypesList.ToArray();

                List<string> marketNamesList = new List<string>();
                foreach (string mName in _marketName.SelectedItems)
                {
                    marketNamesList.Add(mName);
                }
                betline.Filter.MarketNamesArray = marketNamesList.ToArray();
                if (_runnersMin.Text!="")
                {
                    betline.Filter.RunnersMin = int.Parse(_runnersMin.Text);                    
                }
                if (_runnersMax.Text!="")
                {
                    betline.Filter.RunnersMax = int.Parse(_runnersMax.Text);                    
                }
                if (_winnersMin.Text!="")
                {
                    betline.Filter.WinnersMin = int.Parse(_winnersMin.Text);                    
                }
                if (_winnersMax.Text!="")
                {
                    betline.Filter.WinnersMax = int.Parse(_winnersMax.Text);                    
                }

                return true;
	        }
	        catch (Exception e)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", e.Message, e.Source, e.StackTrace, e.InnerException.Message));
                MessageBox.Show("Secondary Filter Parsing Error! Please fill correctly filter data and Try again!");
                return false;
	        }
        }

        public bool FillBetlineParameters(BettingLine betline)
        {
            try
            {
                var ppbet = double.Parse(_profitPerBetTB.Text);
                betline.ProfitPerBudjet = double.Parse(_profitPerBudjetTB.Text);
                betline.InitialProfitPerBet = ppbet;
                betline.ProfitPerBet = ppbet;
                betline.StakeRangeMin = double.Parse(_StakeRangeMin.Text);
                betline.StakeRangeMax = double.Parse(_StakeRangeMax.Text);

                if (_totalAmountMatchedMin.Text!="")
                {
                    betline.Filter.TotalAmountMatchedMin = double.Parse(_totalAmountMatchedMin.Text);                    
                }
                if (_totalAmountMatchedMax.Text!="")
                {
                    betline.Filter.TotalAmountMatchedMax = double.Parse(_totalAmountMatchedMax.Text);                                     
                }                
                try
                {
                    Session.Betline.Filter.MaxAmountIncrementedPerBudjet = double.Parse(_maxAmountPercentTB.Text);
                    Session.Betline.Filter.MaxAmountIncremented = double.Parse(_maxAmountTB.Text);
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", e.Message, e.Source, e.StackTrace, e.InnerException.Message));
                    MessageBox.Show("Wrong Max Bets Count entered. Please enter number From 1 to 16!");
                    return false;
                }
                string maxBets = (_maxBetsCountStack.Children[1] as TextBox).Text;
                if (!String.IsNullOrEmpty(maxBets))
                {
                    try
                    {
                        Session.Betline.Filter.MaxBetsCount = byte.Parse(maxBets);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", e.Message, e.Source, e.StackTrace, e.InnerException.Message));
                        MessageBox.Show("Wrong Max Amount entered try again!");
                        return false;
                    }
                }
                string marketN= "";
                try
                {
                    marketN = string.Format("{0} - {1} -> {2} - {3} - {4}", _BetType.Text, _StakeRangeMin.Text, _StakeRangeMax.Text, betline.Filter.MarketNamesArray[0],_algo.Text);
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", e.Message, e.Source, e.StackTrace, e.InnerException.Message));
                }

                betline.BetlineName = string.IsNullOrWhiteSpace(betlineName.Text) ? marketN : betlineName.Text;
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", e.Message, e.Source, e.StackTrace, e.InnerException.Message));
                MessageBox.Show("Please fill BetLine Parameters, and try again.");
                return false;
            }
        }
        //filling betline with eventTypes              
        //Binding EventTypesList
        
        //??This will be filled with Tasks
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
            List<CountryCode> countryCodesISO3 = new List<CountryCode>(){new CountryCode("AFG","Afghanistan"),new CountryCode("AGO","Angola"), new CountryCode("AIA","Anguilla"),new CountryCode("ALA","Åland Islands"),new CountryCode("ALB","Albania"), new CountryCode("AND","Andorra"), new CountryCode("ARE" , "United Arab Emirates"), new CountryCode("ARG" , "Argentina"), new CountryCode("ARM" , "Armenia"), new CountryCode("ASM" , "American Samoa"), new CountryCode("ATA" , "Antarctica"), new CountryCode("AUS" , "Australia"), new CountryCode("AUT" , "Austria"), new CountryCode("BEL" , "Belgium"), new CountryCode("BGR" , "Bulgaria"), new CountryCode("BLR" , "Belarus"), new CountryCode("BRA" , "Brazil"), new CountryCode("CAN" , "Canada"), new CountryCode("CHE" , "Switzerland"), new CountryCode("CHL" , "Chile"), new CountryCode("CHN" , "China"), new CountryCode("CMR" , "Cameroon"), new CountryCode("COL" , "Colombia"), new CountryCode("CZE" , "Czech Republic"), new CountryCode("CYP" , "Cyprus"), new CountryCode("DEU" , "Germany"), new CountryCode("DNK" , "Denmark"), new CountryCode("ESP" , "Spain"), new CountryCode("EST" , "Estonia"), new CountryCode("FRA" , "France"), new CountryCode("GBR" , "United Kingdom"), new CountryCode("GEO" , "Georgia"), new CountryCode("GRC" , "Greece"), new CountryCode("HUN" , "Hungary"), new CountryCode("ITA" , "Italy"), new CountryCode("JPN" , "Japan"), new CountryCode("ISL" , "Iceland"), new CountryCode("MEX" , "Mexico"), new CountryCode("MNE" , "Montenegro"), new CountryCode("NOR" , "Norway"), new CountryCode("PRT" , "Portugal"), new CountryCode("ROU" , "Romania"), new CountryCode("RUS" , "Russia"), new CountryCode("SRB" , "Serbia"), new CountryCode("SVK" , "Slovakia"), new CountryCode("SVN" , "Slovenia"), new CountryCode("SWE" , "Sweden"), new CountryCode("SWZ" , "Swaziland"), new CountryCode("TUR" , "Turkey"), new CountryCode("UKR" , "Ukraine"), new CountryCode("USA" , "United States")}; 
            _countryCodes.DataContext = countryCodesISO3;
        }               
        
        //This is only for test
        void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //BindFunds();
        }

        //?? Implement to DataGrid        
        internal static void BindBetlinesGrid()
        {
            try
            {                
                DBLBettingApp.BFDBLEntities context = new DBLBettingApp.BFDBLEntities();
                var window = WpfBetApplication.App.Current.MainWindow as WpfBetApplication.MainWindow;
                int index= window._RecentBetlinesCombo.SelectedIndex;
                window.DataContext = context.Betlines.ToList();
                window._RecentBetlinesCombo.SelectedIndex = index < 0 ? 0 : index;
                WpfBetApplication.App.Current.MainWindow.SizeToContent = SizeToContent.WidthAndHeight;
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", e.Message, e.Source, e.StackTrace, e.InnerException.Message));
            }
        }
        //?? not working
        private void PauseBetline(object sender, RoutedEventArgs e)
        {
            try
            {
                DBLBettingApp.BFDBLEntities context = new DBLBettingApp.BFDBLEntities();
                DBLBettingApp.Betline gridBetline = (sender as FrameworkElement).DataContext as DBLBettingApp.Betline;
                DBLBettingApp.Betline contextBetline = context.Betlines.Find(gridBetline.BetlineId);
                contextBetline.isActive = false;
                contextBetline.lastBetLost = false;
                context.SaveChanges();
                BindBetlinesGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                restartTimer_Tick(App.Current, new EventArgs());
            }            
        }

        private void StopOnWinBetline(object sender, RoutedEventArgs e)
        {
            try
            {
                DBLBettingApp.BFDBLEntities context = new DBLBettingApp.BFDBLEntities();
                DBLBettingApp.Betline gridBetline = (sender as FrameworkElement).DataContext as DBLBettingApp.Betline;
                DBLBettingApp.Betline contextBetline = context.Betlines.Find(gridBetline.BetlineId);
                contextBetline.isActive = false;
                context.SaveChanges();
                BindBetlinesGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                restartTimer_Tick(App.Current, new EventArgs());
            }
        }

        private void ResumeBetline(object sender, RoutedEventArgs e)
        {
            try
            {                
                DBLBettingApp.BFDBLEntities context = new DBLBettingApp.BFDBLEntities();
                DBLBettingApp.Betline gridBetline = (sender as FrameworkElement).DataContext as DBLBettingApp.Betline;
                DBLBettingApp.Betline contextBetline = context.Betlines.Find(gridBetline.BetlineId);
                contextBetline.isActive = true;
                context.SaveChanges();
                BindBetlinesGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                restartTimer_Tick(App.Current, new EventArgs());
            }
        }


        private void Restart(object sender, RoutedEventArgs e)
        {
            try
            {                
                DBLBettingApp.BFDBLEntities context = new DBLBettingApp.BFDBLEntities();
                DBLBettingApp.Betline gridBetline = (sender as FrameworkElement).DataContext as DBLBettingApp.Betline;
                DBLBettingApp.Betline contextBetline = context.Betlines.Find(gridBetline.BetlineId);
                contextBetline.isActive = true;
                contextBetline.lastPlacedBetId = "";
                contextBetline.profitPerBet = contextBetline.initialProfitPerBet;
                contextBetline.budjet = 0;
                context.SaveChanges();
                BindBetlinesGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                restartTimer_Tick(App.Current, new EventArgs());
            }
        }

        private void DeleteBetline(object sender, RoutedEventArgs e)
        {
            DBLBettingApp.Betline gridBetline = (sender as FrameworkElement).DataContext as DBLBettingApp.Betline;
            if (gridBetline != null)
            {
                DBLBettingApp.BFDBLEntities context = new DBLBettingApp.BFDBLEntities();
                try
                {
                    DBLBettingApp.Betline contextBetline = context.Betlines.Find(gridBetline.BetlineId);
                    DBLBettingApp.Filter contextFilter = context.Filters.Find(gridBetline.Filter.FilterId);
                    var placedBetsOfBetline = from p in context.PlacedBets
                                              where p.betlineId == gridBetline.BetlineId
                                              select p;
                    foreach (var item in placedBetsOfBetline)
                    {
                        item.Betline = null;
                    }
                    context.Filters.Remove(contextFilter);
                    context.Betlines.Remove(contextBetline);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                    restartTimer_Tick(App.Current, new EventArgs());                   
                }                
            }            
            BindBetlinesGrid();
        }

        private void _RunningBetLines_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            TextBlock block;
            // iteratively traverse the visual tree
            while ((dep != null) && !(dep is TextBlock))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            block = dep as TextBlock;
            while ((dep != null) && !(dep is DataGridCell))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep == null)
                return;

            if (dep is DataGridCell)
            {
                DataGridCell cell = dep as DataGridCell;
                //TextBlock block = (cell.Content as TextBlock);
                //TextBox blockBox = (cell.Content as TextBox);
                List<string> parsePlaced = new List<string>();
                try
                {
                    DBLBettingApp.BFDBLEntities context = new DBLBettingApp.BFDBLEntities();
                    DBLBettingApp.Betline selectedBetline = (sender as DataGrid).SelectedItem as DBLBettingApp.Betline;
                    parsePlaced = block.Text.Trim(':').Split(':').ToList();
                    if (cell.Column.Header.ToString() == "Last Bets" & !FilterMarkets.StringArrayIsEmpty(parsePlaced.ToArray()))
                    {
                        Window placedBets = new WpfBetApplication.PlacedBet();
                        var items = selectedBetline.lastPlacedBetId.Trim(':').Split(':').ToList();
                        if (items.Count < 1)
                        {
                            return;
                        }
                        WrapPanel mainPanel = new WrapPanel();
                        foreach (string id in items)
                        {
                            try
                            {
                                DataGrid grid = new DataGrid();
                                grid.Margin = new Thickness(5);
                                grid.IsReadOnly = true;
                                grid.MaxHeight = 500;
                                var columnName = new DataGridTextColumn();
                                columnName.Binding = new Binding("property");
                                var columnValue = new DataGridTextColumn();
                                columnValue.Binding = new Binding("value");
                                columnName.Header = "Name";
                                columnValue.Header = "Value";
                                columnValue.MaxWidth = 120;
                                grid.Columns.Add(columnName);
                                grid.Columns.Add(columnValue);
                                var selconv = new SelectionConverter();
                                DBLBettingApp.PlacedBet lastPlacedBet = context.PlacedBets.Find(long.Parse(id));
                                if (lastPlacedBet != null)
                                {
                                    grid.Items.Add(new PlacedBetStruct { property = "Bet ID", value = id });
                                    grid.Items.Add(new PlacedBetStruct { property = "BLine ID", value = lastPlacedBet.betlineId.ToString() });
                                    grid.Items.Add(new PlacedBetStruct { property = "AVG Price", value = lastPlacedBet.averagePrice.ToString() });
                                    grid.Items.Add(new PlacedBetStruct { property = "M Size", value = lastPlacedBet.sizeMatched.ToString() });
                                    grid.Items.Add(new PlacedBetStruct { property = "Market", value = lastPlacedBet.marketMenuPath.Split('\\').ToList().Last().Replace(" v ", " v" + Environment.NewLine) });
                                    grid.Items.Add(new PlacedBetStruct { property = "Selected", value = selconv.Convert(lastPlacedBet, "".GetType(),null,System.Globalization.CultureInfo.CurrentCulture).ToString() });
                                    grid.Items.Add(new PlacedBetStruct { property = "Status", value = lastPlacedBet.resultCode });
                                    grid.Items.Add(new PlacedBetStruct { property = "Score", value = lastPlacedBet.score });
                                }
                                mainPanel.Children.Add(grid);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                            }
                        }
                        placedBets.Content = mainPanel;
                        placedBets.Show();
                    }
                    else
                    {

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                }
            }
            e.Handled = false;            
        }

        public static void UpdateLiveScores()
        {
            try
            {                
                XDocument liveScores = XDocument.Load("http://wap.7m.cn/en/quickLive.aspx?t=0&mt=");
                XDocument resultScores = XDocument.Load("http://wap.7m.cn/en/quickLive.aspx?t=2&mt=");
                //Session.soccerLivescore = liveScores.ToString() + resultScores.ToString();
                DateTime hoursBefore2 = DateTime.Now.AddHours(-2);
                DBLBettingApp.BFDBLEntities context = new DBLBettingApp.BFDBLEntities();
                var notSettledOldBets = from bets in context.PlacedBets
                                        where bets.resultCode != "FT" & bets.datePlaced < hoursBefore2
                                        select bets;
                foreach (var bet in notSettledOldBets)
                {
                    context.PlacedBets.Find(bet.betId).resultCode = "FT";
                }
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
            }            
        }

               
        public void _marketName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChangedDialogWindowShow(e);
        }

        public static bool SelectionChangedDialogWindowShow(SelectionChangedEventArgs e)
        {
            try
            {
                Window dialogWindow = new DialogWindow();
                dialogWindow.Owner = App.Current.MainWindow;
                string marketName = "";
                if (e.AddedItems.Count > 0)
                {
                    marketName = e.AddedItems[0].ToString();
                }
                else
                {
                    return false;
                }
                Label labelMarket = dialogWindow.FindName("_TitleLabel") as Label;
                labelMarket.Content = marketName;
                StackPanel checkBoxStackPanel = dialogWindow.FindName("_CheckBoxStackPanel") as StackPanel;
                switch (marketName)
                {
                    case "Match Odds":
                        checkBoxStackPanel.Orientation = Orientation.Horizontal;
                        string[] arrayMatchOdds = { "1", "2", "X" };
                        for (int i = 0; i < arrayMatchOdds.Length; i++)
                        {
                            CheckBox cb = new CheckBox();
                            cb.IsChecked = true;
                            cb.Content = arrayMatchOdds[i];
                            cb.FontSize = 15;
                            cb.Margin = new Thickness(15);
                            checkBoxStackPanel.Children.Add(cb);
                        }
                        break;
                    case "Correct Score":
                        checkBoxStackPanel.Orientation = Orientation.Vertical;
                        string[] arrayCorrectScore = { "0-0", "0-1", "0-2", "0-3", "1-0", "1-1", "1-2", "1-3", "2-0", "2-1", "2-2", "2-3", "3-0", "3-1", "3-2", "3-3", "Any Unquoted" };
                        for (int i = 0; i < arrayCorrectScore.Length; i++)
                        {
                            CheckBox cb = new CheckBox();
                            cb.IsChecked = true;
                            cb.Content = arrayCorrectScore[i];
                            cb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                            cb.Margin = new Thickness(5);
                            cb.FontSize = 15;
                            checkBoxStackPanel.Children.Add(cb);
                        }

                        break;
                    default:
                        return false;
                }
                return (bool)dialogWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                return false;
            }
            
        }

        public void _algo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0].ToString().Contains("Incrementing"))
            {
                _maxAmountStack.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                _maxAmountStack.Visibility = System.Windows.Visibility.Collapsed;
            }
            if (e.AddedItems[0].ToString().Contains("AllRecentlyFound"))
            {
                _maxBetsCountStack.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                _maxBetsCountStack.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void StartGenerationButton_Click(object sender, RoutedEventArgs e)
        {
            StartupActiveMethodsFmDatabase();
            Session.Running = true;
            _startButton.IsEnabled = false;
            _stopButton.IsEnabled = true;
            _resetDatabaseButton.IsEnabled = false;
        }

        private void StopGenerationButton_Click(object sender, RoutedEventArgs e)
        {
            Session.Running = false;            

        }


        private void ResetDatabase(object sender, RoutedEventArgs e)
        {
            DBLBettingApp.BFDBLEntities context = new DBLBettingApp.BFDBLEntities();
            foreach (var item in context.Betlines)
            {
                item.lastPlacedBetId = "";
                item.profitPerBet = item.initialProfitPerBet;
                item.budjet = 0;
                context.SaveChanges();
            }
            context.Database.ExecuteSqlCommand("DELETE from PlacedBets");
            context.SaveChanges();
            FromDateBind();
        }

        private void ChangeFromDate(object sender, RoutedEventArgs e)
        {
            try
            {
                Session.DateTimeNow = DateTime.Parse(_fromDatePicker.Text, System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat);
                BindBetlinesGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                Session.DateTimeNow = DateTime.Now;
            }
        }

        private void DefaultButton(object sender, RoutedEventArgs e)
        {
            if (this.Visibility == System.Windows.Visibility.Hidden)
            {
                this.Visibility = System.Windows.Visibility.Visible;                
            }
            else
            {
                this.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void _showAllCB_Click(object sender, RoutedEventArgs e)
        {
            BindBetlinesGrid();
        }

        public void Update_PPB_MAPB_Click(object sender, RoutedEventArgs e)
        {
            DateTime lastDate = Session.DateTimeNow;
            try
            {
                UpdateFunds();
                lastDate = (DateTime)TryFindResource("LastBudjetUpdateTime");
            }
            catch (NullReferenceException)
            {
                Resources.Add("LastBudjetUpdateTime",lastDate);
                UpdateBetlinesIPPB();
            }
            if (lastDate < Session.DateTimeNow.AddDays(-1))
            {
                Resources["LastBudjetUpdateTime"] = Session.DateTimeNow;
                UpdateBetlinesIPPB();
            }
        }

        public void UpdateFunds()
        {
            this._Funds.Content = decimal.Parse(_InitialFunds.Content.ToString());
            DBLBettingApp.BFDBLEntities context = new BFDBLEntities();
            decimal sumBudjets = 0;
            foreach (var item in context.Betlines)
            {
                sumBudjets += (decimal)item.budjet;
            }
            this._Funds.Content = Math.Round(decimal.Parse(this._Funds.Content.ToString()) + sumBudjets, 2);
        }



        public void UpdateBetlinesIPPB()
        {
            try
            {
                double funds = double.Parse(_Funds.Content.ToString());
                foreach (var item in Session.databaseContext.Betlines)
                {
                    try
                    {
                        funds = ReduceBallance(funds);
                        var curBetline = Session.databaseContext.Betlines.Find(item.BetlineId);
                        curBetline.initialProfitPerBet = funds * curBetline.profitPerBudjet/100;
                        curBetline.Filter.maxAmmountIncremented = item.Filter.maxAmmountIncrementedPerBudjet * funds/100;
                        Session.databaseContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", e.Message, e.Source, e.StackTrace, e.InnerException.Message));
            }
        }

        public static double ReduceBallance(double ballance)
        {
            try
            {
                var usersBetlinesCount = (from b in Session.databaseContext.Betlines
                                          where b.isActive
                                          select b).Count();
                for (int i = 1; i < usersBetlinesCount; i++)
                {
                    ballance *= 0.8;
                }
                return ballance > 0 ? ballance : 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                return ballance > 0 ? ballance : 0;
            }

        }

        private void _RunningBetLines_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            TextBlock block;
            // iteratively traverse the visual tree
            while ((dep != null) && !(dep is TextBlock))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            block = dep as TextBlock;
            while ((dep != null) && !(dep is DataGridCell))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep == null)
                return;

            if (dep is DataGridCell)
            {
                DataGridCell cell = dep as DataGridCell;
                //TextBlock block = (cell.Content as TextBlock);
                //TextBox blockBox = (cell.Content as TextBox);
                List<string> parsePlaced = new List<string>();
                try
                {
                    DBLBettingApp.BFDBLEntities context = new DBLBettingApp.BFDBLEntities();
                    DBLBettingApp.Betline selectedBetline = (sender as DataGrid).SelectedItem as DBLBettingApp.Betline;
                    parsePlaced = block.Text.Trim(':').Split(':').ToList();
                    if (cell.Column.Header.ToString() == "ID" & !FilterMarkets.StringArrayIsEmpty(parsePlaced.ToArray()))
                    {
                        Window placedBetsWindow = new WpfBetApplication.PlacedBet();
                        int betLineIdFmCell;
                        try
                        {
                            betLineIdFmCell = ((sender as DataGrid).SelectedItem as DBLBettingApp.Betline).BetlineId;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                            _RunningBetLines.SelectedIndex = -1;
                            return;
                        }
                        DateTime yesterday = Session.DateTimeNow.AddDays(-1);

                        var placedBetsLinq = from w in context.PlacedBets
                                             where w.betlineId == betLineIdFmCell & w.datePlaced > yesterday
                                             orderby w.datePlaced descending
                                             select new { w.betId, w.averagePrice, w.sizeMatched, w.marketMenuPath, w.Selection, w.datePlaced, w.dateSettled, w.resultCode, w.score, w.success };
                        DataGrid lastPlacedBetsGrid = new DataGrid();
                        if (placedBetsLinq != null)
                        {
                            lastPlacedBetsGrid.ItemsSource = placedBetsLinq.ToList();
                            lastPlacedBetsGrid.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                            lastPlacedBetsGrid.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                            StackPanel mainPanel = new StackPanel();
                            mainPanel.Orientation = Orientation.Horizontal;
                            mainPanel.Children.Add(lastPlacedBetsGrid);
                            placedBetsWindow.SizeToContent = System.Windows.SizeToContent.Width;
                            placedBetsWindow.Content = mainPanel;
                            placedBetsWindow.Owner = this;
                            placedBetsWindow.WindowState = System.Windows.WindowState.Maximized;
                            placedBetsWindow.Show();
                        }
                        else
                        {

                        }
                    }
                    else
                    {                        
                        EditBetlineWindow editBetlineWind = new EditBetlineWindow();
                        editBetlineWind.Owner = this;
                        editBetlineWind.DataContext = context.Betlines.Find(selectedBetline.BetlineId);
                        var contextBetline = (editBetlineWind.DataContext as DBLBettingApp.Betline);
                        if (_events.Items.Count > 0)
                        {
                            editBetlineWind._eventsEdit.ItemsSource = _events.Items;
                        }
                        else
                        {
                            /// Implement GetEvents;
                        }
                        foreach (var item in contextBetline.Filter.eventTypes.Trim(':').Split(':'))
                        {
                            int itemInt = int.Parse(item);
                            var el = (from et in editBetlineWind._eventsEdit.Items.Cast<EventType>()
                                      where et.id == itemInt
                                      select et).FirstOrDefault();
                            editBetlineWind._eventsEdit.SelectedItems.Add(el);
                        }
                        editBetlineWind._countryCodes.ItemsSource = _countryCodes.Items;
                        foreach (var item in contextBetline.Filter.countryCodes.Trim(':').Split(':'))
                        {
                            var el = (from et in editBetlineWind._countryCodes.Items.Cast<CountryCode>()
                                      where et.CountryCode1 == item
                                      select et).FirstOrDefault();
                            editBetlineWind._countryCodes.SelectedItems.Add(el);
                        }
                        if (_marketName.Items.Count > 0)
                        {
                            editBetlineWind._marketName.ItemsSource = _marketName.Items;
                        }
                        else
                        {
                            //Implement GetMarkets
                            //Session.Betline = new BettingLine();
                            //editBetlineWind._marketName.ItemsSource = Session.Betline.GetMarketsRequestFiltered();
                        }
                        foreach (string item in contextBetline.Filter.marketNames.Trim(':').Split(':'))
                        {
                            editBetlineWind._marketName.SelectedItems.Add(item);
                        }
                        editBetlineWind._algo.ItemsSource = _algo.Items;
                        if ((bool)editBetlineWind.ShowDialog())
                        {
                            context.SaveChanges();
                            BindBetlinesGrid();
                        }
                        else
                        {
                            MessageBox.Show("Error has occured. Please try Again.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("msg:{0};\\nsource:{1}\\nstack:{2}\\ninnerEx:{3}", ex.Message, ex.Source, ex.StackTrace, ex.InnerException.Message));
                }
            }
            e.Handled = true;
        }       
    }
}
