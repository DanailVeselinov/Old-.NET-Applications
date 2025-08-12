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

namespace BetFairBot
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialogWindow = ((sender as Button).Parent as StackPanel);
            List<byte> byteList = new List<byte>();
            try
            {
                WrapPanel checkBoxStackPanel = dialogWindow.FindName("_CheckBoxStackPanel") as WrapPanel;
                for (int i = 0; i < checkBoxStackPanel.Children.Count; i++)
                {
                    CheckBox cb = checkBoxStackPanel.Children[i] as CheckBox;
                    if (cb != null)
                    {
                        if ((bool)cb.IsChecked)
                        {
                            byteList.Add((byte)(i + 1));
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            var marketLabel = dialogWindow.FindName("_TitleLabel") as Label;
            var marketLabelType = dialogWindow.FindName("_TitleLabelType") as Label;
            // Ex. Match Odds,1,2,3;Correct Score,1,2,3;
            var filterSO = MainWindow.CurrentBetline.Filters.sortedOrders;
            var marketName = marketLabelType.Content.ToString();
            var sortedOrdersMarket = marketName + MainWindow.SortedOrderSeparator;
            foreach (var item in byteList)
            {
                sortedOrdersMarket += item + MainWindow.SortedOrderSeparator;
            }
            sortedOrdersMarket = sortedOrdersMarket.Trim(MainWindow.SortedOrderSeparator[0]);
            if (string.IsNullOrEmpty(filterSO))
            {
                MainWindow.CurrentBetline.Filters.sortedOrders = sortedOrdersMarket + MainWindow.MainSeparator;
            }
            else
            {
                var marketsSO = filterSO.Trim(MainWindow.MainSeparator[0]).Split(MainWindow.MainSeparator[0]).ToList();
                var msoElement = marketsSO.FirstOrDefault(m => m.ToUpper().Contains(marketName.ToUpper()));
                if (string.IsNullOrWhiteSpace(msoElement))
                {
                    marketsSO.Add(sortedOrdersMarket);
                }
                else
                {
                    if (byteList.Count > 0)
                    {
                        marketsSO[marketsSO.IndexOf(msoElement)] = sortedOrdersMarket;
                    }
                    else
                    {
                        marketsSO.Remove(msoElement);
                    }
                }
                var newFilterSO = "";
                foreach (var so in marketsSO)
                {
                    newFilterSO += so + MainWindow.MainSeparator;
                }
                MainWindow.CurrentBetline.Filters.sortedOrders = newFilterSO;
            }
            Close();
        }
    }
}
