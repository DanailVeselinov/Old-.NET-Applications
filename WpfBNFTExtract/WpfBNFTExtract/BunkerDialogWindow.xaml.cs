using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfBNFTExtract
{
    /// <summary>
    /// Interaction logic for BunkerDialogWindow.xaml
    /// </summary>
    public partial class BunkerDialogWindow : Window
    {
        public BunkerDialogWindow()
        {
            this.Resources.Add("RGBConverter", new RGBConverter());
            this.Resources.Add("RGBConverterForeground", new RGBConverterForeground());
            InitializeComponent();
        }

        private void Button_Click_OK(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = true;
                this.Close();
                Mouse.OverrideCursor = Cursors.Wait;                
            }
            catch (Exception)
            {
                
                this.Close();
                return;
            }
            
        }

        private void UsersGrid_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {                
                case Key.Return:
                    Button_Click_OK(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            } 
        }
    }
}
