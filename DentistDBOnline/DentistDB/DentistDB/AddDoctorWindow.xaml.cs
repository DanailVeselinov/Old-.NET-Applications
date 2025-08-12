using System.Windows;

namespace DentistDB
{
    /// <summary>
    /// Interaction logic for AddDoctorWindow.xaml
    /// </summary>
    public partial class AddDoctorWindow : Window
    {
        public AddDoctorWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DoctorNameTB.Text))
            {
                MessageBox.Show("Моля въведете Име на Доктора!");
            }
            else
            {
                if (DoctorNameTB.Text.Length > 100)
                {
                    MessageBox.Show("Моля Името на Доктора да е до 100 символа!");

                }
                (DataContext as Doctor).Name = DoctorNameTB.Text;
                DialogResult = true;
                this.Close();
            }
        }
    }
}
