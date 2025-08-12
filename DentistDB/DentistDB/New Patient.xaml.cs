using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace DentistDB
{
    /// <summary>
    /// Interaction logic for New_Patient.xaml
    /// </summary>




    public class ListFmStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            var images = (string)value;
            List<string> nl = new List<string>();
            foreach (var item in images.Trim(';').Split(';').ToList())
            {
                nl.Add(item);
            }
            return nl;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public partial class New_Patient : Window
    {
        public New_Patient()
        {
            InitializeComponent();

        }

        private void AddProfileImageBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "CertFile|*.*";
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var cl = DataContext as Client;
                    bool result = true;
                    int firstDash = 0;
                    while (result)
                    {
                        string fname = cl.Id + "-" + firstDash;
                        if (ImagesListProfile.Items.Count == 0)
                        {
                            break;
                        }
                        foreach (string item in ImagesListProfile.Items)
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
                    string filename = cl.Id + "-" + firstDash;
                    int start = dlg.FileName.LastIndexOf(".");
                    string ext = dlg.FileName.Substring(start);
                    string localPath = Directory.GetCurrentDirectory();
                    if (!Directory.Exists(localPath + "\\IMG"))
                    {
                        Directory.CreateDirectory(localPath + "\\IMG");
                    }

                    string newFileName = localPath + "\\IMG\\" + filename + ext;
                    File.Copy(dlg.FileName, newFileName, true);
                    try
                    {
                        //ImagesListProfile.Items.Add(newFileName);
                        cl.Image = cl.Image + newFileName + ";";
                        if (cl.Id > 0)
                        {
                            using (var con = new MySqlConnection("server=45.84.206.101;user=u158550680_Eddy;persistsecurityinfo=True;database=u158550680_dentist_db;password=Eddy8479;allowuservariables=True"))
                            {
                                con.Open();
                                var cmd = new MySqlCommand();
                                cmd.Connection = con;
                                cmd.CommandText = "UPDATE Client SET Image =@image WHERE Id = @cid";
                                cmd.Parameters.AddWithValue("@cId", cl.Id);
                                cmd.Parameters.AddWithValue("@image", cl.Image);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        ImagesListProfile.ItemsSource = cl.Image.Trim(';').Split(';').ToList();
                        for (int i = 0; i < ImagesListProfile.Items.Count; i++)
                        {
                            if ((string)ImagesListProfile.Items[i] == newFileName)
                            {
                                ImagesListProfile.SelectedIndex = i;
                            }
                        }
                        ImagesListProfile.UpdateLayout();
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            byte toothID = byte.Parse((string)btn.Content);
            Client cl = DataContext as Client;
            TaskWindow tw = new TaskWindow();

            tw.DataContext = cl;
            tw.ToothId.Content = toothID;
            tw.SelectedToothDG.ItemsSource = MainWindow.GetClientTaskByToothCode(cl.Id, toothID).OrderBy(d => d.Date);

            //Task task = context.Tasks.Add(new Task());
            Task task = new Task();
            task.Date = DateTime.Now;
            tw.DateDP.SelectedDate = task.Date;
            tw.ImagesList.ItemsSource = new List<string>();
            tw.PaidByCardRB.IsChecked = task.PaidByCard;
            tw.PaidCashRB.IsChecked = task.PaidCash;
            tw.Resources.Add("T", task);
            bool? res = tw.ShowDialog();
            if (res != null)
            {
                if ((bool)res == true)
                {
                    SelectedToothDG.DataContext = MainWindow.GetClientTasks(cl.Id);

                    if (task.ToothCode != null & task.ClientId != null & task.Status != null)
                    {
                        StatusTable status = MainWindow.GetFirstStatusTableByDescription(task.Status);
                        if (status != null)
                        {
                            switch (task.ToothCode)
                            {
                                case 11:
                                    TB11.Text = status.StatusCode;
                                    break;
                                case 12:
                                    TB12.Text = status.StatusCode;
                                    break;
                                case 13:
                                    TB13.Text = status.StatusCode;
                                    break;
                                case 14:
                                    TB14.Text = status.StatusCode;
                                    break;
                                case 15:
                                    TB15.Text = status.StatusCode;
                                    break;
                                case 16:
                                    TB16.Text = status.StatusCode;
                                    break;
                                case 17:
                                    TB17.Text = status.StatusCode;
                                    break;
                                case 18:
                                    TB18.Text = status.StatusCode;
                                    break;
                                case 21:
                                    TB21.Text = status.StatusCode;
                                    break;
                                case 22:
                                    TB22.Text = status.StatusCode;
                                    break;
                                case 23:
                                    TB23.Text = status.StatusCode;
                                    break;
                                case 24:
                                    TB24.Text = status.StatusCode;
                                    break;
                                case 25:
                                    TB25.Text = status.StatusCode;
                                    break;
                                case 26:
                                    TB26.Text = status.StatusCode;
                                    break;
                                case 27:
                                    TB27.Text = status.StatusCode;
                                    break;
                                case 28:
                                    TB28.Text = status.StatusCode;
                                    break;
                                case 31:
                                    TB31.Text = status.StatusCode;
                                    break;
                                case 32:
                                    TB32.Text = status.StatusCode;
                                    break;
                                case 33:
                                    TB33.Text = status.StatusCode;
                                    break;
                                case 34:
                                    TB34.Text = status.StatusCode;
                                    break;
                                case 35:
                                    TB35.Text = status.StatusCode;
                                    break;
                                case 36:
                                    TB36.Text = status.StatusCode;
                                    break;
                                case 37:
                                    TB37.Text = status.StatusCode;
                                    break;
                                case 38:
                                    TB38.Text = status.StatusCode;
                                    break;
                                case 41:
                                    TB41.Text = status.StatusCode;
                                    break;
                                case 42:
                                    TB42.Text = status.StatusCode;
                                    break;
                                case 43:
                                    TB43.Text = status.StatusCode;
                                    break;
                                case 44:
                                    TB44.Text = status.StatusCode;
                                    break;
                                case 45:
                                    TB45.Text = status.StatusCode;
                                    break;
                                case 46:
                                    TB46.Text = status.StatusCode;
                                    break;
                                case 47:
                                    TB47.Text = status.StatusCode;
                                    break;
                                case 48:
                                    TB48.Text = status.StatusCode;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                }
            }
            try
            {
                var stat = (tw.SelectedToothDG.ItemsSource as List<Task>).FirstOrDefault().Status;
                var brush = new BrushConverter().ConvertFromString(MainWindow.GetFirstStatusTableByDescription(stat).StatusColourCode);
                btn.Background = (Brush)brush;
            }
            catch (Exception)
            {
            }
        }

        private void IdTypeCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            Client cl = DataContext as Client;

            if (cl != null)
            {
                if ((string)(cb.SelectedValue as ComboBoxItem).Content != cl.TypeID)
                {
                    cl.TypeID = (string)(cb.SelectedValue as ComboBoxItem).Content;
                }
            }
        }

        private void SexCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            Client cl = DataContext as Client;
            if (cl != null & (string)(cb.SelectedValue as ComboBoxItem).Content != cl.Sex)
            {
                cl.Sex = (string)(cb.SelectedValue as ComboBoxItem).Content;
            }
        }

        private void SelectedToothLV_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Client cl = DataContext as Client;
            cl.FirstDate = DateTime.Now;
            cl.LastDateUpdate = DateTime.Now;
            cl.TypeID = IdTypeCB.Text;
            if (ImagesListProfile.Items.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                List<string> ls = new List<string>();
                string selItem = (string)ImagesListProfile.SelectedItem;
                if (selItem != null)
                {
                    ls.Add(selItem);
                    sb.Append(selItem + ";");
                }
                foreach (string item in ImagesListProfile.Items)
                {
                    if (item == selItem)
                    {
                        continue;
                    }
                    sb.Append(item + ";");
                }
                cl.Image = sb.ToString();
            }
            else
            {
                cl.Image = null;
            }
            using (var con = new MySqlConnection("server=45.84.206.101;user=u158550680_Eddy;persistsecurityinfo=True;database=u158550680_dentist_db;password=Eddy8479;allowuservariables=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "UPDATE Client SET FirstName = @fname, SecondName = @sname, LastName = @lname, PersonalIdNumber = @pid, TypeID = @tid,BirthDate = @bdate, Sex = @sex, Phone1 = @ph1, Phone2 = @ph2, Email = @mail, Address = @addr, FirstDate = @fdate, LastDateUpdate = @ldate, ClinicalDisorders = @cdis, Image = @image, Notes = @note  WHERE Id = @cid";
                cmd.Parameters.AddWithValue("@cid", cl.Id);
                cmd.Parameters.AddWithValue("@fname", cl.FirstName);
                cmd.Parameters.AddWithValue("@sname", cl.SecondName);
                cmd.Parameters.AddWithValue("@lname", cl.LastName);
                cmd.Parameters.AddWithValue("@pid", cl.PersonalIdNumber);
                cmd.Parameters.AddWithValue("@bdate", cl.BirthDate);
                cmd.Parameters.AddWithValue("@sex", cl.Sex);
                cmd.Parameters.AddWithValue("@ph1", cl.Phone1);
                cmd.Parameters.AddWithValue("@ph2", cl.Phone2);
                cmd.Parameters.AddWithValue("@mail", cl.Email);
                cmd.Parameters.AddWithValue("@addr", cl.Address);
                cmd.Parameters.AddWithValue("@cdis", cl.ClinicalDisorders);
                cmd.Parameters.AddWithValue("@note", cl.Notes);

                cmd.Parameters.AddWithValue("@fdate", cl.FirstDate);
                cmd.Parameters.AddWithValue("@ldate", cl.LastDateUpdate);
                cmd.Parameters.AddWithValue("@tid", cl.TypeID);
                cmd.Parameters.AddWithValue("@image", cl.Image);
                cmd.ExecuteNonQuery();

                con.CloseAsync();
            }
            ToothPictureSP.Visibility = Visibility.Visible;
            try
            {
                (Owner as ClientsListWindow).Npw_Closed(Owner, new EventArgs());
            }
            catch (Exception)
            {
            }


        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Client cl = DataContext as Client;
            if (cl == null)
            {
                return;
            }
            if (cl.TypeID != null)
            {
                foreach (ComboBoxItem item in IdTypeCB.Items)
                {
                    if ((string)item.Content == cl.TypeID)
                    {
                        item.IsSelected = true;
                    }
                }
            }
            if (cl.Sex != null)
            {
                foreach (ComboBoxItem item in SexCB.Items)
                {
                    if ((string)item.Content == cl.Sex)
                    {
                        item.IsSelected = true;
                    }
                }
            }
            SelectedToothDG.DataContext = MainWindow.GetClientTasks(cl.Id);
            var statTables = MainWindow.GetStatusTables();
            LegendLV.DataContext = statTables;
            //var tasklist = context.Tasks.Where(t => t.ClientId == cl.Id).ToList();
            //var orderedTl = tasklist.OrderByDescending(d => d.Date).ToList();
            //Resources.Add("ListTasks", orderedTl);
            Resources.Add("ListTasks", MainWindow.GetClientTasks(cl.Id));
            Resources.Add("StatusCodes", statTables);
            LoggedInLabel.Content = (App.Current.MainWindow as MainWindow).LoggedInDoctor.Name;
        }

        public void ToothBtn18_Loaded(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            try
            {
                var cl = DataContext as Client;
                byte toothID = byte.Parse((string)btn.Content);
                var status = (Resources["ListTasks"] as List<Task>).Where(t => t.ToothCode == toothID).FirstOrDefault().Status;

            }
            catch (Exception)
            {
                btn.Background = Brushes.WhiteSmoke;
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
            tw.SelectedToothDG.DataContext = (Resources["ListTasks"] as List<Task>).Where(t => t.ToothCode == selectedTask.ToothCode).ToList();

            tw.NOICodeTB.Text = selectedTask.Price.ToString();
            tw.NOICodeByNOITB.Text = selectedTask.PriceNOI.ToString();
            tw.DTPriceTB.Text = selectedTask.PriceDT.ToString();

            bool? res = tw.ShowDialog();
            if (res != null)
            {
                if ((bool)res == true) { SelectedToothDG.DataContext = MainWindow.GetClientTasks(cl.Id); }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Наистина ли искате да изтриете потребител?", "Confirmation", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    var cl = DataContext as Client;
                    try
                    {
                        var tasksRemoveList = MainWindow.GetClientTasks(cl.Id);
                        foreach (var task in tasksRemoveList)
                        {
                            if (task.Image != null)
                            {
                                var taskImages = task.Image.Trim(';').Split(';').ToList();
                                foreach (var file in taskImages)
                                {
                                    try { System.IO.File.Delete(file); }
                                    catch (Exception) { }
                                }
                            }
                            using (var con = new MySqlConnection("server=45.84.206.101;user=u158550680_Eddy;persistsecurityinfo=True;database=u158550680_dentist_db;password=Eddy8479;allowuservariables=True"))
                            {
                                con.Open();
                                var cmd = new MySqlCommand();
                                cmd.Connection = con;
                                cmd.CommandText = "UPDATE Task SET IsDeleted = 1 WHERE Id = @cid";
                                cmd.Parameters.AddWithValue("@cid", task.Id);
                                cmd.ExecuteNonQuery();
                                con.CloseAsync();
                            }
                        }
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    if (cl.Image != null)
                    {
                        var clientImages = cl.Image.Trim(';').Split(';').ToList();
                        foreach (var cfile in clientImages)
                        {
                            try { System.IO.File.Delete(cfile); }
                            catch (Exception) { }
                        }
                    }
                    using (var con = new MySqlConnection("server=45.84.206.101;user=u158550680_Eddy;persistsecurityinfo=True;database=u158550680_dentist_db;password=Eddy8479;allowuservariables=True"))
                    {
                        con.Open();
                        var cmd = new MySqlCommand();
                        cmd.Connection = con;
                        cmd.CommandText = "UPDATE Client SET IsDeleted = 1 WHERE Id = @cid";
                        cmd.Parameters.AddWithValue("@cid", cl.Id);
                        cmd.ExecuteNonQuery();
                        con.CloseAsync();
                    }
                    try
                    {
                        var clientList = ((Owner as ClientsListWindow).DataContext as List<ClientListed>);
                        var cc = clientList.FirstOrDefault(c => c.Id == cl.Id);
                        clientList.Remove(cc);
                        (Owner as ClientsListWindow).Npw_Closed(Owner, new EventArgs());
                    }
                    catch (Exception)
                    {
                    }
                    this.Close();
                }
            }
            catch (Exception dc)
            {
                MessageBox.Show("Error deleting Client: " +
                        dc.Message);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var asw = new AddStatusWindow();

            asw.DataContext = new StatusTable();
            asw.ShowDialog();
            if (asw.DialogResult.HasValue)
            {
                if ((bool)asw.DialogResult == true)
                {
                    var st = asw.DataContext as StatusTable;
                    using (var con = new MySqlConnection("server=45.84.206.101;user=u158550680_Eddy;persistsecurityinfo=True;database=u158550680_dentist_db;password=Eddy8479;allowuservariables=True"))
                    {
                        con.Open();
                        var cmd = new MySqlCommand();
                        cmd.Connection = con;
                        cmd.CommandText = "INSERT INTO StatusTable (StatusCode, StatusDescription, StatusColourCode) VALUES (@scode, @sdesc, @scol)";
                        cmd.Parameters.AddWithValue("@scode", st.StatusCode);
                        cmd.Parameters.AddWithValue("@sdesc", st.StatusDescription);
                        cmd.Parameters.AddWithValue("@scol", st.StatusColourCode);
                        cmd.ExecuteNonQuery();
                        con.CloseAsync();
                        LegendLV.ItemsSource = MainWindow.GetStatusTables();
                    }
                }
            }
        }

        private void LegendLV_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var send = sender as ListView;
                var status = send.SelectedItem as StatusTable;
                if (e.Key == Key.Delete)
                {
                    if (status.Id > 0)
                    {
                        using (var con = new MySqlConnection("server=45.84.206.101;user=u158550680_Eddy;persistsecurityinfo=True;database=u158550680_dentist_db;password=Eddy8479;allowuservariables=True"))
                        {
                            con.Open();
                            var cmd = new MySqlCommand();
                            cmd.Connection = con;
                            cmd.CommandText = "DELETE FROM StatusTable WHERE Id = @id";
                            cmd.Parameters.AddWithValue("@id", status.Id);
                            cmd.ExecuteNonQuery();
                            con.CloseAsync();
                        }
                        LegendLV.ItemsSource = MainWindow.GetStatusTables();
                    }
                }
                if (e.Key == Key.Enter)
                {
                    var asw = new AddStatusWindow();
                    asw.DataContext = status;
                    asw.ShowDialog();
                    if (asw.DialogResult.HasValue)
                    {
                        if ((bool)asw.DialogResult == true)
                        {
                            using (var con = new MySqlConnection("server=45.84.206.101;user=u158550680_Eddy;persistsecurityinfo=True;database=u158550680_dentist_db;password=Eddy8479;allowuservariables=True"))
                            {
                                con.Open();
                                var cmd = new MySqlCommand();
                                cmd.Connection = con;
                                cmd.CommandText = "UPDATE StatusTable SET StatusCode = @scode, StatusDescription = @sdesc, StatusColourCode = @scol WHERE Id = @id";
                                cmd.Parameters.AddWithValue("@id", status.Id);
                                cmd.Parameters.AddWithValue("@scode", status.StatusCode);
                                cmd.Parameters.AddWithValue("@sdesc", status.StatusDescription);
                                cmd.Parameters.AddWithValue("@scol", status.StatusColourCode);
                                cmd.ExecuteNonQuery();
                            }
                            //ako ne raboti izvle4i spisaka ot BD
                            LegendLV.ItemsSource = MainWindow.GetStatusTables();

                        }
                        else
                        {
                            //context.Entry(asw.DataContext as StatusTable).Reload();
                            //LegendLV.DataContext = context.StatusTables.ToList();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

        }

        private void LegendLV_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var send = sender as ListView;
                var status = send.SelectedItem as StatusTable;

                var asw = new AddStatusWindow();
                asw.DataContext = status;
                asw.ShowDialog();
                if (asw.DialogResult.HasValue)
                {
                    if ((bool)asw.DialogResult == true)
                    {
                        using (var con = new MySqlConnection("server=45.84.206.101;user=u158550680_Eddy;persistsecurityinfo=True;database=u158550680_dentist_db;password=Eddy8479;allowuservariables=True"))
                        {
                            con.Open();
                            var cmd = new MySqlCommand();
                            cmd.Connection = con;
                            cmd.CommandText = "UPDATE StatusTable SET StatusCode = @scode, StatusDescription = @sdesc, StatusColourCode = @scol WHERE Id = @id";
                            cmd.Parameters.AddWithValue("@id", status.Id);
                            cmd.Parameters.AddWithValue("@scode", status.StatusCode);
                            cmd.Parameters.AddWithValue("@sdesc", status.StatusDescription);
                            cmd.Parameters.AddWithValue("@scol", status.StatusColourCode);
                            cmd.ExecuteNonQuery();
                        }
                        //ako ne raboti izvle4i spisaka ot BD
                        LegendLV.UpdateLayout();
                    }
                    else
                    {
                        //context.Entry(asw.DataContext as StatusTable).Reload();
                        //LegendLV.DataContext = context.StatusTables.ToList();
                    }
                }

            }
            catch (Exception)
            {
            }
        }

        private void OtherButton_Click(object sender, RoutedEventArgs e)
        {
            byte toothID = 100;
            Client cl = DataContext as Client;
            TaskWindow tw = new TaskWindow();
            tw.DataContext = cl;
            tw.ToothId.Content = toothID;
            tw.SelectedToothDG.ItemsSource = MainWindow.GetClientTaskByToothCode(cl.Id, toothID);

            Task task = new Task();
            task.Id = -1;
            task.Date = DateTime.Now;
            tw.DateDP.SelectedDate = task.Date;
            tw.PaidByCardRB.IsChecked = task.PaidByCard;
            tw.PaidCashRB.IsChecked = task.PaidCash;
            tw.ImagesList.ItemsSource = new List<string>();
            tw.Resources.Add("T", task);
            bool? res = tw.ShowDialog();
            if (res != null)
            {
                if ((bool)res == true) { SelectedToothDG.DataContext = MainWindow.GetClientTasks(cl.Id); }
            }

        }

        private void AllMilkBtn_Click(object sender, RoutedEventArgs e)
        {
            var cl = DataContext as Client;
            cl.C51 = true;
            cl.C52 = true;
            cl.C53 = true;
            cl.C54 = true;
            cl.C55 = true;
            cl.C61 = true;
            cl.C62 = true;
            cl.C63 = true;
            cl.C64 = true;
            cl.C65 = true;
            cl.C71 = true;
            cl.C72 = true;
            cl.C73 = true;
            cl.C74 = true;
            cl.C75 = true;
            cl.C81 = true;
            cl.C82 = true;
            cl.C83 = true;
            cl.C84 = true;
            cl.C85 = true;
            CB51.IsChecked = true;
            CB52.IsChecked = true;
            CB53.IsChecked = true;
            CB54.IsChecked = true;
            CB55.IsChecked = true;
            CB61.IsChecked = true;
            CB62.IsChecked = true;
            CB63.IsChecked = true;
            CB64.IsChecked = true;
            CB65.IsChecked = true;
            CB71.IsChecked = true;
            CB72.IsChecked = true;
            CB73.IsChecked = true;
            CB74.IsChecked = true;
            CB75.IsChecked = true;
            CB81.IsChecked = true;
            CB82.IsChecked = true;
            CB83.IsChecked = true;
            CB84.IsChecked = true;
            CB85.IsChecked = true;
        }


        private void CBLostFocus(object sender, RoutedEventArgs e)
        {

            var cl = DataContext as Client;
            if (cl.Id > 0)
            {
                using (var con = new MySqlConnection("server=45.84.206.101;user=u158550680_Eddy;persistsecurityinfo=True;database=u158550680_dentist_db;password=Eddy8479;allowuservariables=True"))
                {
                    con.Open();
                    var cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@id", cl.Id);
                    if (typeof(CheckBox).Equals(sender.GetType()))
                    {
                        CheckBox cb = (sender as CheckBox);
                        var col = cb.Name.Substring(2);
                        cmd.CommandText = string.Format("UPDATE Client SET a{0} = @val WHERE Id = @id", col);
                        cmd.Parameters.AddWithValue("@val", cb.IsChecked.HasValue ? cb.IsChecked.Value : false);
                    }
                    else
                    {
                        TextBox tb = (sender as TextBox);
                        var col = tb.Name.Substring(2);
                        cmd.CommandText = string.Format("UPDATE Client SET a{0} = @val WHERE Id = @id", col);
                        cmd.Parameters.AddWithValue("@val", tb.Text);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void ImageTask_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ImagesListProfile.SelectedItem == null)
            {
                return;
            }
            string newFileName = (string)ImagesListProfile.SelectedItem;
            System.Diagnostics.Process.Start(newFileName);
        }

        private void ImagesListProfile_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                try
                {
                    var client = DataContext as Client;
                    string selectedItem = (string)ImagesListProfile.SelectedItem;
                    client.Image = client.Image.Replace(selectedItem + ";", "");
                    ImagesListProfile.ItemsSource = client.Image.Trim(';').Split(';').ToList();
                    if (ImagesListProfile.Items.Count > 0)
                    {
                        ImagesListProfile.SelectedIndex = 0;
                    }
                    ImagesListProfile.UpdateLayout();
                    System.IO.File.Delete(selectedItem);
                }
                catch (Exception)
                {
                }
            }
        }

        void ImagesListProfile_SourceUpdated(object sender, DataTransferEventArgs e)
        {

        }


        private void SelectedToothDG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var status = (Task)SelectedToothDG.SelectedItem;

                if (status.Image != null)
                {
                    var taskImages = status.Image.Trim(';').Split(';').ToList();
                    foreach (var file in taskImages)
                    {
                        try { System.IO.File.Delete(file); }
                        catch (Exception) { }
                    }
                }
                MainWindow.DeleteTaskById(status.Id);
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
    }
}
