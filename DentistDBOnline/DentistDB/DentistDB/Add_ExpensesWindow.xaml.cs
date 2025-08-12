using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DentistDB
{
    /// <summary>
    /// Interaction logic for Add_ExpensesWindow.xaml
    /// </summary>
    public partial class Add_ExpensesWindow : Window
    {
        public Add_ExpensesWindow()
        {
            InitializeComponent();
            InvoiceItemsDG.ItemsSource = new List<InvoiceItem>();
            List<Supplier> suppliersList = new List<Supplier>();
            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
            {
                con.Open();
                var cmd = new MySqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "SELECT Id, Name, Address, BulStat FROM Suppliers";

                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var supplier = new Supplier();
                        supplier.Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                        supplier.Name = rdr.IsDBNull(1) ? "" : rdr.GetString(1);
                        supplier.Address = rdr.IsDBNull(2) ? "" : rdr.GetString(2);
                        supplier.BulStat = rdr.IsDBNull(3) ? "" : rdr.GetString(3);
                        suppliersList.Add(supplier);
                    }
                }
                con.CloseAsync();
            }
            Resources.Add("companies", suppliersList);
        }

        private void CompanyTB_KeyUp(object sender, KeyEventArgs e)
        {

            var supp = Resources["companies"] as List<Supplier>;
            string text = CompanyTB.Text;
            List<Supplier> suppliers = new List<Supplier>();
            if (supp != null)
            {
                foreach (Supplier item in supp)
                {
                    if (item.Name.ToUpper().Contains(text.ToUpper()))
                    {
                        suppliers.Add(item);
                    }
                }
            }
            if (suppliers.Count > 0)
            {
                CompanyLV.ItemsSource = suppliers;
                CompanyLV.Visibility = Visibility.Visible;
            }
            else
            {
                CompanyLV.Visibility = Visibility.Collapsed;
            }
        }

        private void CompanyTB_LostFocus(object sender, RoutedEventArgs e)
        {
            CompanyLV.Visibility = Visibility.Collapsed;
        }

        private void CompanyTB_GotFocus(object sender, RoutedEventArgs e)
        {
            var supp = Resources["companies"] as List<Supplier>;
            string text = CompanyTB.Text;
            List<Supplier> suppliers = new List<Supplier>();
            if (supp != null)
            {
                foreach (Supplier item in supp)
                {
                    if (item.Name.ToUpper().Contains(text))
                    {
                        suppliers.Add(item);
                    }
                }
            }

            if (suppliers.Count > 0)
            {
                CompanyLV.ItemsSource = suppliers;
                CompanyLV.Visibility = Visibility.Visible;
            }
            else
            {
                CompanyLV.Visibility = Visibility.Collapsed;
            }
        }



        private void InvoiceItemsDG_LostFocus(object sender, RoutedEventArgs e)
        {

            decimal sum = 0;
            string curr = "лв.";

            foreach (var it in InvoiceItemsDG.Items)
            {
                InvoiceItem item = new InvoiceItem();
                try
                {
                    item = it as InvoiceItem;
                }
                catch (Exception)
                {
                    continue;
                }
                if (item == null)
                {
                    continue;
                }
                sum += item.ValueOfPayment.HasValue ? item.ValueOfPayment.Value : 0;
                if (!string.IsNullOrEmpty(item.CurrencyOfPayment)) { curr = item.CurrencyOfPayment; }
            }
            SumTB.Text = sum.ToString();
            Currency.Text = curr;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var i = Resources["invoiceId"];
                int? ii;
                if (i == null)
                {
                    ii = null;
                }
                else
                {
                    ii = (int)i;
                }
                string companyName = CompanyTB.Text.Trim();
                int suppId = 0;
                //
                using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
                {
                    con.Open();
                    var cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = "SELECT Id FROM Suppliers WHERE(Name = @name) LIMIT 1";
                    cmd.Parameters.AddWithValue("@name", companyName);
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        suppId = Convert.ToInt32(result);
                    }
                    else
                    {
                        //add supplier
                        cmd.CommandText = "INSERT INTO Suppliers (Name, Address, BulStat) VALUES (?name, ?address, ?bulstat)";
                        cmd.Parameters.AddWithValue("?name", companyName);
                        cmd.Parameters.AddWithValue("?address", AddressTB.Text.Trim());
                        cmd.Parameters.AddWithValue("?bulstat", BulstatTB.Text.Trim());
                        cmd.ExecuteNonQuery();
                        suppId = (int)cmd.LastInsertedId;
                    }

                    Invoice invoice = new Invoice();
                    invoice.InvoiceNumber = RecieptTB.Text;
                    invoice.IsCashPayment = WayPaid.SelectedIndex == 0 ? true : false;
                    invoice.SupplierId = suppId;
                    invoice.DateOfPayment = DatePaidTB.SelectedDate.HasValue ? DatePaidTB.SelectedDate.Value : DateTime.Now;
                    if ((App.Current.MainWindow as MainWindow).LoggedInDoctor != null)
                    {
                        invoice.UserId = (App.Current.MainWindow as MainWindow).LoggedInDoctor.Id;
                    }
                    try
                    {
                        invoice.Sum = decimal.Parse(SumTB.Text);
                    }
                    catch (Exception)
                    {
                        invoice.Sum = 0;
                    }
                    invoice.Currency = Currency.Text;
                    if (ii != null & ii > 0)
                    {
                        invoice.Id = ii.Value;
                        cmd.CommandText = "UPDATE Invoice SET SupplierId =@supId, UserId =@uId, IsCashPayment =@isCash, InvoiceNumber=@invNum, DateOfPayment=@datePay, Sum=@sum, Currency=@curr WHERE Id = @id";
                        cmd.Parameters.AddWithValue("@uId", invoice.UserId);
                        cmd.Parameters.AddWithValue("@supId", invoice.SupplierId);
                        cmd.Parameters.AddWithValue("@isCash", invoice.IsCashPayment);
                        cmd.Parameters.AddWithValue("@invNum", invoice.InvoiceNumber);
                        cmd.Parameters.AddWithValue("@datePay", invoice.DateOfPayment);
                        cmd.Parameters.AddWithValue("@sum", invoice.Sum);
                        cmd.Parameters.AddWithValue("@curr", invoice.Currency);
                        cmd.Parameters.AddWithValue("@id", ii.Value);
                        cmd.ExecuteNonQuery();
                        //check for database availability
                    }
                    else
                    {
                        //add new incoice
                        cmd.CommandText = "INSERT INTO Invoice (SupplierId , UserId , IsCashPayment, InvoiceNumber, DateOfPayment, Sum, Currency) VALUES(@supId,@uId,@isCash,@invNum,@datePay,@sum,@curr)";
                        cmd.Parameters.AddWithValue("@uId", invoice.UserId);
                        cmd.Parameters.AddWithValue("@isCash", invoice.IsCashPayment);
                        cmd.Parameters.AddWithValue("@invNum", invoice.InvoiceNumber);
                        cmd.Parameters.AddWithValue("@datePay", invoice.DateOfPayment);
                        cmd.Parameters.AddWithValue("@sum", invoice.Sum);
                        cmd.Parameters.AddWithValue("@curr", invoice.Currency);
                        cmd.Parameters.AddWithValue("@supId", invoice.SupplierId);
                        cmd.ExecuteNonQuery();
                        invoice.Id = (int)cmd.LastInsertedId;
                        //check for database availability

                    }
                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO InvoiceItem(InvoiceId,ItemName,ValueOfPayment,CurrencyOfPayment) VALUES");
                    foreach (InvoiceItem item in InvoiceItemsDG.ItemsSource)
                    {
                        if (item.ItemName.Length > 50) { item.ItemName = item.ItemName.Substring(0, 50); }
                        sb.Append(String.Format("({0},'{1}',{2},'{3}'),", invoice.Id, item.ItemName, item.ValueOfPayment, item.CurrencyOfPayment));
                    }
                    cmd.CommandText = sb.ToString().TrimEnd(',');
                    cmd.ExecuteNonQuery();
                    var ew = (this.Owner as ExpensesWindow);
                    //dobavi list sys invoices
                    List<Invoice> InvList = new List<Invoice>();
                    cmd.CommandText = "SELECT * FROM Invoice WHERE (IsDeleted =0)";
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

                    ew.InvoicesDG.ItemsSource = InvList;
                    con.CloseAsync();
                }
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Нещо се обърка опитайте отново!");
            }
            
        }






        private void CompanyLV_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                MessageBoxResult sd = MessageBox.Show("Изтриване на Фирма?", "Изтриване", MessageBoxButton.OKCancel);
                if (sd == MessageBoxResult.OK)
                {
                    var tb = e.Source as TextBlock;
                    using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
                    {
                        con.Open();
                        var cmd = new MySqlCommand();
                        cmd.Connection = con;
                        cmd.CommandText = "DELETE FROM Suppliers WHERE(Id = @id)";
                        cmd.Parameters.AddWithValue("@id", (CompanyLV.SelectedItem as Supplier).Id);
                        cmd.ExecuteNonQuery();
                        con.CloseAsync();
                    }
                    e.Handled = true;

                    List<Supplier> supp = Resources["companies"] as List<Supplier>;
                    string text = CompanyTB.Text;

                    List<Supplier> suppliers = supp.Where(s => s.Name.Contains(text)).ToList();
                    if (supp != null)
                    {
                        if (suppliers.Count > 0)
                        {
                            CompanyLV.ItemsSource = suppliers;
                            CompanyLV.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            CompanyLV.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                else
                {
                    e.Handled = true;
                }
            }
        }

        private void CompanyLV_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedCompany = CompanyLV.SelectedItem as Supplier;
            if (selectedCompany == null)
            {
                AddressTB.Text = "";
                BulstatTB.Text = "";
                CompanyLV.Visibility = Visibility.Collapsed;
                return;
            }
            CompanyTB.Text = selectedCompany.Name;
            AddressTB.Text = selectedCompany.Address;
            BulstatTB.Text = selectedCompany.BulStat;
            CompanyLV.Visibility = Visibility.Collapsed;
        }
    }
}
