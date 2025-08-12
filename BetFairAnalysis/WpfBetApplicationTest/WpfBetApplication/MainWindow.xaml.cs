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
using WpfBetApplication.WebExchangeService;
using WpfBetApplication.WebGlobalService;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Net;
using System.Xml.Linq;
using System.Net.Mail;
using mshtml;
using HtmlAgilityPack;



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
            if (!CheckConnection())
            {
                System.Threading.Thread.Sleep(new TimeSpan(0, 0, 10));
                restartTimer_Tick(this, new EventArgs());
            }
            try
            {
                InitializeComponent();
                LivescoreUpdateTimerInitiate();
                CheckConnectionTimerInit();
                DailyReportTimerInit();
                RestartAppTimerInit();
                Session.globalService.logoutCompleted += service_logoutCompleted;
                Session.globalService.getAllEventTypesCompleted += globalService_getAllEventTypesCompleted;
                Session.exchangeService.getAccountFundsCompleted += exchangeService_getAccountFundsCompleted;
                Session.globalService.viewProfileCompleted += globalService_viewProfileCompleted;
                Button_Click_Login(this, null);
                // loads all the active betlines from database and runs them
                BindCountryCodeList();
                BindAlgoComboBox();
                BindEmailTextBox();
                //GetResultByWebBrowser("http://www.footballgoalvideos.net/football/live#");
            }
            catch (Exception)
            {
                System.Threading.Thread.Sleep(new TimeSpan(0, 0, 10));
                restartTimer_Tick(this, new EventArgs());
            }            
            this.WindowState = System.Windows.WindowState.Minimized;           
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

        public HtmlDocument DocumentGetter(string URL)
        {
            try
            {                
                WebClient wc = new WebClient();
                string html = wc.DownloadString(URL);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                return doc;
            }
            catch (Exception)
            {
                return new HtmlDocument();
            }
        }

        public void UpdateScores()
        {
            var d = DocumentGetter("http://www.betfair.com/exchange/inplay");
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
                        if (item.GetAttributeValue("class","error").Contains("result") & score == "")
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
                    if (newParts.Split().Last().Contains("FT") | newParts.Split().Last().Contains("HT") | newParts.Split().Last().Contains("'"))
                    {
                        sb.AppendLine(newParts);
                    }
                }
                catch (Exception)
                {
                }
            }
            Session.soccerLivescore = sb.ToString();
        }


        private void BindEmailTextBox()
        {
            try
            {
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                _email.Text = context.Emails.ToList()[0].email1;
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        internal static void CheckConnectionTimerInit()
        {
            DispatcherTimer checkConnectionTimer = new DispatcherTimer();
            checkConnectionTimer.Interval = TimeSpan.FromSeconds(40);
            checkConnectionTimer.Start();
            checkConnectionTimer.Tick += checkConnectionTimer_Tick;
        }

        static void checkConnectionTimer_Tick(object sender, EventArgs e)
        {
            if (!CheckConnection())
            {
                restartTimer_Tick(App.Current, new EventArgs());
            }
            MainWindow.UpdateLiveScores();            
        }

        internal static void RestartAppTimerInit()
        {
            DispatcherTimer restartTimer = new DispatcherTimer();
            restartTimer.Interval = TimeSpan.FromHours(6);
            restartTimer.Start();
            restartTimer.Tick += restartTimer_Tick;
        }

        private void DailyReportTimerInit()
        {
            DispatcherTimer everyHourTimer = new DispatcherTimer();
            everyHourTimer.Interval = TimeSpan.FromHours(1);
            everyHourTimer.Start();
            everyHourTimer.Tick += everyHourTimer_Tick;
            everyHourTimer_Tick(everyHourTimer, new EventArgs());
        }

        void everyHourTimer_Tick(object sender, EventArgs e)
        {
            try
            {                
                if (DateTime.Now.Hour == 6)
                {
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                    {
                        MailMessage mail = new MailMessage("emailsender8479@yahoo.com", "danailveselinov@yahoo.com", "BFDatabase", "This is Weekly Database File Attachment");
                        System.Net.NetworkCredential auth = new System.Net.NetworkCredential();
                        SmtpClient client = new SmtpClient("smtp.mail.yahoo.com", 587);
                        mail.Priority = MailPriority.High;
                        client.EnableSsl = true;
                        client.UseDefaultCredentials = false;
                        mail.IsBodyHtml = false;
                        string adress = "BFDBL.sdf";
                        mail.Attachments.Add(new Attachment(adress));
                        client.Credentials = new NetworkCredential("emailsender8479@yahoo.com", "Eddy8479");
                        try
                        {
                            client.Send(mail);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    //System.Diagnostics.Process.Start("shutdown.exe", "-r -f -t 0");
                    DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                    DateTime yesterday = DateTime.Now.AddDays(-1);
                    var placedBets = from b in context.PlacedBets
                                     where b.datePlaced > yesterday & b.dateSettled != null
                                     orderby b.betlineId, b.datePlaced, b.dateSettled
                                     select b;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Betline -" + "Price -" + "Size -" + "TimePlaced -" + "TimeSettled -" + "Selection -" + "Result -" + "Won");
                    foreach (var item in placedBets)
                    {
                        try
                        {
                            sb.AppendLine(item.betlineId + "\t" + item.averagePrice + "\t" + item.sizeMatched + "\t" + item.datePlaced.ToShortTimeString() + "\t" + item.dateSettled.Value.ToShortTimeString() + "\t" + item.Selection + "\t" + item.score + "\t" + item.success);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    string message = sb.ToString();
                    foreach (var item in context.Emails)
                    {
                        MailMessage mail = new MailMessage("emailsender8479@yahoo.com", item.email1, "BFReport", message);
                        System.Net.NetworkCredential auth = new System.Net.NetworkCredential();
                        SmtpClient client = new SmtpClient("smtp.mail.yahoo.com", 587);
                        mail.Priority = MailPriority.High;
                        client.EnableSsl = true;
                        client.UseDefaultCredentials = false;
                        mail.IsBodyHtml = false;
                        client.Credentials = new NetworkCredential("emailsender8479@yahoo.com", "Eddy8479");
                        try
                        {
                            client.Send(mail);
                        }
                        catch (Exception)
                        {
                        }
                    }                
                }
            }
            catch (Exception)
            {
            }
        }

        public static void restartTimer_Tick(object sender, EventArgs e)
        {
            //try
            //{
            //    App.Current.Shutdown();
            //}
            //catch (Exception)
            //{
            //    return;
            //}
            //System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
        }

        
        public static bool CheckConnection()
        {
            WebClient client = new WebClient();
            try
            {
                using (client.OpenRead("http://www.betfair.com/exchange/inplay/"))
                {
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        void globalService_viewProfileCompleted(object sender, viewProfileCompletedEventArgs e)
        {
            try
            {
                if (e.Cancelled)
                {

                }
                else
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("mr. ");
                        sb.Append(e.Result.surname);
                        sb.Append(" " + e.Result.firstName[0] + ".");
                        _userFullName.Content = sb.ToString();
                        _currency.Content = e.Result.currency;
                    }
                    catch (Exception)
                    {
                        //restartCompTimer_Tick(this, new EventArgs());
                    }
                }
            }
            catch (Exception)
            {
                restartTimer_Tick(this, new EventArgs());
            }
        }

        void exchangeService_getAccountFundsCompleted(object sender, getAccountFundsCompletedEventArgs e)
        {
            try
            {
                if (e.Result.errorCode == GetAccountFundsErrorEnum.OK)
                {
                    try
                    {
                        this._Funds.Content = e.Result.availBalance;
                    }
                    catch (System.Reflection.TargetInvocationException)
                    {
                    }
                }
            }
            catch (Exception)
            {
                restartTimer_Tick(this, new EventArgs());
            }                        
        }        

        
        //This method controls all server calls frequency
        public static void Button_Click_Login()
        {
            try
            {
                LoginReq logReq = new LoginReq();
                logReq.username = (WpfBetApplication.App.Current.MainWindow.FindName("_username") as TextBox).Text;
                logReq.password = (WpfBetApplication.App.Current.MainWindow.FindName("_password") as PasswordBox).Password;
                logReq.productId = 82; //????
                WpfBetApplication.WebGlobalService.LoginResp loginResp = Session.globalService.login(logReq);
                if (loginResp.errorCode == LoginErrorEnum.OK)
                {
                    Session.checkHeader(loginResp.header);
                    Session.BindEventsList();
                    (WpfBetApplication.App.Current.MainWindow.FindName("_LoginPanel") as StackPanel).Visibility = System.Windows.Visibility.Hidden;
                    (WpfBetApplication.App.Current.MainWindow.FindName("_LoggedinPanel") as StackPanel).Visibility = System.Windows.Visibility.Visible;
                    BindFunds();
                    ViewProfileReq profileReq = new ViewProfileReq();
                    profileReq.header = new WebGlobalService.APIRequestHeader();
                    profileReq.header.sessionToken = Session.globalHeader.sessionToken;
                    Session.globalService.viewProfileAsync(profileReq);
                }
                else
                {
                    //System.Threading.Thread.Sleep(10000);
                    //Button_Click_Login();
                }
            }
            catch (Exception b)
            {
                System.Threading.Thread.Sleep(10000);
                WpfBetApplication.MainWindow.restartTimer_Tick(b, new EventArgs());
            }
            //catch
            //System.Reflection.TargetInvocationException
            //System.Net.WebException
        }
                     
        internal void Button_Click_Login(object sender, RoutedEventArgs e)
        {
            try
            {            
                LoginReq logReq = new LoginReq();
                logReq.username = _username.Text;
                logReq.password = _password.Password;
                logReq.productId = 82; //????
                WpfBetApplication.WebGlobalService.LoginResp loginResp = Session.globalService.login(logReq);
                if (loginResp.errorCode == LoginErrorEnum.OK)
                {
                    Session.checkHeader(loginResp.header);
                    Session.BindEventsList();
                    _LoginPanel.Visibility = System.Windows.Visibility.Hidden;
                    _LoggedinPanel.Visibility = System.Windows.Visibility.Visible;
                    BindFunds();
                    ViewProfileReq profileReq = new ViewProfileReq();
                    profileReq.header = new WebGlobalService.APIRequestHeader();
                    profileReq.header.sessionToken = Session.globalHeader.sessionToken;
                    Session.globalService.viewProfileAsync(profileReq);
                    StartupActiveMethodsFmDatabase();
                    BindBetlinesGrid();
                }
                else
                {
                    System.Threading.Thread.Sleep(2000);
                    Button_Click_Login(this, new RoutedEventArgs());
                }
            }
            catch (Exception b)
            {
                System.Threading.Thread.Sleep(10000);
                WpfBetApplication.MainWindow.restartTimer_Tick(this, new EventArgs());
            }
            //catch
            //System.Reflection.TargetInvocationException
            //System.Net.WebException
        }

        public static void BindFunds()
        {
            GetAccountFundsReq fundsReq = new GetAccountFundsReq();
            fundsReq.header = new WebExchangeService.APIRequestHeader();
            fundsReq.header.sessionToken = Session.globalHeader.sessionToken;
            Session.exchangeService.getAccountFundsAsync(fundsReq , Guid.NewGuid().ToString());
        }
        private void Button_Click_LogOut(object sender, RoutedEventArgs e)
        {
            LogoutReq logoutReq = new LogoutReq();
            logoutReq.header = Session.globalHeader;
            Session.globalService.logoutAsync(logoutReq, Guid.NewGuid().ToString());
            //????maybe stop all running betlines
            
        }

        void service_logoutCompleted(object sender, logoutCompletedEventArgs e)
        {
            if (e.Result.errorCode != LogoutErrorEnum.OK)
            {
                if (e.Error != null)
                    {
                        MessageBox.Show(string.Format("Error occured during operation: {0}", e.Error.Message));            
                    }
                }
            else
            {
                if (e.Cancelled == false)
                {
                    Session.checkHeader(e.Result.header);
                    _LoggedinPanel.Visibility = System.Windows.Visibility.Hidden;
                    _LoginPanel.Visibility = System.Windows.Visibility.Visible;
                }
            }
            restartTimer_Tick(this, new EventArgs());
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
                        Session.Betline.AlgorithmName = "Incrementing StakeTest";
                        IncrementingStakeByOddBetline();
                        BindBetlinesGrid();
                        _SecondaryFilterTabItem.IsEnabled = false;
                        _BetlineTabItem.IsEnabled = false;
                        Session.Betline = new BettingLine();
                        break;
                    case 2:
                        Session.Betline.AlgorithmName = "BetOnAllRecentlyFoundTest";
                        BetOnAllRecentlyFound();
                        BindBetlinesGrid();
                        _SecondaryFilterTabItem.IsEnabled = false;
                        _BetlineTabItem.IsEnabled = false;
                        _PrimaryFilterTabItem.Focus();
                        Session.Betline = new BettingLine();
                        break;
                    case 3:
                        Session.Betline.AlgorithmName = "BetOnAllRecentlyFoundIncrementingTest";
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
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                var activeBetlines = from b in context.Betlines
                                     where b.isActive
                                     select b;
                var interval = 12 * (activeBetlines.Count()+1);
                BettingLine betline = Session.Betline;
                betline.IsActive = true;
                betline.LastBetLost = false;
                betline.CurrencyCode = "EUR";
                betline.timer = new DispatcherTimer();
                betline.timer.Interval = TimeSpan.FromSeconds(interval); //use smaller interval at Tests            
                betline.timer.Start();
                //We can change these with filters
                betline.Bet.betCategoryType = BetCategoryTypeEnum.E;
                betline.Bet.betPersistenceType = BetPersistenceTypeEnum.NONE;
                betline.Bet.betType = _BetType.SelectedIndex == 0 ? BetTypeEnum.B : BetTypeEnum.L;
                betline.BetType = betline.Bet.betType == BetTypeEnum.B ? "B" : "L";
                betline.BetlineName = betlineName.Text;
                // Adding betting Line to Database
                betline.AddToDB();
                //Execute selected Method for betline
                betline.timer.Tick += betline.Betline_timer_TickAsync;
            }
            catch (Exception)
            {
                restartTimer_Tick(this, new EventArgs());
            }
                        
        }
        public void IncrementingStakeByOddBetline(BettingLine betline)
        {
            try
            {
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                var activeBetlines = from b in context.Betlines
                                     where b.isActive
                                     select b;
                var interval = 12 * activeBetlines.Count();                
                betline.timer = new System.Windows.Threading.DispatcherTimer();
                betline.timer.Interval = TimeSpan.FromSeconds(interval); //use smaller interval at Tests            
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
            catch (Exception)
            {
                restartTimer_Tick(this, new EventArgs());
            }
        }

        //???
        public void BetOnAllRecentlyFound()
        {
            try
            {
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                var activeBetlines = from b in context.Betlines
                                     where b.isActive
                                     select b;
                var interval = 12 * (activeBetlines.Count() + 1);
                BettingLine betline = Session.Betline;
                betline.IsActive = true;
                betline.LastBetLost = false;
                betline.CurrencyCode = "EUR";
                betline.timer = new DispatcherTimer();
                betline.timer.Interval = TimeSpan.FromSeconds(interval); //use smaller interval at Tests            
                betline.timer.Start();
                //We can change these with filters
                betline.Bet.betCategoryType = BetCategoryTypeEnum.E;
                betline.Bet.betPersistenceType = BetPersistenceTypeEnum.NONE;
                betline.Bet.betType = _BetType.SelectedIndex == 0 ? BetTypeEnum.B : BetTypeEnum.L;
                betline.BetType = betline.Bet.betType == BetTypeEnum.B ? "B" : "L";
                betline.BetlineName = betlineName.Text;
                // Adding betting Line to Database
                betline.AddToDB();
                //Execute selected Method for betline
                betline.timer.Tick += betline.timer_Tick_BetOnAllRecentlyFoundIncrementing_Async;

            }
            catch (Exception)
            {
                restartTimer_Tick(this, new EventArgs());
            }
        }

        public void BetOnAllRecentlyFound(BettingLine betline)
        {
            try
            {
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                var activeBetlines = from b in context.Betlines
                                     where b.isActive
                                     select b;
                var interval = 12 * activeBetlines.Count();
                betline.timer = new System.Windows.Threading.DispatcherTimer();
                betline.timer.Interval = TimeSpan.FromSeconds(interval); //use smaller interval at Tests            
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
            catch (Exception)
            {                
                restartTimer_Tick(this, new EventArgs());
            }
        }

        public void BetOnAllRecentlyFoundIncrementing()
        {
            try
            {
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                var activeBetlines = from b in context.Betlines
                                     where b.isActive
                                     select b;
                var interval = 12 * (activeBetlines.Count() + 1);
                BettingLine betline = Session.Betline;
                betline.IsActive = true;
                betline.LastBetLost = false;
                betline.CurrencyCode = "EUR";
                betline.timer = new DispatcherTimer();
                betline.timer.Interval = TimeSpan.FromSeconds(interval); //use smaller interval at Tests            
                betline.timer.Start();
                //We can change these with filters
                betline.Bet.betCategoryType = BetCategoryTypeEnum.E;
                betline.Bet.betPersistenceType = BetPersistenceTypeEnum.NONE;
                betline.Bet.betType = _BetType.SelectedIndex == 0 ? BetTypeEnum.B : BetTypeEnum.L;
                betline.BetType = betline.Bet.betType == BetTypeEnum.B ? "B" : "L";
                betline.BetlineName = betline.BetlineName;
                // Adding betting Line to Database
                betline.AddToDB();
                //Execute selected Method for betline
                betline.timer.Tick += betline.timer_Tick_BetOnAllRecentlyFoundIncrementing_Async;
            }
            catch (Exception)
            {
                restartTimer_Tick(this, new EventArgs());                
            }
        }

        public void BetOnAllRecentlyFoundIncrementing(BettingLine betline)
        {
            try
            {
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                var activeBetlines = from b in context.Betlines
                                     where b.isActive
                                     select b;
                var interval = 12 * activeBetlines.Count();
                betline.timer = new System.Windows.Threading.DispatcherTimer();
                betline.timer.Interval = TimeSpan.FromSeconds(interval); //use smaller interval at Tests            
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
            catch (Exception)
            {
                restartTimer_Tick(this, new EventArgs());                
            }
        }

        private void StartupActiveMethodsFmDatabase()
        {
            try
            {                
                var betlinesList = Session.databaseContext.Betlines.ToList<DBLBettingApp.Betline>();
                foreach (DBLBettingApp.Betline betline in betlinesList)
                {
                    BettingLine bettingLine = new BettingLine(betline);
                    switch (bettingLine.AlgorithmName)
                    {
                        case "Incrementing StakeTest":
                            IncrementingStakeByOddBetline(bettingLine);
                            break;
                        case "BetOnAllRecentlyFoundTest":
                            BetOnAllRecentlyFound(bettingLine);
                            break;
                        case "BetOnAllRecentlyFoundIncrementingTest":
                            BetOnAllRecentlyFoundIncrementing(bettingLine);
                            break;
                        default:
                            break;
                    }
                    System.Threading.Thread.Sleep(new TimeSpan(0,0,10));
                }
            }
            catch (Exception)
            {
                restartTimer_Tick(this, new EventArgs());                
            }            
        }

        // Fill the Session betline with the primary filters and requested markets by these filters. Also steps forward to the secondary filters selection.
        void FillPrimaryFilters(object sender, RoutedEventArgs e)
        {
                if (FillPrimaryFilterSuccessfuly(Session.Betline))
                {
                    Session.Betline.GetMarketsRequestFiltered();
                    HashSet<string> marketNamesSet = new HashSet<string>();
                    foreach (WebExchangeService.Market market in Session.Betline.CurrentMarketList)
                    {
                        marketNamesSet.Add(market.name);
                    }
                    _marketName.ItemsSource = marketNamesSet;
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
            catch (Exception)
            {

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
	        catch (Exception)
	        {
                MessageBox.Show("Secondary Filter Parsing Error! Please fill correctly filter data and Try again!");
                return false;
	        }
        }

        public bool FillBetlineParameters(BettingLine betline)
        {
            try
            {
                betline.InitialProfitPerBet = double.Parse(_profit.Text);
                betline.ProfitPerBet = double.Parse(_profit.Text);
                betline.StakeRangeMin = double.Parse(_StakeRangeMin.Text);
                betline.StakeRangeMax = double.Parse(_StakeRangeMax.Text);

                if (_totalAmmountMatchedMin.Text!="")
                {
                    betline.Filter.TotalAmountMatchedMin = double.Parse(_totalAmmountMatchedMin.Text);                    
                }
                if (_totalAmmountMatchedMax.Text!="")
                {
                    betline.Filter.TotalAmountMatchedMax = double.Parse(_totalAmmountMatchedMax.Text);                                     
                }
                string maxAmmount = (_maxAmmountStack.Children[1] as TextBox).Text;
                if (!String.IsNullOrEmpty(maxAmmount))
                {
                    try
                    {
                        Session.Betline.Filter.MaxAmmountIncremented = (int)double.Parse(maxAmmount);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Wrong Max Ammount entered try again!");
                        return false;
                    }                    
                }
                string maxBets = (_maxBetsCountStack.Children[1] as TextBox).Text;
                if (!String.IsNullOrEmpty(maxBets))
                {
                    try
                    {
                        Session.Betline.Filter.MaxBetsCount = byte.Parse(maxBets);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Wrong Max Bets Count entered. Please enter number From 1 to 16!");
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
                                throw new Exception();
                            }
                        }
                        Session.Betline.Filter.GoalDifferenceMin = short.Parse(_goalDifferenceMin.Text);
                    }
                    catch (Exception)
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
                                throw new Exception();
                            }
                        }
                        Session.Betline.Filter.GoalDifferenceMax = short.Parse(_goalDifferenceMax.Text);
                    }
                    catch (Exception)
                    {
                    }
                }
                betline.BetlineName = string.IsNullOrWhiteSpace(betlineName.Text) ? string.Format("{0} - {1} -> {2} {3} - {4}", _BetType.Text, _StakeRangeMin.Text, _StakeRangeMax.Text, betline.Filter.MarketTypesArray[0], _algo.Text) : betlineName.Text;
                betline.Ballance = 0;
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("Please fill BetLine Parameters, and try again.");
                return false;
            }
        }
        //filling betline with eventTypes              
        //Binding EventTypesList
        public void globalService_getAllEventTypesCompleted(object sender, getAllEventTypesCompletedEventArgs e)
        {
            try
            {
                GetEventTypesResp getEventTypesResp = e.Result;
                Session.checkHeader(getEventTypesResp.header);
                if (getEventTypesResp.errorCode == GetEventsErrorEnum.OK)
                {
                    _events.ItemsSource = getEventTypesResp.eventTypeItems;
                }
                else
                {
                    MessageBox.Show("{0} error occured. Please try again.", getEventTypesResp.errorCode.ToString());
                }
            }
            catch (Exception)
            {
                restartTimer_Tick(this, new EventArgs());
            }
            
        }
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
            BindFunds();
        }

        //?? Implement to DataGrid        
        internal static void BindBetlinesGrid()
        {
            try
            {                
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                var common = context.Betlines;
                var gridLines = from b in common
                                select b;
                if (gridLines.Count() > 0)
                {
                    var list = gridLines.ToList();
                    (WpfBetApplication.App.Current.MainWindow.FindName("_RunningBetLines") as DataGrid).ItemsSource = list;
                }
            }
            catch (Exception)
            {
            }
        }
        //?? not working
        private void PauseBetline(object sender, RoutedEventArgs e)
        {
            try
            {
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                DBLBettingApp.Betline gridBetline = (sender as FrameworkElement).DataContext as DBLBettingApp.Betline;
                DBLBettingApp.Betline contextBetline = context.Betlines.Find(gridBetline.BetlineId);
                contextBetline.isActive = false;
                contextBetline.lastBetLost = false;
                context.SaveChanges();
                BindBetlinesGrid();
            }
            catch (Exception)
            {
                restartTimer_Tick(App.Current, new EventArgs());
            }            
        }

        private void StopOnWinBetline(object sender, RoutedEventArgs e)
        {
            try
            {
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                DBLBettingApp.Betline gridBetline = (sender as FrameworkElement).DataContext as DBLBettingApp.Betline;
                DBLBettingApp.Betline contextBetline = context.Betlines.Find(gridBetline.BetlineId);
                contextBetline.isActive = false;
                context.SaveChanges();
                BindBetlinesGrid();
            }
            catch (Exception)
            {
                restartTimer_Tick(App.Current, new EventArgs());
            }
        }

        private void ResumeBetline(object sender, RoutedEventArgs e)
        {
            try
            {                
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                DBLBettingApp.Betline gridBetline = (sender as FrameworkElement).DataContext as DBLBettingApp.Betline;
                DBLBettingApp.Betline contextBetline = context.Betlines.Find(gridBetline.BetlineId);
                contextBetline.isActive = true;
                context.SaveChanges();
                BindBetlinesGrid();
            }
            catch (Exception)
            {
                restartTimer_Tick(App.Current, new EventArgs());
            }
        }

        private void UpdateBetline(object sender, RoutedEventArgs e)
        {
            try
            {                
                BindBetlinesGrid(); 
            }
            catch (Exception)
            {
                restartTimer_Tick(App.Current, new EventArgs());
            }
        }

        private void Restart(object sender, RoutedEventArgs e)
        {
            try
            {                
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                DBLBettingApp.Betline gridBetline = (sender as FrameworkElement).DataContext as DBLBettingApp.Betline;
                DBLBettingApp.Betline contextBetline = context.Betlines.Find(gridBetline.BetlineId);
                contextBetline.isActive = true;
                contextBetline.lastPlacedBetId = "";
                contextBetline.profitPerBet = contextBetline.initialProfitPerBet;
                context.SaveChanges();
                BindBetlinesGrid();
            }
            catch (Exception)
            {
                restartTimer_Tick(App.Current, new EventArgs());
            }
        }

        private void DeleteBetline(object sender, RoutedEventArgs e)
        {
            DBLBettingApp.Betline gridBetline = (sender as FrameworkElement).DataContext as DBLBettingApp.Betline;
            if (gridBetline != null)
            {
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
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
                catch (Exception)
                {
                    restartTimer_Tick(App.Current, new EventArgs());                   
                }                
            }            
            BindBetlinesGrid();
        }

        private void _RunningBetLines_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
 
            // iteratively traverse the visual tree
            while ((dep != null) && !(dep is DataGridCell))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
 
            if (dep == null)
                return;
 
            if (dep is DataGridCell)
            {
                DataGridCell cell = dep as DataGridCell;
                
                TextBlock block = (cell.Content as TextBlock);
                TextBox blockBox = (cell.Content as TextBox);
                List<string> parsePlaced = new List<string>();
                try 
	            {
                    if (block == null)
                    {
                        parsePlaced = blockBox.Text.Trim(':').Split(':').ToList();
                    }
                    else
                    {
                        parsePlaced = block.Text.Trim(':').Split(':').ToList();
                    }
	                                            
                    if (cell.Column.Header.ToString() == "Last Bet" & !FilterMarkets.StringArrayIsEmpty(parsePlaced.ToArray()))
                    {
                        DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                        Window placedBets = new WpfBetApplication.PlacedBet();
                        DBLBettingApp.Betline selectedBetline = (sender as DataGrid).SelectedItem as DBLBettingApp.Betline;
                        var items = selectedBetline.lastPlacedBetId.Trim(':').Split(':').ToList();
                        if (items.Count < 1)
                        {
                            return;
                        }
                        DataGrid grid = new DataGrid();
                        grid.IsReadOnly = true;
                        grid.MaxHeight = 500;
                        var columnName = new DataGridTextColumn();
                        columnName.Binding = new Binding("property");
                        var columnValue = new DataGridTextColumn();
                        columnValue.Binding = new Binding("value");
                        columnName.Header = "Name";
                        columnValue.Header = "Value";
                        grid.Columns.Add(columnName);
                        grid.Columns.Add(columnValue);
                        foreach (string id in items)
                        {
                            try 
	                        {
                                DBLBettingApp.PlacedBet lastPlacedBet = context.PlacedBets.Find(long.Parse(id));
                                if (lastPlacedBet != null)
                                {
                                    grid.Items.Add(new PlacedBetStruct { property = "Bet ID", value = id });
                                    grid.Items.Add(new PlacedBetStruct { property = "Betline ID", value = lastPlacedBet.betlineId.ToString() });
                                    grid.Items.Add(new PlacedBetStruct { property = "Average Price", value = lastPlacedBet.averagePrice.ToString() });
                                    grid.Items.Add(new PlacedBetStruct { property = "Matched Size", value = lastPlacedBet.sizeMatched.ToString() });
                                    grid.Items.Add(new PlacedBetStruct { property = "Market Path", value = lastPlacedBet.marketMenuPath.Split('\\').ToList().Last() });
                                    grid.Items.Add(new PlacedBetStruct { property = "Selection", value = lastPlacedBet.Selection });
                                    grid.Items.Add(new PlacedBetStruct { property = "Status", value = lastPlacedBet.resultCode });
                                    grid.Items.Add(new PlacedBetStruct { property = "Score", value = lastPlacedBet.score });
                                }		
	                        }
	                        catch (Exception)
	                        {
		
	                        }                        
                        
                        }
                        grid.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                        StackPanel mainPanel = new StackPanel();
                        mainPanel.Children.Add(grid);
                        placedBets.Content = mainPanel;
                        placedBets.Show();                                       
                    }
                    else
                    {
                        int filterId = 0;
                        try 
	                    {
                            if (block == null)
                            {
                                filterId = int.Parse(blockBox.Text);
                            }
                            else
                            {
                                filterId = int.Parse(block.Text);
                            }
	                    }
                        catch(Exception)
                        {
                        }                        
                        DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                        Window placedBetsWindow = new WpfBetApplication.PlacedBet();
                        int betLineIdFmCell;
                        try
                        {
                            betLineIdFmCell = ((sender as DataGrid).SelectedItem as DBLBettingApp.Betline).BetlineId;
                        }
                        catch (Exception)
                        {
                            return;
                        }
                        DateTime yesterday = DateTime.Now.AddDays(-1);
                        var placedBetsLinq = from w in context.PlacedBets
                                                where w.betlineId == betLineIdFmCell & w.datePlaced > yesterday
                                                orderby w.datePlaced descending
                                             select new { w.betId, w.averagePrice, w.sizeMatched, w.marketMenuPath, w.Selection, w.datePlaced, w.dateSettled, w.resultCode, w.score, w.success };
                        DataGrid lastPlacedBetsGrid = new DataGrid();
                        lastPlacedBetsGrid.MaxHeight = 500;
                        lastPlacedBetsGrid.MaxWidth = 900;                        
                        if (placedBetsLinq != null)
                        {
                            lastPlacedBetsGrid.ItemsSource = placedBetsLinq.ToList();
                            lastPlacedBetsGrid.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                            lastPlacedBetsGrid.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                            StackPanel mainPanel = new StackPanel();
                            mainPanel.Orientation = Orientation.Horizontal;
                            mainPanel.Children.Add(lastPlacedBetsGrid);
                            StackPanel statisticsPanel = new StackPanel();
                            UniformGrid statsUniformGrid = new UniformGrid();
                            statsUniformGrid.Columns = 2;
                            var betsResults = from pb in context.PlacedBets
                                          where pb.betlineId == betLineIdFmCell
                                          select pb.success;
                            var wonBets = from won in betsResults
                                          where won == true
                                          select won;
                            double percentWonBets = wonBets.Count() / betsResults.Count();
                            mainPanel.Children.Add(statisticsPanel);
                            placedBetsWindow.Content = mainPanel;
                            placedBetsWindow.Show();
                        }                 
                    }
                }
                catch (Exception)
                {
                    restartTimer_Tick(this, new EventArgs());
                }
            }            
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Button_Click_LogOut(this, new RoutedEventArgs());
            WpfBetApplication.App.Current.Dispatcher.InvokeShutdown();
            WpfBetApplication.App.Current.Shutdown();
 	        base.OnClosing(e);
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
                DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                var notSettledNewBets = from bets in context.PlacedBets
                                        where bets.dateSettled == null
                                        select bets;
                foreach (var betLive in notSettledNewBets)
                {
                    BettingLine.UpdatePlacedBetResult(betLive, Session.soccerLivescore);
                }
                context.SaveChanges();
                var notSettledOldBets = from bets in context.PlacedBets
                                        where bets.resultCode != "Settled" & bets.resultCode != "FT" & bets.datePlaced < hoursBefore2
                                        select bets;
                foreach (var bet in notSettledOldBets)
                {
                    context.PlacedBets.Find(bet.betId).resultCode = "FT";
                }
                context.SaveChanges();
            }
            catch (Exception)
            {
                restartTimer_Tick(null, new EventArgs());
            }            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
        }       

        private void SetEmail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_email.Text != "" & _email.Text.Contains('@'))
                {
                    try
                    {
                        DBLBettingApp.BFDBLEntities1 context = new DBLBettingApp.BFDBLEntities1();
                        foreach (var item in context.Emails)
                        {
                            context.Emails.Remove(item);
                        }
                        context.SaveChanges();
                        DBLBettingApp.Email newMail = new DBLBettingApp.Email();
                        newMail.email1 = _email.Text.Trim();
                        context.Emails.Add(newMail);
                        context.SaveChanges();
                    }
                    catch (Exception)
                    {                        
                    }
                    
                }
            }
            catch (Exception)
            {
                
            }
            

        }

        private void _marketName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Window dialogWindow = new DialogWindow();
            dialogWindow.Owner = App.Current.MainWindow;
            string marketName;
            try
            {
                marketName = e.AddedItems[0].ToString();
            }
            catch (IndexOutOfRangeException)
            {
                return;
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
                case "Half Time":
                    checkBoxStackPanel.Orientation = Orientation.Horizontal;
                    string[] arrayHTOdds = { "1", "2", "X" };
                    for (int i = 0; i < arrayHTOdds.Length; i++)
                    {
                        CheckBox cb = new CheckBox();
                        cb.IsChecked = true;
                        cb.Content = arrayHTOdds[i];
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
                    return;
            }
            dialogWindow.ShowDialog();
        }

        private void _algo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0].ToString().Contains("Incrementing"))
            {
                _maxAmmountStack.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                _maxAmmountStack.Visibility = System.Windows.Visibility.Hidden;
            }
            if (e.AddedItems[0].ToString().Contains("AllRecentlyFound"))
            {
                _maxBetsCountStack.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                _maxBetsCountStack.Visibility = System.Windows.Visibility.Hidden;
            }
        }

    }
}
