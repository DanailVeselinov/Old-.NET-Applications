using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DentistDB
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

        public void OpenLoginWindow()
        {
            LoginWindow lw = new LoginWindow();
            lw.DataContext = MainWindow.GetDoctorsList();
            lw.Owner = this;
            var res = lw.ShowDialog();
            if (res.HasValue)
            {
                if (res.Value)
                {
                    LoggedInDoctor = (DataContext as Doctor);
                    LoggedInLabel.Content = LoggedInDoctor.Name;
                }
                else
                {
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }
        }

        public Doctor LoggedInDoctor;

        private void NewPatientBtn_Click(object sender, RoutedEventArgs e)
        {
            New_Patient patientWindow = new New_Patient();

            Client newClient = new Client();
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "INSERT INTO Client () VALUES ()";
                cmd.ExecuteNonQuery();
                newClient.Id = (int)cmd.LastInsertedId;
                con.CloseAsync();
            }
            patientWindow.DataContext = newClient;
            patientWindow.Owner = this;
            patientWindow.ShowDialog();

        }


        private void PatientsBtn_Click(object sender, RoutedEventArgs e)
        {
            var clients = new List<ClientListed>();
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "SELECT Id, FirstName, SecondName, LastName, PersonalIdNumber, TypeID, Phone1, Phone2, Email, LastDateUpdate FROM Client WHERE(IsDeleted = 0)";
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        clients.Add(new ClientListed(rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0), rdr.IsDBNull(1) ? "" : rdr.GetString(1), rdr.IsDBNull(2) ? "" : rdr.GetString(2), rdr.IsDBNull(3) ? "" : rdr.GetString(3), rdr.IsDBNull(4) ? "" : rdr.GetString(4), rdr.IsDBNull(5) ? "" : rdr.GetString(5), rdr.IsDBNull(6) ? "" : rdr.GetString(6), rdr.IsDBNull(7) ? "" : rdr.GetString(7), rdr.IsDBNull(8) ? "" : rdr.GetString(8), rdr.IsDBNull(5) ? DateTime.Now : rdr.GetDateTime(9)));
                    }
                }
                con.CloseAsync();
            }

            var clwindow = new ClientsListWindow();
            clwindow.DataContext = clients;
            clwindow.Show();
        }

        public static BitmapSource GetPNGImageFmBinary(byte[] imageArray)
        {
            if (imageArray == null)
            {
                return null;
            }
            var byteList = new List<byte>(imageArray);
            byteList.TrimExcess();
            BitmapDecoder pngDecoder;
            Stream stream = new MemoryStream();
            stream.Write(byteList.ToArray(), 0, byteList.Count);
            pngDecoder = JpegBitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
            BitmapSource bs = pngDecoder.Frames[0];
            return bs;

        }

        private void StatusBtn_Click(object sender, RoutedEventArgs e)
        {
            var aw = new AccountWindow();
            List<Doctor> DocList = GetDoctorsList();
            var nd = new Doctor();
            nd.Name = "All";
            DocList.Add(nd);
            aw.DoctorsCB.ItemsSource = DocList;
            var fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            aw.FromDP.SelectedDate = fromDate;
            aw.ToDP.SelectedDate = DateTime.Now;
            aw.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            aw.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            aw.Show();
        }

        public static List<Doctor> GetDoctorsList()
        {
            List<Doctor> DocList = new List<Doctor>();
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "SELECT * FROM Doctors WHERE(IsDeleted = 0)";
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var doc = new Doctor();
                        doc.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                        doc.Name = rdr.IsDBNull(1) ? "" : rdr.GetString(1);
                        doc.Password = rdr.IsDBNull(2) ? "" : rdr.GetString(2);
                        doc.IsDeleted = rdr.IsDBNull(3) ? false : rdr.GetBoolean(3);
                        DocList.Add(doc);
                    }
                }
                con.CloseAsync();
            }
            return DocList;
        }

        public static Doctor GetDoctorByName(string name)
        {
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "SELECT * FROM Doctors WHERE IsDeleted = 0 AND Name = @name";
                cmd.Parameters.AddWithValue("@name", name);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var doc = new Doctor();
                        doc.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                        doc.Name = rdr.IsDBNull(1) ? "" : rdr.GetString(1);
                        doc.Password = rdr.IsDBNull(2) ? "" : rdr.GetString(2);
                        doc.IsDeleted = rdr.IsDBNull(3) ? false : rdr.GetBoolean(3);
                        con.CloseAsync();
                        return doc;
                    }
                }
            }
            return null;
        }

        public static List<Supplier> GetSuppliersList()
        {
            List<Supplier> supList = new List<Supplier>();
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "SELECT * FROM Suppliers";
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var sup = new Supplier();
                        sup.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                        sup.Name = rdr.IsDBNull(1) ? "" : rdr.GetString(1);
                        sup.Address = rdr.IsDBNull(2) ? "" : rdr.GetString(2);
                        sup.BulStat = rdr.IsDBNull(3) ? "" : rdr.GetString(3);
                        supList.Add(sup);
                    }
                }
                con.CloseAsync();
            }
            return supList;
        }

        public static Supplier GetSupplierById(int id)
        {
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "SELECT * FROM Suppliers WHERE Id = @id LIMIT 1";
                cmd.Parameters.AddWithValue("@id", id);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var sup = new Supplier();
                        sup.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                        sup.Name = rdr.IsDBNull(1) ? "" : rdr.GetString(1);
                        sup.Address = rdr.IsDBNull(2) ? "" : rdr.GetString(2);
                        sup.BulStat = rdr.IsDBNull(3) ? "" : rdr.GetString(3);
                        con.CloseAsync();
                        return sup;
                    }
                }
            }
            return null;
        }

        public static List<InvoiceItem> GetInvoiceItemsByInvoiceId(int invoiceId)
        {
            List<InvoiceItem> invItemsList = new List<InvoiceItem>();
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "SELECT * FROM InvoiceItem WHERE InvoiceId = @id";
                cmd.Parameters.AddWithValue("@id", invoiceId);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var invitem = new InvoiceItem();
                        invitem.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                        invitem.InvoiceId = rdr.IsDBNull(1) ? 0 : rdr.GetInt32(1);
                        invitem.ItemName = rdr.IsDBNull(2) ? "" : rdr.GetString(2);
                        if (rdr.IsDBNull(3)) { invitem.ValueOfPayment = rdr.GetDecimal(3); }
                        invitem.CurrencyOfPayment = rdr.IsDBNull(4) ? "" : rdr.GetString(4);
                        invItemsList.Add(invitem);
                    }
                }
                con.CloseAsync();
            }
            return invItemsList;
        }

        public static List<Invoice> GetAllInvoices()
        {
            List<Invoice> invItemsList = new List<Invoice>();
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "SELECT * FROM Invoice WHERE(IsDeleted=0)";
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var item = new Invoice();
                        item.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                        item.SupplierId = rdr.IsDBNull(1) ? 0 : rdr.GetInt32(1);
                        item.UserId = rdr.IsDBNull(2) ? 0 : rdr.GetInt32(2);
                        if (rdr.IsDBNull(3)) { item.IsCashPayment = rdr.GetBoolean(3); }
                        item.InvoiceNumber = rdr.IsDBNull(4) ? "" : rdr.GetString(4);
                        if (rdr.IsDBNull(5)) { item.DateOfPayment = rdr.GetDateTime(5); }
                        if (rdr.IsDBNull(6)) { item.Sum = rdr.GetDecimal(6); }
                        item.Currency = rdr.IsDBNull(7) ? "" : rdr.GetString(7);
                        if (rdr.IsDBNull(8)) { item.IsDeleted = rdr.GetBoolean(8); }
                        invItemsList.Add(item);
                    }
                }
                con.CloseAsync();
            }
            return invItemsList;
        }

        public static List<Task> GetClientTaskByToothCode(int cid, byte toothCode)
        {
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "SELECT * FROM Task WHERE ClientId = @cid AND ToothCode = @tcode AND IsDeleted = 0 ORDER BY Date DESC";
                cmd.Parameters.AddWithValue("@cid", cid);
                cmd.Parameters.AddWithValue("@tcode", toothCode);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    List<Task> tasksList = new List<Task>();
                    while (rdr.Read())
                    {
                        var item = new Task();
                        item.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                        item.ClientId = rdr.IsDBNull(1) ? 0 : rdr.GetInt32(1);
                        if (!rdr.IsDBNull(2)) { item.ToothCode = rdr.GetByte(2); }
                        if (!rdr.IsDBNull(3)) { item.Date = rdr.GetDateTime(3); }
                        if (!rdr.IsDBNull(4)) { item.Status = rdr.GetString(4); }
                        if (!rdr.IsDBNull(5)) { item.Diagnose = rdr.GetString(5); }
                        if (!rdr.IsDBNull(6)) { item.Price = rdr.GetDouble(6); }
                        if (!rdr.IsDBNull(7)) { item.Image = rdr.GetString(7); }
                        if (!rdr.IsDBNull(8)) { item.DoctorId = rdr.GetInt32(8); }
                        if (!rdr.IsDBNull(9)) { item.PriceNOI = rdr.GetDouble(9); }
                        if (!rdr.IsDBNull(10)) { item.PriceDT = rdr.GetDouble(10); }
                        if (!rdr.IsDBNull(11)) { item.IsDeleted = rdr.GetBoolean(11); }
                        if (!rdr.IsDBNull(12)) { item.PaidByCard = rdr.GetBoolean(12); }
                        if (!rdr.IsDBNull(13)) { item.PaidCash = rdr.GetBoolean(13); }
                        tasksList.Add(item);
                    }
                    con.CloseAsync();
                    return tasksList;
                }
            }
            return null;
        }

        public static List<Task> GetClientTasks(int cid)
        {
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "SELECT * FROM Task WHERE ClientId = @cid AND IsDeleted = 0 ORDER BY Date DESC";
                cmd.Parameters.AddWithValue("@cid", cid);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    List<Task> tasksList = new List<Task>();
                    while (rdr.Read())
                    {
                        var item = new Task();
                        item.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                        item.ClientId = rdr.IsDBNull(1) ? 0 : rdr.GetInt32(1);
                        if (!rdr.IsDBNull(2)) { item.ToothCode = rdr.GetByte(2); }
                        if (!rdr.IsDBNull(3)) { item.Date = rdr.GetDateTime(3); }
                        if (!rdr.IsDBNull(4)) { item.Status = rdr.GetString(4); }
                        if (!rdr.IsDBNull(5)) { item.Diagnose = rdr.GetString(5); }
                        if (!rdr.IsDBNull(6)) { item.Price = rdr.GetDouble(6); }
                        if (!rdr.IsDBNull(7)) { item.Image = rdr.GetString(7); }
                        if (!rdr.IsDBNull(8)) { item.DoctorId = rdr.GetInt32(8); }
                        if (!rdr.IsDBNull(9)) { item.PriceNOI = rdr.GetDouble(9); }
                        if (!rdr.IsDBNull(10)) { item.PriceDT = rdr.GetDouble(10); }
                        if (!rdr.IsDBNull(11)) { item.IsDeleted = rdr.GetBoolean(11); }
                        if (!rdr.IsDBNull(12)) { item.PaidByCard = rdr.GetBoolean(12); }
                        if (!rdr.IsDBNull(13)) { item.PaidCash = rdr.GetBoolean(13); }
                        tasksList.Add(item);
                    }
                    con.CloseAsync();
                    return tasksList;
                }
            }
            return null;
        }

        public static List<StatusTable> GetStatusTableByDescription(string description)
        {
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "SELECT * FROM StatusTable WHERE StatusDescription = @sdesc";
                cmd.Parameters.AddWithValue("@sdesc", description);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    List<StatusTable> statusList = new List<StatusTable>();
                    while (rdr.Read())
                    {
                        var item = new StatusTable();
                        item.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                        if (!rdr.IsDBNull(1)) { item.StatusCode = rdr.GetString(1); }
                        if (!rdr.IsDBNull(2)) { item.StatusDescription = rdr.GetString(2); }
                        if (!rdr.IsDBNull(3)) { item.StatusColourCode = rdr.GetString(3); }
                        statusList.Add(item);
                    }
                    con.CloseAsync();
                    return statusList;
                }
            }
            return null;
        }

        public static List<StatusTable> GetStatusTables()
        {
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "SELECT * FROM StatusTable";
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    List<StatusTable> statusList = new List<StatusTable>();
                    while (rdr.Read())
                    {
                        var item = new StatusTable();
                        item.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                        if (!rdr.IsDBNull(1)) { item.StatusCode = rdr.GetString(1); }
                        if (!rdr.IsDBNull(2)) { item.StatusDescription = rdr.GetString(2); }
                        if (!rdr.IsDBNull(3)) { item.StatusColourCode = rdr.GetString(3); }
                        statusList.Add(item);
                    }
                    con.CloseAsync();
                    return statusList;
                }
            }
            return null;
        }

        public static StatusTable GetFirstStatusTableByDescription(string description)
        {
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "SELECT * FROM StatusTable WHERE StatusDescription = @sdesc LIMIT 1";
                cmd.Parameters.AddWithValue("@sdesc", description);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var item = new StatusTable();
                        item.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                        if (!rdr.IsDBNull(1)) { item.StatusCode = rdr.GetString(1); }
                        if (!rdr.IsDBNull(2)) { item.StatusDescription = rdr.GetString(2); }
                        if (!rdr.IsDBNull(3)) { item.StatusColourCode = rdr.GetString(3); }

                        con.CloseAsync();
                        return item;
                    }
                }
            }
            return null;
        }

        private void ExpensesBtn_Click(object sender, RoutedEventArgs e)
        {
            var ew = new ExpensesWindow();
            List<Invoice> InvList = new List<Invoice>();
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "SELECT * FROM Invoice WHERE(IsDeleted = 0)";
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var inv = new Invoice();
                        inv.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                        inv.SupplierId = rdr.GetInt32(1);
                        inv.UserId = rdr.GetInt32(2);
                        inv.IsCashPayment = rdr.GetBoolean(3);
                        inv.InvoiceNumber = rdr.IsDBNull(4) ? "" : rdr.GetString(4);
                        inv.DateOfPayment = rdr.GetDateTime(5);
                        inv.Sum = rdr.GetDecimal(6);
                        inv.Currency = rdr.IsDBNull(7) ? "" : rdr.GetString(7);
                        inv.IsDeleted = rdr.IsDBNull(8) ? false : rdr.GetBoolean(8);
                        InvList.Add(inv);
                    }
                }
                con.CloseAsync();
            }
            ew.InvoicesDG.ItemsSource = InvList;
            ew.Show();
        }

        public static void DeleteTaskById(int id)
        {
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "UPDATE Task SET IsDeleted = 1 WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteDoctorById(int id)
        {
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "UPDATE Doctors SET IsDeleted = 1 WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public void LogOut_Click(object sender, RoutedEventArgs e)
        {
            this.DataContext = null;
            LoggedInDoctor = null;
            OpenLoginWindow();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OpenLoginWindow();

        }
    }
}