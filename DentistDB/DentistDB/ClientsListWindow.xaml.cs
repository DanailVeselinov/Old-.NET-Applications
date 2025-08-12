using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DentistDB
{
    /// <summary>
    /// Interaction logic for ClientsListWindow.xaml
    /// </summary>
    public class ImageFmStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var images = (string)value;
            if (value == null)
            {
                return null;
            }
            return images.Trim(';').Split(';').ToList().First();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class HeightValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string percent = null;
            double perD;
            if (parameter != null)
            {
                percent = (string)parameter;
            }
            if (!double.TryParse(percent, out perD))
            {
                perD = 0.85;
            }

            if (value != null)
            {
                double height = (double)value;
                return height * perD;
            }
            else
            {
                return double.NaN;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public partial class ClientsListWindow : Window
    {
        public ClientsListWindow()
        {
            InitializeComponent();
            SearchTB.Focus();
        }

        public void SearchTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            var context = DataContext as List<ClientListed>;
            ClientsLVs.DataContext = context.Where(p => p.FirstName.ToUpper().Contains(SearchTB.Text.ToUpper()) | p.SecondName.ToUpper().Contains(SearchTB.Text.ToUpper()) | p.LastName.ToUpper().Contains(SearchTB.Text.ToUpper()) | p.PersonalIdNumber.ToUpper().Contains(SearchTB.Text.ToUpper()) | p.Phone1.ToUpper().Contains(SearchTB.Text.ToUpper()) | p.Phone2.ToUpper().Contains(SearchTB.Text.ToUpper()) | p.Email.ToUpper().Contains(SearchTB.Text.ToUpper())).ToList(); 
            this.Focus();
        }

        private void SearchTB_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ClientListed currentClient = ClientsLVs.Items[0] as ClientListed;
                OpenCurrentClient(currentClient);
            }
        }

        public void OpenCurrentClient(ClientListed currentClient)
        {
            try
            {
                Client cli = new Client();
                using (var con = new MySqlConnection("server=45.84.206.101;user=u158550680_Eddy;persistsecurityinfo=True;database=u158550680_dentist_db;password=Eddy8479;allowuservariables=True"))
                {
                    con.Open();
                    var cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = "SELECT * FROM Client WHERE(Id=@id) LIMIT 1";
                    cmd.Parameters.AddWithValue("@id", currentClient.Id);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        rdr.Read();
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
                    con.CloseAsync();
                }
                if (cli == null)
                {
                    return;
                }
                var npw = new New_Patient();
                npw.Owner = this;
                npw.Closed += Npw_Closed;
                npw.DataContext = cli;
                npw.ToothPictureSP.Visibility = Visibility.Visible;
                npw.Show();

                //DataContext = context.Clients.ToList();
                //SearchTB.TextChanged += SearchTB_TextChanged;
            }
            catch (Exception)
            {
            }
        }

        public void Npw_Closed(object sender, EventArgs e)
        {
            try
            {
                SearchTB_TextChanged(this, null);
                SearchTB.TextChanged += SearchTB_TextChanged;
            }
            catch (Exception)
            {
            }

        }

        private void ClientsLVs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var cl = (sender as ListView).SelectedItem as ClientListed;
            OpenCurrentClient(cl);
        }

        private void ClientsLVs_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var cl = (sender as ListView).SelectedItem as ClientListed;
                OpenCurrentClient(cl);
            }
            if (e.Key == Key.Delete)
            {
                try
                {
                    var result = MessageBox.Show("Наистина ли искате да изтриете потребител?", "Confirmation", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        var client = ClientsLVs.SelectedItem as ClientListed;
                        var clientsList = new List<ClientListed>();
                        using (var con = new MySqlConnection("server=45.84.206.101;user=u158550680_Eddy;persistsecurityinfo=True;database=u158550680_dentist_db;password=Eddy8479;allowuservariables=True"))
                        {
                            con.Open();
                            var cmd = new MySqlCommand();
                            cmd.Connection = con;
                            cmd.CommandText = "UPDATE Client SET IsDeleted=1 WHERE (Id = @id)";
                            cmd.Parameters.AddWithValue("@id", client.Id);
                            cmd.ExecuteNonQuery();
                            cmd.CommandText = "SELECT Id, FirstName, SecondName, LastName, PersonalIdNumber, TypeID, Phone1, Phone2, Email, LastDateUpdate FROM Client WHERE(IsDeleted = 0)";
                            using (MySqlDataReader rdr = cmd.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    clientsList.Add(new ClientListed(rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0), rdr.IsDBNull(1) ? "" : rdr.GetString(1), rdr.IsDBNull(2) ? "" : rdr.GetString(2), rdr.IsDBNull(3) ? "" : rdr.GetString(3), rdr.IsDBNull(4) ? "" : rdr.GetString(4), rdr.IsDBNull(5) ? "" : rdr.GetString(5), rdr.IsDBNull(6) ? "" : rdr.GetString(6), rdr.IsDBNull(7) ? "" : rdr.GetString(7), rdr.IsDBNull(8) ? "" : rdr.GetString(8), rdr.IsDBNull(5) ? DateTime.Now : rdr.GetDateTime(9)));
                                }
                            }
                        }

                        DataContext = clientsList;
                        SearchTB.TextChanged += SearchTB_TextChanged;
                        ////update listview
                    }

                }
                catch (Exception dc)
                {
                    MessageBox.Show("Error deleting Client: " +
                        dc.Message);
                }
            }
        }

    }
}
