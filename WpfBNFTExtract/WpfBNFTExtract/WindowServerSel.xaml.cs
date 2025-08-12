using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfBNFTExtract
{
    /// <summary>
    /// Interaction logic for WindowServerSel.xaml
    /// </summary>
    public partial class WindowServerSel : Window
    {
       public WindowServerSel()
        {            
            InitializeComponent();
            var servers = MainWindow.GetServersList();
            ServersGrid.ItemsSource = servers;


        }

        private void ButtonSelect_Click(object sender, RoutedEventArgs e)
        {            
            var selectedServer = (ServersGrid.SelectedItem as Server);
            if (string.IsNullOrWhiteSpace(selectedServer.ServerName) | string.IsNullOrWhiteSpace(selectedServer.InstanceName))
            {
                DataContext = null;
                DialogResult = false;
                this.Close();
            }
            else
            {
                DataContext = selectedServer;
                try
                {
                    var selectedServerText = string.Format("{0}\\{1}", selectedServer.ServerName, selectedServer.InstanceName);                    
                    App.ChangeConnectionString(selectedServerText);
                    App.RestartApp();
                    this.Close();
                }
                catch (Exception)
                {
                    this.Close();
                }

                
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DataContext = null;
            DialogResult = false;
            this.Close();
        }
    }
}