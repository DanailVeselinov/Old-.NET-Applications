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
using System.Windows.Shapes;
using DBLBettingApp;
using WpfBetApplication.BFGlobalService;

namespace WpfBetApplication
{
    /// <summary>
    /// Interaction logic for EditBetlineWindow.xaml
    /// </summary>
    public partial class EditBetlineWindow : Window
    {
        public EditBetlineWindow()
        {
            InitializeComponent();            
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            var dataContext = DataContext as DBLBettingApp.Betline;
            try
            {
                dataContext.initialProfitPerBet = double.Parse(_profitPerBetTB.Text);
                dataContext.Filter.maxAmmountIncremented = double.Parse(_maxAmountTB.Text);
            }
            catch (Exception)
            {
            }            
        }

        private void _marketName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listview = (sender as ListView);
            if (listview.SelectedItems.Count>0)
            {
                _marketAssets.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                _marketAssets.Visibility = System.Windows.Visibility.Collapsed;
            }
            List<string> selList = (this.DataContext as DBLBettingApp.Betline).Filter.marketNames.Trim(':').Split(':').ToList();
            foreach (string item in e.RemovedItems)
            {
                if (selList.Contains(item))
                {
                    selList.Remove(item);                    
                }
                switch (item)
                {
                    case "Match Odds":
                        _matchOddsAssets.Visibility = System.Windows.Visibility.Collapsed;
                        break;
                    case "Correct Score":
                        _correctScoreAssets.Visibility = System.Windows.Visibility.Collapsed;
                        break;
                    default:
                        break;
                }
            }
            foreach (string item in e.AddedItems)
            {
                if (!selList.Contains(item))
                {
                    selList.Add(item);                    
                }
                switch (item)
                {
                    case "Match Odds":
                        _matchOddsAssets.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case "Correct Score":
                        _correctScoreAssets.Visibility = System.Windows.Visibility.Visible;
                        break;
                    default:
                        break;
                }
            }
            (this.DataContext as DBLBettingApp.Betline).Filter.marketNames = FilterMarkets.ArrayToString(selList.ToArray());
        }

        private void _eventsEdit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            List<string> selList = (this.DataContext as DBLBettingApp.Betline).Filter.eventTypeIds.Trim(':').Split(':').ToList();
            foreach (EventType item in e.RemovedItems)
            {
                if (selList.Contains(item.id.ToString()))
                {
                    selList.Remove(item.id.ToString());
                }
            }
            foreach (EventType item in e.AddedItems)
            {
                if (!selList.Contains(item.id.ToString()))
                {
                    selList.Add(item.id.ToString());
                }
            }
            (this.DataContext as DBLBettingApp.Betline).Filter.eventTypeIds = FilterMarkets.ArrayToString(selList.ToArray());
        }

        private void _countryCodes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<string> selList = (this.DataContext as DBLBettingApp.Betline).Filter.marketCountries.Trim(':').Split(':').ToList();
            foreach (CountryCode item in e.RemovedItems)
            {
                if (selList.Contains(item.CountryCode1.ToString()))
                {
                    selList.Remove(item.CountryCode1.ToString());
                }
            }
            foreach (CountryCode item in e.AddedItems)
            {
                if (!selList.Contains(item.CountryCode1.ToString()))
                {
                    selList.Add(item.CountryCode1.ToString());
                }
            }
            (this.DataContext as DBLBettingApp.Betline).Filter.marketCountries = FilterMarkets.ArrayToString(selList.ToArray());
        }
        
        private void _BetType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var editWindow = App.Current.Windows.OfType<EditBetlineWindow>().FirstOrDefault();
                DBLBettingApp.Betline dataContext = editWindow.DataContext as DBLBettingApp.Betline;
                dataContext.betType = ((sender as ComboBox).SelectedItem as ComboBoxItem).Content.ToString().Substring(0, 1);
            }
            catch (Exception)
            {
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var contextBetline = (DataContext as DBLBettingApp.Betline);
            _BetType.SelectedIndex = contextBetline.betType == "B" ? 0 : 1;
            for (int i = 0; i < _algo.Items.Count; i++)
            {
                System.Collections.Generic.KeyValuePair<int, string> kvItem = (System.Collections.Generic.KeyValuePair<int, string>)_algo.Items[i];
                if (kvItem.Value == contextBetline.algorithmName)
                {
                    _algo.SelectedIndex = i;
                    continue;
                }
            }
            
            //BindMarketAssets();
        }

        //private void BindMarketAssets()
        //{
        //    var dataContext = this.DataContext as DBLBettingApp.Betline;
        //    foreach (string marketName in _marketName.Items)
        //    {
        //        switch (marketName)
        //        {
        //            case "Match Odds":
        //                string[] arrayMatchOdds = { "1", "2", "X" };
        //                for (int i = 0; i < arrayMatchOdds.Length; i++)
        //                {
        //                    CheckBox cb = new CheckBox();
        //                    cb.IsChecked = dataContext == null ? true : dataContext.Filter.sortedOrdersMatchOdds.Contains((i+1).ToString());
        //                    cb.Content = arrayMatchOdds[i];                            
        //                    cb.FontSize = 10;
        //                    cb.Margin = new Thickness(5);
        //                    var binding = new Binding();
        //                    cb.Checked += cb_CheckedMatchOdds;
        //                    cb.Unchecked +=cb_UncheckedMatchOds;
        //                    _matchOddsAssets.Children.Add(cb);
        //                }
        //                break;
        //            case "Correct Score":
        //                string[] arrayCorrectScore = { "0-0", "0-1", "0-2", "0-3", "1-0", "1-1", "1-2", "1-3", "2-0", "2-1", "2-2", "2-3", "3-0", "3-1", "3-2", "3-3", "Any Unquoted" };
        //                for (int i = 0; i < arrayCorrectScore.Length; i++)
        //                {
        //                    CheckBox cb = new CheckBox();
        //                    cb.IsChecked = dataContext == null ? true : dataContext.Filter.sortedOrdersCorrectScore.Contains((i+1).ToString());
        //                    cb.Content = arrayCorrectScore[i];
        //                    cb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
        //                    cb.Margin = new Thickness(5);
        //                    cb.FontSize = 10;
        //                    cb.Checked += cb_CheckedCorrectScore;
        //                    cb.Unchecked += cb_UncheckedCorrectScore;
        //                    _correctScoreAssets.Children.Add(cb);
        //                }

        //                break;
        //            default:
        //                return;
        //        }
                
        //    }
            
        //}

        //void cb_UncheckedCorrectScore(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        List<string> listCorrectScore = new List<string>() { "0-0", "0-1", "0-2", "0-3", "1-0", "1-1", "1-2", "1-3", "2-0", "2-1", "2-2", "2-3", "3-0", "3-1", "3-2", "3-3", "Any Unquoted" };
        //        var cb = (e.Source as CheckBox);
        //        var betline = (this.DataContext as DBLBettingApp.Betline);
        //        if (!cb.IsLoaded)
        //        {
        //            return;
        //        }
        //        var aritem = listCorrectScore.IndexOf(cb.Content.ToString()) + 1;
        //        if (betline.Filter.sortedOrdersCorrectScore.Contains(aritem.ToString()))
        //        {
        //            betline.Filter.sortedOrdersCorrectScore = betline.Filter.sortedOrdersCorrectScore.Replace(aritem.ToString() + ":", string.Empty);
        //        }
        //        e.Handled = true;
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

        //void cb_CheckedCorrectScore(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        List<string> listCorrectScore = new List<string>(){ "0-0", "0-1", "0-2", "0-3", "1-0", "1-1", "1-2", "1-3", "2-0", "2-1", "2-2", "2-3", "3-0", "3-1", "3-2", "3-3", "Any Unquoted" };
        //        var cb = (e.Source as CheckBox);
        //        var betline = (this.DataContext as DBLBettingApp.Betline);
        //        if (!cb.IsLoaded)
        //        {
        //            return;
        //        }
        //        var aritem = listCorrectScore.IndexOf(cb.Content.ToString())+1;
        //        if (!betline.Filter.sortedOrdersCorrectScore.Contains(aritem.ToString()))
        //        {
        //            betline.Filter.sortedOrdersCorrectScore += aritem.ToString() + ":";
        //        }
        //        e.Handled = true;
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

        //void cb_UncheckedMatchOds(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var cb = (e.Source as CheckBox);
        //        var betline = (this.DataContext as DBLBettingApp.Betline);
        //        if (!cb.IsLoaded)
        //        {
        //            return;
        //        }
        //        var content = (cb.Content.ToString() == "X") ? "3" : (string)cb.Content;
        //        if (betline.Filter.sortedOrdersMatchOdds.Contains(content))
        //        {
        //            betline.Filter.sortedOrdersMatchOdds = betline.Filter.sortedOrdersMatchOdds.Replace((content.ToString() + ":"), string.Empty);
        //        }
        //        e.Handled = true;
        //    }
        //    catch (Exception)
        //    {
        //    }   
        //}

        //void cb_CheckedMatchOdds(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var cb = (e.Source as CheckBox);
        //        var betline = (this.DataContext as DBLBettingApp.Betline);
        //        if (!cb.IsLoaded)
        //        {
        //            return;
        //        }
        //        var content = (cb.Content.ToString() == "X") ? "3" : cb.Content.ToString();
        //        if (!betline.Filter.sortedOrdersMatchOdds.Contains(content))
        //        {
        //            betline.Filter.sortedOrdersMatchOdds+= content.ToString() + ":";
        //        }
        //        e.Handled = true;
        //    }
        //    catch (Exception)
        //    {
        //    }            
        //}

        private void _algo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender as ComboBox).IsLoaded)
            {
                return;
            }
            var contextBetline = (DataContext as DBLBettingApp.Betline);
            var comboAlgo = (sender as ComboBox);
            System.Collections.Generic.KeyValuePair<int,string> val = (System.Collections.Generic.KeyValuePair<int,string>)comboAlgo.SelectedItem;
            contextBetline.algorithmName = val.Value;
        }
    }
    
    public class BoolRadioButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var splitParameter = parameter.ToString().Trim(':').Split(':');
                switch (splitParameter[0])
                {
                    case "Null":
                        return value == null ? true : false;
                    case "True":
                        return value == null ? false : ((bool)value ? true : false);
                    case "False":
                        return value == null ? false : ((bool)value ? false : true);
                    case "1":
                        return value == null ? false : ((byte)value == 1 ? true : false);
                    case "2":
                        return value == null ? false : ((byte)value == 2 ? true : false);
                    case "MT":
                        return value.ToString().Contains(splitParameter[1]);
                    default:
                        return "";
                }
            }
            catch (Exception)
            {
                return false;
            }            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var editWindow = App.Current.Windows.OfType<EditBetlineWindow>().FirstOrDefault();
                DBLBettingApp.Betline dataContext = editWindow.DataContext as DBLBettingApp.Betline;
                //if ((bool)value)
                //{
                    var splitParameter = parameter.ToString().Trim(':').Split(':');
                    switch (splitParameter[0])
                    {
                        case "Null":
                            return null;
                        case "True":
                            return true;
                        case "False":
                            return false;
                        case "1":
                            return 1;
                        case "2":
                            return 2;
                        case "MT":
                            if ((bool)value)
                            {
                                return dataContext.Filter.marketBettingTypes.Contains(splitParameter[1]) ? dataContext.Filter.marketTypes : string.Format("{0}{1}:", dataContext.Filter.marketTypes, splitParameter[1]); 
                            }
                            else
                            {
                                var newList = dataContext.Filter.marketBettingTypes.Trim(':').Split(':').ToList();
                                return newList.Remove(splitParameter[1]) ? FilterMarkets.ArrayToString(newList.ToArray()) : dataContext.Filter.marketTypes; 
                            }
                        default:
                            return null;
                    }
                //}
                //return "";
            }
            catch (Exception)
            {
                return "";
            }
            
        }
    }


    public class ListSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((string)parameter)
            {
                case "M":
                    if (string.IsNullOrWhiteSpace((string)value))
                    {
                        return "";
                    }
                    var editWindow = App.Current.Windows.OfType<EditBetlineWindow>().FirstOrDefault();
                    var mn = (editWindow.DataContext as DBLBettingApp.Betline).Filter.marketNames;
                    editWindow._marketName.SelectedItems.Clear();
                    foreach (string item in (mn).Trim(':').Split(':'))
                    {
                        editWindow._marketName.SelectedItems.Add(item);
                    }
                    return "";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace((string)value))
            {
                return "";
            }
            var editWindow = App.Current.Windows.OfType<EditBetlineWindow>().FirstOrDefault();
            return (editWindow.DataContext as DBLBettingApp.Betline).Filter.marketNames;
        }
    }        

}
