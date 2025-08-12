using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for BrowseFolder.xaml
    /// </summary>
    public partial class BrowseFolder : Window
    {
        public BrowseFolder()
        {
            InitializeComponent();
        }

        private void UpLevel_Click(object sender, RoutedEventArgs e)
        {
            int lastDir = PathTB.Text.LastIndexOf('\\');
            if (Directory.Exists(PathTB.Text) & lastDir > 0)
            {
                PathTB.Text = PathTB.Text.Substring(0, lastDir);
            }
            else
            {
                ChildList.ItemsSource = null;
            }
        }

        private void PathTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            var pathBox = (sender as TextBox);
            try
            {
                Directory.GetDirectories(pathBox.Text);
            }
            catch (Exception)
            {
                return;
            }
            List<string> cl = Directory.GetDirectories(pathBox.Text).ToList();
            cl.AddRange(Directory.GetFiles(pathBox.Text, "*.d*", SearchOption.TopDirectoryOnly).ToList());
            cl.AddRange(Directory.GetFiles(pathBox.Text, "*.x*", SearchOption.TopDirectoryOnly).ToList());
            List<string> removeindexes = new List<string>();
            for (int i = 0; i < cl.Count; i++)
            {
                if (System.IO.File.GetAttributes(cl[i]).HasFlag(System.IO.FileAttributes.Hidden))
                {
                    removeindexes.Add(cl[i]);
                    continue;
                }
                if (pathBox.Text.Contains('\\'))
                {
                    if (pathBox.Text[pathBox.Text.Length - 1] != '\\')
                    {
                        cl[i] = cl[i].Replace(pathBox.Text + "\\", "");
                    }
                    else
                    {
                        cl[i] = cl[i].Replace(pathBox.Text, "");
                    }
                }
                else
                {
                    if (pathBox.Text.Contains(':'))
                    {
                        pathBox.Text = pathBox.Text + '\\';
                        return;
                    }
                    cl[i] = cl[i].Replace(pathBox.Text, "");
                }
            }
            foreach (var ri in removeindexes)
            {
                cl.Remove(ri);
            }
            ChildList.ItemsSource = cl;
        }

        private void Select_Click(object sender, MouseButtonEventArgs e)
        {
            var dirList = Directory.GetDirectories(PathTB.Text,"*",SearchOption.TopDirectoryOnly).ToList();
            var dirNew = (PathTB.Text + ("\\" + ChildList.SelectedValue).Replace("\\\\", "\\"));
            if (dirList.Contains(dirNew))
            {
                PathTB.Text = dirNew;                
            }
            else
            {
                Select_Click_Button(this, new RoutedEventArgs());
            }
        }

        private void Select_Click_Button(object sender, RoutedEventArgs e)
        {
            
            var dirList = Directory.GetDirectories(PathTB.Text, "*", SearchOption.TopDirectoryOnly).ToList();
            var dirNew = (PathTB.Text + ("\\" + ChildList.SelectedValue).Replace("\\\\", "\\"));
            if (dirList.Contains(dirNew))
            {
                PathTB.Text = dirNew;
                return;
            }
            if (ChildList.SelectedItems.Count == 1)
            {
                (this.Owner as MainWindow).PathTBL.Text = (PathTB.Text + ("\\" + ChildList.SelectedValue.ToString()).Replace("\\\\","\\"));                
            }
            else
            {
                (this.Owner as MainWindow).PathTBL.Text = "";
            }
            this.Owner.Focus();
            this.DialogResult = true;
            this.Close();                
        }

        private void ChildList_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {                
                case Key.Enter:
                    
                    Select_Click_Button((sender as ListView).SelectedItem, new RoutedEventArgs());                    
                    break;
                case Key.Back:
                    break;
                default:
                    break;
            }
        }
    }
}
