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
using System.Xml.Linq;

namespace WpfBNFTExtract
{
    /// <summary>
    /// Interaction logic for WindowStartBNFT.xaml
    /// </summary>
    public partial class WindowStartBNFT : Window
    {
        public WindowStartBNFT()
        {
            this.Resources.Add("RGBConverter", new RGBConverter());
            this.Resources.Add("RGBConverterForeground", new RGBConverterForeground());
            Mouse.OverrideCursor = Cursors.Arrow;
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<System.Diagnostics.Process> proc = System.Diagnostics.Process.GetProcesses().ToList();
                var procesBNFT = proc.Find(p => p.ProcessName.ToUpper().Contains("BNFT_VSL"));
                if (procesBNFT == null)
                {
                    MessageBox.Show("Please open BNFT_VSL program when prompted and try again.");                    
                }
                string workingDirectory = procesBNFT.MainModule.FileName;
                int lastInd = workingDirectory.ToUpper().LastIndexOf("\\PROGRAM");
                workingDirectory = workingDirectory.Substring(0, lastInd + 1) + "CONNECT_DB\\UserLogin.udl";
                var reader =  System.IO.File.OpenText(workingDirectory);
                var connection = reader.ReadToEnd();
                ChangeConnectionString(connection);
                DialogResult = true;
                Mouse.OverrideCursor = Cursors.Wait;
            }
            catch (Exception)
            {
                DialogResult = false;
                Mouse.OverrideCursor = Cursors.Wait;
                MessageBox.Show("Please open BNFT_VSL program when prompted and try again.");
            }
        }

        public static void ChangeConnectionString(string connectionFile)
        {
            try
            {
                string dataSource = GetTagContent(connectionFile, "DATA SOURCE");
                string passowrd = GetTagContent(connectionFile, "Password");
                string userId = GetTagContent(connectionFile, "User ID");
                string dataBaseName = GetTagContent(connectionFile, "Initial Catalog");
                
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
                            var dataSourceXML = GetTagContent(atr.NextAttribute.Value, "data source");
                            var passwordXML = GetTagContent(atr.NextAttribute.Value, "password");
                            var userIdXML = GetTagContent(atr.NextAttribute.Value, "user id");
                            var dataBaseNameXML = GetTagContent(atr.NextAttribute.Value, "initial catalog");
                            if (dataSourceXML.ToUpper() == dataSource.ToUpper() & passwordXML.ToUpper() == passowrd.ToUpper() & userIdXML.ToUpper() == userId.ToUpper() & dataBaseNameXML.ToUpper() == dataBaseName.ToUpper())
                            {
                                return;
                            }
                            else
                            {
                                var oldstring = atr.NextAttribute.Value;
                                oldstring = ConnectionStringReplace("data source", dataSource, oldstring);
                                oldstring = ConnectionStringReplace("initial catalog", dataBaseName, oldstring);
                                oldstring = ConnectionStringReplace("user id", userId, oldstring);
                                oldstring = ConnectionStringReplace("password", passowrd, oldstring);
                                atr.NextAttribute.Value = oldstring;
                            }
                        }
                    }
                }
                doc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                App.RestartApp();
            }
            catch (Exception)
            {
            }
        }

        private static string ConnectionStringReplace(string tag ,string newValue, string connectionString)
        {
            var first = connectionString.ToUpper().IndexOf(tag.ToUpper() + "=");
            var last = connectionString.IndexOf(";", first);
            newValue = tag + "=" + newValue;
            var oldValue = connectionString.Substring(first, last - first);
            return connectionString.Replace(oldValue, newValue);
        }

        private static string GetTagContent(string connectionFile, string tag)
        {
            int StartIndex = connectionFile.ToUpper().IndexOf(tag.ToUpper() + "=") + tag.Length +1;
            int LastIndex = connectionFile.ToUpper().IndexOf(';', StartIndex);
            if (LastIndex == -1)
            {
                var i = StartIndex;
                
                while (LastIndex <= 0)
                {
                    try
                    {
                        char c = connectionFile.ToUpper()[i];
                        if (!char.IsLetterOrDigit(c) & c != '\\')
                        {
                            LastIndex = i;
                            continue;
                        }
                        i++;
                        if (i>=connectionFile.Length)
                        {
                            LastIndex = i;
                            continue;
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        LastIndex = i - 1;
                    }
                    
                }
            }
            return connectionFile.Substring(StartIndex, LastIndex - StartIndex);
        }
    }
}
