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
using Microsoft.Office.Interop;

namespace WpfBNFTExtract
{
    /// <summary>
    /// Interaction logic for WindowMonthlyCrewPrint.xaml
    /// </summary>
    public partial class WindowMonthlyCrewPrint : Window
    {
        public WindowMonthlyCrewPrint()
        {
            this.Resources.Add("RGBConverter", new RGBConverter());
            this.Resources.Add("RGBConverterForeground", new RGBConverterForeground());
            InitializeComponent();
        }

        private void Button_UserView_Click(object sender, RoutedEventArgs e)
        {
            CrewMember crew;
            try
            {
                crew = (((sender as Button).TemplatedParent) as ContentPresenter).Content as CrewMember;
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                excel.FileValidation = Microsoft.Office.Core.MsoFileValidationMode.msoFileValidationSkip;             
                PrintCrewForm(crew, excel);                
                excel.Application.Visible = true;
                excel.Application.ActiveWindow.WindowState = Microsoft.Office.Interop.Excel.XlWindowState.xlMaximized;
                excel.Application.ActiveWindow.Visible = true;
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error Please check connection to database and try again.");
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exc.Message, "\\b\\r Stack Trace:", exc.StackTrace));
            }
            
            
        }

        private void PrintCrewForm(CrewMember crew, Microsoft.Office.Interop.Excel.Application excel)
        {
            MainWindow O = (this.Owner as MainWindow);
            try
            {
                var dir = System.IO.Directory.GetCurrentDirectory();
                var histDir = System.IO.Directory.GetCurrentDirectory() + "\\History";
                if (!System.IO.Directory.Exists(histDir))
                {
                    System.IO.Directory.CreateDirectory(histDir);
                }
                var today = O.DPicker.SelectedDate.Value;
                if (today == null)
                {
                    today = DateTime.Now;
                }
                var crewDir = System.IO.Directory.GetCurrentDirectory() + string.Format("\\History\\{0}-{1}", today.ToString("MMM"), today.Year);
                if (!System.IO.Directory.Exists(crewDir))
                {
                    System.IO.Directory.CreateDirectory(crewDir);
                }
                var wb = excel.Workbooks.Open(dir + "\\Report.xls");
                //wb.BeforeClose += O.wb_BeforeClose;
                string url = string.Format("{2}\\{3}MonthlyReport{0:0000}.{1:00}.xls", today.Year, today.Month, crewDir, crew.Name);
                foreach (Microsoft.Office.Interop.Excel.Worksheet item in excel.ActiveWorkbook.Sheets)
                {
                    if (item.Name.ToUpper().Contains("MAIN"))
                    {
                        item.Cells.set_Item(3, 3, O.ShipName.Text.ToUpper());
                        item.Cells.set_Item(3, 7, O.IMONumber.Text.ToUpper());
                        item.Cells.set_Item(3, 10, O.Flag.Text.ToUpper());
                        item.Cells.set_Item(4, 5, crew.Name.ToUpper());
                        item.Cells.set_Item(4, 9, crew.Rank.ToUpper());
                        item.Cells.set_Item(5, 4, today.ToString("MMM/yyyy",System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat));
                        item.Cells.set_Item(5, 9, (crew.Watchkeeper == true ? "Yes" : "No"));
                        item.Cells.set_Item(14, 7, (ResponsibleCombo.SelectedValue as CrewMember).Name);
                    }
                    if (item.Name.ToUpper().Contains("TABLE"))
                    {
                        var context = new BNFT_VSLEntities();
                        var rsthrsAllDaysList = context.CWRESTHRS.Where(c => c.CWCREW_ID == crew.ID & c.RH_DATE.Value.Month == today.Month & c.RH_DATE.Value.Year == today.Year).OrderByDescending(cr => cr.RH_DATE);
                        List<MonthlyTableRow> tableRows = new List<MonthlyTableRow>();
                        if (rsthrsAllDaysList.Count()<1)
                        {
                            excel.Quit();
                            return;
                        }
                        foreach (var day in rsthrsAllDaysList)
                        {
                            try
                            {
                                string comment = "";
                                var events = context.SQEVENT_HST.Where(en => en.EVENT_DT.Value == day.RH_DATE.Value).ToList();
                                if (events.Count > 0)
                                {
                                    foreach (SQEVENT_HST ev in events)
                                    {
                                        try
                                        {
                                            var userEvent = context.SQEVENT_HST_PARTIC.FirstOrDefault(ue => ue.SQEVENT_HST_ID == ev.ID & ue.CWCREW_ID == day.CWCREW_ID);
                                            if (userEvent != null)
                                            {
                                                comment = string.Format("{0}{1};", comment, ev.EVT_NOTES.Substring(0,4));
                                            }
                                        }
                                        catch (Exception)
                                        {
                                        
                                        }                                    
                                    }
                                }
                                double restin7d = day.REST_IN7D.HasValue ? day.REST_IN7D.Value : 0.0 ;
                                double restin24d = day.REST_IN24.HasValue ? day.REST_IN24.Value : 0.0;
                                double rest24 = day.REST_24.HasValue ? day.REST_24.Value : 0.0;
                                var mtr = new MonthlyTableRow(day.RH_DATE.Value, day.H01A, day.H01B, day.H02A, day.H02B, day.H03A, day.H03B, day.H04A, day.H04B, day.H05A, day.H05B, day.H06A, day.H06B, day.H07A, day.H07B, day.H08A, day.H08B, day.H09A, day.H09B, day.H10A, day.H10B, day.H11A, day.H11B, day.H12A, day.H12B, day.H13A, day.H13B, day.H14A, day.H14B, day.H15A, day.H15B, day.H16A, day.H16B, day.H17A, day.H17B, day.H18A, day.H18B, day.H19A, day.H19B, day.H20A, day.H20B, day.H21A, day.H21B, day.H22A, day.H22B, day.H23A, day.H23B, day.H24A, day.H24B, rest24, restin24d, restin7d, comment);
                                tableRows.Add(mtr);                                
                            }
                            catch (Exception)
                            {
                                MessageBox.Show(string.Format("Please Check Crew Working Time for Crewmember {0}, on Date: {1}",crew.Name,day.RH_DATE.Value));
                            }
                        }
                        foreach (var tday in tableRows)
                        {
                            PrintDayToExcel(tday, item);
                        }
                        var range = item.Cells.get_Range("A7", "BA7");
                        range.Delete();                        
                        item.PageSetup.PrintArea = "$A$1:$BA$40"; 
                        //dnite ot meseca v tablica
                    }
                }
                try
                {
                    wb.SaveAs(url, Type.Missing, Type.Missing, Type.Missing, false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlShared, Microsoft.Office.Interop.Excel.XlSaveConflictResolution.xlLocalSessionChanges, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                }
                catch (Exception)
                {
                    MessageBox.Show(String.Format("Cannot Save Excel File for crewmember {0}. Please check the rights for Application Folder to be accessible for Everyone For Read and Write.",crew.Name));
                }
            }
            catch (Exception exa)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}",exa.Message, "\\b\\r Stack Trace:", exa.StackTrace));
                try
                {
                    foreach (Microsoft.Office.Interop.Excel.Workbook item in excel.Workbooks)
                    {

                        item.Close(false,Type.Missing,Type.Missing);

                    }
                    excel.Application.Quit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", ex.Message, "\\b\\r Stack Trace:", ex.StackTrace));
                }
            }
        }

        private string Replace(string value)
        {
            switch (value)
            {
                case "R":
                    return "";
                case "Z":
                    return "XE";
                case "V":
                    return "WE";
                case "X":
                    return "X";
                case "W":
                    return "W";
                case "E":
                    return "E";
                default:
                    return "X";
            }
        }

        private void PrintDayToExcel(MonthlyTableRow userRow, Microsoft.Office.Interop.Excel.Worksheet item)
        {        
            int firstRow = 7;
            
            var range = item.Cells.get_Range("A" + firstRow, "BA" + firstRow);
            range.Cells.Font.Size = 7;
            
            try
            {
                if (userRow.MinRestIn24Hrs < 10)
                {
                    Exception except = new Exception(string.Format("Rest hours for any 24 hours are less than 10 hours for the date of {0}. Please check and print again.", userRow.Date.ToString("dd.MMM.yy")));
                    throw except;
                }
                if (userRow.MinRestIn7Days < 77)
                {
                    Exception except = new Exception(string.Format("Rest hours for any 7 days are less than 77 hours for the date of {0}. Please check and print again.", userRow.Date.ToString("dd.MMM.yy")));
                    throw except;
                }
                item.Cells.set_Item(firstRow, 1, userRow.Date.ToString("dd.MMM.yy"));

                item.get_Range("A7").ShrinkToFit = true;
                item.Cells.set_Item(firstRow, 2, Replace(userRow.H01A));
                item.Cells.set_Item(firstRow, 3, Replace(userRow.H01B));
                item.Cells.set_Item(firstRow, 4, Replace(userRow.H02A));
                item.Cells.set_Item(firstRow, 5, Replace(userRow.H02B));
                item.Cells.set_Item(firstRow, 6, Replace(userRow.H03A));
                item.Cells.set_Item(firstRow, 7, Replace(userRow.H03B));
                item.Cells.set_Item(firstRow, 8, Replace(userRow.H04A));
                item.Cells.set_Item(firstRow, 9, Replace(userRow.H04B));
                item.Cells.set_Item(firstRow, 10, Replace(userRow.H05A));
                item.Cells.set_Item(firstRow, 11, Replace(userRow.H05B));
                item.Cells.set_Item(firstRow, 12, Replace(userRow.H06A));
                item.Cells.set_Item(firstRow, 13, Replace(userRow.H06B));
                item.Cells.set_Item(firstRow, 14, Replace(userRow.H07A));
                item.Cells.set_Item(firstRow, 15, Replace(userRow.H07B));
                item.Cells.set_Item(firstRow, 16, Replace(userRow.H08A));
                item.Cells.set_Item(firstRow, 17, Replace(userRow.H08B));
                item.Cells.set_Item(firstRow, 18, Replace(userRow.H09A));
                item.Cells.set_Item(firstRow, 19, Replace(userRow.H09B));
                item.Cells.set_Item(firstRow, 20, Replace(userRow.H10A));
                item.Cells.set_Item(firstRow, 21, Replace(userRow.H10B));
                item.Cells.set_Item(firstRow, 22, Replace(userRow.H11A));
                item.Cells.set_Item(firstRow, 23, Replace(userRow.H11B));
                item.Cells.set_Item(firstRow, 24, Replace(userRow.H12A));
                item.Cells.set_Item(firstRow, 25, Replace(userRow.H12B));
                item.Cells.set_Item(firstRow, 26, Replace(userRow.H13A));
                item.Cells.set_Item(firstRow, 27, Replace(userRow.H13B));
                item.Cells.set_Item(firstRow, 28, Replace(userRow.H14A));
                item.Cells.set_Item(firstRow, 29, Replace(userRow.H14B));
                item.Cells.set_Item(firstRow, 30, Replace(userRow.H15A));
                item.Cells.set_Item(firstRow, 31, Replace(userRow.H15B));
                item.Cells.set_Item(firstRow, 32, Replace(userRow.H16A));
                item.Cells.set_Item(firstRow, 33, Replace(userRow.H16B));
                item.Cells.set_Item(firstRow, 34, Replace(userRow.H17A));
                item.Cells.set_Item(firstRow, 35, Replace(userRow.H17B));
                item.Cells.set_Item(firstRow, 36, Replace(userRow.H18A));
                item.Cells.set_Item(firstRow, 37, Replace(userRow.H18B));
                item.Cells.set_Item(firstRow, 38, Replace(userRow.H19A));
                item.Cells.set_Item(firstRow, 39, Replace(userRow.H19B));
                item.Cells.set_Item(firstRow, 40, Replace(userRow.H20A));
                item.Cells.set_Item(firstRow, 41, Replace(userRow.H20B));
                item.Cells.set_Item(firstRow, 42, Replace(userRow.H21A));
                item.Cells.set_Item(firstRow, 43, Replace(userRow.H21B));
                item.Cells.set_Item(firstRow, 44, Replace(userRow.H22A));
                item.Cells.set_Item(firstRow, 45, Replace(userRow.H22B));
                item.Cells.set_Item(firstRow, 46, Replace(userRow.H23A));
                item.Cells.set_Item(firstRow, 47, Replace(userRow.H23B));
                item.Cells.set_Item(firstRow, 48, Replace(userRow.H24A));
                item.Cells.set_Item(firstRow, 49, Replace(userRow.H24B));
                item.Cells.set_Item(firstRow, 50, userRow.RestHoursIn24Hrs);
                item.Cells.set_Item(firstRow, 51, userRow.Comments);
                item.get_Range("AY7").WrapText = true;
                item.Cells.set_Item(firstRow, 52, userRow.MinRestIn24Hrs);
                item.Cells.set_Item(firstRow, 53, userRow.MinRestIn7Days);
                
                range.Cells.VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                range.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                range.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop).Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
                range.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                range.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
                range.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                range.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft).Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
                range.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeRight).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                range.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeRight).Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
                range.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideHorizontal).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                range.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideHorizontal).Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
                range.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideVertical).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                range.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideVertical).Weight = Microsoft.Office.Interop.Excel.XlBorderWeight.xlThin;
               }
            catch (Exception exb)
            {
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", exb.Message, "\\b\\r Stack Trace:", exb.StackTrace));
            }            
            range.Insert(Microsoft.Office.Interop.Excel.XlInsertShiftDirection.xlShiftDown);
            
        
        }

        private void Button_UserPrint_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            excel.FileValidation = Microsoft.Office.Core.MsoFileValidationMode.msoFileValidationSkip;
            PrintUserToPrinter(sender, excel);
            excel.Application.Quit();
        }

        private void PrintUserToPrinter(object sender, Microsoft.Office.Interop.Excel.Application excel)
        {
            CrewMember crew;
            try
            {
                //System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
                //var dupl = ps.CanDuplex;
                //if (dupl)
                //{
                //    ps.Duplex = System.Drawing.Printing.Duplex.Horizontal;
                //}
                var senderType = sender.GetType().Name;
                if ( senderType ==  "CrewMember")
                {
                    crew = sender as CrewMember;
                }
                else
                {
                    crew = (((sender as Button).TemplatedParent) as ContentPresenter).Content as CrewMember;
                }
                PrintCrewForm(crew, excel);

                //excel.Application.Visible = true;
                //excel.Application.ActiveWindow.WindowState = Microsoft.Office.Interop.Excel.XlWindowState.xlMaximized;
                //excel.Application.ActiveWindow.Visible = true;
                int coppies = 2;
                if (crew.DisembarkDate.HasValue)
                {
                    coppies = 1;
                }
                string printer = string.IsNullOrWhiteSpace((string)PrinterSelector.SelectedItem) ? excel.Application.ActivePrinter : (string)PrinterSelector.SelectedItem;
                excel.Sheets.PrintOutEx(Type.Missing, Type.Missing, coppies, false, printer, false, true, Type.Missing, Type.Missing);
                foreach (Microsoft.Office.Interop.Excel.Workbook item in excel.Workbooks)
                {
                    if (item != null)
                    {                        
                        item.Close(false, Type.Missing, Type.Missing);                        
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error Please check connection to database and try again.");
                MessageBox.Show(string.Format("Inner Exception: {0}. {1} {2}", e.Message, "\\b\\r Stack Trace:", e.StackTrace));
            }
        }



        private void PrintAll_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            excel.FileValidation = Microsoft.Office.Core.MsoFileValidationMode.msoFileValidationSkip;
            foreach (CrewMember crew in CREWPRINTGRID.Items)
            {
                PrintUserToPrinter(crew,excel);
            }
            excel.Application.Quit();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please check Default Printer settings and change to Double sided printing if available.");
            PrinterSelector.ItemsSource = System.Drawing.Printing.PrinterSettings.InstalledPrinters;
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            bool cancel = false;
            (Owner as MainWindow).wb_BeforeClose(ref cancel);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            } 
        }
    }
}
