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
using System.Xml.Linq;

namespace TestDB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public static List<Server> GetServersList()
        {
            var server = System.Data.Sql.SqlDataSourceEnumerator.Instance.GetDataSources().Rows;
            var listServers = new List<Server>();
            foreach (System.Data.DataRow item in server)
            {
                var ser = new Server(item.ItemArray[0].ToString(), item.ItemArray[1].ToString(), item.ItemArray[3].ToString());
                listServers.Add(ser);
            }
            return listServers;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var servers = MainWindow.GetServersList();
            XDocument doc = XDocument.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            foreach (var selectedServer in servers)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(selectedServer.ServerName) | string.IsNullOrWhiteSpace(selectedServer.InstanceName))
                    {
                        continue;
                    }
                    else
                    {
                        try
                        {
                            var selectedServerText = string.Format("{0}\\{1}", selectedServer.ServerName, selectedServer.InstanceName);
                            var context = new BNFT_VSLEntities();
                            int firstSymbol = context.Database.Connection.ConnectionString.IndexOf("data source=") + 12;
                            int lastSymbol = context.Database.Connection.ConnectionString.IndexOf(';',firstSymbol);
                            string oldServerName = context.Database.Connection.ConnectionString.Substring(firstSymbol, lastSymbol - firstSymbol);
                            context.Database.Connection.ConnectionString = context.Database.Connection.ConnectionString.Replace(oldServerName, selectedServerText);
                            var crew = context.CWCREWs.FirstOrDefault();
                            var vsl = context.VSLs.FirstOrDefault();
                            if (crew != null & vsl != null)
	                        {
		                        try
                                {
                                    var query = from p in doc.Descendants("connectionStrings").Descendants()
                                                select p;
                                    foreach (var child in query)
                                    {
                                        foreach (var atr in child.Attributes())
                                        {
                                            if (atr.Name.LocalName == "name" && atr.Value == "BNFT_VSL_DataEntities" &&
                                                atr.NextAttribute != null && atr.NextAttribute.Name == "connectionString")
                                            {

                                                if (!atr.NextAttribute.Value.Contains(selectedServerText))
                                                {
                                                    var oldstring = atr.NextAttribute.Value.ToUpper();
                                                    var first = oldstring.IndexOf("DATA SOURCE=");
                                                    var last = oldstring.IndexOf(";", first);
                                                    var oldProvider = atr.NextAttribute.Value.Substring(first + 12, last - first - 12);
                                                    atr.NextAttribute.Value = atr.NextAttribute.Value.Replace(oldProvider, selectedServerText);
                                                    doc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                                                }
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                }
                                catch (Exception)
                                {

                                    throw;
                                }
                                App.ChangeConnectionString(selectedServerText);
                                App.RestartApp();
	                        }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }
    }
}
