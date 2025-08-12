using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChiefPortDocuments
{
    /// <summary>
    /// Interaction logic for FolderSelect.xaml
    /// </summary>
    public partial class FolderSelect : Window
    {
        public FolderSelect()
        {
            InitializeComponent();
            var fll = System.IO.Directory.GetDirectories(System.IO.Directory.GetCurrentDirectory() + "\\TemplatedDocuments", "*", System.IO.SearchOption.TopDirectoryOnly);
            List<string> fllIs = new List<string>();
            foreach (string f in fll)
            {
                int lastInd = f.LastIndexOf("\\");
                fllIs.Add(f.Substring(lastInd+1));
            }
            fllIs.Add("New Folder");
            foldersLV.ItemsSource = fllIs; 
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var si = foldersLV.SelectedItem;
            if (si == null & SelectPanel.Visibility == System.Windows.Visibility.Visible)
            {
                SelectPanel.Visibility = System.Windows.Visibility.Collapsed;
                NamePanel.Visibility = System.Windows.Visibility.Visible;
                fileNameTB.Focus();
                fileNameTB.SelectAll();
                return;
            }
            if (SelectPanel.Visibility == System.Windows.Visibility.Visible)
            {
                if (foldersLV.SelectedItem == "New Folder")
                {
                    SelectPanel.Visibility = System.Windows.Visibility.Collapsed;
                    NamePanel.Visibility = System.Windows.Visibility.Visible;
                    fileNameTB.Focus();
                    fileNameTB.SelectAll();
                    return;
                }
                else
                {
                    (this.Owner as MainWindow).Resources["Folder"] = foldersLV.SelectedItem + "\\";
                    DialogResult = true;
                    this.Close();
                }
            }
            else
            {
                (this.Owner as MainWindow).Resources["Folder"] = fileNameTB.Text + "\\";
                DialogResult = true;
                this.Close();
                //save Path to Main window var;
            }
            
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            } 
        }

    }
}
