using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace DentistDB
{
    /// <summary>
    /// Interaction logic for AccountWindow.xaml
    /// </summary>
    public partial class AccountWindow : Window
    {
        public AccountWindow()
        {
            InitializeComponent();

        }


        private void FromDP_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTaskDG();
        }

        private void UpdateTaskDG()
        {
            DateTime from = FromDP.SelectedDate.HasValue ? new DateTime(FromDP.SelectedDate.Value.Year, FromDP.SelectedDate.Value.Month, FromDP.SelectedDate.Value.Day, 0, 0, 0) : DateTime.MinValue;
            DateTime to = ToDP.SelectedDate.HasValue ? new DateTime(ToDP.SelectedDate.Value.Year, ToDP.SelectedDate.Value.Month, ToDP.SelectedDate.Value.Day, 23, 59, 59) : DateTime.Now;
            Doctor doc = DoctorsCB.SelectedItem as Doctor;

            List<AccountTask> lAT = GetAccountTasks(from, to, doc, IsDeletedCB.IsChecked.Value);
            TasksDG.ItemsSource = lAT;
            List<Invoice> ListInv = new List<Invoice>();
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = String.Format("SELECT Id, SupplierId, UserId, IsCashPayment, InvoiceNumber, DateOfPayment, Sum, Currency, IsDeleted FROM Invoice WHERE (CAST(DateOfPayment AS DATETIME) BETWEEN CAST('{0}' AS DATETIME) AND CAST('{1}' AS DATETIME)) AND (IsDeleted = 0)", from.ToString("yyyy-MM-dd HH:mm:ss"), to.ToString("yyyy-MM-dd HH:mm:ss"));
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var inv = new Invoice();
                        inv.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                        inv.SupplierId = rdr.IsDBNull(1) ? 0 : rdr.GetInt32(1);
                        inv.UserId = rdr.IsDBNull(2) ? 0 : rdr.GetInt32(2);
                        inv.IsCashPayment = rdr.GetBoolean(3);
                        inv.InvoiceNumber = rdr.IsDBNull(4) ? "" : rdr.GetString(4);
                        inv.DateOfPayment = rdr.GetDateTime(5);
                        inv.Sum = rdr.GetDecimal(6);
                        inv.Currency = rdr.IsDBNull(7) ? "" : rdr.GetString(7);
                        inv.IsDeleted = rdr.IsDBNull(8) ? false : rdr.GetBoolean(8);
                        ListInv.Add(inv);
                    }
                }
                con.CloseAsync();
            }

            InvoicesDG.ItemsSource = ListInv;
            decimal expences = 0;
            foreach (Invoice item in InvoicesDG.ItemsSource)
            {
                try
                {
                    expences = expences + item.Sum.Value;
                }
                catch (Exception)
                {
                }
            }
            TotalExpences.Text = expences.ToString("N2");
            double p = 0;
            double pi = 0;
            double pii = 0;
            foreach (AccountTask tk in TasksDG.ItemsSource)
            {
                p += tk.Price.HasValue ? tk.Price.Value : 0;
                pi += tk.PriceNOI.HasValue ? tk.PriceNOI.Value : 0;
                pii += tk.PriceDT.HasValue ? tk.PriceDT.Value : 0;
            }
            TotalTB.Text = p.ToString("N2");
            TotalNOITB.Text = pi.ToString("N2");
            TotalDTTB.Text = pii.ToString("N2");
        }

        private static List<AccountTask> GetAccountTasks(DateTime from, DateTime to, Doctor doc, bool isDeleted = false)
        {
            List<AccountTask> lAT = new List<AccountTask>();
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                if (doc != null)
                {
                    if (doc.Id > 0)
                    {
                        cmd.CommandText = String.Format("SELECT a.Id, a.ClientId, a.ToothCode, a.Date, a.Status, a.Diagnose, a.Price, a.PriceNOI, a.PriceDT, b.FirstName, b.SecondName, b.LastName , c.Name, a.IsDeleted FROM Task a JOIN Client b ON a.ClientId = b.Id JOIN Doctors c ON a.DoctorId = c.Id WHERE (CAST(a.Date AS DATETIME) BETWEEN CAST('{0}' AS DATETIME) AND CAST('{1}' AS DATETIME)) {2} AND (a.DoctorId =@did)", from.ToString("yyyy-MM-dd HH:mm:ss"), to.ToString("yyyy-MM-dd HH:mm:ss"), !isDeleted ? "AND (a.IsDeleted = 0)" : "");
                        cmd.Parameters.AddWithValue("@did", doc.Id);
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                var task = new AccountTask();
                                task.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                                task.CId = rdr.IsDBNull(1) ? 0 : rdr.GetInt32(1);
                                task.ToothCode = rdr.IsDBNull(2) ? (byte)0 : rdr.GetByte(2);
                                task.Date = rdr.IsDBNull(3) ? DateTime.Now : rdr.GetDateTime(3);
                                task.Status = rdr.IsDBNull(4) ? "" : rdr.GetString(4);
                                task.Diagnose = rdr.IsDBNull(5) ? "" : rdr.GetString(5);
                                if (!rdr.IsDBNull(6)) { task.Price = rdr.GetDouble(6); }
                                if (!rdr.IsDBNull(12)) { task.DoctorId = rdr.GetString(12); }
                                if (!rdr.IsDBNull(7)) { task.PriceNOI = rdr.GetDouble(7); }
                                if (!rdr.IsDBNull(8)) { task.PriceDT = rdr.GetDouble(8); }
                                task.ClientId = (rdr.IsDBNull(9) ? "" : rdr.GetString(9)) + " " + (rdr.IsDBNull(10) ? "" : rdr.GetString(10)) + " " + (rdr.IsDBNull(11) ? "" : rdr.GetString(11)) + " ";
                                if (!rdr.IsDBNull(13)) { task.IsDeleted = rdr.GetBoolean(13); }

                                lAT.Add(task);
                            }
                        }
                        return lAT;
                    }
                }
                cmd.CommandText = String.Format("SELECT a.Id, a.ClientId, a.ToothCode, a.Date, a.Status, a.Diagnose, a.Price, a.PriceNOI, a.PriceDT, b.FirstName, b.SecondName, b.LastName, a.PaidByCard, a.PaidCash, c.Name, a.IsDeleted FROM Task a JOIN Client b ON a.ClientId = b.Id JOIN Doctors c ON a.DoctorId = c.Id WHERE (CAST(a.Date AS DATETIME) BETWEEN CAST('{0}' AS DATETIME) AND CAST('{1}' AS DATETIME)) {2}", from.ToString("yyyy-MM-dd HH:mm:ss"), to.ToString("yyyy-MM-dd HH:mm:ss"), !isDeleted ? "AND (a.IsDeleted = 0)" : "");
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var task = new AccountTask();
                        task.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                        task.CId = rdr.IsDBNull(1) ? 0 : rdr.GetInt32(1);
                        task.ToothCode = rdr.IsDBNull(2) ? (byte)0 : rdr.GetByte(2);
                        task.Date = rdr.IsDBNull(3) ? DateTime.Now : rdr.GetDateTime(3);
                        task.Status = rdr.IsDBNull(4) ? "" : rdr.GetString(4);
                        task.Diagnose = rdr.IsDBNull(5) ? "" : rdr.GetString(5);
                        if (!rdr.IsDBNull(6)) { task.Price = rdr.GetDouble(6); }
                        if (!rdr.IsDBNull(12)) { task.PaidByCard = rdr.GetBoolean(12); }
                        if (!rdr.IsDBNull(13)) { task.PaidCash = rdr.GetBoolean(13); }
                        if (!rdr.IsDBNull(7)) { task.PriceNOI = rdr.GetDouble(7); }
                        if (!rdr.IsDBNull(8)) { task.PriceDT = rdr.GetDouble(8); }
                        task.ClientId = (rdr.IsDBNull(9) ? "" : rdr.GetString(9)) + " " + (rdr.IsDBNull(10) ? "" : rdr.GetString(10)) + " " + (rdr.IsDBNull(11) ? "" : rdr.GetString(11)) + " ";

                        if (!rdr.IsDBNull(14)) { task.DoctorId = rdr.GetString(14); }
                        if (!rdr.IsDBNull(15)) { task.IsDeleted = rdr.GetBoolean(15); }
                        lAT.Add(task);
                    }
                }
                return lAT;
            }
        }

        //private void DoctorsCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    Doctor doc = DoctorsCB.SelectedItem as Doctor;
        //    if (doc == null)
        //    {
        //        return;
        //    }
        //    DateTime from = FromDP.SelectedDate.HasValue ? new DateTime(FromDP.SelectedDate.Value.Year, FromDP.SelectedDate.Value.Month, FromDP.SelectedDate.Value.Day, 0, 0, 0) : DateTime.MinValue;
        //    DateTime to = ToDP.SelectedDate.HasValue ? new DateTime(ToDP.SelectedDate.Value.Year, ToDP.SelectedDate.Value.Month, ToDP.SelectedDate.Value.Day, 23, 59, 59) : DateTime.Now;

        //    List<AccountTask> lAT = new List<AccountTask>();
        //    using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
        //    {
        //        con.Open();

        //        var cmd = new MySqlCommand();
        //        cmd.Connection = con;
        //        if (doc.Id > 0)
        //        {
        //            cmd.CommandText = String.Format("SELECT a.Id, a.ClientId, a.ToothCode, a.Date, a.Status, a.Diagnose, a.Price, a.PriceNOI, a.PriceDT, b.FirstName, b.SecondName, b.LastName , c.Name FROM Task a JOIN Client b ON a.ClientId = b.Id JOIN Doctors c ON a.DoctorId = c.Id WHERE (CAST(a.Date AS DATETIME) BETWEEN CAST('{0}' AS DATETIME) AND CAST('{1}' AS DATETIME)) AND (a.IsDeleted = 0) AND (a.DoctorId =@did)", from.ToString("yyyy-MM-dd HH:mm:ss"), to.ToString("yyyy-MM-dd HH:mm:ss"));
        //            cmd.Parameters.AddWithValue("@did", doc.Id);
        //            using (MySqlDataReader rdr = cmd.ExecuteReader())
        //            {
        //                while (rdr.Read())
        //                {
        //                    var task = new AccountTask();
        //                    task.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
        //                    task.CId = rdr.IsDBNull(1) ? 0 : rdr.GetInt32(1);
        //                    task.ToothCode = rdr.IsDBNull(2) ? (byte)0 : rdr.GetByte(2);
        //                    task.Date = rdr.IsDBNull(3) ? DateTime.Now : rdr.GetDateTime(3);
        //                    task.Status = rdr.IsDBNull(4) ? "" : rdr.GetString(4);
        //                    task.Diagnose = rdr.IsDBNull(5) ? "" : rdr.GetString(5);
        //                    if (!rdr.IsDBNull(6)) { task.Price = rdr.GetDouble(6); }
        //                    if (!rdr.IsDBNull(12)) { task.DoctorId = rdr.GetString(12); }
        //                    if (!rdr.IsDBNull(7)) { task.PriceNOI = rdr.GetDouble(7); }
        //                    if (!rdr.IsDBNull(8)) { task.PriceDT = rdr.GetDouble(8); }
        //                    task.ClientId = (rdr.IsDBNull(9) ? "" : rdr.GetString(9)) + " " + (rdr.IsDBNull(10) ? "" : rdr.GetString(10)) + " " + (rdr.IsDBNull(11) ? "" : rdr.GetString(11)) + " ";
        //                    task.PaidByCard = rdr.IsDBNull(13) ? false : rdr.GetBoolean(13);
        //                    lAT.Add(task);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            cmd.CommandText = String.Format("SELECT a.Id, a.ClientId, a.ToothCode, a.Date, a.Status, a.Diagnose, a.Price, a.PriceNOI, a.PriceDT, b.FirstName, b.SecondName, b.LastName, a.PaidByCard, a.PaidCash FROM Task a JOIN Client b ON a.ClientId = b.Id WHERE (CAST(a.Date AS DATETIME) BETWEEN CAST('{0}' AS DATETIME) AND CAST('{1}' AS DATETIME)) AND (a.IsDeleted = 0)", from.ToString("yyyy-MM-dd HH:mm:ss"), to.ToString("yyyy-MM-dd HH:mm:ss"));
        //            using (MySqlDataReader rdr = cmd.ExecuteReader())
        //            {
        //                while (rdr.Read())
        //                {
        //                    var task = new AccountTask();
        //                    task.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
        //                    task.CId = rdr.IsDBNull(1) ? 0 : rdr.GetInt32(1);
        //                    task.ToothCode = rdr.IsDBNull(2) ? (byte)0 : rdr.GetByte(2);
        //                    task.Date = rdr.IsDBNull(3) ? DateTime.Now : rdr.GetDateTime(3);
        //                    task.Status = rdr.IsDBNull(4) ? "" : rdr.GetString(4);
        //                    task.Diagnose = rdr.IsDBNull(5) ? "" : rdr.GetString(5);
        //                    if (!rdr.IsDBNull(6)) { task.Price = rdr.GetDouble(6); }
        //                    if (!rdr.IsDBNull(12)) { task.PaidByCard = rdr.GetBoolean(12); }
        //                    if (!rdr.IsDBNull(13)) { task.PaidCash = rdr.GetBoolean(13); }
        //                    if (!rdr.IsDBNull(7)) { task.PriceNOI = rdr.GetDouble(7); }
        //                    if (!rdr.IsDBNull(8)) { task.PriceDT = rdr.GetDouble(8); }
        //                    task.ClientId = (rdr.IsDBNull(9) ? "" : rdr.GetString(9)) + " " + (rdr.IsDBNull(10) ? "" : rdr.GetString(10)) + " " + (rdr.IsDBNull(11) ? "" : rdr.GetString(11)) + " ";
        //                    task.PaidByCard = rdr.IsDBNull(13) ? false : rdr.GetBoolean(13);
        //                    lAT.Add(task);
        //                }
        //            }
        //        }
        //    }
        //    TasksDG.ItemsSource = lAT;

        //    decimal expences = 0;
        //    foreach (Invoice item in InvoicesDG.ItemsSource)
        //    {
        //        try
        //        {
        //            expences = expences + item.Sum.Value;
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }
        //    TotalExpences.Text = expences.ToString("N2");
        //    double p = 0;
        //    double pi = 0;
        //    double pii = 0;
        //    foreach (AccountTask tk in TasksDG.ItemsSource)
        //    {
        //        p += tk.Price.HasValue ? tk.Price.Value : 0;
        //        pi += tk.PriceNOI.HasValue ? tk.PriceNOI.Value : 0;
        //        pii += tk.PriceDT.HasValue ? tk.PriceDT.Value : 0;
        //    }
        //    TotalTB.Text = p.ToString("N2");
        //    TotalNOITB.Text = pi.ToString("N2");
        //    TotalDTTB.Text = pii.ToString("N2");
        //}

        private void InvoicesDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            Invoice inv = (InvoicesDG.SelectedItem as Invoice);
            if (inv == null)
            {
                return;
            }
            var ew = new Add_ExpensesWindow();


            if (inv.SupplierId != null)
            {
                Supplier supplier = null;
                using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
                {
                    con.Open();
                    var cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = "SELECT Id, Name, Address, BulStat FROM Suppliers WHERE (Id = @id) LIMIT 1";
                    cmd.Parameters.AddWithValue("@id", inv.SupplierId);

                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            supplier = new Supplier();
                            supplier.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                            supplier.Name = rdr.IsDBNull(1) ? "" : rdr.GetString(1);
                            supplier.Address = rdr.IsDBNull(2) ? "" : rdr.GetString(2);
                            supplier.BulStat = rdr.IsDBNull(3) ? "" : rdr.GetString(3);
                        }
                    }
                    con.CloseAsync();
                }
                if (supplier != null)
                {
                    ew.AddressTB.Text = supplier.Address;
                    ew.CompanyTB.Text = supplier.Name;
                    ew.BulstatTB.Text = supplier.BulStat;

                }
            }
            ew.RecieptTB.Text = inv.InvoiceNumber;
            ew.DatePaidTB.SelectedDate = inv.DateOfPayment;
            List<InvoiceItem> invItemsList = new List<InvoiceItem>();
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "SELECT Id, InvoiceId, ItemName, ValueOfPayment, CurrencyOfPayment FROM InvoiceItem WHERE (InvoiceId = @id)";
                cmd.Parameters.AddWithValue("@id", inv.Id);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        InvoiceItem ii = new InvoiceItem();
                        ii.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                        ii.InvoiceId = rdr.IsDBNull(1) ? 0 : rdr.GetInt32(1);
                        ii.ValueOfPayment = rdr.GetDecimal(2);
                        ii.CurrencyOfPayment = rdr.IsDBNull(3) ? "" : rdr.GetString(3);
                        invItemsList.Add(ii);
                    }
                }
                con.CloseAsync();
            }
            ew.InvoiceItemsDG.ItemsSource = invItemsList;
            ew.SumTB.Text = inv.Sum.ToString();
            ew.Currency.Text = inv.Currency;
            ew.WayPaid.SelectedIndex = inv.IsCashPayment == true ? 0 : 1;
            ew.Resources.Add("invoiceId", inv.Id);
            ew.UpdateButton.Visibility = Visibility.Hidden;
            ew.Show();
            // otvarqne na redaktor na invoice
        }

        private void TasksDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var cl = TasksDG.SelectedItem as AccountTask;
            try
            {
                Client cli = null;
                using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
                {
                    con.Open();
                    var cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = "SELECT * FROM Client WHERE (Id = @id) LIMIT 1";
                    cmd.Parameters.AddWithValue("@id", cl.CId);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            cli = new Client();
                            cli.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                            cli.FirstName = rdr.IsDBNull(1) ? "" : rdr.GetString(1);
                            cli.SecondName = rdr.IsDBNull(2) ? "" : rdr.GetString(2);
                            cli.LastName = rdr.IsDBNull(3) ? "" : rdr.GetString(3);
                            cli.PersonalIdNumber = rdr.IsDBNull(4) ? "" : rdr.GetString(4);
                            cli.TypeID = rdr.IsDBNull(5) ? "" : rdr.GetString(5);
                            if (!rdr.IsDBNull(6)) { cli.BirthDate = rdr.GetDateTime(6); }
                            cli.Sex = rdr.IsDBNull(7) ? "" : rdr.GetString(7);
                            cli.Phone1 = rdr.IsDBNull(8) ? "" : rdr.GetString(8);
                            cli.Phone2 = rdr.IsDBNull(9) ? "" : rdr.GetString(9);
                            cli.Email = rdr.IsDBNull(10) ? "" : rdr.GetString(10);
                            cli.Address = rdr.IsDBNull(11) ? "" : rdr.GetString(11);
                            cli.FirstDate = rdr.GetDateTime(12);
                            cli.LastDateUpdate = rdr.GetDateTime(13);
                            cli.ClinicalDisorders = rdr.IsDBNull(14) ? "" : rdr.GetString(14);
                            cli.Image = rdr.IsDBNull(15) ? "" : rdr.GetString(15);
                            cli.C11 = rdr.IsDBNull(16) ? "" : rdr.GetString(16);
                            cli.C12 = rdr.IsDBNull(17) ? "" : rdr.GetString(17);
                            cli.C13 = rdr.IsDBNull(18) ? "" : rdr.GetString(18);
                            cli.C14 = rdr.IsDBNull(19) ? "" : rdr.GetString(19);
                            cli.C15 = rdr.IsDBNull(20) ? "" : rdr.GetString(20);
                            cli.C16 = rdr.IsDBNull(21) ? "" : rdr.GetString(21);
                            cli.C17 = rdr.IsDBNull(22) ? "" : rdr.GetString(22);
                            cli.C18 = rdr.IsDBNull(23) ? "" : rdr.GetString(23);
                            cli.C21 = rdr.IsDBNull(24) ? "" : rdr.GetString(24);
                            cli.C22 = rdr.IsDBNull(25) ? "" : rdr.GetString(25);
                            cli.C23 = rdr.IsDBNull(26) ? "" : rdr.GetString(26);
                            cli.C24 = rdr.IsDBNull(27) ? "" : rdr.GetString(27);
                            cli.C25 = rdr.IsDBNull(28) ? "" : rdr.GetString(28);
                            cli.C26 = rdr.IsDBNull(29) ? "" : rdr.GetString(29);
                            cli.C27 = rdr.IsDBNull(30) ? "" : rdr.GetString(30);
                            cli.C28 = rdr.IsDBNull(31) ? "" : rdr.GetString(31);
                            cli.C31 = rdr.IsDBNull(32) ? "" : rdr.GetString(32);
                            cli.C32 = rdr.IsDBNull(33) ? "" : rdr.GetString(33);
                            cli.C33 = rdr.IsDBNull(34) ? "" : rdr.GetString(34);
                            cli.C34 = rdr.IsDBNull(35) ? "" : rdr.GetString(35);
                            cli.C35 = rdr.IsDBNull(36) ? "" : rdr.GetString(36);
                            cli.C36 = rdr.IsDBNull(37) ? "" : rdr.GetString(37);
                            cli.C37 = rdr.IsDBNull(38) ? "" : rdr.GetString(38);
                            cli.C38 = rdr.IsDBNull(39) ? "" : rdr.GetString(39);
                            cli.C41 = rdr.IsDBNull(40) ? "" : rdr.GetString(40);
                            cli.C42 = rdr.IsDBNull(41) ? "" : rdr.GetString(41);
                            cli.C43 = rdr.IsDBNull(42) ? "" : rdr.GetString(42);
                            cli.C44 = rdr.IsDBNull(43) ? "" : rdr.GetString(43);
                            cli.C45 = rdr.IsDBNull(44) ? "" : rdr.GetString(44);
                            cli.C46 = rdr.IsDBNull(45) ? "" : rdr.GetString(45);
                            cli.C47 = rdr.IsDBNull(46) ? "" : rdr.GetString(46);
                            cli.C48 = rdr.IsDBNull(47) ? "" : rdr.GetString(47);
                            cli.C51 = rdr.IsDBNull(48) ? false : rdr.GetBoolean(48);
                            cli.C52 = rdr.IsDBNull(49) ? false : rdr.GetBoolean(49);
                            cli.C53 = rdr.IsDBNull(50) ? false : rdr.GetBoolean(50);
                            cli.C54 = rdr.IsDBNull(51) ? false : rdr.GetBoolean(51);
                            cli.C55 = rdr.IsDBNull(52) ? false : rdr.GetBoolean(52);
                            cli.C61 = rdr.IsDBNull(53) ? false : rdr.GetBoolean(53);
                            cli.C62 = rdr.IsDBNull(54) ? false : rdr.GetBoolean(54);
                            cli.C63 = rdr.IsDBNull(55) ? false : rdr.GetBoolean(55);
                            cli.C64 = rdr.IsDBNull(56) ? false : rdr.GetBoolean(56);
                            cli.C65 = rdr.IsDBNull(57) ? false : rdr.GetBoolean(57);
                            cli.C71 = rdr.IsDBNull(58) ? false : rdr.GetBoolean(58);
                            cli.C72 = rdr.IsDBNull(59) ? false : rdr.GetBoolean(59);
                            cli.C73 = rdr.IsDBNull(60) ? false : rdr.GetBoolean(60);
                            cli.C74 = rdr.IsDBNull(61) ? false : rdr.GetBoolean(61);
                            cli.C75 = rdr.IsDBNull(62) ? false : rdr.GetBoolean(62);
                            cli.C81 = rdr.IsDBNull(63) ? false : rdr.GetBoolean(63);
                            cli.C82 = rdr.IsDBNull(64) ? false : rdr.GetBoolean(64);
                            cli.C83 = rdr.IsDBNull(65) ? false : rdr.GetBoolean(65);
                            cli.C84 = rdr.IsDBNull(66) ? false : rdr.GetBoolean(66);
                            cli.C85 = rdr.IsDBNull(67) ? false : rdr.GetBoolean(67);
                            cli.Notes = rdr.IsDBNull(68) ? "" : rdr.GetString(68);
                            cli.IsDeleted = rdr.IsDBNull(69) ? false : rdr.GetBoolean(69);
                        }
                    }
                    con.CloseAsync();
                }
                if (cli == null)
                {
                    return;
                }
                var npw = new New_Patient();
                npw.DataContext = cli;
                npw.ToothPictureSP.Visibility = Visibility.Visible;
                npw.Show();
            }
            catch (Exception)
            {
            }
        }

        private void IsDeletedCB_Checked(object sender, RoutedEventArgs e)
        {
            UpdateTaskDG();
        }
    }
}
