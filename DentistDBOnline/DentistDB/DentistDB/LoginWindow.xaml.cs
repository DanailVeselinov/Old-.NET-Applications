using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DentistDB
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }


        private void LoginTB_GotFocus(object sender, RoutedEventArgs e)
        {
            DoctorsLV.Visibility = Visibility.Visible;
        }

        private void DoctorsLV_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LoginTB.Text = (DoctorsLV.SelectedItem as Doctor).Name;
            DoctorsLV.Visibility = Visibility.Collapsed;
            ChangePasswordBtn.Visibility = Visibility.Visible;
            PasswordTB.Focus();
        }

        private void DoctorsLV_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DoctorsLV.SelectedItems.Count > 0)
                {
                    LoginTB.Text = (DoctorsLV.SelectedItem as Doctor).Name;
                    DoctorsLV.Visibility = Visibility.Collapsed;
                    ChangePasswordBtn.Visibility = Visibility.Visible;
                    PasswordTB.Focus();
                }
            }
        }

        private void DoctorsLV_LostFocus(object sender, RoutedEventArgs e)
        {
            DoctorsLV.Visibility = Visibility.Collapsed;
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            var doctor = MainWindow.GetDoctorByName(LoginTB.Text);
            if (doctor != null)
            {
                if (doctor.Id > 0)
                {
                    if (doctor.Password == PasswordTB.Password)
                    {
                        this.DialogResult = true;
                        App.Current.MainWindow.DataContext = doctor;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Грешна парола, опитайте отново!");
                    }
                }
            }
            else
            {
                doctor = new Doctor();
                if (!string.IsNullOrWhiteSpace(LoginTB.Text) & !string.IsNullOrWhiteSpace(PasswordTB.Password))
                {
                    using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
                    {
                        con.Open();
                        var cmd = new MySqlCommand();
                        doctor.Name = LoginTB.Text;
                        doctor.Password = PasswordTB.Password;
                        doctor.IsDeleted = false;
                        cmd.Connection = con;
                        cmd.CommandText = "INSERT INTO Doctors ( Name ,Password) VALUES(@name, @pass)";
                        cmd.Parameters.AddWithValue("@name", doctor.Name);
                        cmd.Parameters.AddWithValue("@pass", doctor.Password);
                        cmd.ExecuteNonQuery();
                        doctor.Id = (int)cmd.LastInsertedId;
                    }
                    this.DialogResult = true;
                    (this.Owner as MainWindow).DataContext = doctor;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Въведете валидни данни за име и парола и опитайте отново!");
                }
            }

        }
        private void ChangePasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow changePassWin = new LoginWindow();

            changePassWin.LoginTB.Text = LoginTB.Text;
            changePassWin.LoginTB.IsReadOnly = true;
            changePassWin.LoginBtn.Visibility = Visibility.Hidden;
            changePassWin.ChangePassBtn.Visibility = Visibility.Visible;
            changePassWin.NewPassSP.Visibility = Visibility.Visible;
            var res = changePassWin.ShowDialog();
            if (res.HasValue)
            {
                if (res.Value)
                {
                    PasswordTB.Password = changePassWin.NewPasswordTB.Password;
                    LoginBtn_Click(this.LoginBtn, new RoutedEventArgs());

                }
            }
        }

        private void ChangePassBtn_Click(object sender, RoutedEventArgs e)
        {
            var doctor = MainWindow.GetDoctorByName(LoginTB.Text);
            if (doctor != null)
            {
                if (doctor.Id > 0)
                {
                    if (PasswordTB.Password == doctor.Password)
                    {
                        if (!string.IsNullOrWhiteSpace(NewPasswordTB.Password))
                        {
                            //запази паролата
                            using (var con = new MySqlConnection("user id=u158550680_EddyNew;password=Eddy8479@?;host=srv711.hstgr.io;database=u158550680_DentistDBNew;persist security info=True"))
                            {
                                con.Open();
                                var cmd = new MySqlCommand();
                                cmd.Connection = con;
                                cmd.CommandText = "UPDATE Doctors SET Password = @pass WHERE Id = @id";
                                cmd.Parameters.AddWithValue("@id", doctor.Id);
                                cmd.Parameters.AddWithValue("@pass", NewPasswordTB.Password);
                                cmd.ExecuteNonQuery();
                            }
                            this.DialogResult = true;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Новата парола е неправилно въведена. Опитайте отново!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Текущата парола е неправилно въведена. Опитайте отново!");

                    }
                }
                else
                {
                    MessageBox.Show("Докторът не е валиден опитайте отново!");
                }
            }
            else
            {
                MessageBox.Show("Докторът не е валиден опитайте отново!");
            }

        }

        private void DoctorsLV_Loaded(object sender, RoutedEventArgs e)
        {
            DoctorsLV.DataContext = DataContext as List<Doctor>;
        }

        private void LoginTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext != null)
            {
                var context = DataContext as List<Doctor>;
                DoctorsLV.DataContext = context.Where(p => p.Name.ToUpper().Contains(LoginTB.Text.ToUpper())).ToList(); //smeni 
                this.Focus();
            }
        }

        private void PasswordTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginBtn_Click(LoginBtn, new RoutedEventArgs());
            }
        }
    }
}
