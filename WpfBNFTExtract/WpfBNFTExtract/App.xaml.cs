using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using WpfBNFTExtract.Properties;

namespace WpfBNFTExtract
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                if (System.IO.File.Exists("srvr"))
                {
                    var rdr = System.IO.File.ReadAllText("srvr");
                    var selectedServerText = rdr.Replace(" ", string.Empty);
                    FindDatabase();
                }
            }
            catch (Exception)
            {

            }
        }

        private bool FindDatabase()
        {
            var servers = GetServersList();
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
                            int lastSymbol = context.Database.Connection.ConnectionString.IndexOf(';', firstSymbol);
                            string oldServerName = context.Database.Connection.ConnectionString.Substring(firstSymbol, lastSymbol - firstSymbol);
                            context.Database.Connection.ConnectionString = context.Database.Connection.ConnectionString.Replace(oldServerName, selectedServerText);
                            var crew = context.CWCREW.FirstOrDefault();
                            var vsl = context.VSL.FirstOrDefault();
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
                                            if (atr.Name.LocalName == "name" && atr.Value == "BNFT_VSLEntities" &&
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
                                return true;
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
            return false;
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
        public static void ChangeConnectionString(string newDatasource)
        {
            try
            {
                XDocument doc = XDocument.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                var query = from p in doc.Descendants("connectionStrings").Descendants()
                            select p;
                foreach (var child in query)
                {
                    foreach (var atr in child.Attributes())
                    {
                        if (atr.Name.LocalName == "name" && atr.Value == "BNFT_VSLEntities" &&
                            atr.NextAttribute != null && atr.NextAttribute.Name == "connectionString")
                        {

                            if (atr.NextAttribute.Value.Contains(newDatasource))
                            {
                                return;
                            }
                            else
                            {
                                var oldstring = atr.NextAttribute.Value.ToUpper();
                                var first = oldstring.IndexOf("DATA SOURCE=");
                                var last = oldstring.IndexOf(";",first);
                                var oldProvider = atr.NextAttribute.Value.Substring(first+12, last - first-12);
                                atr.NextAttribute.Value = atr.NextAttribute.Value.Replace(oldProvider, newDatasource);
                            }                            
                        }
                    }
                }
                doc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                RestartApp();
            }
            catch (Exception)
            {
            }
        }

        public static void RestartApp()
        {
            try
            {
                App.Current.Shutdown();
            }
            catch (Exception)
            {
                return;
            }
            System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
            return;
        }

    }
}
