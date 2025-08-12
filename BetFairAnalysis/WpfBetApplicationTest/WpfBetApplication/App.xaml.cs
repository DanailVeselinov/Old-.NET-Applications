using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;

namespace WpfBetApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            if (processes.Count() > 1)
            {
                MessageBox.Show("Application already Running.");
                Process.GetCurrentProcess().Kill();
            }
            //while (!CheckConnection())
            //{
            //}            
            base.OnStartup(e);
        }
        private bool CheckConnection()
        {
            bool kleir = false;
            using (Ping ping = new Ping())
            {
                try
                {
                    if (ping.Send("http://www.betfair.com/exchange/inplay/", 2000).Status == IPStatus.Success)
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
    }
}
