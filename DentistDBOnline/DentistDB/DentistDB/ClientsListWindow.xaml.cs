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
            ClientsLVs.DataContext = context.Where(p => p.FirstName.ToUpper().Contains(SearchTB.Text.ToUpper()) | p.SecondName.ToUpper().Contains(SearchTB.Text.ToUpper()) | p.LastName.ToUpper().Contains(SearchTB.Text.ToUpper())).ToList(); //smeni 
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
                using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
                {
                    con.Open();
                    var cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = "SELECT * FROM Client WHERE(Id=@id) LIMIT 1";
                    cmd.Parameters.AddWithValue("@id", currentClient.Id);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        rdr.Read();
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
                //var context = new DentistDatabaseEntities1();
                //DataContext = context.Clients.ToList();
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
                        using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
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
