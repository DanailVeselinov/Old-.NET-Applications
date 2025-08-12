using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DentistDB
{
    /// <summary>
    /// Interaction logic for AddStatusWindow.xaml
    /// </summary>
    public partial class AddStatusWindow : Window
    {
        public AddStatusWindow()
        {
            InitializeComponent();
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.AllowFullOpen = false;
            cd.ShowHelp = true;
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    ColorTB.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(cd.Color.R, cd.Color.G, cd.Color.B));
                    //ColorTB.Background = (Brush)(new BrushConverter().ConvertFrom(cd.Color.Name));
                    //ocveti go
                }
                catch (Exception)
                {

                }

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = true;
            }
            catch (Exception)
            {
            }
        }
    }
}
