using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace DentistDB
{
    /// <summary>
    /// Interaction logic for TaskWindow.xaml
    /// </summary>
    public partial class TaskWindow : Window
    {
        public TaskWindow()
        {
            InitializeComponent();
            StatusCB.DataContext = MainWindow.GetStatusTables();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Task t = Resources["T"] as Task;
                t.ClientId = (DataContext as Client).Id;
                t.Date = DateTime.Now;
                t.Diagnose = DiagnoseTB.Text;
                if (string.IsNullOrWhiteSpace(NOICodeTB.Text))
                {
                    t.Price = null;
                }
                else
                {
                    try
                    {
                        t.Price = double.Parse(NOICodeTB.Text);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Моля проверете въведената стойност. Приемат се само числа. пример:12,00");
                        return;
                    }
                }
                if (string.IsNullOrWhiteSpace(NOICodeByNOITB.Text))
                {
                    t.PriceNOI = null;
                }
                else
                {
                    try
                    {
                        t.PriceNOI = double.Parse(NOICodeByNOITB.Text);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Моля проверете въведената стойност. Приемат се само числа. пример:12,00");
                        return;
                    }
                }
                if (string.IsNullOrWhiteSpace(DTPriceTB.Text))
                {
                    t.PriceDT = null;
                }
                else
                {
                    try
                    {
                        t.PriceDT = double.Parse(DTPriceTB.Text);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Моля проверете въведената стойност. Приемат се само числа. пример:12,00");
                        return;
                    }
                }
                if (ImagesList.Items.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    List<string> ls = new List<string>();
                    string selItem = (string)ImagesList.SelectedItem;
                    if (selItem != null)
                    {
                        ls.Add(selItem);
                        sb.Append(selItem + ";");
                    }
                    foreach (string item in ImagesList.Items)
                    {
                        if (item == selItem)
                        {
                            continue;
                        }
                        sb.Append(item + ";");
                    }
                    t.Image = sb.ToString();
                }
                else
                {
                    t.Image = null;
                }
                if (DateDP.SelectedDate.HasValue) { if (DateDP.SelectedDate.Value.Date != t.Date.Date) { t.Date = DateDP.SelectedDate.Value; } else { t.Date = DateTime.Now; } } else { t.Date = DateTime.Now; }
                if (StatusCB.SelectedItem != null)
                {
                    t.Status = (StatusCB.SelectedItem as StatusTable).StatusDescription;
                }
                else
                {
                    t.Status = "Друг статус";
                }
                t.ToothCode = byte.Parse(ToothId.Content.ToString());
                if ((App.Current.MainWindow as MainWindow).LoggedInDoctor != null)
                {
                    t.DoctorId = (App.Current.MainWindow as MainWindow).LoggedInDoctor.Id;
                }
                if (string.IsNullOrWhiteSpace(t.Diagnose) & t.Price == null & t.PriceDT == null & t.PriceNOI == null & StatusCB.SelectedItem == null)
                {
                    MessageBox.Show("Всички полета са празни. Моля въведете необходимата информация и опитайте отново.");
                    return;
                }
                if (PaidByCardRB.IsChecked.HasValue)
                {
                    if (PaidByCardRB.IsChecked.Value)
                    {
                        t.PaidByCard = true;
                        t.PaidCash = false;
                    }
                }
                if (PaidCashRB.IsChecked.HasValue)
                {
                    if (PaidCashRB.IsChecked.Value)
                    {
                        t.PaidCash = true;
                        t.PaidByCard = false;
                    }
                }
                if (NotPaidRB.IsChecked.HasValue)
                {
                    if (NotPaidRB.IsChecked.Value)
                    {
                        t.PaidCash = false;
                        t.PaidByCard = false;
                    }
                }
                if (t.Id > 0)
                {
                    //var lastTask = context.Tasks.ToList().LastOrDefault();
                    //t.Id = lastTask != null ? lastTask.Id + 1 : 0;
                    try
                    {

                        using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
                        {
                            con.Open();
                            var cmd = new MySqlCommand();
                            cmd.Connection = con;
                            cmd.CommandText = "UPDATE Task SET ClientId = @cid, ToothCode = @tcode, Date = @date, Status =@status, Diagnose = @diag, Price = @price, Image = @image, DoctorId = @docid, PriceNOI = @pnoi, PriceDT = @pdt, PaidByCard = @pbc, PaidCash = @pcash  WHERE Id = @tid";
                            cmd.Parameters.AddWithValue("@cid", t.ClientId);
                            cmd.Parameters.AddWithValue("@tcode", t.ToothCode);
                            cmd.Parameters.AddWithValue("@date", t.Date);
                            cmd.Parameters.AddWithValue("@status", t.Status);
                            cmd.Parameters.AddWithValue("@price", t.Price);
                            cmd.Parameters.AddWithValue("@image", t.Image);
                            cmd.Parameters.AddWithValue("@docid", t.DoctorId);
                            cmd.Parameters.AddWithValue("@pnoi", t.PriceNOI);
                            cmd.Parameters.AddWithValue("@pdt", t.PriceDT);
                            cmd.Parameters.AddWithValue("@pbc", t.PaidByCard);
                            cmd.Parameters.AddWithValue("@pcash", t.PaidCash);
                            cmd.Parameters.AddWithValue("@tid", t.Id);

                            cmd.ExecuteNonQuery();
                        }
                        DialogResult = true;
                    }
                    catch (Exception)
                    {
                        DialogResult = false;
                        this.Close();
                    }
                }
                else
                {
                    try
                    {
                        using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
                        {
                            con.Open();
                            var cmd = new MySqlCommand();
                            cmd.Connection = con;
                            cmd.CommandText = "INSERT INTO Task (ClientId, ToothCode, Date, Status, Diagnose, Price, Image, DoctorId, PriceNOI, PriceDT, PaidByCard, PaidCash) VALUES (@cid, @tcode, @date, @status, @diag, @price, @image, @docid, @pnoi, @pdt, @pbc, @pcash)";
                            cmd.Parameters.AddWithValue("@cid", t.ClientId);
                            cmd.Parameters.AddWithValue("@tcode", t.ToothCode);
                            cmd.Parameters.AddWithValue("@date", t.Date);
                            cmd.Parameters.AddWithValue("@status", t.Status);
                            cmd.Parameters.AddWithValue("@price", t.Price);
                            cmd.Parameters.AddWithValue("@image", t.Image);
                            cmd.Parameters.AddWithValue("@docid", t.DoctorId);
                            cmd.Parameters.AddWithValue("@pnoi", t.PriceNOI);
                            cmd.Parameters.AddWithValue("@pdt", t.PriceDT);
                            cmd.Parameters.AddWithValue("@pbc", t.PaidByCard);
                            cmd.Parameters.AddWithValue("@pcash", t.PaidCash);
                            cmd.ExecuteNonQuery();
                            t.Id = (int)cmd.LastInsertedId;
                        }
                        DialogResult = true;


                    }
                    catch (Exception)
                    {
                        DialogResult = false;
                        this.Close();
                    }
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Грешка при създаването на Дейност./n {0}", ex.InnerException.ToString());
            }
        }



        private void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Task t = Resources["T"] as Task;
                var dlg = new OpenFileDialog();
                dlg.Filter = "CertFile|*.*";
                if (dlg.ShowDialog() == true)
                {
                    try
                    {
                        bool result = true;
                        int firstDash = 0;
                        while (result)
                        {
                            string fname = t.Id + "-" + firstDash;
                            if (ImagesList.Items.Count == 0)
                            {
                                break;
                            }
                            foreach (string item in ImagesList.Items)
                            {
                                if (item.Contains(fname))
                                {
                                    result = true;
                                    firstDash++;
                                    break;
                                }
                                else
                                {
                                    result = false;
                                }
                            }
                        }
                        string filename = t.Id + "-" + firstDash;
                        int start = dlg.FileName.LastIndexOf(".");
                        string ext = dlg.FileName.Substring(start);
                        string localPath = Directory.GetCurrentDirectory();
                        if (!Directory.Exists(localPath + "\\x-ray"))
                        {
                            Directory.CreateDirectory(localPath + "\\x-ray");
                        }
                        string newFileName = localPath + "\\x-ray\\" + filename + ext;
                        File.Copy(dlg.FileName, newFileName, true);
                        try
                        {
                            t.Image = t.Image + newFileName + ";";
                            ImagesList.ItemsSource = t.Image.Trim(';').Split(';').ToList();
                            for (int i = 0; i < ImagesList.Items.Count; i++)
                            {
                                if ((string)ImagesList.Items[i] == newFileName)
                                {
                                    ImagesList.SelectedIndex = i;
                                }
                            }
                            ImagesList.UpdateLayout();
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("File Type is not supported. Please try again!");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Error loading image: " +
                        ex.Message);
                    }
                }
            }
            catch (Exception)
            {
            }
        }



        private void SelectedToothDG_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            string status = (string)Resources["StatusLabel"];
            if (Resources["StatusLabel"] != null)
            {
                status = (string)Resources["StatusLabel"];
            }
            if (!string.IsNullOrEmpty(status))
            {
                foreach (StatusTable item in StatusCB.Items)
                {
                    if (item.StatusDescription == status)
                    {
                        StatusCB.SelectedItem = item;
                    }
                }
            }
        }

        private void ImageTask_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ImagesList.SelectedItem == null)
            {
                return;
            }
            string newFileName = (string)ImagesList.SelectedItem;
            System.Diagnostics.Process.Start(newFileName);
        }

        private void ImagesList_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                Task t = Resources["T"] as Task;

                string selectedItem = (string)ImagesList.SelectedItem;
                t.Image = t.Image.Replace(selectedItem + ";", "");
                ImagesList.ItemsSource = t.Image.Trim(';').Split(';').ToList();
                if (ImagesList.Items.Count > 0)
                {
                    ImagesList.SelectedIndex = 0;
                }
                ImagesList.UpdateLayout();
                System.IO.File.Delete(selectedItem);
            }
            catch (Exception)
            {
            }
        }

        private void SelectedToothDG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var status = (Task)SelectedToothDG.SelectedItem;
                Task t = Resources["T"] as Task;
                if (t.Image != null)
                {
                    var taskImages = t.Image.Trim(';').Split(';').ToList();
                    foreach (var file in taskImages)
                    {
                        try { System.IO.File.Delete(file); }
                        catch (Exception) { }
                    }
                }

                MainWindow.DeleteTaskById(status.Id);
            }
        }

        private void SelectedToothDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedTask = (SelectedToothDG.SelectedItem as Task);
            if (selectedTask == null)
            {
                return;
            }
            Client cl = DataContext as Client;
            TaskWindow tw = new TaskWindow();
            tw.DateDP.SelectedDate = selectedTask.Date;
            tw.PaidByCardRB.IsChecked = selectedTask.PaidByCard;
            tw.PaidCashRB.IsChecked = selectedTask.PaidCash;
            tw.DataContext = cl;
            tw.ToothId.Content = selectedTask.ToothCode;
            tw.Resources.Add("T", selectedTask);
            tw.Resources.Add("StatusLabel", selectedTask.Status);
            tw.DiagnoseTB.Text = selectedTask.Diagnose;
            List<string> il = new List<string>();
            if (!string.IsNullOrEmpty(selectedTask.Image))
            {

                foreach (string im in selectedTask.Image.Trim(';').Split(';').ToList())
                {
                    il.Add(im);
                }
                tw.ImagesList.ItemsSource = il;
                tw.ImagesList.SelectedIndex = 0;
            }
            tw.SelectedToothDG.DataContext = MainWindow.GetClientTaskByToothCode(selectedTask.ClientId, selectedTask.ToothCode);
            tw.NOICodeTB.Text = selectedTask.Price.ToString();
            //tw.SaveButton.Visibility = System.Windows.Visibility.Hidden;
            tw.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var doctor = new Doctor();

            var dw = new AddDoctorWindow();
            dw.DataContext = doctor;
            var tdw = dw.ShowDialog();
            if (tdw.HasValue & tdw.Value == true)
            {
                using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
                {
                    con.Open();
                    var cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = "SELECT Id FROM Doctors WHERE Name = @docname";
                    cmd.Parameters.AddWithValue("@docname", doctor.Name);
                    if (cmd.ExecuteScalar() != null)
                    {
                        MessageBox.Show("Докторът е вече добавен в списъка. Моля изберете името от списъка.");
                        con.Close();
                        return;
                    }
                }
                using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
                {
                    con.Open();
                    var cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = "INSERT INTO Doctors(Name,Password) VALUES(@name,@pass)";
                    cmd.Parameters.AddWithValue("@name", doctor.Name);
                    cmd.Parameters.AddWithValue("@pass", doctor.Password);
                    cmd.ExecuteNonQuery();
                    doctor.Id = (int)cmd.LastInsertedId;
                }
            }
        }


        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            var mw = (App.Current.MainWindow as MainWindow);
            mw.DataContext = null;
            mw.LoggedInDoctor = null;
            mw.LoggedInLabel.Content = "";
            foreach (Window win in App.Current.Windows)
            {
                if (win.Name != "DentistDBMainWindow")
                {
                    win.Close();
                }
            }
            mw.OpenLoginWindow();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoggedInLabel.Content = (App.Current.MainWindow as MainWindow).LoggedInDoctor.Name;
            PaidByCardRB.Checked += PaidCashRB_Checked;
            PaidCashRB.Checked += PaidCashRB_Checked;
        }


        private void PaidCashRB_Checked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Потвърдете плащането на суната", "Плащане", MessageBoxButton.YesNo);
            if (res != null)
            {
                if (res != MessageBoxResult.Yes)
                {
                    NotPaidRB.IsChecked = true;
                }
            }
        }
    }
}
