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

namespace WpfBetApplication
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
            var dialogWindow = this;
            List<byte> byteList = new List<byte>();
            try
            {
                StackPanel checkBoxStackPanel = _CheckBoxStackPanel;
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
            var marketLabel = _TitleLabel;
            switch (marketLabel.Content.ToString())
            {
                case "Match Odds":
                    Session.Betline.Filter.SortedOrdersMatchOddsList = byteList;
                    break;
                case "Correct Score":
                    Session.Betline.Filter.SortedOrdersCorrectScoreList = byteList;
                    break;
                default:
                    break;
            }
            Close();
        }
    }
}
