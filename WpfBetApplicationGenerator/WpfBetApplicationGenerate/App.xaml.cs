using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
            StopSimilarProcesses(Process.GetCurrentProcess().ProcessName);
            base.OnStartup(e);
        }

        private static void StopSimilarProcesses(string ProcessName)
        {
            var processes = Process.GetProcessesByName(ProcessName);
            if (processes.Count() > 1)
            {
                var orderedProcesses = from pr in processes
                                       orderby pr.StartTime ascending
                                       select pr;
                var firstStartTime = orderedProcesses.FirstOrDefault().StartTime;
                var latestProcessesList = from p in processes
                                          where p.StartTime > firstStartTime
                                          select p;
                if (latestProcessesList.Count() > 0)
                {
                    MessageBox.Show("Application already Running.");
                    foreach (var item in latestProcessesList)
                    {
                        item.Kill();
                    }
                }
            }
        }
    }
}
