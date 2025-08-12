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
            using (var con = new MySqlConnection("server=45.84.206.101;user=u158550680_Eddy;persistsecurityinfo=True;database=u158550680_dentist_db;password=Eddy8479;allowuservariables=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = String.Format("SELECT a.Id, a.SupplierId, a.UserId, a.IsCashPayment, a.InvoiceNumber, a.DateOfPayment, a.Sum, a.Currency, a.IsDeleted, s.Name FROM Invoice a JOIN Suppliers s ON a.SupplierId = s.Id WHERE (CAST(DateOfPayment AS DATETIME) BETWEEN CAST('{0}' AS DATETIME) AND CAST('{1}' AS DATETIME)) AND (IsDeleted = 0)", from.ToString("yyyy-MM-dd HH:mm:ss"), to.ToString("yyyy-MM-dd HH:mm:ss"));
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var inv = new Invoice();

                        if (!rdr.IsDBNull(0)) { inv.Id = rdr.GetInt32(0); }
                        if (!rdr.IsDBNull(1)) { inv.SupplierId = rdr.GetInt32(1); }
                        if (!rdr.IsDBNull(2)) { inv.UserId = rdr.GetInt32(2); }
                        if (!rdr.IsDBNull(3)) { inv.IsCashPayment = rdr.GetBoolean(3); }
                        if (!rdr.IsDBNull(4)) { inv.InvoiceNumber = rdr.GetString(4); }
                        if (!rdr.IsDBNull(5)) { inv.DateOfPayment = rdr.GetDateTime(5); }
                        if (!rdr.IsDBNull(6)) { inv.Sum = rdr.GetDecimal(6); }
                        if (!rdr.IsDBNull(7)) { inv.Currency = rdr.GetString(7); }
                        if (!rdr.IsDBNull(8)) { inv.IsDeleted = rdr.GetBoolean(8); }
                        if (!rdr.IsDBNull(9)) { inv.SupplierName = rdr.GetString(9); }

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
            using (var con = new MySqlConnection("server=45.84.206.101;user=u158550680_Eddy;persistsecurityinfo=True;database=u158550680_dentist_db;password=Eddy8479;allowuservariables=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                if (doc != null)
                {
                    if (doc.Id > 0)
                    {
                        cmd.CommandText = String.Format("SELECT a.Id, a.ClientId, a.ToothCode, a.Date, a.Status, a.Diagnose, a.Price, a.PriceNOI, a.PriceDT, b.FirstName, b.SecondName, b.LastName , c.Name, a.IsDeleted, a.PaidByCard, a.PaidCash FROM Task a JOIN Client b ON a.ClientId = b.Id JOIN Doctors c ON a.DoctorId = c.Id WHERE (CAST(a.Date AS DATETIME) BETWEEN CAST('{0}' AS DATETIME) AND CAST('{1}' AS DATETIME)) {2} AND (a.DoctorId =@did)", from.ToString("yyyy-MM-dd HH:mm:ss"), to.ToString("yyyy-MM-dd HH:mm:ss"), !isDeleted ? "AND (a.IsDeleted = 0)" : "");
                        cmd.Parameters.AddWithValue("@did", doc.Id);
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                var task = new AccountTask();
                                if (!rdr.IsDBNull(0)) { task.Id = rdr.GetInt32(0); }
                                if (!rdr.IsDBNull(1)) { task.CId = rdr.GetInt32(1); }
                                if (!rdr.IsDBNull(2)) { task.ToothCode = rdr.GetByte(2); }
                                task.Date = rdr.IsDBNull(3) ? DateTime.Now : rdr.GetDateTime(3);
                                if (!rdr.IsDBNull(4)) { task.Status = rdr.GetString(4); }
                                if (!rdr.IsDBNull(5)) { task.Diagnose = rdr.GetString(5); }
                                if (!rdr.IsDBNull(6)) { task.Price = rdr.GetDouble(6); }
                                if (!rdr.IsDBNull(12)) { task.DoctorId = rdr.GetString(12); }
                                if (!rdr.IsDBNull(7)) { task.PriceNOI = rdr.GetDouble(7); }
                                if (!rdr.IsDBNull(8)) { task.PriceDT = rdr.GetDouble(8); }
                                task.ClientId = (rdr.IsDBNull(9) ? "" : rdr.GetString(9)) + " " + (rdr.IsDBNull(10) ? "" : rdr.GetString(10)) + " " + (rdr.IsDBNull(11) ? "" : rdr.GetString(11)) + " ";
                                if (!rdr.IsDBNull(13)) { task.IsDeleted = rdr.GetBoolean(13); }
                                if (!rdr.IsDBNull(12)) { task.PaidByCard = rdr.GetBoolean(14); }
                                if (!rdr.IsDBNull(13)) { task.PaidCash = rdr.GetBoolean(15); }
                                if (task.PaidByCard | task.PaidCash) { task.IsPaid = true; }

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
                        if (!rdr.IsDBNull(0)) { task.Id = rdr.GetInt32(0); }
                        if (!rdr.IsDBNull(1)) { task.CId = rdr.GetInt32(1); }
                        if (!rdr.IsDBNull(2)) { task.ToothCode = rdr.GetByte(2); }
                        task.Date = rdr.IsDBNull(3) ? DateTime.Now : rdr.GetDateTime(3);
                        if (!rdr.IsDBNull(4)) { task.Status = rdr.GetString(4); }
                        if (!rdr.IsDBNull(5)) { task.Diagnose = rdr.GetString(5); }
                        if (!rdr.IsDBNull(6)) { task.Price = rdr.GetDouble(6); }
                        if (!rdr.IsDBNull(12)) { task.PaidByCard = rdr.GetBoolean(12); }
                        if (!rdr.IsDBNull(13)) { task.PaidCash = rdr.GetBoolean(13); }
                        if (!rdr.IsDBNull(7)) { task.PriceNOI = rdr.GetDouble(7); }
                        if (!rdr.IsDBNull(8)) { task.PriceDT = rdr.GetDouble(8); }
                        task.ClientId = (rdr.IsDBNull(9) ? "" : rdr.GetString(9)) + " " + (rdr.IsDBNull(10) ? "" : rdr.GetString(10)) + " " + (rdr.IsDBNull(11) ? "" : rdr.GetString(11)) + " ";

                        if (!rdr.IsDBNull(14)) { task.DoctorId = rdr.GetString(14); }
                        if (!rdr.IsDBNull(15)) { task.IsDeleted = rdr.GetBoolean(15); }
                        if (task.PaidByCard | task.PaidCash) { task.IsPaid = true; }
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
        //    using (var con = new MySqlConnection("server=45.84.206.101;user=u158550680_Eddy;persistsecurityinfo=True;database=u158550680_dentist_db;password=Eddy8479;allowuservariables=True"))
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
                using (var con = new MySqlConnection("server=45.84.206.101;user=u158550680_Eddy;persistsecurityinfo=True;database=u158550680_dentist_db;password=Eddy8479;allowuservariables=True"))
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
                            if (!rdr.IsDBNull(0)){ supplier.Id = rdr.GetInt32(0); }
                            if (!rdr.IsDBNull(1)){ supplier.Name = rdr.GetString(1); }
                            if (!rdr.IsDBNull(2)){ supplier.Address = rdr.GetString(2); }
                            if (!rdr.IsDBNull(3)){ supplier.BulStat = rdr.GetString(3); }
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
            using (var con = new MySqlConnection("server=45.84.206.101;user=u158550680_Eddy;persistsecurityinfo=True;database=u158550680_dentist_db;password=Eddy8479;allowuservariables=True"))
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
                        if (!rdr.IsDBNull(0)) { ii.Id = rdr.GetInt32(0); }
                        if (!rdr.IsDBNull(1)) { ii.InvoiceId = rdr.GetInt32(1); }
                        if (!rdr.IsDBNull(2)) { ii.ItemName = rdr.GetString(2); }
                        if (!rdr.IsDBNull(3)) { ii.ValueOfPayment = rdr.GetDecimal(3); }
                        if (!rdr.IsDBNull(4)) { ii.CurrencyOfPayment = rdr.GetString(4); }
                        
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
                using (var con = new MySqlConnection("server=45.84.206.101;user=u158550680_Eddy;persistsecurityinfo=True;database=u158550680_dentist_db;password=Eddy8479;allowuservariables=True"))
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
                            if (!rdr.IsDBNull(0)) { cli.Id = rdr.GetInt32(0); }
                            if (!rdr.IsDBNull(1)) { cli.FirstName = rdr.GetString(1); }
                            if (!rdr.IsDBNull(2)) { cli.SecondName = rdr.GetString(2); }
                            if (!rdr.IsDBNull(3)) { cli.LastName = rdr.GetString(3); }
                            if (!rdr.IsDBNull(4)) { cli.PersonalIdNumber = rdr.GetString(4); }
                            if (!rdr.IsDBNull(5)) { cli.TypeID = rdr.GetString(5); }
                            if (!rdr.IsDBNull(6)) { cli.BirthDate = rdr.GetDateTime(6); }
                            if (!rdr.IsDBNull(7)) { cli.Sex = rdr.GetString(7); }
                            if (!rdr.IsDBNull(8)) { cli.Phone1 = rdr.GetString(8); }
                            if (!rdr.IsDBNull(9)) { cli.Phone2 = rdr.GetString(9); }
                            if (!rdr.IsDBNull(10)) { cli.Email = rdr.GetString(10); }
                            if (!rdr.IsDBNull(11)) { cli.Address = rdr.GetString(11); }
                            if (!rdr.IsDBNull(12)) { cli.FirstDate = rdr.GetDateTime(12); }
                            if (!rdr.IsDBNull(13)) { cli.LastDateUpdate = rdr.GetDateTime(13); }
                            if (!rdr.IsDBNull(14)) { cli.ClinicalDisorders = rdr.GetString(14); }
                            if (!rdr.IsDBNull(15)) { cli.Image = rdr.GetString(15); }
                            if (!rdr.IsDBNull(16)) { cli.C11 = rdr.GetString(16); }
                            if (!rdr.IsDBNull(17)) { cli.C12 = rdr.GetString(17); }
                            if (!rdr.IsDBNull(18)) { cli.C13 = rdr.GetString(18); }
                            if (!rdr.IsDBNull(19)) { cli.C14 = rdr.GetString(19); }
                            if (!rdr.IsDBNull(20)) { cli.C15 = rdr.GetString(20); }
                            if (!rdr.IsDBNull(21)) { cli.C16 = rdr.GetString(21); }
                            if (!rdr.IsDBNull(22)) { cli.C17 = rdr.GetString(22); }
                            if (!rdr.IsDBNull(23)) { cli.C18 = rdr.GetString(23); }
                            if (!rdr.IsDBNull(24)) { cli.C21 = rdr.GetString(24); }
                            if (!rdr.IsDBNull(25)) { cli.C22 = rdr.GetString(25); }
                            if (!rdr.IsDBNull(26)) { cli.C23 = rdr.GetString(26); }
                            if (!rdr.IsDBNull(27)) { cli.C24 = rdr.GetString(27); }
                            if (!rdr.IsDBNull(28)) { cli.C25 = rdr.GetString(28); }
                            if (!rdr.IsDBNull(29)) { cli.C26 = rdr.GetString(29); }
                            if (!rdr.IsDBNull(30)) { cli.C27 = rdr.GetString(30); }
                            if (!rdr.IsDBNull(31)) { cli.C28 = rdr.GetString(31); }
                            if (!rdr.IsDBNull(32)) { cli.C31 = rdr.GetString(32); }                        
                            if (!rdr.IsDBNull(33)) { cli.C32 = rdr.GetString(33); }
                            if (!rdr.IsDBNull(34)) { cli.C33 = rdr.GetString(34); }
                            if (!rdr.IsDBNull(35)) { cli.C34 = rdr.GetString(35); }
                            if (!rdr.IsDBNull(36)) { cli.C35 = rdr.GetString(36); }
                            if (!rdr.IsDBNull(37)) { cli.C36 = rdr.GetString(37); }
                            if (!rdr.IsDBNull(38)) { cli.C37 = rdr.GetString(38); }
                            if (!rdr.IsDBNull(39)) { cli.C38 = rdr.GetString(39); }
                            if (!rdr.IsDBNull(40)) { cli.C41 = rdr.GetString(40); }
                            if (!rdr.IsDBNull(41)) { cli.C42 = rdr.GetString(41); }
                            if (!rdr.IsDBNull(42)) { cli.C43 = rdr.GetString(42); }
                            if (!rdr.IsDBNull(43)) { cli.C44 = rdr.GetString(43); }
                            if (!rdr.IsDBNull(44)) { cli.C45 = rdr.GetString(44); }
                            if (!rdr.IsDBNull(45)) { cli.C46 = rdr.GetString(45); }
                            if (!rdr.IsDBNull(46)) { cli.C47 = rdr.GetString(46); }
                            if (!rdr.IsDBNull(47)) { cli.C48 = rdr.GetString(47); }
                            if (!rdr.IsDBNull(48)) { cli.C51 = rdr.GetBoolean(48); }
                            if (!rdr.IsDBNull(49)) { cli.C52 = rdr.GetBoolean(49); }
                            if (!rdr.IsDBNull(50)) { cli.C53 = rdr.GetBoolean(50); }
                            if (!rdr.IsDBNull(51)) { cli.C54 = rdr.GetBoolean(51); }
                            if (!rdr.IsDBNull(52)) { cli.C55 = rdr.GetBoolean(52); }
                            if (!rdr.IsDBNull(53)) { cli.C61 = rdr.GetBoolean(53); }
                            if (!rdr.IsDBNull(54)) { cli.C62 = rdr.GetBoolean(54); }
                            if (!rdr.IsDBNull(55)) { cli.C63 = rdr.GetBoolean(55); }
                            if (!rdr.IsDBNull(56)) { cli.C64 = rdr.GetBoolean(56); }
                            if (!rdr.IsDBNull(57)) { cli.C65 = rdr.GetBoolean(57); }
                            if (!rdr.IsDBNull(58)) { cli.C71 = rdr.GetBoolean(58); }
                            if (!rdr.IsDBNull(59)) { cli.C72 = rdr.GetBoolean(59); }
                            if (!rdr.IsDBNull(60)) { cli.C73 = rdr.GetBoolean(60); }
                            if (!rdr.IsDBNull(61)) { cli.C74 = rdr.GetBoolean(61); }
                            if (!rdr.IsDBNull(62)) { cli.C75 = rdr.GetBoolean(62); }
                            if (!rdr.IsDBNull(63)) { cli.C81 = rdr.GetBoolean(63); }
                            if (!rdr.IsDBNull(64)) { cli.C82 = rdr.GetBoolean(64); }
                            if (!rdr.IsDBNull(65)) { cli.C83 = rdr.GetBoolean(65); }
                            if (!rdr.IsDBNull(66)) { cli.C84 = rdr.GetBoolean(66); }
                            if (!rdr.IsDBNull(67)) { cli.C85 = rdr.GetBoolean(67); }
                            if (!rdr.IsDBNull(68)) { cli.Notes = rdr.GetString(68); }
                            if (!rdr.IsDBNull(69)) { cli.IsDeleted = rdr.GetBoolean(69); }
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateTaskDG();
        }
    }
}
