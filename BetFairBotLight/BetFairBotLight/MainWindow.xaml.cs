using Betfair_Non_interactive_login;
using BetFairBot;
using BetFairBot.TO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
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

namespace BetFairBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string Url = "https://api.betfair.com/exchange/betting";
        public static Entities databaseContext = new Entities();
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
            CurrentBetline = new BettingLine();
            CurrentBetline.Filters = new Filters();
            CurrentBetline.MarketFilter = new TO.MarketFilter();
            Button_Click_Login(this, new RoutedEventArgs());
            CheckConnectionTimerInit();
            LivescoreUpdateTimerInitiate();
            ReportTimerInitiate();
        }

        private void Button_Click_Login(object sender, RoutedEventArgs e)
        {
            bool loggedIn = LoginSuccessfull();
            if (!loggedIn)
            {
                return;
            }
            var user = databaseContext.Users.FirstOrDefault(u => u.userName == "sabina8234");
            if (user != null)
            {
                foreach (var u in databaseContext.Users.Where(us => us.isActive))
                {
                    if (u.userName != "sabina8234")
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
                user.userName = "sabina8234";
                user.Password = "Samota8234";
                user.appKey = "vBgUxOJsNetcV5I5";
                user.certFileName = "EddyBetFairApp.p12";
                user.isActive = true;
                UserActive = databaseContext.Users.Add(user);
            }
            databaseContext.SaveChanges();
            StartupActiveMethodsFmDatabase();
            CurrentBetline.Users = UserActive;
            CurrentBetline.ownersUserName = UserActive.userName;            
        }

        private bool LoginSuccessfull()
        {
            var username = "sabina8234"; //args.First();
            var appKey = "vBgUxOJsNetcV5I5"; //args.ElementAt(1);
            var certFilename = "EddyBetFairApp.p12";//args.ElementAt(2);
            var password = "Samota8234";
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

        private static bool SendEmail(string recipient, string subject, string message, bool sendAsFile)
        {
            try
            {
                string rarUrl = "";
                string fileUrl = "";
                System.Net.Mail.MailMessage mail = new MailMessage();
                mail.From = new System.Net.Mail.MailAddress("newstoemailsender@gmail.com");
                mail.To.Add(recipient);
                mail.Subject = subject.Replace('\r', ' ').Replace('\n', ' ');
                mail.Body = message;                
                mail.Priority = System.Net.Mail.MailPriority.Normal;
                mail.IsBodyHtml = false;
                using (var client = new SmtpClient("smtp.gmail.com", 587) { EnableSsl = true, DeliveryFormat = SmtpDeliveryFormat.International, UseDefaultCredentials = false, Credentials = new NetworkCredential("newstoemailsender@gmail.com", "Eddy8479"), })
                {
                    client.Send(mail);
                }
                mail.Dispose();
                if (sendAsFile)
                {
                    try
                    {
                        System.IO.File.Delete(fileUrl);
                        System.IO.File.Delete(rarUrl);
                    }
                    catch (System.Exception)
                    {
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                var ent = ex.ToString();
                return false;
            }
        }

        private void ReportTimerInitiate()
        {
            DispatcherTimer reportTimer = new DispatcherTimer();
            reportTimer.Interval = TimeSpan.FromHours(0.5);
            reportTimer.Tick += reportTimer_Tick;
            reportTimer.Start();
        }

        void reportTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now.Hour == 1)
            {
                var context = new Entities();
                var yesterday = DateTime.Now.AddDays(1);
                var lastBets = context.PlacedBets.Where(b => b.datePlaced > yesterday).ToList();
                if (lastBets.Count>0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var bet in lastBets)
                    {
                        sb.AppendLine(string.Format("{0}-{1}-{2}-{3}-{4}-{5}",bet.marketMenuPath,bet.score, bet.Selection, bet.averagePrice, bet.sizeMatched, bet.success));

                    }
                    decimal bal = 0;
                    try
                    {
                        bal = context.Betlines.FirstOrDefault().ballance;
                    }
                    catch (System.Exception)
                    {
                    }
                    sb.AppendLine(string.Format("Ballance",bal));
                    SendEmail("danailveselinov@yahoo.com", "BFReport", sb.ToString(), false);
                }
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
    }
}
