using MySql.Data.MySqlClient;
using System.Windows;
using System.Windows.Input;

namespace DentistDB
{
    /// <summary>
    /// Interaction logic for ExpensesWindow.xaml
    /// </summary>
    public partial class ExpensesWindow : Window
    {
        public ExpensesWindow()
        {
            InitializeComponent();
        }

        private void NewExpence_Click(object sender, RoutedEventArgs e)
        {

            var ew = new Add_ExpensesWindow();
            ew.Owner = this;
            ew.Show();

        }


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
                Supplier supplier = MainWindow.GetSupplierById((int)inv.SupplierId);
                if (supplier != null)
                {
                    ew.AddressTB.Text = supplier.Address;
                    ew.CompanyTB.Text = supplier.Name;
                    ew.BulstatTB.Text = supplier.BulStat;

                }
            }
            ew.RecieptTB.Text = inv.InvoiceNumber;
            ew.DatePaidTB.SelectedDate = inv.DateOfPayment;
            ew.InvoiceItemsDG.ItemsSource = MainWindow.GetInvoiceItemsByInvoiceId(inv.Id);
            ew.SumTB.Text = inv.Sum.ToString();
            ew.Currency.Text = inv.Currency;
            ew.WayPaid.SelectedIndex = inv.IsCashPayment == true ? 0 : 1;
            ew.Resources.Add("invoiceId", inv.Id);
            ew.UpdateButton.Content = "Обнови";
            ew.Owner = this;
            ew.Show();
            InvoicesDG.ItemsSource = MainWindow.GetAllInvoices();
            // otvarqne na redaktor na invoice
        }


        private void InvoicesDG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                MessageBoxResult sd = MessageBox.Show("Изтриване на Фактура?", "Изтриване", MessageBoxButton.OKCancel);
                if (sd == MessageBoxResult.OK)
                {
                    Invoice inv = InvoicesDG.SelectedItem as Invoice;
                    if (inv == null)
                    {
                        return;
                    }
                    using (var con = new MySqlConnection("server=45.84.206.101;user=u158550680_Eddy;persistsecurityinfo=True;database=u158550680_dentist_db;password=Eddy8479;allowuservariables=True"))
                    {
                        con.Open();
                        var cmd = new MySqlCommand();
                        cmd.Connection = con;
                        cmd.CommandText = "UPDATE Invoice SET IsDeleted=1 WHERE(Id = @id)";
                        cmd.Parameters.AddWithValue("@id", inv.Id);
                        cmd.ExecuteNonQuery();
                        con.CloseAsync();
                    }
                    e.Handled = true;
                }
                else
                {
                    e.Handled = true;
                }

            }

        }

        private void FromDP_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            InvoicesDG.ItemsSource = MainWindow.GetInvoicesFiltered(FromDP.SelectedDate, ToDP.SelectedDate);
        }
    }
}
