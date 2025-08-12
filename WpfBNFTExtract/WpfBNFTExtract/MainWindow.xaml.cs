using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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

namespace WpfBNFTExtract
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
                Mouse.OverrideCursor = Cursors.Wait;
                Cursor = Cursors.Wait;
                this.Resources.Add("RGBConverter", new RGBConverter());
                this.Resources.Add("RGBConverterForeground", new RGBConverterForeground());
                this.Resources.Add("HolidaysList", GetListHolidays());
                this.Resources.Add("SliderValue", GetSliderValue());
                InitializeComponent();
                try
                {
                    ThemeSlider.Value = (int)Resources["SliderValue"];
                }
                catch (Exception)
                {
                }
                UpdateLastFiles();
            }
            catch (Exception exa)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exa.Message, "\\b\\r Stack Trace:", exa.StackTrace));

            }
        }

        private int GetSliderValue()
        {
            int sliderValue = 60;
            try
            {
                var fileResult = System.IO.File.OpenText("slider");
                var value = fileResult.ReadLine();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    try
                    {
                        sliderValue = int.Parse(value.Trim());
                    }
                    catch (Exception)
                    {
                    }
                }                
                return sliderValue;
            }
            catch (Exception exa)
            {
                return sliderValue;
            }
        }

        private void UpdateLastFiles()
        {
            try
            {
                var dir = System.IO.Directory.GetCurrentDirectory();
                if (!System.IO.Directory.Exists(dir+"\\History"))
                {
                    return;
                }
                var lastList = System.IO.Directory.GetFiles(dir+"\\History", "*FormDailyReport*.*", System.IO.SearchOption.TopDirectoryOnly);
                string lastItem;
                if (lastList.Count() < 1)
                {
                    return;
                }
                var datesList = new List<DateTime>();
                foreach (string item in lastList)
                {
                    int first = item.ToUpper().IndexOf("FORMDAILYREPORT");
                    first = first + 15;
                    lastItem = item.ToUpper().Substring(first);
                    lastItem = lastItem.ToUpper().Replace(".XLS", "");
                    try
                    {
                        var date = DateTime.Parse(lastItem);
                        datesList.Add(date);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Error: date Format.");
                    }
                }
                datesList.Sort();
                LastDailyReport.Content = "Last: " + datesList.Last().ToString("dd/MMM/yyyy");
            }
            catch (Exception exa)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exa.Message, "\\b\\r Stack Trace:", exa.StackTrace));

            }
        }

        

        public ObservableCollection<DateTime> GetListHolidays()
        {
            try
            {
                ObservableCollection<DateTime> holidaysDates = new ObservableCollection<DateTime>();
                ObservableCollection<DateTime> holidaysDatesFinal = new ObservableCollection<DateTime>();
                var fileResult = System.IO.File.OpenText("holidays");
                bool hasline = true;
                while (hasline)
                {
                    var dateText = fileResult.ReadLine();
                    if (!string.IsNullOrWhiteSpace(dateText))
                    {
                        try
                        {
                            holidaysDates.Add(DateTime.Parse(dateText,System.Globalization.CultureInfo.InvariantCulture));
                        }
                        catch (Exception exa)
                        {
                            MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exa.Message, "\\b\\r Stack Trace:", exa.StackTrace));
                        }
                    }
                    else
                    {
                        hasline = false;
                    }
                }
                fileResult.Close();
                fileResult.Dispose();
                holidaysDates.OrderByDescending(d=>d);
                DateTime? today = DateTime.Now;
                try
                {
                    today = DPicker.SelectedDate.Value;
                }
                catch (Exception)
                {
                }
                var col = holidaysDates.OrderBy(d => d.Year).ThenBy(da => da.DayOfYear);
                foreach (DateTime date in col)
                {
                    if (date.Year == today.Value.Year)
	                {
                        holidaysDatesFinal.Add(date);
	                }
                }
                return holidaysDatesFinal;
            }
            catch (Exception exa)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exa.Message, "\\b\\r Stack Trace:", exa.StackTrace));
                return new ObservableCollection<DateTime>();
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Resources["HolidaysList"] = GetListHolidays();
                DataContext = new BNFT_VSLEntities();
                if (ShipName.Text.Length<1 | Flag.Text.Length<1 | IMONumber.Text.Length<1)
                {
                    try
                    {
                        PopulateShipData(DataContext as BNFT_VSLEntities);
                    }
                    catch (Exception)
                    {
                    }
                }
                var selectedDate = (sender as DatePicker).SelectedDate;
                //check for idtl East 
                var IDLPresent = (DataContext as BNFT_VSLEntities).PFINT_DATELINE.FirstOrDefault(dt => dt.DATE_LINE == selectedDate);
                if (IDLPresent!=null)
                {
                    IDLCB.Visibility = Visibility.Visible;
                }
                else
                {
                    IDLCB.IsChecked = false;
                    IDLCB.Visibility = Visibility.Collapsed;

                }
                //napravi da vzema za IDTL datata ako e checked
                List<TableRow> TableRowsList = GetTableRows(selectedDate);
                RESTHOURSGRID.DataContext = TableRowsList;
            }
            catch (System.Data.Entity.Core.EntityException)
            {
                if (!FindDatabase())
                {
                    MessageBox.Show("Please make shure your computer is connected to the network assocciated with the BNFT Rest Hours Program DataBase, and restart the application.");
                }
                return;
            }
            catch (Exception exa)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exa.Message, "\\b\\r Stack Trace:", exa.StackTrace));
            }
        }

        private bool FindDatabase()
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
                                catch (Exception exa)
                                {
                                    MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exa.Message, "\\b\\r Stack Trace:", exa.StackTrace));
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
            var startBNFTWindow = new WindowStartBNFT();
            startBNFTWindow.ThemeSlider.Value = ThemeSlider.Value;
            return startBNFTWindow.ShowDialog().Value;
        }

        private List<TableRow> GetTableRows(DateTime? selectedDate)
        {
            try
            {
                Int16 flWdate = 0;
                try
                {
                    if (IDLCB.IsChecked.Value)
                    {
                        flWdate = 1;
                    }
                }
                catch (Exception)
                {
                }
                var context = new BNFT_VSLEntities();
                var crewList = context.CWRESTHRS.Where(l => l.RH_DATE.Value == selectedDate & l.FL_EDate == flWdate);            
                var list = crewList.ToList();
                List<TableRow> TableRowsList = new List<TableRow>();
                List<CWRESTHRS> printList = new List<CWRESTHRS>();
                
                foreach (var cw in list)
                {
                    var dub = printList.FirstOrDefault(u => u.CWCREW_ID == cw.CWCREW_ID );
                    if (dub == null)
                    {
                        printList.Add(cw);
                    }
                }
                var dbuserList = context.CWCREW.ToList();
                var dbRANKList = context.CWPRANK.ToList();
                foreach (var item in printList)
                {
                    try
                    {
                        switch (item.H01A) { case "R": item.H01A = ""; break; case "E": item.H01A = "w"; break; case "W": item.H01A = "d"; break; case "V": item.H01A = "d"; break; default: item.H01A = "w"; break; }
                        switch (item.H01B) { case "R": item.H01B = ""; break; case "E": item.H01B = "w"; break; case "W": item.H01B = "d"; break; case "V": item.H01B = "d"; break; default: item.H01B = "w"; break; }
                        switch (item.H02A) { case "R": item.H02A = ""; break; case "E": item.H02A = "w"; break; case "W": item.H02A = "d"; break; case "V": item.H02A = "d"; break; default: item.H02A = "w"; break; }
                        switch (item.H02B) { case "R": item.H02B = ""; break; case "E": item.H02B = "w"; break; case "W": item.H02B = "d"; break; case "V": item.H02B = "d"; break; default: item.H02B = "w"; break; }
                        switch (item.H03A) { case "R": item.H03A = ""; break; case "E": item.H03A = "w"; break; case "W": item.H03A = "d"; break; case "V": item.H03A = "d"; break; default: item.H03A = "w"; break; }
                        switch (item.H03B) { case "R": item.H03B = ""; break; case "E": item.H03B = "w"; break; case "W": item.H03B = "d"; break; case "V": item.H03B = "d"; break; default: item.H03B = "w"; break; }
                        switch (item.H04A) { case "R": item.H04A = ""; break; case "E": item.H04A = "w"; break; case "W": item.H04A = "d"; break; case "V": item.H04A = "d"; break; default: item.H04A = "w"; break; }
                        switch (item.H04B) { case "R": item.H04B = ""; break; case "E": item.H04B = "w"; break; case "W": item.H04B = "d"; break; case "V": item.H04B = "d"; break; default: item.H04B = "w"; break; }
                        switch (item.H05A) { case "R": item.H05A = ""; break; case "E": item.H05A = "w"; break; case "W": item.H05A = "d"; break; case "V": item.H05A = "d"; break; default: item.H05A = "w"; break; }
                        switch (item.H05B) { case "R": item.H05B = ""; break; case "E": item.H05B = "w"; break; case "W": item.H05B = "d"; break; case "V": item.H05B = "d"; break; default: item.H05B = "w"; break; }
                        switch (item.H06A) { case "R": item.H06A = ""; break; case "E": item.H06A = "w"; break; case "W": item.H06A = "d"; break; case "V": item.H06A = "d"; break; default: item.H06A = "w"; break; }
                        switch (item.H06B) { case "R": item.H06B = ""; break; case "E": item.H06B = "w"; break; case "W": item.H06B = "d"; break; case "V": item.H06B = "d"; break; default: item.H06B = "w"; break; }
                        switch (item.H07A) { case "R": item.H07A = ""; break; case "E": item.H07A = "w"; break; case "W": item.H07A = "d"; break; case "V": item.H07A = "d"; break; default: item.H07A = "w"; break; }
                        switch (item.H07B) { case "R": item.H07B = ""; break; case "E": item.H07B = "w"; break; case "W": item.H07B = "d"; break; case "V": item.H07B = "d"; break; default: item.H07B = "w"; break; }
                        switch (item.H08A) { case "R": item.H08A = ""; break; case "E": item.H08A = "w"; break; case "W": item.H08A = "d"; break; case "V": item.H08A = "d"; break; default: item.H08A = "w"; break; }
                        switch (item.H08B) { case "R": item.H08B = ""; break; case "E": item.H08B = "w"; break; case "W": item.H08B = "d"; break; case "V": item.H08B = "d"; break; default: item.H08B = "w"; break; }
                        switch (item.H09A) { case "R": item.H09A = ""; break; case "E": item.H09A = "w"; break; case "W": item.H09A = "d"; break; case "V": item.H09A = "d"; break; default: item.H09A = "w"; break; }
                        switch (item.H09B) { case "R": item.H09B = ""; break; case "E": item.H09B = "w"; break; case "W": item.H09B = "d"; break; case "V": item.H09B = "d"; break; default: item.H09B = "w"; break; }
                        switch (item.H10A) { case "R": item.H10A = ""; break; case "E": item.H10A = "w"; break; case "W": item.H10A = "d"; break; case "V": item.H10A = "d"; break; default: item.H10A = "w"; break; }
                        switch (item.H10B) { case "R": item.H10B = ""; break; case "E": item.H10B = "w"; break; case "W": item.H10B = "d"; break; case "V": item.H10B = "d"; break; default: item.H10B = "w"; break; }
                        switch (item.H11A) { case "R": item.H11A = ""; break; case "E": item.H11A = "w"; break; case "W": item.H11A = "d"; break; case "V": item.H11A = "d"; break; default: item.H11A = "w"; break; }
                        switch (item.H11B) { case "R": item.H11B = ""; break; case "E": item.H11B = "w"; break; case "W": item.H11B = "d"; break; case "V": item.H11B = "d"; break; default: item.H11B = "w"; break; }
                        switch (item.H12A) { case "R": item.H12A = ""; break; case "E": item.H12A = "w"; break; case "W": item.H12A = "d"; break; case "V": item.H12A = "d"; break; default: item.H12A = "w"; break; }
                        switch (item.H12B) { case "R": item.H12B = ""; break; case "E": item.H12B = "w"; break; case "W": item.H12B = "d"; break; case "V": item.H12B = "d"; break; default: item.H12B = "w"; break; }
                        switch (item.H13A) { case "R": item.H13A = ""; break; case "E": item.H13A = "w"; break; case "W": item.H13A = "d"; break; case "V": item.H13A = "d"; break; default: item.H13A = "w"; break; }
                        switch (item.H13B) { case "R": item.H13B = ""; break; case "E": item.H13B = "w"; break; case "W": item.H13B = "d"; break; case "V": item.H13B = "d"; break; default: item.H13B = "w"; break; }
                        switch (item.H14A) { case "R": item.H14A = ""; break; case "E": item.H14A = "w"; break; case "W": item.H14A = "d"; break; case "V": item.H14A = "d"; break; default: item.H14A = "w"; break; }
                        switch (item.H14B) { case "R": item.H14B = ""; break; case "E": item.H14B = "w"; break; case "W": item.H14B = "d"; break; case "V": item.H14B = "d"; break; default: item.H14B = "w"; break; }
                        switch (item.H15A) { case "R": item.H15A = ""; break; case "E": item.H15A = "w"; break; case "W": item.H15A = "d"; break; case "V": item.H15A = "d"; break; default: item.H15A = "w"; break; }
                        switch (item.H15B) { case "R": item.H15B = ""; break; case "E": item.H15B = "w"; break; case "W": item.H15B = "d"; break; case "V": item.H15B = "d"; break; default: item.H15B = "w"; break; }
                        switch (item.H16A) { case "R": item.H16A = ""; break; case "E": item.H16A = "w"; break; case "W": item.H16A = "d"; break; case "V": item.H16A = "d"; break; default: item.H16A = "w"; break; }
                        switch (item.H16B) { case "R": item.H16B = ""; break; case "E": item.H16B = "w"; break; case "W": item.H16B = "d"; break; case "V": item.H16B = "d"; break; default: item.H16B = "w"; break; }
                        switch (item.H17A) { case "R": item.H17A = ""; break; case "E": item.H17A = "w"; break; case "W": item.H17A = "d"; break; case "V": item.H17A = "d"; break; default: item.H17A = "w"; break; }
                        switch (item.H17B) { case "R": item.H17B = ""; break; case "E": item.H17B = "w"; break; case "W": item.H17B = "d"; break; case "V": item.H17B = "d"; break; default: item.H17B = "w"; break; }
                        switch (item.H18A) { case "R": item.H18A = ""; break; case "E": item.H18A = "w"; break; case "W": item.H18A = "d"; break; case "V": item.H18A = "d"; break; default: item.H18A = "w"; break; }
                        switch (item.H18B) { case "R": item.H18B = ""; break; case "E": item.H18B = "w"; break; case "W": item.H18B = "d"; break; case "V": item.H18B = "d"; break; default: item.H18B = "w"; break; }
                        switch (item.H19A) { case "R": item.H19A = ""; break; case "E": item.H19A = "w"; break; case "W": item.H19A = "d"; break; case "V": item.H19A = "d"; break; default: item.H19A = "w"; break; }
                        switch (item.H19B) { case "R": item.H19B = ""; break; case "E": item.H19B = "w"; break; case "W": item.H19B = "d"; break; case "V": item.H19B = "d"; break; default: item.H19B = "w"; break; }
                        switch (item.H20A) { case "R": item.H20A = ""; break; case "E": item.H20A = "w"; break; case "W": item.H20A = "d"; break; case "V": item.H20A = "d"; break; default: item.H20A = "w"; break; }
                        switch (item.H20B) { case "R": item.H20B = ""; break; case "E": item.H20B = "w"; break; case "W": item.H20B = "d"; break; case "V": item.H20B = "d"; break; default: item.H20B = "w"; break; }
                        switch (item.H21A) { case "R": item.H21A = ""; break; case "E": item.H21A = "w"; break; case "W": item.H21A = "d"; break; case "V": item.H21A = "d"; break; default: item.H21A = "w"; break; }
                        switch (item.H21B) { case "R": item.H21B = ""; break; case "E": item.H21B = "w"; break; case "W": item.H21B = "d"; break; case "V": item.H21B = "d"; break; default: item.H21B = "w"; break; }
                        switch (item.H22A) { case "R": item.H22A = ""; break; case "E": item.H22A = "w"; break; case "W": item.H22A = "d"; break; case "V": item.H22A = "d"; break; default: item.H22A = "w"; break; }
                        switch (item.H22B) { case "R": item.H22B = ""; break; case "E": item.H22B = "w"; break; case "W": item.H22B = "d"; break; case "V": item.H22B = "d"; break; default: item.H22B = "w"; break; }
                        switch (item.H23A) { case "R": item.H23A = ""; break; case "E": item.H23A = "w"; break; case "W": item.H23A = "d"; break; case "V": item.H23A = "d"; break; default: item.H23A = "w"; break; }
                        switch (item.H23B) { case "R": item.H23B = ""; break; case "E": item.H23B = "w"; break; case "W": item.H23B = "d"; break; case "V": item.H23B = "d"; break; default: item.H23B = "w"; break; }
                        switch (item.H24A) { case "R": item.H24A = ""; break; case "E": item.H24A = "w"; break; case "W": item.H24A = "d"; break; case "V": item.H24A = "d"; break; default: item.H24A = "w"; break; }
                        switch (item.H24B) { case "R": item.H24B = ""; break; case "E": item.H24B = "w"; break; case "W": item.H24B = "d"; break; case "V": item.H24B = "d"; break; default: item.H24B = "w"; break; }
                        var user = dbuserList.FirstOrDefault(u => u.ID == item.CWCREW_ID);
                        if (user == null)
                        {
                            continue;
                        }
                        CWPRANK RANK = new CWPRANK();
                        try
                        {
                            RANK = dbRANKList.Find(r => r.ID == user.CWPRANK_ID);
                            if (RANK == null)
                            {
                                RANK = new CWPRANK();
                                MessageBox.Show(String.Format("Missing Rank of {0} {1}", user.FIRSTNAME, user.LASTNAME));
                                RANK.DESCR = "";
                                RANK.AA = 200;
                            }
                        }
                        catch (Exception exa)
                        {
                            MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exa.Message, "\\b\\r Stack Trace:", exa.StackTrace));
                        }
                        var n = user.LASTNAME + " " + user.FIRSTNAME;
                        var row = new TableRow(n, RANK.DESCR, RANK.AA, item.H01A, item.H01B, item.H02A, item.H02B, item.H03A, item.H03B, item.H04A, item.H04B, item.H05A, item.H05B, item.H06A, item.H06B, item.H07A, item.H07B, item.H08A, item.H08B, item.H09A, item.H09B, item.H10A, item.H10B, item.H11A, item.H11B, item.H12A, item.H12B, item.H13A, item.H13B, item.H14A, item.H14B, item.H15A, item.H15B, item.H16A, item.H16B, item.H17A, item.H17B, item.H18A, item.H18B, item.H19A, item.H19B, item.H20A, item.H20B, item.H21A, item.H21B, item.H22A, item.H22B, item.H23A, item.H23B, item.H24A, item.H24B, item.WRK_24);
                        TableRowsList.Add(row);
                    }
                    catch (System.InvalidOperationException)
                    {}
                    catch (NullReferenceException)
                    {}
                    catch (Exception exb)
                    {
                        MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exb.Message, "\\b\\r Stack Trace:", exb.StackTrace));
                    }
                }
                TableRowsList.Sort();
                return TableRowsList;
            }
            catch (Exception exc)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exc.Message, "\\b\\r Stack Trace:", exc.StackTrace));
                return new List<TableRow>();
            }
        }

        private List<TableRow> GetTableRowsBunker(DateTime? selectedDate)
        {
            try
            {

                var context = new BNFT_VSLEntities();
                var crewList = context.CWRESTHRS.Where(l => l.RH_DATE.Value == selectedDate);
                var list = crewList.ToList();
                var dbuserList = context.CWCREW.ToList();
                var dbRANKList = context.CWPRANK.ToList();
                List<TableRow> TableRowsList = new List<TableRow>();
                foreach (var item in list)
                {
                    switch (item.H01A) { case "R": item.H01A = ""; break; case "E": item.H01A = "w"; break; case "W": item.H01A = "w"; break; case "V": item.H01A = "w"; break; default: item.H01A = "w"; break; }
                    switch (item.H01B) { case "R": item.H01B = ""; break; case "E": item.H01B = "w"; break; case "W": item.H01B = "w"; break; case "V": item.H01B = "w"; break; default: item.H01B = "w"; break; }
                    switch (item.H02A) { case "R": item.H02A = ""; break; case "E": item.H02A = "w"; break; case "W": item.H02A = "w"; break; case "V": item.H02A = "w"; break; default: item.H02A = "w"; break; }
                    switch (item.H02B) { case "R": item.H02B = ""; break; case "E": item.H02B = "w"; break; case "W": item.H02B = "w"; break; case "V": item.H02B = "w"; break; default: item.H02B = "w"; break; }
                    switch (item.H03A) { case "R": item.H03A = ""; break; case "E": item.H03A = "w"; break; case "W": item.H03A = "w"; break; case "V": item.H03A = "w"; break; default: item.H03A = "w"; break; }
                    switch (item.H03B) { case "R": item.H03B = ""; break; case "E": item.H03B = "w"; break; case "W": item.H03B = "w"; break; case "V": item.H03B = "w"; break; default: item.H03B = "w"; break; }
                    switch (item.H04A) { case "R": item.H04A = ""; break; case "E": item.H04A = "w"; break; case "W": item.H04A = "w"; break; case "V": item.H04A = "w"; break; default: item.H04A = "w"; break; }
                    switch (item.H04B) { case "R": item.H04B = ""; break; case "E": item.H04B = "w"; break; case "W": item.H04B = "w"; break; case "V": item.H04B = "w"; break; default: item.H04B = "w"; break; }
                    switch (item.H05A) { case "R": item.H05A = ""; break; case "E": item.H05A = "w"; break; case "W": item.H05A = "w"; break; case "V": item.H05A = "w"; break; default: item.H05A = "w"; break; }
                    switch (item.H05B) { case "R": item.H05B = ""; break; case "E": item.H05B = "w"; break; case "W": item.H05B = "w"; break; case "V": item.H05B = "w"; break; default: item.H05B = "w"; break; }
                    switch (item.H06A) { case "R": item.H06A = ""; break; case "E": item.H06A = "w"; break; case "W": item.H06A = "w"; break; case "V": item.H06A = "w"; break; default: item.H06A = "w"; break; }
                    switch (item.H06B) { case "R": item.H06B = ""; break; case "E": item.H06B = "w"; break; case "W": item.H06B = "w"; break; case "V": item.H06B = "w"; break; default: item.H06B = "w"; break; }
                    switch (item.H07A) { case "R": item.H07A = ""; break; case "E": item.H07A = "w"; break; case "W": item.H07A = "w"; break; case "V": item.H07A = "w"; break; default: item.H07A = "w"; break; }
                    switch (item.H07B) { case "R": item.H07B = ""; break; case "E": item.H07B = "w"; break; case "W": item.H07B = "w"; break; case "V": item.H07B = "w"; break; default: item.H07B = "w"; break; }
                    switch (item.H08A) { case "R": item.H08A = ""; break; case "E": item.H08A = "w"; break; case "W": item.H08A = "w"; break; case "V": item.H08A = "w"; break; default: item.H08A = "w"; break; }
                    switch (item.H08B) { case "R": item.H08B = ""; break; case "E": item.H08B = "w"; break; case "W": item.H08B = "w"; break; case "V": item.H08B = "w"; break; default: item.H08B = "w"; break; }
                    switch (item.H09A) { case "R": item.H09A = ""; break; case "E": item.H09A = "w"; break; case "W": item.H09A = "w"; break; case "V": item.H09A = "w"; break; default: item.H09A = "w"; break; }
                    switch (item.H09B) { case "R": item.H09B = ""; break; case "E": item.H09B = "w"; break; case "W": item.H09B = "w"; break; case "V": item.H09B = "w"; break; default: item.H09B = "w"; break; }
                    switch (item.H10A) { case "R": item.H10A = ""; break; case "E": item.H10A = "w"; break; case "W": item.H10A = "w"; break; case "V": item.H10A = "w"; break; default: item.H10A = "w"; break; }
                    switch (item.H10B) { case "R": item.H10B = ""; break; case "E": item.H10B = "w"; break; case "W": item.H10B = "w"; break; case "V": item.H10B = "w"; break; default: item.H10B = "w"; break; }
                    switch (item.H11A) { case "R": item.H11A = ""; break; case "E": item.H11A = "w"; break; case "W": item.H11A = "w"; break; case "V": item.H11A = "w"; break; default: item.H11A = "w"; break; }
                    switch (item.H11B) { case "R": item.H11B = ""; break; case "E": item.H11B = "w"; break; case "W": item.H11B = "w"; break; case "V": item.H11B = "w"; break; default: item.H11B = "w"; break; }
                    switch (item.H12A) { case "R": item.H12A = ""; break; case "E": item.H12A = "w"; break; case "W": item.H12A = "w"; break; case "V": item.H12A = "w"; break; default: item.H12A = "w"; break; }
                    switch (item.H12B) { case "R": item.H12B = ""; break; case "E": item.H12B = "w"; break; case "W": item.H12B = "w"; break; case "V": item.H12B = "w"; break; default: item.H12B = "w"; break; }
                    switch (item.H13A) { case "R": item.H13A = ""; break; case "E": item.H13A = "w"; break; case "W": item.H13A = "w"; break; case "V": item.H13A = "w"; break; default: item.H13A = "w"; break; }
                    switch (item.H13B) { case "R": item.H13B = ""; break; case "E": item.H13B = "w"; break; case "W": item.H13B = "w"; break; case "V": item.H13B = "w"; break; default: item.H13B = "w"; break; }
                    switch (item.H14A) { case "R": item.H14A = ""; break; case "E": item.H14A = "w"; break; case "W": item.H14A = "w"; break; case "V": item.H14A = "w"; break; default: item.H14A = "w"; break; }
                    switch (item.H14B) { case "R": item.H14B = ""; break; case "E": item.H14B = "w"; break; case "W": item.H14B = "w"; break; case "V": item.H14B = "w"; break; default: item.H14B = "w"; break; }
                    switch (item.H15A) { case "R": item.H15A = ""; break; case "E": item.H15A = "w"; break; case "W": item.H15A = "w"; break; case "V": item.H15A = "w"; break; default: item.H15A = "w"; break; }
                    switch (item.H15B) { case "R": item.H15B = ""; break; case "E": item.H15B = "w"; break; case "W": item.H15B = "w"; break; case "V": item.H15B = "w"; break; default: item.H15B = "w"; break; }
                    switch (item.H16A) { case "R": item.H16A = ""; break; case "E": item.H16A = "w"; break; case "W": item.H16A = "w"; break; case "V": item.H16A = "w"; break; default: item.H16A = "w"; break; }
                    switch (item.H16B) { case "R": item.H16B = ""; break; case "E": item.H16B = "w"; break; case "W": item.H16B = "w"; break; case "V": item.H16B = "w"; break; default: item.H16B = "w"; break; }
                    switch (item.H17A) { case "R": item.H17A = ""; break; case "E": item.H17A = "w"; break; case "W": item.H17A = "w"; break; case "V": item.H17A = "w"; break; default: item.H17A = "w"; break; }
                    switch (item.H17B) { case "R": item.H17B = ""; break; case "E": item.H17B = "w"; break; case "W": item.H17B = "w"; break; case "V": item.H17B = "w"; break; default: item.H17B = "w"; break; }
                    switch (item.H18A) { case "R": item.H18A = ""; break; case "E": item.H18A = "w"; break; case "W": item.H18A = "w"; break; case "V": item.H18A = "w"; break; default: item.H18A = "w"; break; }
                    switch (item.H18B) { case "R": item.H18B = ""; break; case "E": item.H18B = "w"; break; case "W": item.H18B = "w"; break; case "V": item.H18B = "w"; break; default: item.H18B = "w"; break; }
                    switch (item.H19A) { case "R": item.H19A = ""; break; case "E": item.H19A = "w"; break; case "W": item.H19A = "w"; break; case "V": item.H19A = "w"; break; default: item.H19A = "w"; break; }
                    switch (item.H19B) { case "R": item.H19B = ""; break; case "E": item.H19B = "w"; break; case "W": item.H19B = "w"; break; case "V": item.H19B = "w"; break; default: item.H19B = "w"; break; }
                    switch (item.H20A) { case "R": item.H20A = ""; break; case "E": item.H20A = "w"; break; case "W": item.H20A = "w"; break; case "V": item.H20A = "w"; break; default: item.H20A = "w"; break; }
                    switch (item.H20B) { case "R": item.H20B = ""; break; case "E": item.H20B = "w"; break; case "W": item.H20B = "w"; break; case "V": item.H20B = "w"; break; default: item.H20B = "w"; break; }
                    switch (item.H21A) { case "R": item.H21A = ""; break; case "E": item.H21A = "w"; break; case "W": item.H21A = "w"; break; case "V": item.H21A = "w"; break; default: item.H21A = "w"; break; }
                    switch (item.H21B) { case "R": item.H21B = ""; break; case "E": item.H21B = "w"; break; case "W": item.H21B = "w"; break; case "V": item.H21B = "w"; break; default: item.H21B = "w"; break; }
                    switch (item.H22A) { case "R": item.H22A = ""; break; case "E": item.H22A = "w"; break; case "W": item.H22A = "w"; break; case "V": item.H22A = "w"; break; default: item.H22A = "w"; break; }
                    switch (item.H22B) { case "R": item.H22B = ""; break; case "E": item.H22B = "w"; break; case "W": item.H22B = "w"; break; case "V": item.H22B = "w"; break; default: item.H22B = "w"; break; }
                    switch (item.H23A) { case "R": item.H23A = ""; break; case "E": item.H23A = "w"; break; case "W": item.H23A = "w"; break; case "V": item.H23A = "w"; break; default: item.H23A = "w"; break; }
                    switch (item.H23B) { case "R": item.H23B = ""; break; case "E": item.H23B = "w"; break; case "W": item.H23B = "w"; break; case "V": item.H23B = "w"; break; default: item.H23B = "w"; break; }
                    switch (item.H24A) { case "R": item.H24A = ""; break; case "E": item.H24A = "w"; break; case "W": item.H24A = "w"; break; case "V": item.H24A = "w"; break; default: item.H24A = "w"; break; }
                    switch (item.H24B) { case "R": item.H24B = ""; break; case "E": item.H24B = "w"; break; case "W": item.H24B = "w"; break; case "V": item.H24B = "w"; break; default: item.H24B = "w"; break; }
                    var user = dbuserList.Find(u => u.ID == item.CWCREW_ID);
                    CWPRANK RANK = new CWPRANK();
                    try
                    {
                        RANK = dbRANKList.Find(r => r.ID == user.CWPRANK_ID);
                        if (RANK == null)
                        {
                            RANK = new CWPRANK();
                            MessageBox.Show(String.Format("Missing Rank of {0} {1}", user.FIRSTNAME, user.LASTNAME));
                            RANK.DESCR = "";
                            RANK.AA = 200;
                        }
                    }
                    catch (Exception exb)
                    {
                        MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exb.Message, "\\b\\r Stack Trace:", exb.StackTrace));                    
                    }
                    var n = user.LASTNAME + " " + user.FIRSTNAME;
                    var row = new TableRow(n, RANK.DESCR, RANK.AA, item.H01A, item.H01B, item.H02A, item.H02B, item.H03A, item.H03B, item.H04A, item.H04B, item.H05A, item.H05B, item.H06A, item.H06B, item.H07A, item.H07B, item.H08A, item.H08B, item.H09A, item.H09B, item.H10A, item.H10B, item.H11A, item.H11B, item.H12A, item.H12B, item.H13A, item.H13B, item.H14A, item.H14B, item.H15A, item.H15B, item.H16A, item.H16B, item.H17A, item.H17B, item.H18A, item.H18B, item.H19A, item.H19B, item.H20A, item.H20B, item.H21A, item.H21B, item.H22A, item.H22B, item.H23A, item.H23B, item.H24A, item.H24B, item.WRK_24);
                    TableRowsList.Add(row);
                }
                TableRowsList.Sort();
                return TableRowsList;
            }
            catch (Exception exc)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exc.Message, "\\b\\r Stack Trace:", exc.StackTrace));
                return new List<TableRow>();
            }
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

        private void ChangeDataSourceButton_Click(object sender, RoutedEventArgs e)
        {
            var serversWindow = new WindowServerSel();
            serversWindow.Owner = this;
            serversWindow.ShowDialog();

            //var context = DataContext as BNFT_VSLEntities;
            //context.Database.Connection.ConnectionString = context.Database.Connection.ConnectionString.Replace("EDDY-PC\\BNFT2008", "CHIEFOFFICER-PC\\BNFT2008");
        }

        private void DailyReport_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            excel.FileValidation = Microsoft.Office.Core.MsoFileValidationMode.msoFileValidationSkip;
            try
            {                
                var dir = System.IO.Directory.GetCurrentDirectory();
                var histDir = System.IO.Directory.GetCurrentDirectory() + "\\History";
                if (!System.IO.Directory.Exists(histDir))
                {
                    System.IO.Directory.CreateDirectory(histDir);
                }
                var wb = excel.Workbooks.Open(dir + "\\Forms.xls");
                wb.BeforeClose += wb_BeforeClose;
                var today = DPicker.SelectedDate.Value;
                string url = string.Format("{3}\\FormDailyReport{0:0000}.{1:00}.{2:00}.xls", today.Year, today.Month, today.Day, histDir);
                foreach (Microsoft.Office.Interop.Excel.Worksheet item in excel.Sheets)
                {
                    if (item.Name.ToUpper().Contains("DAILYREPORT"))
                    {
                        item.Visible = Microsoft.Office.Interop.Excel.XlSheetVisibility.xlSheetVisible;
                        PrintDailyReport(item);
                    }
                    else
                    {
                        item.Visible = Microsoft.Office.Interop.Excel.XlSheetVisibility.xlSheetVeryHidden;
                    }
                }
                wb.SaveAs(url);
                UpdateLastFiles();
            }
            catch (Exception exc)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exc.Message, "\\b\\r Stack Trace:", exc.StackTrace));

                this.IsEnabled = true;
                try
                {
                    foreach (Microsoft.Office.Interop.Excel.Workbook item in excel.Workbooks)
                    {

                        item.Close(false, Type.Missing, Type.Missing);
                            
                    }
                    excel.Application.Quit();
                    Mouse.OverrideCursor = Cursors.Arrow;
                }
                catch (Exception)
                {
                    Mouse.OverrideCursor = Cursors.Arrow; 
                }
            }
            this.IsEnabled = true;
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        public void PrintDailyReport(Microsoft.Office.Interop.Excel.Worksheet item)
        {
            try
            {
                item.Cells.set_Item(7, 1, "VESSEL'S NAME:" + ShipName.Text);
                var selectedDate = DPicker.SelectedDate;
                double notHolidayOvertimeReduce = GetNotHolidayOvertimeReduce(selectedDate);
                if (selectedDate.HasValue)
                {
                    var tableRows = RESTHOURSGRID.DataContext as List<TableRow>;
                    if (tableRows.Count<=0)
                    {
                        return;
                    }
                    int r = tableRows.Count - 1;
                    for (int row = r; row >= 0; row--)
                    {
                        var userRow = new DailyUserPrint();
                        userRow.Name = tableRows[row].Name;
                        userRow.Rank = tableRows[row].Rank;
                        var ovr = tableRows[row].overtime + notHolidayOvertimeReduce;
                        userRow.Overtime = ovr < 0 ? 0 : ovr;
                        RowCompact(tableRows[row], userRow);
                        PrintRowToExcel(userRow, item);
                    }
                    var ra = item.Cells.get_Range("A12", "L12");
                    ra.Delete(Microsoft.Office.Interop.Excel.XlDeleteShiftDirection.xlShiftUp);
                    //Show the Excel Window
                    item.Application.Visible = true;
                    item.Application.ActiveWindow.WindowState = Microsoft.Office.Interop.Excel.XlWindowState.xlMaximized;
                    item.Application.ActiveWindow.Visible = true;
                    item.Application.ActiveWindow.Activate();                    
                    //print
                    //var dialogWindow = new PrintDialog();
                    //if (dialogWindow.ShowDialog().Value)
                    //{
                    //    try
                    //    {
                    //        item.Application.ActivePrinter = dialogWindow.PrintQueue.QueueDriver.Name;
                    //    }
                    //    catch (Exception)
                    //    {
                    //    }
                    //    item.PrintOutEx();
                    //}

                }
                else
                {
                    MessageBox.Show("Please select date");
                    DPicker.Focus();
                }
            }
            catch (Exception exa)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exa.Message, "\\b\\r Stack Trace:", exa.StackTrace));
            }
        }

        private void PrintRowToExcel(DailyUserPrint userRow, Microsoft.Office.Interop.Excel.Worksheet item)
        {
            item.Cells.set_Item(7, 10, DPicker.SelectedDate.Value);
            var rowsNumber = userRow.GetRowsNumber();
            int firstRow = 12;
            for (int i = rowsNumber - 1; i > -1; i--)
            {
                var range = item.Cells.get_Range("A" + firstRow, "L" + firstRow);
                range.Cells.Font.Size = 12;
                try
                {
                    if (!(userRow.watchkeepingPeriods.Count < (i + 1))) { item.Cells.set_Item(12, 3, userRow.watchkeepingPeriods[i].From); item.Cells.set_Item(12, 4, userRow.watchkeepingPeriods[i].To); item.Cells.set_Item(12, 5, userRow.watchkeepingPeriods[i].Hours); }
                }
                catch (Exception exw) {
                    MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exw.Message, "\\b\\r Stack Trace:", exw.StackTrace));
                }
                try { if (!(userRow.otherWorkPeriods.Count < (i + 1))) { item.Cells.set_Item(12, 6, userRow.otherWorkPeriods[i].From); item.Cells.set_Item(12, 7, userRow.otherWorkPeriods[i].To); item.Cells.set_Item(12, 8, userRow.otherWorkPeriods[i].Hours); } }
                catch (Exception exo) {
                    MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exo.Message, "\\b\\r Stack Trace:", exo.StackTrace));
                }
                try { if (!(userRow.restTimePeriods.Count < (i + 1))) { item.Cells.set_Item(12, 9, userRow.restTimePeriods[i].From); item.Cells.set_Item(12, 10, userRow.restTimePeriods[i].To); item.Cells.set_Item(12, 11, userRow.restTimePeriods[i].Hours); } }
                catch (Exception exr) {
                    MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exr.Message, "\\b\\r Stack Trace:", exr.StackTrace));
                }
                if (i == 0)
                {
                    var rankCells = item.Cells.get_Range("A" + firstRow, "A" + (firstRow + rowsNumber - 1));
                    rankCells.Merge();
                    rankCells.WrapText = true;
                    rankCells.Cells.VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                    rankCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                    rankCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop).Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlMedium;
                    rankCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                    rankCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlMedium;
                    item.Cells.set_Item(12, 1, userRow.Rank);
                    var nameCells = item.Cells.get_Range("B" + firstRow, "B" + (firstRow + rowsNumber - 1));
                    nameCells.Merge();

                    nameCells.Cells.VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignTop;
                    nameCells.Cells.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
                    nameCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                    nameCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop).Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlMedium;
                    nameCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                    nameCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlMedium;

                    item.Cells.set_Item(12, 2, userRow.Name);
                    var hoursCells = item.Cells.get_Range("L" + firstRow, "L" + (firstRow + rowsNumber - 1));
                    hoursCells.Merge();
                    hoursCells.Cells.VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                    hoursCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                    hoursCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop).Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlMedium;
                    hoursCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                    hoursCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlMedium;

                    item.Cells.set_Item(12, 12, userRow.Overtime);
                }
                range.Insert(Microsoft.Office.Interop.Excel.XlInsertShiftDirection.xlShiftDown);
            }
        }

        private static void RowCompact(TableRow row, DailyUserPrint userRow)
        {
            var startTime = "0000";
            var endTime = "0000";
            double per = 0;
            var currentWorkTipe = row.H01A;
            per += 0.5;
            if (row.H01B != currentWorkTipe)
            {
                endTime = "0030";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "0030";
                per = 0;
                currentWorkTipe = row.H01B;

            }
            per += 0.5;
            if (row.H02A != currentWorkTipe)
            {
                endTime = "0100";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "0100";
                per = 0;
                currentWorkTipe = row.H02A;
            }
            per += 0.5;
            if (row.H02B != currentWorkTipe)
            {
                endTime = "0130";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "0130";
                per = 0;
                currentWorkTipe = row.H02B;
            }

            per += 0.5;
            if (row.H03A != currentWorkTipe)
            {
                endTime = "0200";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "0200";
                per = 0;
                currentWorkTipe = row.H03A;
            }
            per += 0.5;
            if (row.H03B != currentWorkTipe)
            {
                endTime = "0230";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "0230";
                per = 0;
                currentWorkTipe = row.H03B;
            }

            per += 0.5;
            if (row.H04A != currentWorkTipe)
            {
                endTime = "0300";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "0300";
                per = 0;
                currentWorkTipe = row.H04A;
            }
            per += 0.5;
            if (row.H04B != currentWorkTipe)
            {
                endTime = "0330";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "0330";
                per = 0;
                currentWorkTipe = row.H04B;
            }

            per += 0.5;
            if (row.H05A != currentWorkTipe)
            {
                endTime = "0400";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "0400";
                per = 0;
                currentWorkTipe = row.H05A;
            }
            per += 0.5;
            if (row.H05B != currentWorkTipe)
            {
                endTime = "0430";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "0430";
                per = 0;
                currentWorkTipe = row.H05B;
            }

            per += 0.5;
            if (row.H06A != currentWorkTipe)
            {
                endTime = "0500";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "0500";
                per = 0;
                currentWorkTipe = row.H06A;
            }
            per += 0.5;
            if (row.H06B != currentWorkTipe)
            {
                endTime = "0530";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "0530";
                per = 0;
                currentWorkTipe = row.H06B;
            }

            per += 0.5;
            if (row.H07A != currentWorkTipe)
            {
                endTime = "0600";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "0600";
                per = 0;
                currentWorkTipe = row.H07A;
            }
            per += 0.5;
            if (row.H07B != currentWorkTipe)
            {
                endTime = "0630";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "0630";
                per = 0;
                currentWorkTipe = row.H07B;
            }

            per += 0.5;
            if (row.H08A != currentWorkTipe)
            {
                endTime = "0700";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "0700";
                per = 0;
                currentWorkTipe = row.H08A;
            }
            per += 0.5;
            if (row.H08B != currentWorkTipe)
            {
                endTime = "0730";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "0730";
                per = 0;
                currentWorkTipe = row.H08B;
            }

            per += 0.5;
            if (row.H09A != currentWorkTipe)
            {
                endTime = "0800";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "0800";
                per = 0;
                currentWorkTipe = row.H09A;
            }
            per += 0.5;
            if (row.H09B != currentWorkTipe)
            {
                endTime = "0830";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "0830";
                per = 0;
                currentWorkTipe = row.H09B;
            }

            per += 0.5;
            if (row.H10A != currentWorkTipe)
            {
                endTime = "0900";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "0900";
                per = 0;
                currentWorkTipe = row.H10A;
            }
            per += 0.5;
            if (row.H10B != currentWorkTipe)
            {
                endTime = "0930";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "0930";
                per = 0;
                currentWorkTipe = row.H10B;
            }

            per += 0.5;
            if (row.H11A != currentWorkTipe)
            {
                endTime = "1000";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1000";
                per = 0;
                currentWorkTipe = row.H11A;
            }
            per += 0.5;
            if (row.H11B != currentWorkTipe)
            {
                endTime = "1030";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1030";
                per = 0;
                currentWorkTipe = row.H11B;
            }

            per += 0.5;
            if (row.H12A != currentWorkTipe)
            {
                endTime = "1100";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1100";
                per = 0;
                currentWorkTipe = row.H12A;
            }
            per += 0.5;
            if (row.H12B != currentWorkTipe)
            {
                endTime = "1130";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1130";
                per = 0;
                currentWorkTipe = row.H12B;
            }

            per += 0.5;
            if (row.H13A != currentWorkTipe)
            {
                endTime = "1200";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1200";
                per = 0;
                currentWorkTipe = row.H13A;
            }
            per += 0.5;
            if (row.H13B != currentWorkTipe)
            {
                endTime = "1230";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1230";
                per = 0;
                currentWorkTipe = row.H13B;
            }

            per += 0.5;
            if (row.H14A != currentWorkTipe)
            {
                endTime = "1300";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1300";
                per = 0;
                currentWorkTipe = row.H14A;
            }
            per += 0.5;
            if (row.H14B != currentWorkTipe)
            {
                endTime = "1330";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1330";
                per = 0;
                currentWorkTipe = row.H14B;
            }

            per += 0.5;
            if (row.H15A != currentWorkTipe)
            {
                endTime = "1400";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1400";
                per = 0;
                currentWorkTipe = row.H15A;
            }
            per += 0.5;
            if (row.H15B != currentWorkTipe)
            {
                endTime = "1430";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1430";
                per = 0;
                currentWorkTipe = row.H15B;
            }

            per += 0.5;
            if (row.H16A != currentWorkTipe)
            {
                endTime = "1500";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1500";
                per = 0;
                currentWorkTipe = row.H16A;
            }
            per += 0.5;
            if (row.H16B != currentWorkTipe)
            {
                endTime = "1530";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1530";
                per = 0;
                currentWorkTipe = row.H16B;
            }

            per += 0.5;
            if (row.H17A != currentWorkTipe)
            {
                endTime = "1600";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1600";
                per = 0;
                currentWorkTipe = row.H17A;
            }
            per += 0.5;
            if (row.H17B != currentWorkTipe)
            {
                endTime = "1630";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1630";
                per = 0;
                currentWorkTipe = row.H17B;
            }

            per += 0.5;
            if (row.H18A != currentWorkTipe)
            {
                endTime = "1700";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1700";
                per = 0;
                currentWorkTipe = row.H18A;
            }
            per += 0.5;
            if (row.H18B != currentWorkTipe)
            {
                endTime = "1730";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1730";
                per = 0;
                currentWorkTipe = row.H18B;
            }

            per += 0.5;
            if (row.H19A != currentWorkTipe)
            {
                endTime = "1800";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1800";
                per = 0;
                currentWorkTipe = row.H19A;
            }
            per += 0.5;
            if (row.H19B != currentWorkTipe)
            {
                endTime = "1830";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1830";
                per = 0;
                currentWorkTipe = row.H19B;
            }

            per += 0.5;
            if (row.H20A != currentWorkTipe)
            {
                endTime = "1900";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1900";
                per = 0;
                currentWorkTipe = row.H20A;
            }
            per += 0.5;
            if (row.H20B != currentWorkTipe)
            {
                endTime = "1930";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "1930";
                per = 0;
                currentWorkTipe = row.H20B;
            }

            per += 0.5;
            if (row.H21A != currentWorkTipe)
            {
                endTime = "2000";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "2000";
                per = 0;
                currentWorkTipe = row.H21A;
            }
            per += 0.5;
            if (row.H21B != currentWorkTipe)
            {
                endTime = "2030";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "2030";
                per = 0;
                currentWorkTipe = row.H21B;
            }

            per += 0.5;
            if (row.H22A != currentWorkTipe)
            {
                endTime = "2100";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "2100";
                per = 0;
                currentWorkTipe = row.H22A;
            }
            per += 0.5;
            if (row.H22B != currentWorkTipe)
            {
                endTime = "2130";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "2130";
                per = 0;
                currentWorkTipe = row.H22B;
            }

            per += 0.5;
            if (row.H23A != currentWorkTipe)
            {
                endTime = "2200";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "2200";
                per = 0;
                currentWorkTipe = row.H23A;
            }
            per += 0.5;
            if (row.H23B != currentWorkTipe)
            {
                endTime = "2230";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "2230";
                per = 0;
                currentWorkTipe = row.H23B;
            }

            per += 0.5;
            if (row.H24A != currentWorkTipe)
            {
                endTime = "2300";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "2300";
                per = 0;
                currentWorkTipe = row.H24A;
            }
            per += 0.5;
            if (row.H24B != currentWorkTipe)
            {
                endTime = "2330";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
                startTime = "2330";
                per = 0;
                currentWorkTipe = row.H24B;
            }
            else
            {
                per += 0.5;
                endTime = "2400";
                switch (currentWorkTipe)
                {
                    case "":
                        userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "w":
                        userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    case "d":
                        userRow.watchkeepingPeriods.Add(new Period(startTime, endTime, per));
                        break;
                    default:
                        break;
                }
            }
        }

        private void MonthlyReport_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            excel.FileValidation = Microsoft.Office.Core.MsoFileValidationMode.msoFileValidationSkip;
            
            try
            {
                var dir = System.IO.Directory.GetCurrentDirectory();
                var histDir = System.IO.Directory.GetCurrentDirectory() + "\\History";
                if (!System.IO.Directory.Exists(histDir))
                {
                    System.IO.Directory.CreateDirectory(histDir);
                }
                var wb = excel.Workbooks.Open(dir + "\\Forms.xls");
                wb.BeforeClose += wb_BeforeClose;
                var today = DPicker.SelectedDate.Value;
                string url = string.Format("{2}\\FormMonthlyReport{0:0000}.{1:00}.xls", today.Year, today.Month, histDir);
                foreach (Microsoft.Office.Interop.Excel.Worksheet item in excel.Sheets)
                {
                    if (item.Name.ToUpper().Contains("OVERTIME"))
                    {
                        item.Visible = Microsoft.Office.Interop.Excel.XlSheetVisibility.xlSheetVisible;
                        PrintOvertimeReport(item);
                    }
                    else
                    {
                        item.Visible = Microsoft.Office.Interop.Excel.XlSheetVisibility.xlSheetVeryHidden;
                    }
                }
                wb.SaveAs(url);
            }
            catch (Exception exa)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exa.Message, "\\b\\r Stack Trace:", exa.StackTrace));
                try
                {
                    Mouse.OverrideCursor = Cursors.Arrow;
                    foreach (Microsoft.Office.Interop.Excel.Workbook item in excel.Workbooks)
                    {

                        item.Close();

                    }
                    excel.Application.Quit();
                }
                catch (Exception)
                {
                    Mouse.OverrideCursor = Cursors.Arrow;
                }
            }
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void PrintOvertimeReport(Microsoft.Office.Interop.Excel.Worksheet item)
        {
            try
            {
                item.Cells.set_Item(1, 1, "MV: " + ShipName.Text);
                var selectedDate = DPicker.SelectedDate;
                item.Cells.set_Item(4, 1, selectedDate);
                var crewDict = new Dictionary<int, double>();
                var context = new BNFT_VSLEntities();
                var allDaysInMonth = context.CWRESTHRS.Where(c => c.RH_DATE.Value.Year == selectedDate.Value.Year & c.RH_DATE.Value.Month == selectedDate.Value.Month).ToList();
                var daysInMonth = DateTime.DaysInMonth(selectedDate.Value.Year, selectedDate.Value.Month);
                for (int i = 1; i < daysInMonth + 1; i++)
                {
                    try
                    {
                        var day = new DateTime(selectedDate.Value.Year, selectedDate.Value.Month, i);
                        var daysRecords = allDaysInMonth.Where(d => d.RH_DATE == day);
                        double notHolidayOvertimeReduce = GetNotHolidayOvertimeReduce(day);
                        foreach (var record in daysRecords)
                        {
                            var ovr = (double)record.WRK_24 + notHolidayOvertimeReduce;
                            double overtime = ovr < 0 ? 0 : ovr;
                            try
                            {
                                crewDict[(int)record.CWCREW_ID] = crewDict[(int)record.CWCREW_ID] + overtime;
                            }
                            catch (Exception exb)
                            {
                                //MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exb.Message, "\\b\\r Stack Trace:", exb.StackTrace));
                                crewDict.Add((int)record.CWCREW_ID, overtime);
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exc.Message, "\\b\\r Stack Trace:", exc.StackTrace));

                    }                    
                }
                var crewList = new List<UserOvertime>();
                var dbcrewList = context.CWCREW.ToList();
                var dbranksList = context.CWPRANK.ToList();
                foreach (var id in crewDict)
                {
                    var us = new UserOvertime();
                    us.id = id.Key;
                    var cw = dbcrewList.Find(c => c.ID == id.Key);
                    CWPRANK rank = new CWPRANK();
                    try
                    {
                        rank = dbranksList.Find(r => r.ID == cw.CWPRANK_ID);
                        if (rank == null)
                        {
                            rank = new CWPRANK();
                            MessageBox.Show(String.Format("Missing Rank of {0} {1}", cw.FIRSTNAME,cw.LASTNAME));
                            rank.DESCR = "";
                            rank.AA = 200;
                        }
                    }
                    catch (Exception exa)
                    {
                        MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exa.Message, "\\b\\r Stack Trace:", exa.StackTrace));                        
                    }
                    us.sortOrder = (short)rank.AA;
                    us.Name = cw.LASTNAME + " " + cw.FIRSTNAME;
                    us.Rank = rank.DESCR;
                    us.totalOvertime = id.Value;
                    crewList.Add(us);
                }
                crewList.Sort();
                foreach (var itemCrew in crewList)
                {
                    PrintOvertimeRowToExcel(itemCrew, item);
                }
                var ra = item.Cells.get_Range("A10", "T10");
                ra.Delete(Microsoft.Office.Interop.Excel.XlDeleteShiftDirection.xlShiftUp);

                //Show the Excel Window
                item.Application.Visible = true;
                item.Application.ActiveWindow.WindowState = Microsoft.Office.Interop.Excel.XlWindowState.xlMaximized;
                item.Application.ActiveWindow.Visible = true;
            }

            catch (Exception e)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", e.Message, "\\b\\r Stack Trace:", e.StackTrace));
            }
        }

        private void PrintOvertimeRowToExcel(UserOvertime itemCrew, Microsoft.Office.Interop.Excel.Worksheet item)
        {
            try
            {

                int firstRow = 10;
                var range = item.Cells.get_Range("A" + firstRow, "C" + firstRow);
                range.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlDouble;
                range.Cells.Font.Size = 12;
                range.Cells.Font.Bold = false;
                item.Cells.get_Item(firstRow, 1).Cells.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
                item.Cells.set_Item(firstRow, 1, itemCrew.Name);
                item.Cells.set_Item(firstRow, 2, itemCrew.Rank);
                item.Cells.set_Item(firstRow, 3, itemCrew.totalOvertime);
                var nrange = item.Cells.get_Range("A" + firstRow, "C" + firstRow);
                nrange.Insert(Microsoft.Office.Interop.Excel.XlInsertShiftDirection.xlShiftDown);
            }

            catch (Exception e)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", e.Message, "\\b\\r Stack Trace:", e.StackTrace));
            }
        }


        private double GetNotHolidayOvertimeReduce(DateTime? selectedDate)
        {
            try
            {
                var fileResult = System.IO.File.ReadAllText("holidays");
                var isInListOfHolidays = fileResult.Contains(selectedDate.Value.ToShortDateString());
                double notHolidayOvertimeReduce = -8;
                if (selectedDate.Value.DayOfWeek == DayOfWeek.Saturday | selectedDate.Value.DayOfWeek == DayOfWeek.Sunday | isInListOfHolidays)
                {
                    notHolidayOvertimeReduce = 0;
                }
                return notHolidayOvertimeReduce;
            }
            catch (Exception)
            {
                return -8;
            }
        }

        private void BunkerReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                excel.FileValidation = Microsoft.Office.Core.MsoFileValidationMode.msoFileValidationSkip;
                try
                {

                    var dir = System.IO.Directory.GetCurrentDirectory();
                    var histDir = System.IO.Directory.GetCurrentDirectory() + "\\History";
                    if (!System.IO.Directory.Exists(histDir))
                    {
                        System.IO.Directory.CreateDirectory(histDir);
                    }
                    var wb = excel.Workbooks.Open(dir + "\\Forms.xls");
                    wb.BeforeClose += wb_BeforeClose;
                    var today = DPicker.SelectedDate.Value;
                    string url = string.Format("{3}\\FormBunkerReport{0:0000}.{1:00}.{2:00}.xls", today.Year, today.Month, today.Day, histDir);
                    foreach (Microsoft.Office.Interop.Excel.Worksheet item in excel.Sheets)
                    {
                        if (item.Name.ToUpper().Contains("BUNKER"))
                        {
                            item.Visible = Microsoft.Office.Interop.Excel.XlSheetVisibility.xlSheetVisible;
                            Mouse.OverrideCursor = Cursors.Arrow;
                            PrintBunkerReport(item);
                        }
                        else
                        {
                            item.Visible = Microsoft.Office.Interop.Excel.XlSheetVisibility.xlSheetVeryHidden;
                        }
                    }
                    wb.SaveAs(url);
                }
                catch (Exception exa)
                {
                    MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exa.Message, "\\b\\r Stack Trace:", exa.StackTrace));
                    try
                    {
                        Mouse.OverrideCursor = Cursors.Arrow;
                        foreach (Microsoft.Office.Interop.Excel.Workbook item in excel.Workbooks)
                        {

                            item.Close();

                        }
                        excel.Application.Quit();
                    }
                    catch (Exception exe)
                    {
                        Mouse.OverrideCursor = Cursors.Arrow;
                        MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exe.Message, "\\b\\r Stack Trace:", exe.StackTrace));
                    }
                }
                Mouse.OverrideCursor = Cursors.Arrow;
            }
            catch (Exception exd)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exd.Message, "\\b\\r Stack Trace:", exd.StackTrace));
            }
        }

        public void wb_BeforeClose(ref bool Cancel)
        {
            try
            {                
                var excelProcesses = System.Diagnostics.Process.GetProcesses().Where(p => p.ProcessName.ToUpper().Contains("EXCEL"));
                excelProcesses.OrderByDescending(ex => ex.StartTime);
                excelProcesses.First().Kill();
            }
            catch (Exception)
            {
            }
        }

        private void PrintBunkerReport(Microsoft.Office.Interop.Excel.Worksheet item)
        {
            try
            {
                item.Cells.set_Item(7, 1, "VESSEL'S NAME:" + ShipName.Text);
                var selectedDate = DPicker.SelectedDate;
                double notHolidayOvertimeReduce = GetNotHolidayOvertimeReduce(selectedDate);
                if (selectedDate.HasValue)
                {
                    var listRows = new List<BunkerUserPrint>();
                    var tableRows = GetTableRowsBunker(selectedDate.Value);
                    var tableRows1 = GetTableRowsBunker(selectedDate.Value.AddDays(-1));
                    var tableRows2 = GetTableRowsBunker(selectedDate.Value.AddDays(-2));
                    var userRow = new BunkerUserPrint();
                    for (int row = tableRows.Count - 1; row >= 0; row--)
                    {
                        userRow = new BunkerUserPrint();
                        userRow.SortOrder = tableRows[row].SortOrder;
                        userRow.Name = tableRows[row].Name;
                        userRow.Rank = tableRows[row].Rank;
                        var ovr = tableRows[row].overtime + notHolidayOvertimeReduce;
                        userRow.Overtime = ovr < 0 ? 0 : ovr;
                        BunkerRowCompact(tableRows[row], userRow, 0);
                        listRows.Add(userRow);
                    }
                    notHolidayOvertimeReduce = GetNotHolidayOvertimeReduce(selectedDate.Value.AddDays(-1));
                    for (int row1 = tableRows1.Count - 1; row1 >= 0; row1--)
                    {
                        var userRow1 = listRows.FirstOrDefault(u => u.Name == tableRows1[row1].Name);
                        if (userRow1 != null)
                        {
                            var ovr = tableRows1[row1].overtime + notHolidayOvertimeReduce;
                            userRow1.Overtime1 = ovr < 0 ? 0 : ovr;
                            BunkerRowCompact(tableRows1[row1], userRow1, 1);
                        }
                        else
                        {
                            userRow1 = new BunkerUserPrint();
                            userRow1.SortOrder = tableRows1[row1].SortOrder;
                            userRow1.Name = tableRows1[row1].Name;
                            var tr = tableRows.Find(r=>r.Name == userRow1.Name);
                            if (tr == null)
                            {
                                continue;
                            }
                            userRow1.Rank = tableRows1[row1].Rank;
                            var ovr = tableRows1[row1].overtime + notHolidayOvertimeReduce;
                            userRow1.Overtime1 = ovr < 0 ? 0 : ovr;
                            BunkerRowCompact(tableRows1[row1], userRow, 1);
                            listRows.Add(userRow1);
                        }
                    }
                    notHolidayOvertimeReduce = GetNotHolidayOvertimeReduce(selectedDate.Value.AddDays(-2));
                    for (int row2 = tableRows2.Count - 1; row2 >= 0; row2--)
                    {
                        var userRow2 = listRows.FirstOrDefault(u => u.Name == tableRows2[row2].Name);
                        if (userRow2 != null)
                        {
                            var ovr = tableRows2[row2].overtime + notHolidayOvertimeReduce;
                            userRow2.Overtime2 = ovr < 0 ? 0 : ovr;
                            BunkerRowCompact(tableRows2[row2], userRow2, 2);
                        }
                        else
                        {
                            userRow2 = new BunkerUserPrint();
                            userRow2.SortOrder = tableRows2[row2].SortOrder;
                            userRow2.Name = tableRows2[row2].Name;
                            var tr = tableRows.Find(r => r.Name == userRow2.Name);
                            if (tr == null)
                            {
                                continue;
                            }
                            userRow2.Rank = tableRows2[row2].Rank;
                            var ovr = tableRows2[row2].overtime + notHolidayOvertimeReduce;
                            userRow2.Overtime2 = ovr < 0 ? 0 : ovr;
                            BunkerRowCompact(tableRows2[row2], userRow, 2);
                            listRows.Add(userRow2);
                        }
                    }
                    listRows.Reverse();
                    //listRows.Sort();
                    //show dialog window
                    var BunkerDialog = new BunkerDialogWindow();
                    BunkerDialog.Owner = this;
                    BunkerDialog.Resources.Add("UsersList", listRows);
                    BunkerDialog.ThemeSlider.Value = ThemeSlider.Value;
                    var res = BunkerDialog.ShowDialog();
                    if (res.HasValue)
                    {
                        if (res.Value == false)
                        {
                            return;
                        }
                    }
                    listRows.Reverse();
                    item.Cells.set_Item(10, 3, "Date: " + DPicker.SelectedDate.Value.AddDays(-2).ToString("dd.MMM.yyyy"));
                    item.Cells.set_Item(10, 9, "Date: " + DPicker.SelectedDate.Value.AddDays(-1).ToString("dd.MMM.yyyy"));
                    item.Cells.set_Item(10, 15, "Date: " + DPicker.SelectedDate.Value.ToString("dd.MMM.yyyy"));
                    foreach (var ur in listRows)
                    {
                        if (ur.Selected.HasValue)
                        {
                            if (!ur.Selected.Value)
                            {
                                continue;                                
                            }
                        }
                        PrintBunkerRowToExcel(ur, item);                                                    
                    }
                    var ra = item.Cells.get_Range("A13", "T13");
                    ra.Delete(Microsoft.Office.Interop.Excel.XlDeleteShiftDirection.xlShiftUp);
                    //Show the Excel Window
                    item.Application.Visible = true;
                    item.Application.ActiveWindow.WindowState = Microsoft.Office.Interop.Excel.XlWindowState.xlMaximized;
                    item.Application.ActiveWindow.Visible = true;
                    //print
                    //var dialogWindow = new PrintDialog();
                    //if (dialogWindow.ShowDialog().Value)
                    //{
                    //    try
                    //    {
                    //        item.Application.ActivePrinter = dialogWindow.PrintQueue.QueueDriver.Name;
                    //    }
                    //    catch (Exception)
                    //    {
                    //    }
                    //    item.PrintOutEx();
                    //}

                }
                else
                {
                    MessageBox.Show("Please select date");
                    DPicker.Focus();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", e.Message, "\\b\\r Stack Trace:", e.StackTrace));
            }
        }

        private void PrintBunkerRowToExcel(BunkerUserPrint userRow, Microsoft.Office.Interop.Excel.Worksheet item)
        {
            try
            {

                var rowsNumber = userRow.GetRowsNumber();
                int firstRow = 13;
                for (int i = rowsNumber - 1; i > -1; i--)
                {
                    var range = item.Cells.get_Range("C" + firstRow, "T" + firstRow);
                    range.Cells.Font.Size = 12;
                    item.Cells.get_Range("C" + firstRow, "D" + firstRow).NumberFormat = "0000";
                    item.Cells.get_Range("C" + firstRow, "E" + firstRow).Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideVertical).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                    item.Cells.get_Range("F" + firstRow, "G" + firstRow).NumberFormat = "0000";
                    item.Cells.get_Range("F" + firstRow, "H" + firstRow).Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideVertical).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                    item.Cells.get_Range("I" + firstRow, "J" + firstRow).NumberFormat = "0000";
                    item.Cells.get_Range("I" + firstRow, "K" + firstRow).Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideVertical).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                    item.Cells.get_Range("L" + firstRow, "M" + firstRow).NumberFormat = "0000";
                    item.Cells.get_Range("L" + firstRow, "N" + firstRow).Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideVertical).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                    item.Cells.get_Range("O" + firstRow, "P" + firstRow).NumberFormat = "0000";
                    item.Cells.get_Range("O" + firstRow, "Q" + firstRow).Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideVertical).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                    item.Cells.get_Range("R" + firstRow, "S" + firstRow).NumberFormat = "0000";
                    item.Cells.get_Range("R" + firstRow, "T" + firstRow).Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideVertical).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                    //range.Cells.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                    //range.Cells.Borders.Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
                    try { if (!(userRow.otherWork2Periods.Count < (i + 1))) { item.Cells.set_Item(firstRow, 3, userRow.otherWork2Periods[i].From); item.Cells.set_Item(firstRow, 4, userRow.otherWork2Periods[i].To); item.Cells.set_Item(firstRow, 5, userRow.otherWork2Periods[i].Hours); } }
                    catch (Exception exo2) {
                        MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exo2.Message, "\\b\\r Stack Trace:", exo2.StackTrace));
                    }
                    try { if (!(userRow.restTime2Periods.Count < (i + 1))) { item.Cells.set_Item(firstRow, 6, userRow.restTime2Periods[i].From); item.Cells.set_Item(firstRow, 7, userRow.restTime2Periods[i].To); item.Cells.set_Item(firstRow, 8, userRow.restTime2Periods[i].Hours); } }
                    catch (Exception exr2) {
                        MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exr2.Message, "\\b\\r Stack Trace:", exr2.StackTrace));
                    }
                    try { if (!(userRow.otherWork1Periods.Count < (i + 1))) { item.Cells.set_Item(firstRow, 9, userRow.otherWork1Periods[i].From); item.Cells.set_Item(firstRow, 10, userRow.otherWork1Periods[i].To); item.Cells.set_Item(firstRow, 11, userRow.otherWork1Periods[i].Hours); } }
                    catch (Exception exo1) {
                        MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exo1.Message, "\\b\\r Stack Trace:", exo1.StackTrace));
                    }
                    try { if (!(userRow.restTime1Periods.Count < (i + 1))) { item.Cells.set_Item(firstRow, 12, userRow.restTime1Periods[i].From); item.Cells.set_Item(firstRow, 13, userRow.restTime1Periods[i].To); item.Cells.set_Item(firstRow, 14, userRow.restTime1Periods[i].Hours); } }
                    catch (Exception exr1) {
                        MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exr1.Message, "\\b\\r Stack Trace:", exr1.StackTrace));
                    }
                    try { if (!(userRow.otherWorkPeriods.Count < (i + 1))) { item.Cells.set_Item(firstRow, 15, userRow.otherWorkPeriods[i].From); item.Cells.set_Item(firstRow, 16, userRow.otherWorkPeriods[i].To); item.Cells.set_Item(firstRow, 17, userRow.otherWorkPeriods[i].Hours); } }
                    catch (Exception exo) {
                        MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exo.Message, "\\b\\r Stack Trace:", exo.StackTrace));
                    }
                    try { if (!(userRow.restTimePeriods.Count < (i + 1))) { item.Cells.set_Item(firstRow, 18, userRow.restTimePeriods[i].From); item.Cells.set_Item(firstRow, 19, userRow.restTimePeriods[i].To); item.Cells.set_Item(firstRow, 20, userRow.restTimePeriods[i].Hours); } }
                    catch (Exception exr) {
                        MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exr.Message, "\\b\\r Stack Trace:", exr.StackTrace));
                    }
                    if (i == 0)
                    {
                        var rankCells = item.Cells.get_Range("A" + firstRow, "A" + (firstRow + rowsNumber - 1));
                        rankCells.Merge();
                        rankCells.WrapText = true;
                        rankCells.Cells.VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                        rankCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                        rankCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop).Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlMedium;
                        rankCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                        rankCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlMedium;
                        item.Cells.set_Item(firstRow, 1, userRow.Rank);
                        var nameCells = item.Cells.get_Range("B" + firstRow, "B" + (firstRow + rowsNumber - 1));
                        nameCells.Merge();
                        nameCells.Cells.VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignTop;
                        nameCells.Cells.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
                        nameCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                        nameCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop).Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlMedium;
                        nameCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                        nameCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlMedium;
                        item.Cells.set_Item(firstRow, 2, userRow.Name);

                    }
                    var allRowCells = item.Cells.get_Range("A" + firstRow, "T" + (firstRow + rowsNumber - 1));
                    allRowCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlDouble;
                    allRowCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlDouble;
                    allRowCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlDouble;
                    allRowCells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeRight).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlDouble;
                    allRowCells.Cells.Font.Size = 12;
                    allRowCells.Font.Bold = false;
                    allRowCells.RowHeight = 14;
                    var nrange = item.Cells.get_Range("A" + firstRow, "T" + firstRow);
                    nrange.Insert(Microsoft.Office.Interop.Excel.XlInsertShiftDirection.xlShiftDown);
                }
            }
            catch (Exception exd)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exd.Message, "\\b\\r Stack Trace:", exd.StackTrace));
            }
        }

        private void BunkerRowCompact(TableRow row, BunkerUserPrint userRow, int index)
        {
            try
            {
                switch (index)
                {
                    case 0:
                        var startTime = "0000";
                        var endTime = "0000";
                        double per = 0;
                        var currentWorkTipe = row.H01A; per += 0.5;
                        if (row.H01B != currentWorkTipe)
                        {
                            endTime = "0030";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0030";
                            per = 0;
                            currentWorkTipe = row.H01B;

                        }
                        per += 0.5;
                        if (row.H02A != currentWorkTipe)
                        {
                            endTime = "0100";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0100";
                            per = 0;
                            currentWorkTipe = row.H02A;
                        }
                        per += 0.5;
                        if (row.H02B != currentWorkTipe)
                        {
                            endTime = "0130";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0130";
                            per = 0;
                            currentWorkTipe = row.H02B;
                        }

                        per += 0.5;
                        if (row.H03A != currentWorkTipe)
                        {
                            endTime = "0200";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0200";
                            per = 0;
                            currentWorkTipe = row.H03A;
                        }
                        per += 0.5;
                        if (row.H03B != currentWorkTipe)
                        {
                            endTime = "0230";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0230";
                            per = 0;
                            currentWorkTipe = row.H03B;
                        }

                        per += 0.5;
                        if (row.H04A != currentWorkTipe)
                        {
                            endTime = "0300";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0300";
                            per = 0;
                            currentWorkTipe = row.H04A;
                        }
                        per += 0.5;
                        if (row.H04B != currentWorkTipe)
                        {
                            endTime = "0330";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0330";
                            per = 0;
                            currentWorkTipe = row.H04B;
                        }

                        per += 0.5;
                        if (row.H05A != currentWorkTipe)
                        {
                            endTime = "0400";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0400";
                            per = 0;
                            currentWorkTipe = row.H05A;
                        }
                        per += 0.5;
                        if (row.H05B != currentWorkTipe)
                        {
                            endTime = "0430";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0430";
                            per = 0;
                            currentWorkTipe = row.H05B;
                        }

                        per += 0.5;
                        if (row.H06A != currentWorkTipe)
                        {
                            endTime = "0500";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0500";
                            per = 0;
                            currentWorkTipe = row.H06A;
                        }
                        per += 0.5;
                        if (row.H06B != currentWorkTipe)
                        {
                            endTime = "0530";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0530";
                            per = 0;
                            currentWorkTipe = row.H06B;
                        }

                        per += 0.5;
                        if (row.H07A != currentWorkTipe)
                        {
                            endTime = "0600";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0600";
                            per = 0;
                            currentWorkTipe = row.H07A;
                        }
                        per += 0.5;
                        if (row.H07B != currentWorkTipe)
                        {
                            endTime = "0630";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0630";
                            per = 0;
                            currentWorkTipe = row.H07B;
                        }

                        per += 0.5;
                        if (row.H08A != currentWorkTipe)
                        {
                            endTime = "0700";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0700";
                            per = 0;
                            currentWorkTipe = row.H08A;
                        }
                        per += 0.5;
                        if (row.H08B != currentWorkTipe)
                        {
                            endTime = "0730";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0730";
                            per = 0;
                            currentWorkTipe = row.H08B;
                        }

                        per += 0.5;
                        if (row.H09A != currentWorkTipe)
                        {
                            endTime = "0800";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0800";
                            per = 0;
                            currentWorkTipe = row.H09A;
                        }
                        per += 0.5;
                        if (row.H09B != currentWorkTipe)
                        {
                            endTime = "0830";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0830";
                            per = 0;
                            currentWorkTipe = row.H09B;
                        }

                        per += 0.5;
                        if (row.H10A != currentWorkTipe)
                        {
                            endTime = "0900";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0900";
                            per = 0;
                            currentWorkTipe = row.H10A;
                        }
                        per += 0.5;
                        if (row.H10B != currentWorkTipe)
                        {
                            endTime = "0930";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0930";
                            per = 0;
                            currentWorkTipe = row.H10B;
                        }

                        per += 0.5;
                        if (row.H11A != currentWorkTipe)
                        {
                            endTime = "1000";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1000";
                            per = 0;
                            currentWorkTipe = row.H11A;
                        }
                        per += 0.5;
                        if (row.H11B != currentWorkTipe)
                        {
                            endTime = "1030";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1030";
                            per = 0;
                            currentWorkTipe = row.H11B;
                        }

                        per += 0.5;
                        if (row.H12A != currentWorkTipe)
                        {
                            endTime = "1100";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1100";
                            per = 0;
                            currentWorkTipe = row.H12A;
                        }
                        per += 0.5;
                        if (row.H12B != currentWorkTipe)
                        {
                            endTime = "1130";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1130";
                            per = 0;
                            currentWorkTipe = row.H12B;
                        }

                        per += 0.5;
                        if (row.H13A != currentWorkTipe)
                        {
                            endTime = "1200";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1200";
                            per = 0;
                            currentWorkTipe = row.H13A;
                        }
                        per += 0.5;
                        if (row.H13B != currentWorkTipe)
                        {
                            endTime = "1230";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1230";
                            per = 0;
                            currentWorkTipe = row.H13B;
                        }

                        per += 0.5;
                        if (row.H14A != currentWorkTipe)
                        {
                            endTime = "1300";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1300";
                            per = 0;
                            currentWorkTipe = row.H14A;
                        }
                        per += 0.5;
                        if (row.H14B != currentWorkTipe)
                        {
                            endTime = "1330";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1330";
                            per = 0;
                            currentWorkTipe = row.H14B;
                        }

                        per += 0.5;
                        if (row.H15A != currentWorkTipe)
                        {
                            endTime = "1400";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1400";
                            per = 0;
                            currentWorkTipe = row.H15A;
                        }
                        per += 0.5;
                        if (row.H15B != currentWorkTipe)
                        {
                            endTime = "1430";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1430";
                            per = 0;
                            currentWorkTipe = row.H15B;
                        }

                        per += 0.5;
                        if (row.H16A != currentWorkTipe)
                        {
                            endTime = "1500";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1500";
                            per = 0;
                            currentWorkTipe = row.H16A;
                        }
                        per += 0.5;
                        if (row.H16B != currentWorkTipe)
                        {
                            endTime = "1530";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1530";
                            per = 0;
                            currentWorkTipe = row.H16B;
                        }

                        per += 0.5;
                        if (row.H17A != currentWorkTipe)
                        {
                            endTime = "1600";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1600";
                            per = 0;
                            currentWorkTipe = row.H17A;
                        }
                        per += 0.5;
                        if (row.H17B != currentWorkTipe)
                        {
                            endTime = "1630";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1630";
                            per = 0;
                            currentWorkTipe = row.H17B;
                        }

                        per += 0.5;
                        if (row.H18A != currentWorkTipe)
                        {
                            endTime = "1700";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1700";
                            per = 0;
                            currentWorkTipe = row.H18A;
                        }
                        per += 0.5;
                        if (row.H18B != currentWorkTipe)
                        {
                            endTime = "1730";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1730";
                            per = 0;
                            currentWorkTipe = row.H18B;
                        }

                        per += 0.5;
                        if (row.H19A != currentWorkTipe)
                        {
                            endTime = "1800";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1800";
                            per = 0;
                            currentWorkTipe = row.H19A;
                        }
                        per += 0.5;
                        if (row.H19B != currentWorkTipe)
                        {
                            endTime = "1830";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1830";
                            per = 0;
                            currentWorkTipe = row.H19B;
                        }

                        per += 0.5;
                        if (row.H20A != currentWorkTipe)
                        {
                            endTime = "1900";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1900";
                            per = 0;
                            currentWorkTipe = row.H20A;
                        }
                        per += 0.5;
                        if (row.H20B != currentWorkTipe)
                        {
                            endTime = "1930";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1930";
                            per = 0;
                            currentWorkTipe = row.H20B;
                        }

                        per += 0.5;
                        if (row.H21A != currentWorkTipe)
                        {
                            endTime = "2000";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2000";
                            per = 0;
                            currentWorkTipe = row.H21A;
                        }
                        per += 0.5;
                        if (row.H21B != currentWorkTipe)
                        {
                            endTime = "2030";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2030";
                            per = 0;
                            currentWorkTipe = row.H21B;
                        }

                        per += 0.5;
                        if (row.H22A != currentWorkTipe)
                        {
                            endTime = "2100";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2100";
                            per = 0;
                            currentWorkTipe = row.H22A;
                        }
                        per += 0.5;
                        if (row.H22B != currentWorkTipe)
                        {
                            endTime = "2130";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2130";
                            per = 0;
                            currentWorkTipe = row.H22B;
                        }

                        per += 0.5;
                        if (row.H23A != currentWorkTipe)
                        {
                            endTime = "2200";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2200";
                            per = 0;
                            currentWorkTipe = row.H23A;
                        }
                        per += 0.5;
                        if (row.H23B != currentWorkTipe)
                        {
                            endTime = "2230";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2230";
                            per = 0;
                            currentWorkTipe = row.H23B;
                        }

                        per += 0.5;
                        if (row.H24A != currentWorkTipe)
                        {
                            endTime = "2300";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2300";
                            per = 0;
                            currentWorkTipe = row.H24A;
                        }
                        per += 0.5;
                        if (row.H24B != currentWorkTipe)
                        {
                            endTime = "2330";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2330";
                            per = 0;
                            currentWorkTipe = row.H24B;
                        }
                        else
                        {
                            per += 0.5;
                            endTime = "2400";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTimePeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWorkPeriods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case 1:
                        startTime = "0000";
                        endTime = "0000";
                        per = 0;
                        currentWorkTipe = row.H01A;
                        per += 0.5;
                        if (row.H01B != currentWorkTipe)
                        {
                            endTime = "0030";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0030";
                            per = 0;
                            currentWorkTipe = row.H01B;

                        }
                        per += 0.5;
                        if (row.H02A != currentWorkTipe)
                        {
                            endTime = "0100";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0100";
                            per = 0;
                            currentWorkTipe = row.H02A;
                        }
                        per += 0.5;
                        if (row.H02B != currentWorkTipe)
                        {
                            endTime = "0130";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0130";
                            per = 0;
                            currentWorkTipe = row.H02B;
                        }

                        per += 0.5;
                        if (row.H03A != currentWorkTipe)
                        {
                            endTime = "0200";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0200";
                            per = 0;
                            currentWorkTipe = row.H03A;
                        }
                        per += 0.5;
                        if (row.H03B != currentWorkTipe)
                        {
                            endTime = "0230";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0230";
                            per = 0;
                            currentWorkTipe = row.H03B;
                        }

                        per += 0.5;
                        if (row.H04A != currentWorkTipe)
                        {
                            endTime = "0300";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0300";
                            per = 0;
                            currentWorkTipe = row.H04A;
                        }
                        per += 0.5;
                        if (row.H04B != currentWorkTipe)
                        {
                            endTime = "0330";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0330";
                            per = 0;
                            currentWorkTipe = row.H04B;
                        }

                        per += 0.5;
                        if (row.H05A != currentWorkTipe)
                        {
                            endTime = "0400";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0400";
                            per = 0;
                            currentWorkTipe = row.H05A;
                        }
                        per += 0.5;
                        if (row.H05B != currentWorkTipe)
                        {
                            endTime = "0430";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0430";
                            per = 0;
                            currentWorkTipe = row.H05B;
                        }

                        per += 0.5;
                        if (row.H06A != currentWorkTipe)
                        {
                            endTime = "0500";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0500";
                            per = 0;
                            currentWorkTipe = row.H06A;
                        }
                        per += 0.5;
                        if (row.H06B != currentWorkTipe)
                        {
                            endTime = "0530";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0530";
                            per = 0;
                            currentWorkTipe = row.H06B;
                        }

                        per += 0.5;
                        if (row.H07A != currentWorkTipe)
                        {
                            endTime = "0600";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0600";
                            per = 0;
                            currentWorkTipe = row.H07A;
                        }
                        per += 0.5;
                        if (row.H07B != currentWorkTipe)
                        {
                            endTime = "0630";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0630";
                            per = 0;
                            currentWorkTipe = row.H07B;
                        }

                        per += 0.5;
                        if (row.H08A != currentWorkTipe)
                        {
                            endTime = "0700";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0700";
                            per = 0;
                            currentWorkTipe = row.H08A;
                        }
                        per += 0.5;
                        if (row.H08B != currentWorkTipe)
                        {
                            endTime = "0730";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0730";
                            per = 0;
                            currentWorkTipe = row.H08B;
                        }

                        per += 0.5;
                        if (row.H09A != currentWorkTipe)
                        {
                            endTime = "0800";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0800";
                            per = 0;
                            currentWorkTipe = row.H09A;
                        }
                        per += 0.5;
                        if (row.H09B != currentWorkTipe)
                        {
                            endTime = "0830";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0830";
                            per = 0;
                            currentWorkTipe = row.H09B;
                        }

                        per += 0.5;
                        if (row.H10A != currentWorkTipe)
                        {
                            endTime = "0900";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0900";
                            per = 0;
                            currentWorkTipe = row.H10A;
                        }
                        per += 0.5;
                        if (row.H10B != currentWorkTipe)
                        {
                            endTime = "0930";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0930";
                            per = 0;
                            currentWorkTipe = row.H10B;
                        }

                        per += 0.5;
                        if (row.H11A != currentWorkTipe)
                        {
                            endTime = "1000";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1000";
                            per = 0;
                            currentWorkTipe = row.H11A;
                        }
                        per += 0.5;
                        if (row.H11B != currentWorkTipe)
                        {
                            endTime = "1030";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1030";
                            per = 0;
                            currentWorkTipe = row.H11B;
                        }

                        per += 0.5;
                        if (row.H12A != currentWorkTipe)
                        {
                            endTime = "1100";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1100";
                            per = 0;
                            currentWorkTipe = row.H12A;
                        }
                        per += 0.5;
                        if (row.H12B != currentWorkTipe)
                        {
                            endTime = "1130";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1130";
                            per = 0;
                            currentWorkTipe = row.H12B;
                        }

                        per += 0.5;
                        if (row.H13A != currentWorkTipe)
                        {
                            endTime = "1200";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1200";
                            per = 0;
                            currentWorkTipe = row.H13A;
                        }
                        per += 0.5;
                        if (row.H13B != currentWorkTipe)
                        {
                            endTime = "1230";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1230";
                            per = 0;
                            currentWorkTipe = row.H13B;
                        }

                        per += 0.5;
                        if (row.H14A != currentWorkTipe)
                        {
                            endTime = "1300";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1300";
                            per = 0;
                            currentWorkTipe = row.H14A;
                        }
                        per += 0.5;
                        if (row.H14B != currentWorkTipe)
                        {
                            endTime = "1330";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1330";
                            per = 0;
                            currentWorkTipe = row.H14B;
                        }

                        per += 0.5;
                        if (row.H15A != currentWorkTipe)
                        {
                            endTime = "1400";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1400";
                            per = 0;
                            currentWorkTipe = row.H15A;
                        }
                        per += 0.5;
                        if (row.H15B != currentWorkTipe)
                        {
                            endTime = "1430";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1430";
                            per = 0;
                            currentWorkTipe = row.H15B;
                        }

                        per += 0.5;
                        if (row.H16A != currentWorkTipe)
                        {
                            endTime = "1500";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1500";
                            per = 0;
                            currentWorkTipe = row.H16A;
                        }
                        per += 0.5;
                        if (row.H16B != currentWorkTipe)
                        {
                            endTime = "1530";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1530";
                            per = 0;
                            currentWorkTipe = row.H16B;
                        }

                        per += 0.5;
                        if (row.H17A != currentWorkTipe)
                        {
                            endTime = "1600";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1600";
                            per = 0;
                            currentWorkTipe = row.H17A;
                        }
                        per += 0.5;
                        if (row.H17B != currentWorkTipe)
                        {
                            endTime = "1630";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1630";
                            per = 0;
                            currentWorkTipe = row.H17B;
                        }

                        per += 0.5;
                        if (row.H18A != currentWorkTipe)
                        {
                            endTime = "1700";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1700";
                            per = 0;
                            currentWorkTipe = row.H18A;
                        }
                        per += 0.5;
                        if (row.H18B != currentWorkTipe)
                        {
                            endTime = "1730";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1730";
                            per = 0;
                            currentWorkTipe = row.H18B;
                        }

                        per += 0.5;
                        if (row.H19A != currentWorkTipe)
                        {
                            endTime = "1800";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1800";
                            per = 0;
                            currentWorkTipe = row.H19A;
                        }
                        per += 0.5;
                        if (row.H19B != currentWorkTipe)
                        {
                            endTime = "1830";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1830";
                            per = 0;
                            currentWorkTipe = row.H19B;
                        }

                        per += 0.5;
                        if (row.H20A != currentWorkTipe)
                        {
                            endTime = "1900";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1900";
                            per = 0;
                            currentWorkTipe = row.H20A;
                        }
                        per += 0.5;
                        if (row.H20B != currentWorkTipe)
                        {
                            endTime = "1930";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1930";
                            per = 0;
                            currentWorkTipe = row.H20B;
                        }

                        per += 0.5;
                        if (row.H21A != currentWorkTipe)
                        {
                            endTime = "2000";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2000";
                            per = 0;
                            currentWorkTipe = row.H21A;
                        }
                        per += 0.5;
                        if (row.H21B != currentWorkTipe)
                        {
                            endTime = "2030";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2030";
                            per = 0;
                            currentWorkTipe = row.H21B;
                        }

                        per += 0.5;
                        if (row.H22A != currentWorkTipe)
                        {
                            endTime = "2100";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2100";
                            per = 0;
                            currentWorkTipe = row.H22A;
                        }
                        per += 0.5;
                        if (row.H22B != currentWorkTipe)
                        {
                            endTime = "2130";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2130";
                            per = 0;
                            currentWorkTipe = row.H22B;
                        }

                        per += 0.5;
                        if (row.H23A != currentWorkTipe)
                        {
                            endTime = "2200";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2200";
                            per = 0;
                            currentWorkTipe = row.H23A;
                        }
                        per += 0.5;
                        if (row.H23B != currentWorkTipe)
                        {
                            endTime = "2230";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2230";
                            per = 0;
                            currentWorkTipe = row.H23B;
                        }

                        per += 0.5;
                        if (row.H24A != currentWorkTipe)
                        {
                            endTime = "2300";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2300";
                            per = 0;
                            currentWorkTipe = row.H24A;
                        }
                        per += 0.5;
                        if (row.H24B != currentWorkTipe)
                        {
                            endTime = "2330";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2330";
                            per = 0;
                            currentWorkTipe = row.H24B;
                        }
                        else
                        {
                            per += 0.5;
                            endTime = "2400";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork1Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case 2:
                        startTime = "0000";
                        endTime = "0000";
                        per = 0;
                        currentWorkTipe = row.H01A;
                        per += 0.5;
                        if (row.H01B != currentWorkTipe)
                        {
                            endTime = "0030";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0030";
                            per = 0;
                            currentWorkTipe = row.H01B;

                        }
                        per += 0.5;
                        if (row.H02A != currentWorkTipe)
                        {
                            endTime = "0100";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0100";
                            per = 0;
                            currentWorkTipe = row.H02A;
                        }
                        per += 0.5;
                        if (row.H02B != currentWorkTipe)
                        {
                            endTime = "0130";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0130";
                            per = 0;
                            currentWorkTipe = row.H02B;
                        }

                        per += 0.5;
                        if (row.H03A != currentWorkTipe)
                        {
                            endTime = "0200";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0200";
                            per = 0;
                            currentWorkTipe = row.H03A;
                        }
                        per += 0.5;
                        if (row.H03B != currentWorkTipe)
                        {
                            endTime = "0230";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0230";
                            per = 0;
                            currentWorkTipe = row.H03B;
                        }

                        per += 0.5;
                        if (row.H04A != currentWorkTipe)
                        {
                            endTime = "0300";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0300";
                            per = 0;
                            currentWorkTipe = row.H04A;
                        }
                        per += 0.5;
                        if (row.H04B != currentWorkTipe)
                        {
                            endTime = "0330";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0330";
                            per = 0;
                            currentWorkTipe = row.H04B;
                        }

                        per += 0.5;
                        if (row.H05A != currentWorkTipe)
                        {
                            endTime = "0400";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0400";
                            per = 0;
                            currentWorkTipe = row.H05A;
                        }
                        per += 0.5;
                        if (row.H05B != currentWorkTipe)
                        {
                            endTime = "0430";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0430";
                            per = 0;
                            currentWorkTipe = row.H05B;
                        }

                        per += 0.5;
                        if (row.H06A != currentWorkTipe)
                        {
                            endTime = "0500";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0500";
                            per = 0;
                            currentWorkTipe = row.H06A;
                        }
                        per += 0.5;
                        if (row.H06B != currentWorkTipe)
                        {
                            endTime = "0530";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0530";
                            per = 0;
                            currentWorkTipe = row.H06B;
                        }

                        per += 0.5;
                        if (row.H07A != currentWorkTipe)
                        {
                            endTime = "0600";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0600";
                            per = 0;
                            currentWorkTipe = row.H07A;
                        }
                        per += 0.5;
                        if (row.H07B != currentWorkTipe)
                        {
                            endTime = "0630";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0630";
                            per = 0;
                            currentWorkTipe = row.H07B;
                        }

                        per += 0.5;
                        if (row.H08A != currentWorkTipe)
                        {
                            endTime = "0700";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0700";
                            per = 0;
                            currentWorkTipe = row.H08A;
                        }
                        per += 0.5;
                        if (row.H08B != currentWorkTipe)
                        {
                            endTime = "0730";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0730";
                            per = 0;
                            currentWorkTipe = row.H08B;
                        }

                        per += 0.5;
                        if (row.H09A != currentWorkTipe)
                        {
                            endTime = "0800";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0800";
                            per = 0;
                            currentWorkTipe = row.H09A;
                        }
                        per += 0.5;
                        if (row.H09B != currentWorkTipe)
                        {
                            endTime = "0830";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0830";
                            per = 0;
                            currentWorkTipe = row.H09B;
                        }

                        per += 0.5;
                        if (row.H10A != currentWorkTipe)
                        {
                            endTime = "0900";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0900";
                            per = 0;
                            currentWorkTipe = row.H10A;
                        }
                        per += 0.5;
                        if (row.H10B != currentWorkTipe)
                        {
                            endTime = "0930";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "0930";
                            per = 0;
                            currentWorkTipe = row.H10B;
                        }

                        per += 0.5;
                        if (row.H11A != currentWorkTipe)
                        {
                            endTime = "1000";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1000";
                            per = 0;
                            currentWorkTipe = row.H11A;
                        }
                        per += 0.5;
                        if (row.H11B != currentWorkTipe)
                        {
                            endTime = "1030";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1030";
                            per = 0;
                            currentWorkTipe = row.H11B;
                        }

                        per += 0.5;
                        if (row.H12A != currentWorkTipe)
                        {
                            endTime = "1100";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1100";
                            per = 0;
                            currentWorkTipe = row.H12A;
                        }
                        per += 0.5;
                        if (row.H12B != currentWorkTipe)
                        {
                            endTime = "1130";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1130";
                            per = 0;
                            currentWorkTipe = row.H12B;
                        }

                        per += 0.5;
                        if (row.H13A != currentWorkTipe)
                        {
                            endTime = "1200";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1200";
                            per = 0;
                            currentWorkTipe = row.H13A;
                        }
                        per += 0.5;
                        if (row.H13B != currentWorkTipe)
                        {
                            endTime = "1230";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1230";
                            per = 0;
                            currentWorkTipe = row.H13B;
                        }

                        per += 0.5;
                        if (row.H14A != currentWorkTipe)
                        {
                            endTime = "1300";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1300";
                            per = 0;
                            currentWorkTipe = row.H14A;
                        }
                        per += 0.5;
                        if (row.H14B != currentWorkTipe)
                        {
                            endTime = "1330";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1330";
                            per = 0;
                            currentWorkTipe = row.H14B;
                        }

                        per += 0.5;
                        if (row.H15A != currentWorkTipe)
                        {
                            endTime = "1400";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1400";
                            per = 0;
                            currentWorkTipe = row.H15A;
                        }
                        per += 0.5;
                        if (row.H15B != currentWorkTipe)
                        {
                            endTime = "1430";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1430";
                            per = 0;
                            currentWorkTipe = row.H15B;
                        }

                        per += 0.5;
                        if (row.H16A != currentWorkTipe)
                        {
                            endTime = "1500";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1500";
                            per = 0;
                            currentWorkTipe = row.H16A;
                        }
                        per += 0.5;
                        if (row.H16B != currentWorkTipe)
                        {
                            endTime = "1530";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1530";
                            per = 0;
                            currentWorkTipe = row.H16B;
                        }

                        per += 0.5;
                        if (row.H17A != currentWorkTipe)
                        {
                            endTime = "1600";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1600";
                            per = 0;
                            currentWorkTipe = row.H17A;
                        }
                        per += 0.5;
                        if (row.H17B != currentWorkTipe)
                        {
                            endTime = "1630";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1630";
                            per = 0;
                            currentWorkTipe = row.H17B;
                        }

                        per += 0.5;
                        if (row.H18A != currentWorkTipe)
                        {
                            endTime = "1700";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1700";
                            per = 0;
                            currentWorkTipe = row.H18A;
                        }
                        per += 0.5;
                        if (row.H18B != currentWorkTipe)
                        {
                            endTime = "1730";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1730";
                            per = 0;
                            currentWorkTipe = row.H18B;
                        }

                        per += 0.5;
                        if (row.H19A != currentWorkTipe)
                        {
                            endTime = "1800";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1800";
                            per = 0;
                            currentWorkTipe = row.H19A;
                        }
                        per += 0.5;
                        if (row.H19B != currentWorkTipe)
                        {
                            endTime = "1830";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1830";
                            per = 0;
                            currentWorkTipe = row.H19B;
                        }

                        per += 0.5;
                        if (row.H20A != currentWorkTipe)
                        {
                            endTime = "1900";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1900";
                            per = 0;
                            currentWorkTipe = row.H20A;
                        }
                        per += 0.5;
                        if (row.H20B != currentWorkTipe)
                        {
                            endTime = "1930";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "1930";
                            per = 0;
                            currentWorkTipe = row.H20B;
                        }

                        per += 0.5;
                        if (row.H21A != currentWorkTipe)
                        {
                            endTime = "2000";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2000";
                            per = 0;
                            currentWorkTipe = row.H21A;
                        }
                        per += 0.5;
                        if (row.H21B != currentWorkTipe)
                        {
                            endTime = "2030";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2030";
                            per = 0;
                            currentWorkTipe = row.H21B;
                        }

                        per += 0.5;
                        if (row.H22A != currentWorkTipe)
                        {
                            endTime = "2100";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2100";
                            per = 0;
                            currentWorkTipe = row.H22A;
                        }
                        per += 0.5;
                        if (row.H22B != currentWorkTipe)
                        {
                            endTime = "2130";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2130";
                            per = 0;
                            currentWorkTipe = row.H22B;
                        }

                        per += 0.5;
                        if (row.H23A != currentWorkTipe)
                        {
                            endTime = "2200";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2200";
                            per = 0;
                            currentWorkTipe = row.H23A;
                        }
                        per += 0.5;
                        if (row.H23B != currentWorkTipe)
                        {
                            endTime = "2230";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2230";
                            per = 0;
                            currentWorkTipe = row.H23B;
                        }

                        per += 0.5;
                        if (row.H24A != currentWorkTipe)
                        {
                            endTime = "2300";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2300";
                            per = 0;
                            currentWorkTipe = row.H24A;
                        }
                        per += 0.5;
                        if (row.H24B != currentWorkTipe)
                        {
                            endTime = "2330";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                            startTime = "2330";
                            per = 0;
                            currentWorkTipe = row.H24B;
                        }
                        else
                        {
                            per += 0.5;
                            endTime = "2400";
                            switch (currentWorkTipe)
                            {
                                case "":
                                    userRow.restTime2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "w":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                case "d":
                                    userRow.otherWork2Periods.Add(new Period(startTime, endTime, per));
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception exd)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exd.Message, "\\b\\r Stack Trace:", exd.StackTrace));
            }

        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            try 
            {
                HolidaySelectCalendar.IsEnabled = true;
                HolidaySelectCalendar.Visibility = System.Windows.Visibility.Visible;
                HolidaySelectCalendar.Focus();
            }
            catch (Exception exd)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exd.Message, "\\b\\r Stack Trace:", exd.StackTrace));
            }
        }

        private void Button_Click_Del(object sender, RoutedEventArgs e)
        {
            try
            {
                if (HolidaysGrid.SelectedItem != null)
                {
                    string fileHoliday = System.IO.File.ReadAllText("holidays");
                    string day = (HolidaysGrid.SelectedItem as DateTime?).Value.ToShortDateString();
                    (HolidaysGrid.ItemsSource as ObservableCollection<DateTime>).Remove((DateTime)HolidaysGrid.SelectedItem);
                    
                    StringBuilder sb = new StringBuilder(fileHoliday);
                    if (fileHoliday.Contains(day))
                    {
                        sb.Replace(day+"\r\n","");
                    }
                    System.IO.File.WriteAllText("holidays", sb.ToString());
                    //StringBuilder sb = new StringBuilder();
                    //foreach (var item in HolidaysGrid.Items)
                    //{
                    //    sb.AppendLine(item.ToString() + System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.DateSeparator +"2000");
                    //}
                    //System.IO.File.WriteAllText("holidays", sb.ToString());
                }
            }
            catch (Exception exd)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exd.Message, "\\b\\r Stack Trace:", exd.StackTrace));
            }
            
        }

        private void HolidaySelectCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //StringBuilder sb = new StringBuilder(System.IO.File.ReadAllText("holidays"));
                //sb.AppendLine(string.Format("{0:dd}{1}{0:MMM}{1}2000", HolidaySelectCalendar.SelectedDate.Value, System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.DateSeparator));
                //System.IO.File.WriteAllText("holidays", sb.ToString());
                DateTime today = DateTime.Now;
                if (DPicker.SelectedDate != null)
                {
                    today = DPicker.SelectedDate.Value;
                }
                if (HolidaySelectCalendar.SelectedDate.Value.Year == today.Year)
                {
                    (Resources["HolidaysList"] as ObservableCollection<DateTime>).Add(HolidaySelectCalendar.SelectedDate.Value);                                    
                }
                HolidaySelectCalendar.IsEnabled = false;
                HolidaySelectCalendar.Visibility = System.Windows.Visibility.Collapsed;
                SaveAdHolidaysToFile(HolidaySelectCalendar.SelectedDate.Value);
                //HolidaysGrid.ItemsSource = GetListHolidays();
            }
            catch (Exception exd)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exd.Message, "\\b\\r Stack Trace:", exd.StackTrace));
            }

        }

        private void SaveAdHolidaysToFile(DateTime date)
        {
            string fileHoliday = System.IO.File.ReadAllText("holidays");
            StringBuilder sb = new StringBuilder(fileHoliday);
            if (!fileHoliday.Contains(date.ToShortDateString()))
            {
                sb.AppendLine(date.ToShortDateString());                
            }            
            System.IO.File.WriteAllText("holidays", sb.ToString());
        }

        private void MonthlyPersonal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!DPicker.SelectedDate.HasValue)
                {
                    MessageBox.Show("Select Date and try again!");
                    return;
                }
                var today = DPicker.SelectedDate.Value;                
                var crewDict = new Dictionary<int, double>();
                var context = new BNFT_VSLEntities();
                var allCrewInMonth = context.CWCREWSERV.Where(c => (c.DATED.HasValue == false & c.DATEE.Value.Month <= today.Month & c.DATEE.Value.Year == today.Year) | (c.DATED.HasValue == false & c.DATEE.Value.Year < today.Year) | (c.DATED.Value.Month == today.Month & c.DATED.Value.Year == today.Year) | c.DATEE.Value <= today & c.DATED.Value >= today).ToList();                
                var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
                List<CrewMember> listCrew= new List<CrewMember>();
                foreach (var crew in allCrewInMonth)
                {
                    var crewData = context.CWCREW.FirstOrDefault(c=>c.ID == crew.CWCREW_ID);
                    var crewRank = context.CWPRANK.FirstOrDefault(r=>r.ID==crew.CWPRANK_ID);
                    short? watch = 0;
                    try
                    {
                        var cwrhs = context.CWRHSCHED.FirstOrDefault(w => w.CWPRANK_ID == crew.CWPRANK_ID);
                        if (cwrhs !=null)
                        {
                            watch = cwrhs.FL_WK;                            
                        }
                    }
                    catch (Exception exa)
                    {
                        MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exa.Message, "\\b\\r Stack Trace:", exa.StackTrace));
                    }
                    //listCrew.Add(new CrewMember(crew.CWCREW_ID,crewData.FIRSTNAME,crewData.LASTNAME,crewRank.DESCR,crew.DATEE,crew.DATED, crewRank.AA, watch));
                    listCrew.Add(new CrewMember(crew, crewData, crewRank, watch));
                }
                listCrew.Sort();
                WindowMonthlyCrewPrint monthlyWindow = new WindowMonthlyCrewPrint();
                monthlyWindow.Owner = this;
                monthlyWindow.CREWPRINTGRID.DataContext = listCrew;
                monthlyWindow.ResponsibleCombo.DataContext = listCrew;
                monthlyWindow.Date.Content = string.Format("{0}/{1}",today.ToString("MMMM"),today.Year.ToString()).ToUpper();
                monthlyWindow.ThemeSlider.Value = ThemeSlider.Value;
                monthlyWindow.Show();                                
            }
            catch (Exception exd)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exd.Message, "\\b\\r Stack Trace:", exd.StackTrace));
            }
        }




        private void MainWindowName_Loaded(object sender, RoutedEventArgs e)
        {            
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                Cursor = Cursors.Wait;
                var context = new BNFT_VSLEntities();
                PopulateShipData(context);
            }
            catch (System.Data.Entity.Core.EntityException er)
            {
                if (!FindDatabase())
                {
                    MessageBox.Show("Try starting the Program as Administrator and/or stop the antivirus program.");
                    MessageBox.Show(string.Format("Please make shure your computer is connected to the network associated with the BNFT Rest Hours Program DataBase, and restart the application or click \"Change Database Button\". {0}",er.StackTrace));
                    MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", er.Message, "\\b\\r Stack Trace:", er.StackTrace));
                }
                Cursor = Cursors.Arrow;
                Mouse.OverrideCursor = Cursors.Arrow;
                return;
            }
            catch (Exception)
            {
                Cursor = Cursors.Arrow;
                Mouse.OverrideCursor = Cursors.Arrow;
            }
            Cursor = Cursors.Arrow;
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void PopulateShipData(BNFT_VSLEntities context)
        {
            var vsl = context.VSL.Find(context.VSL.FirstOrDefault().ID);
            ShipName.Text = string.IsNullOrEmpty(vsl.VSL_NAME)? "" : vsl.VSL_NAME;
            IMONumber.Text = string.IsNullOrEmpty(vsl.IMO_NO) ? "" : vsl.IMO_NO;
            var flag = context.VSL_FLAG.FirstOrDefault(f => f.ID == vsl.VSL_FLAG_ID);
            if (flag == null)
            {
                return;
            }
            Flag.Text = string.IsNullOrEmpty(flag.DESCR)? "BAHAMAS" : flag.DESCR;            
        }

        private void MainWindowName_Closed(object sender, EventArgs e)
        {
            bool cancel = false;
            wb_BeforeClose(ref cancel);
        }

        private void MainWindowName_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
	        {
                this.DragMove();
	        } 
        }

        private void ThemeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                System.IO.File.WriteAllText("slider", ThemeSlider.Value.ToString());
            }
            catch (Exception)
            {
            }
        }

        private void RESTHOURSGRID_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
        }

        private void IDLCB_Checked(object sender, RoutedEventArgs e)
        {
            DatePicker_SelectedDateChanged(DPicker, null);
        }

        private void IDLCB_Unchecked(object sender, RoutedEventArgs e)
        {

            if (sender.GetType() == IDLCB.GetType())
            {
                DatePicker_SelectedDateChanged(DPicker, null);
            }
        }
    }
    public class RGBConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                int betlineId = int.Parse(value.ToString());
                var colour = RGBConverterForeground.GetMeColour(betlineId);
                var brush = new System.Windows.Media.SolidColorBrush(colour);
                return brush;
            }
            catch (Exception)
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RGBConverterForeground : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                int betlineId = int.Parse(value.ToString());
                var colour = GetMeColour(betlineId);
                if ((colour.G + colour.B + colour.R) > 300)
                {
                    colour = System.Windows.Media.Colors.Black;
                }
                else
                {
                    colour = System.Windows.Media.Colors.Red;
                }
                var brush = new System.Windows.Media.SolidColorBrush(colour);
                return brush;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static System.Windows.Media.Color GetMeColour(int betlineId)
        {
            try
            {
                var colour = new System.Windows.Media.Color();
                int betlineIdInt = betlineId % 64;
                int step = 8;
                int colourNumber = step * ((betlineIdInt % step) - 1) + (betlineIdInt / step);

                colour.R = (byte)((colourNumber / 16) * 85);
                colour.G = (byte)((colourNumber / 4) * 85);
                colour.B = (byte)((colourNumber % 4) * 85);
                colour.A = 255;
                return colour;
            }

            catch (Exception e)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", e.Message, "\\b\\r Stack Trace:", e.StackTrace));
                return new Color();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new object();
        }
    }
}
