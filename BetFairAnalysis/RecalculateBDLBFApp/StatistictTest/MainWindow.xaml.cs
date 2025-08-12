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
using DBLBF;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;
using System.Diagnostics;

namespace StatisticsTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class PriceStats
    {
        public List<SelectionStats> SelectionStatsList { get; set; }
        public double Price { get; set; }
        public int TotalBets { get; set; }
        public class SelectionStats
        {
            public string Selection { get; set; }
            public double Won { get; set; }
            public int BetsCount { get; set; }
        }
    }


    public class BudgetPlan
    {

        double neighboursAvRatio;

        public double NeighboursAvRatio
        {
            get { return Math.Round(neighboursAvRatio, 2); }
            set { neighboursAvRatio = value; }
        }

        private double ratio;

        public double Ratio
        {
            get { return Math.Round(ratio, 2); }
            set { ratio = value; }
        }
        int taxPercent;

        double ppb;

        public double PPBudjet
        {
            get { return  Math.Round(ppb,2); }
            set { ppb = value; }
        }

        private double maxBet;

        public double MaxBet
        {
            get { return maxBet; }
            set { maxBet = value; }
        }

        private double budget;

        public double Budget
        {
            get { return Math.Round(budget, 2); }
            set { budget = value; }
        }

        private double totalProfit;

        public double TotalProfit
        {
            get { return Math.Round(totalProfit, 2); }
            set { totalProfit = value; }
        }

        private double totalLoss;

        public double TotalLoss
        {
            get { return Math.Round(totalLoss,2); }
            set { totalLoss = value; }
        }

        int placedBetsCount;

        public int PlacedBetsCount
        {
            get { return placedBetsCount; }
            set { placedBetsCount = value; }
        }
        private int canceledBetsCount;

        public int CanceledN
        {
            get { return canceledBetsCount; }
            set { canceledBetsCount = value; }
        }
        byte lostInARowMax;

        internal byte LARM
        {
            get { return lostInARowMax; }
            set { lostInARowMax = value; }
        }

        internal byte LARRun { get; set; }

        internal void GetBudjetProfitAndLoss(ICollection<PlacedBet> placedBets, int taxPercent)
        {
            double limitSize = this.maxBet > 0 ? this.maxBet : double.MaxValue;
            this.totalProfit = 0;
            this.totalLoss = 0;
            try
            {
                var placedBetsToMaxStakeSorted = from b in placedBets
                                           where b.sizeMatched <= limitSize
                                           orderby b.sizeMatched descending
                                           select b;
                foreach (var item in placedBetsToMaxStakeSorted)
                {
                    if (item.Betline.betType == "Back")
                    {
                        if (item.success)
                        {
                            this.totalProfit += item.sizeMatched.Value * (item.averagePrice.Value - 1);
                        }
                        else
                        {
                            this.totalLoss += item.sizeMatched.Value;
                        }
                    }
                    else
                    {
                        if (!item.success)
                        {
                            this.totalProfit += item.sizeMatched.Value / (item.averagePrice.Value - 1);
                        }
                        else
                        {
                            this.totalLoss += item.sizeMatched.Value;
                        }
                    }
                }                
            }
            catch (Exception)
            {
            }
            this.totalProfit *= (double)(1 - ((double)taxPercent / 100));
        }
        internal void GetLostLongestSeries(ICollection<PlacedBet> placedBets)
        {
            try
            {
                var currentBetsList = placedBets.Where(b => b.sizeMatched <= this.MaxBet);
                PlacedBet biggestBet = currentBetsList.OrderByDescending(b => b.sizeMatched).FirstOrDefault();
                this.LARRun = 1;
                while (biggestBet.sizeMatched > 2)
                {
                    var betsByDatePlaced = from p in currentBetsList
                                           where p.datePlaced < biggestBet.datePlaced & p.dateSettled.HasValue
                                           orderby p.datePlaced descending
                                           select p;
                    PlacedBet previousPlaced = betsByDatePlaced.FirstOrDefault();
                    var biggestPlacedDate = biggestBet.datePlaced.AddMinutes(-5);
                    var previousPlacedDate = previousPlaced.datePlaced.AddMinutes(-5);
                    var childBetsBySize = from b in betsByDatePlaced
                                          where b.dateSettled < biggestPlacedDate & b.dateSettled.Value > previousPlacedDate
                                          orderby b.sizeMatched descending
                                          select b;
                    biggestBet = childBetsBySize.FirstOrDefault();
                    this.LARRun++;
                    if (biggestBet == null)
                    {
                        return;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        internal void CalculateRealBudjet(ICollection<PlacedBet> placedBets)
        {
            try
            {
                var orderedBetsFirst = placedBets.OrderBy(p => p.datePlaced);
                //var orderedBets = new PlacedBet[orderedBetsFirst.Count()];
                //orderedBetsFirst.ToArray().CopyTo(orderedBets,0);
                List<PlacedBet> currentBets = new List<PlacedBet>();
                double biggestBudjet = 0;
                double currentBudjet = 0;
                var betType = placedBets.FirstOrDefault().Betline.betType;
                foreach (var bet in orderedBetsFirst)
                {
                    try
                    {
                        double size = bet.sizeMatched.Value > MaxBet ? MaxBet : bet.sizeMatched.Value;
                        currentBets.Add(bet);
                        currentBudjet += size;
                        var currentBetsCopy = new PlacedBet[currentBets.Count];
                        currentBets.CopyTo(currentBetsCopy, 0);
                        for (int i = 0; i < currentBetsCopy.Length; i++)
                        {
                            try
                            {
                                var currentBet = currentBetsCopy[i];
                                double currentSizeMatched = currentBet.sizeMatched.Value > MaxBet ? MaxBet : currentBet.sizeMatched.Value;
                                if (currentBet.dateSettled <= bet.datePlaced)
                                {
                                    double profitAndLoss = 0;
                                    if (currentBet.success)
                                    {
                                        if (betType == "Back")
                                        {
                                            profitAndLoss = Math.Round(currentBet.averagePrice.Value * currentSizeMatched, 2);
                                        }
                                    }
                                    else
                                    {
                                        if (betType != "Back")
                                        {
                                            profitAndLoss = Math.Round((currentSizeMatched / (currentBet.averagePrice.Value - 1)) + currentSizeMatched, 2);
                                        }
                                    }
                                    if (currentBudjet < 0)
                                    {
                                        currentBudjet = 0;
                                    }
                                    currentBudjet -= profitAndLoss;
                                    currentBets.Remove(currentBets.Find(b=>b.betId == currentBetsCopy[i].betId));
                                }
                            }
                            catch (Exception)
                            {
                            }                            
                        }
                        if (biggestBudjet < currentBudjet)
                        {
                            biggestBudjet = currentBudjet;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                this.Budget = biggestBudjet;
            }
            catch (Exception)
            {
            }            
        }
    }

    

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            
            InitializeComponent();
            _taxPercent.ItemsSource = Enumerable.Range(1, 5).Reverse();
        }

        public byte GetLostNumbersInARow(ICollection<PlacedBet> placedBets)
        {
            try
            {
                var currentBetsList = placedBets.ToList();
                PlacedBet firstBet = currentBetsList[0];
                byte LARM = 0;
                byte counter = 0;
                for (int i = 0; i < currentBetsList.Count; i++)
                {
                    firstBet = currentBetsList[i];
                    if (firstBet.Betline.betType == "Back" ? !firstBet.success : firstBet.success)
                    {
                        counter++;
                    }
                    else
                    {
                        if (LARM < counter)
                        {
                            LARM = counter;
                        }
                        counter = 0;
                    }
                }
                return LARM;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _dataFileTextBox.Text = System.IO.Directory.GetCurrentDirectory() + "\\BFBDB.MDF";
            BindBetlineCombo();
        }

        private void BindBetlineCombo()
        {
            BFBDBEntities context = new BFBDBEntities();
            _betlineId.ItemsSource = context.Betlines.ToList();
            _betlineId.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {            
            try
            {
                int betlineId = 1;
                try
                {
                    betlineId = (_betlineId.SelectedItem as DBLBF.Betline).BetlineId;
                }
                catch (Exception)
                {
                    return;
                }
                StringBuilder sb = new StringBuilder();
                DBLBF.BFBDBEntities context = new DBLBF.BFBDBEntities();
                var betsResults = from pb in context.PlacedBets
                                  where pb.betlineId == betlineId
                                  select pb;
                var betline = context.Betlines.Find(betlineId);
                bool betSuccessWon = true;
                if (betline.betType == "Back")
                {
                    betSuccessWon = true;
                }
                else
                {
                    betSuccessWon = false;
                }
                var wonBets = from won in betsResults
                              where won.success == betSuccessWon
                              select won;
                var filter = betline.Filter;
                List<string> sortedOrdersMatchOddsArray = new List<string>();
                List<string> sortedOrdersCorrectScoreArray = new List<string>();
                if (!String.IsNullOrEmpty(filter.sortedOrders))
                {
                    var odds = filter.sortedOrders.Split(';');
                    for (int i = 0; i < odds.Length; i++)
			        {
                        if (odds[i].Contains("MATCH_ODDS"))
	                    {
                            sortedOrdersMatchOddsArray = odds[i].Replace("MATCH_ODDS,", "").Split(',').ToList();
	                    }
                        if (odds[i].Contains("CORRECT_SCORE"))
                        {
                            sortedOrdersCorrectScoreArray = odds[i].Replace("CORRECT_SCORE,", "").Split(',').ToList();
                        }
			        }
                }
                double wo = wonBets.Count();
                double all = betsResults.Count();
                double percentWonBets = Math.Round(wo / all, 2);
                sb.AppendLine(String.Format("Statistics for betline:{0}",betlineId));
                sb.AppendLine("Won: " + percentWonBets + "%" + " (" + all + ")");
                foreach (var item in sortedOrdersMatchOddsArray)
                {
                    int it = int.Parse(item);
                    var itemsAll = from o in betsResults
                                   where o.Betline.Filter.marketNames.Contains("MATCH_ODDS") & GetSortedOrder(o) == it
                                   select o;
                    var itemsWon = from w in itemsAll
                                   where betSuccessWon ? w.success : !w.success
                                   select w;
                    double percentWonItems = Math.Round((double)itemsWon.Count() / (double)itemsAll.Count(), 2);
                    string itemRepresentative = item == "3" ? "X" : item;
                    sb.AppendLine(itemRepresentative + " : " + percentWonItems + "%" + " (" + itemsAll.Count() + ")");
                }
                foreach (var item in sortedOrdersCorrectScoreArray)
                {
                    int it = int.Parse(item);
                    var itemsAll = from o in context.PlacedBets
                                   where o.Betline.Filter.marketNames.Contains("MATCH_ODDS") & GetSortedOrder(o) == it
                                   select o;
                    var itemsWon = from w in itemsAll
                                   where betSuccessWon ? w.success : !w.success
                                   select w;
                    double percentWonItems = Math.Round((double)itemsWon.Count() / (double)itemsAll.Count(), 2);
                    string score = item == "17" ? "Any Unquoted" : GetResultFmSelection(itemsWon.ToList()[0].Selection);
                    sb.AppendLine(score + " : " + percentWonItems + "%" + " (" + itemsAll.Count() + ")");
                }
                //average PPD
                double profiAll = 0;
                double lossAll = 0;
                var pBBetline = from b in context.PlacedBets
                                where b.betlineId == betlineId
                                select b;
                foreach (var item in pBBetline.ToList())
                {
                    if (item.Betline.betType == "Back")
                    {
                        if (item.success)
                        {
                            profiAll += item.sizeMatched.Value * (item.averagePrice.Value - 1);
                        }
                        else
                        {
                            lossAll += item.sizeMatched.Value;
                        }
                    }
                    else
                    {
                        if (!item.success)
                        {
                            profiAll += item.sizeMatched.Value / (item.averagePrice.Value - 1);
                        }
                        else
                        {
                            lossAll += item.sizeMatched.Value;
                        }
                    }
                }

                //var allInitialBets = from b in betsResults
                //                     where b.sizeMatched == 2
                //                     orderby b.datePlaced
                //                     select b;
                //double profit = 0;
                //foreach (var bet in allInitialBets)
                //{
                //    if (betline.betType == "B")
                //    {
                //        try
                //        {
                //            profit += ((bet.averagePrice - 1) * bet.sizeMatched);
                //        }
                //        catch (Exception)
                //        {
                //        }
                //    }
                //    else
                //    {
                //        try
                //        {
                //            profit += (bet.sizeMatched / (bet.averagePrice - 1));
                //        }
                //        catch (Exception)
                //        {
                //        }
                //    }
                //}
                var placedBetsList = context.PlacedBets.ToList();
                var startDate = placedBetsList[0].datePlaced;
                var endDate = placedBetsList[placedBetsList.Count - 1].datePlaced;
                var allDays = endDate - startDate;
                var ppd = Math.Round((profiAll-lossAll) / allDays.Days, 2);
                sb.AppendLine("Profit per day: " + ppd + "("+ allDays.Days +"days)");
                var allBetsSorted = from b in betsResults
                                    orderby b.sizeMatched descending
                                    select b;
                //top 30 biggest bets
                sb.AppendLine("Top 30 Matched size:");
                var allBetsSortedList = allBetsSorted.ToList();
                for (int i = 0; i < 30; i++)
                {
                    sb.AppendLine((i + 1).ToString() + ": " + allBetsSortedList[i].sizeMatched.ToString() + "E " + allBetsSortedList[i].success + allBetsSortedList[i].averagePrice);
                }
                double maxStake = 0;
                try
                {
                    maxStake = double.Parse(_maxStakeTextBox.Text);
                }
                catch (Exception)
                {
                }
                if (maxStake>0)
                {
                    int counter = 0;
                    for (int i = 0; i < allBetsSortedList.Count; i++)
                    {
                        if (allBetsSortedList[i].sizeMatched < double.Parse(_maxStakeTextBox.Text))
                        {
                            sb.AppendLine(counter + " bets with over");
                            sb.AppendLine(_maxStakeTextBox.Text + " matched size");
                            break;
                        }
                        if (allBetsSortedList[i].Betline.betType == "Back")
                        {
                            if (allBetsSortedList[i].success)
                            {
                                counter++;
                            }
                        }
                        else
                        {
                            if (!allBetsSortedList[i].success)
                            {
                                counter++;
                            }
                        }                        
                    }
                }
                Window newWindow = new Window();
                newWindow.Owner = this;
                TextBox tb = new TextBox();
                tb.Text = sb.ToString();
                newWindow.Content = tb;
                newWindow.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
                newWindow.Show();
                //MessageBox.Show(sb.ToString());
            }
            catch (Exception)
            {
            }            
        }

        private int GetSortedOrder(PlacedBet o)
        {
            //Fixtures 23 February  / Karsiyaka v Istanbul BB / Match Odds
            var marketPathSplit = o.marketMenuPath.Split('/');
            var marketName = marketPathSplit[marketPathSplit.Count() - 2];
            string first = marketName.Split('v').First().Trim();
            string last = marketName.Split('v').Last().Trim();
            if (first.Contains(o.Selection))
            {
                return 1;
            }
            else if (last.Contains(o.Selection))
            {
                return 2;
            }
            else if (o.Selection == "The Draw")
            {
                return 3;
            }
            else
            {
                return 0;
            }
        }

        private string GetResultFmSelection(string selection)
        {
            switch (selection)
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
        }

        private void Button_Click_Browse(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "SDFFIle|*.sdf";
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    System.IO.File.Copy(dlg.FileName, System.IO.Directory.GetCurrentDirectory()+"\\BFDBL.SDF",true);
                    _dataFileTextBox.Text = dlg.FileName;
                    BindBetlineCombo();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading image: " +
                    ex.Message);
                }
            }
        }        

        private int GetPlacedBetsCount(double MaxStake, ICollection<PlacedBet> placedBets)
        {
            var pbs = (from b in placedBets
                      where b.sizeMatched <= MaxStake
                      select b).Count();
            return pbs;
        }


        private int GetNumberOfCanceledBets(double MaxStake, ICollection<PlacedBet> placedBets)
        {
            try
            {
                bool lost = placedBets.FirstOrDefault().Betline.betType == "Back" ? true : false;
                var lostBetsList = (from b in placedBets
                                        where b.sizeMatched > MaxStake & b.success == lost
                                        select b).Count();
                return lostBetsList;
            }
            catch (Exception)
            {
                return 0;
            }            
        }

        private async void BestRealBudjetButton_Click(object sender, RoutedEventArgs e)
        {
            int betlineId;
            BFBDBEntities context = new BFBDBEntities();
            betlineId = (_betlineId.SelectedItem as DBLBF.Betline).BetlineId;
            var taxPercent = (int)_taxPercent.SelectedValue;
            Betline betline = context.Betlines.Find(betlineId);
            BudjetsWindow budjetsW = new BudjetsWindow();
            budjetsW.Left = 50 + (_taxPercent.SelectedIndex * 20);
            budjetsW.Top = 50 + (_taxPercent.SelectedIndex * 20);
            budjetsW.Title = string.Format("Betline name {0} of BetType: {1}.", betline.betlineName, betline.betType);
            int maxBetSize = 0;
            int.TryParse(_maxStakeTextBox.Text,out maxBetSize);
            List<BudgetPlan> bestBudjetsDisplay = new List<BudgetPlan>();
            List<BudgetPlan> worstBudjetsDisplay = new List<BudgetPlan>();
            double limit = 0;
            try
            {
                limit = double.Parse(_budjetsLimit.Text);
            }
            catch (Exception)
            {
            }
            double step = 0.1;
            double.TryParse(_budjetStep.Text, out step);
            await Task.Run(() => {
                
                List<BudgetPlan> budjetsList = new List<BudgetPlan>();
                try
                {
                    //var placedBetsFirst = new PlacedBet[betline.PlacedBets.Count];
                    //betline.PlacedBets.CopyTo(placedBetsFirst,0);
                    
                    if (!(limit>0))
                    {
                        limit = (from ms in betline.PlacedBets //placedBetsFirst
                                 orderby ms.sizeMatched descending
                                 select ms.sizeMatched.Value).FirstOrDefault();
                    }                    
                    //byte newValue = GetLostNumbersInARow(placedBets);
                    for (double MaxStake = 2; MaxStake < limit*(step+1); MaxStake = Math.Round(MaxStake * (step+1), 2))
                    {
                        //var placedBets = new PlacedBet[betline.PlacedBets.Count];
                        //betline.PlacedBets.CopyTo(placedBets, 0);
                        try
                        {
                            BudgetPlan budjet = new BudgetPlan();
                            budjet.MaxBet = MaxStake;
                            budjet.CanceledN = GetNumberOfCanceledBets(MaxStake, betline.PlacedBets);
                            budjet.PlacedBetsCount = GetPlacedBetsCount(MaxStake, betline.PlacedBets);
                            budjet.GetBudjetProfitAndLoss(betline.PlacedBets, taxPercent);
                            budjet.Ratio = (budjet.TotalProfit - budjet.TotalLoss) / budjet.PlacedBetsCount;
                            budjet.NeighboursAvRatio = 0;
                            //budjet.LARM = newValue;
                            //budjet.GetLostLongestSeries(placedBets);
                            budjet.CalculateRealBudjet(betline.PlacedBets);
                            budjetsList.Add(budjet);
                        }
                        catch (Exception)
                        {
                            
                        }                        
                    }
                    for (int i = 0; i < budjetsList.Count; i++)
                    {
                        var bjetI = budjetsList[i];
                        //var biggestLARRun = (from b in budjetsList
                        //                orderby b.LARRun descending
                                        //select b.LARRun).FirstOrDefault();
                        //var LRRunInterpulated = Math.Round((((1-(limit - bjetI.MaxBet)/limit))*biggestLARRun),2);
                        //bjetI.Budget *= bjetI.LARM/(LRRunInterpulated+1);
                        int divider = (int)(i * ((((betline.stakeRangeMax - betline.stakeRangeMin) / 2) + betline.stakeRangeMin) - 1));
                        if (divider > 0)
                        {
                            for (int j = -divider; j <= 0; j++)
                            {
                                double r;
                                if ((i + j) < 0 | (i + j) > budjetsList.Count - 1)
                                {
                                    r = 0;
                                    divider--;
                                }
                                else
                                {
                                    r = budjetsList[i + j].Ratio;
                                }
                                bjetI.NeighboursAvRatio += r;
                            }
                            bjetI.NeighboursAvRatio /= (divider + 1);
                        }
                        else
                        {
                            bjetI.NeighboursAvRatio = bjetI.Ratio;
                        }
                        bjetI.PPBudjet = (bjetI.Ratio / bjetI.Budget) * 10000;
                    }


                    var orderedBudjetsRatio = from b in budjetsList
                                              orderby b.PPBudjet descending, b.NeighboursAvRatio descending, b.Ratio descending
                                              select b;
                    
                    List<int> canceledBetsCountList = new List<int>();
                    foreach (var budjet in orderedBudjetsRatio)
                    {
                        //if (canceledBetsCountList.Count > 50)
                        //{
                        //    break;
                        //}
                        if (!canceledBetsCountList.Contains(budjet.CanceledN) | budjet.MaxBet == maxBetSize)
                        {
                            canceledBetsCountList.Add(budjet.CanceledN);
                            bestBudjetsDisplay.Add(budjet);
                        }
                    }

                    var orderedBudjetsRatioAsc = from b in budjetsList
                                                 orderby b.PPBudjet ascending, b.NeighboursAvRatio ascending, b.Ratio ascending
                                                 select b;
                    canceledBetsCountList = new List<int>();
                    
                    foreach (var budjet in orderedBudjetsRatioAsc)
                    {
                        //if (canceledBetsCountList.Count > 50)
                        //{
                        //    break;
                        //}
                        if (!canceledBetsCountList.Contains(budjet.CanceledN) | budjet.MaxBet == maxBetSize)
                        {
                            canceledBetsCountList.Add(budjet.CanceledN);
                            worstBudjetsDisplay.Add(budjet);
                        }
                    }
                }
                catch (Exception)
                {
                }
            });
            budjetsW._bestBudjetsDataGrid.ItemsSource = bestBudjetsDisplay;
            budjetsW._worstBudjetsDataGrid.ItemsSource = worstBudjetsDisplay;
            budjetsW.Show();
        }

        private async void BestPricesButton_Click(object sender, RoutedEventArgs e)
        {
            int betlineId;
            BFBDBEntities context = new BFBDBEntities();
            betlineId = (_betlineId.SelectedItem as DBLBF.Betline).BetlineId;
            Betline betline = context.Betlines.Find(betlineId);
            BudjetsWindow budjetsW = new BudjetsWindow();
            budjetsW.Left = 50 + (_taxPercent.SelectedIndex * 20);
            budjetsW.Top = 50 + (_taxPercent.SelectedIndex * 20);
            budjetsW.Title = string.Format("Betline name {0} of BetType: {1}.", betline.betlineName, betline.betType);            
            List<PriceStats> priceStatsList = new List<PriceStats>();
            await Task.Run(() =>
            {
                var betlinesPlacedBets = from b in betline.PlacedBets
                                         orderby b.averagePrice
                                         select b;
                double maxPrice = betlinesPlacedBets.LastOrDefault().averagePrice.Value;
                double minPrice = betlinesPlacedBets.FirstOrDefault().averagePrice.Value;                
                for (double k = minPrice; k <= maxPrice; k= Math.Round(k+0.01,2))
                {
                    var placedBetsFiltered = from b in betlinesPlacedBets
                                             where b.averagePrice == k
                                             orderby b.Selection
                                             select b;
                    if (placedBetsFiltered.Count()<=0)
                    {
                        continue;
                    }
                    PriceStats priceStat = new PriceStats();
                    priceStat.SelectionStatsList = new List<PriceStats.SelectionStats>();
                    priceStat.TotalBets = placedBetsFiltered.Count();
                    priceStat.Price = k;
                    try
                    {
                        foreach (var item in betline.Filter.marketNames.Trim(':').Split(':'))
                        {
                            for (int i = 1; i < 18; i++)
                            {                                
                                var iteBets = from b in placedBetsFiltered
                                              where GetSortedOrder(b) == i
                                              select b;
                                if (iteBets.Count() > 0)
                                {
                                    PriceStats.SelectionStats stats = new PriceStats.SelectionStats();
                                    switch (item)
                                    {
                                        case "Match Odds":
                                            if (i == 3)
                                            {
                                                stats.Selection = "X";
                                            }
                                            else
                                            {
                                                stats.Selection = i.ToString();
                                            }
                                            break;
                                        case "Correct Score" :
                                            stats.Selection = GetResultFmSelection(i.ToString());
                                            break;
                                        default:
                                            stats.Selection = i.ToString();
                                            break;
                                    }
                                    stats.BetsCount = iteBets.Count();
                                    var wonCount = (from b in iteBets
                                                    where b.success
                                                    select b.success).Count();
                                    stats.Won = Math.Round(betline.betType == "Back" ? ((double)wonCount / stats.BetsCount) * 100.00 : 100.00 - ((double)wonCount * 100 / stats.BetsCount), 2);
                                    
                                    priceStat.SelectionStatsList.Add(stats);
                                }
                            }
                        }
                        priceStatsList.Add(priceStat);
                    }
                    catch (Exception)
                    {
                    }
                }
                
            });
            WrapPanel mainPanel = new WrapPanel();
            foreach (var item in priceStatsList)
            {
                StackPanel detailPanel = new StackPanel();
                Label title = new Label();
                title.Content = string.Format("{0} of {1} ",item.Price,item.TotalBets);
                DataGrid detailsGrid = new DataGrid();
                detailsGrid.ItemsSource = item.SelectionStatsList;
                detailsGrid.AutoGenerateColumns = true;
                detailPanel.Children.Add(title);
                detailPanel.Children.Add(detailsGrid);
                mainPanel.Children.Add(detailPanel);
            }
            budjetsW.Content = mainPanel;
            budjetsW.Show();
        }

        private async void BestPricesAllButton_Click(object sender, RoutedEventArgs e)
        {
            DatabaseDBL.BFBDBFinalEntities context = new DatabaseDBL.BFBDBFinalEntities();
            BudjetsWindow budjetsW = new BudjetsWindow();
            List<PriceStats> priceStatsList = new List<PriceStats>();
            await Task.Run(() =>
            {
                var betlinesPlacedBets = (from b in context.PlacedBets
                                         orderby b.averagePrice
                                         select b).ToList();
                double maxPrice = betlinesPlacedBets.LastOrDefault().averagePrice.Value;
                double minPrice = betlinesPlacedBets.FirstOrDefault().averagePrice.Value;
                for (double k = minPrice; k <= maxPrice; k = Math.Round(k + 0.01, 2))
                {
                    var placedBetsFiltered = from b in betlinesPlacedBets
                                             where b.averagePrice == k
                                             orderby b.Selection
                                             select b;
                    if (placedBetsFiltered.Count() <= 0)
                    {
                        continue;
                    }
                    PriceStats priceStat = new PriceStats();
                    priceStat.SelectionStatsList = new List<PriceStats.SelectionStats>();
                    priceStat.TotalBets = placedBetsFiltered.Count();
                    priceStat.Price = k;
                    try
                    {
                        var marketsList = new List<string>() { "Match Odds","Correct Score"};
                        foreach (var item in marketsList)
                        {
                            for (int i = 1; i < 18; i++)
                            {
                                var iteBets = from b in placedBetsFiltered
                                              where b.sortedOrder == i
                                              select b;
                                if (iteBets.Count() > 0)
                                {
                                    PriceStats.SelectionStats stats = new PriceStats.SelectionStats();
                                    switch (item)
                                    {
                                        case "Match Odds":
                                            if (i == 3)
                                            {
                                                stats.Selection = "X";
                                            }
                                            else
                                            {
                                                stats.Selection = i.ToString();
                                            }
                                            break;
                                        case "Correct Score":
                                            stats.Selection = GetResultFmSelection(i.ToString());
                                            break;
                                        default:
                                            stats.Selection = i.ToString();
                                            break;
                                    }
                                    stats.BetsCount = iteBets.Count();
                                    var wonCount = (from b in iteBets
                                                    where b.success
                                                    select b.success).Count();
                                    string bettype = "Lay";
                                    stats.Won = Math.Round(bettype == "Back" ? ((double)wonCount / stats.BetsCount) * 100.00 : 100.00 - ((double)wonCount * 100 / stats.BetsCount), 2);

                                    priceStat.SelectionStatsList.Add(stats);
                                }
                            }
                        }
                        priceStatsList.Add(priceStat);
                    }
                    catch (Exception)
                    {
                    }
                }

            });
            WrapPanel mainPanel = new WrapPanel();
            
            foreach (var item in priceStatsList)
            {
                StackPanel detailPanel = new StackPanel();
                Label title = new Label();
                title.Content = string.Format("{0} of {1} ", item.Price, item.TotalBets);
                DataGrid detailsGrid = new DataGrid();
                detailsGrid.ItemsSource = item.SelectionStatsList;
                detailsGrid.AutoGenerateColumns = true;
                detailPanel.Children.Add(title);
                detailPanel.Children.Add(detailsGrid);
                mainPanel.Children.Add(detailPanel);
            }
            ScrollViewer sv = new ScrollViewer();
            sv.Content = mainPanel;
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            sv.CanContentScroll = true;
            budjetsW.Content = sv;
            budjetsW.MaxHeight = 500;
            budjetsW.Show();
        }
    }
}
