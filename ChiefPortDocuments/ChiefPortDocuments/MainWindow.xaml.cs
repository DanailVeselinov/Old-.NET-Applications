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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Office.Interop;
using System.Collections.ObjectModel;

namespace ChiefPortDocuments
{
    /// <summary>
    /// Interaction logic for MainWindow.xam
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            try
            {
                Resources.Add("StoredDocuments", LoadDocuments());
                Resources.Add("StoredTags", GetSavedTagsFiltered());
                Resources.Add("excelWB", null);
                Resources.Add("dgdSelInd", null);
                Resources.Add("Folder", null);
                InitializeComponent();
                PrinterSelector.ItemsSource = System.Drawing.Printing.PrinterSettings.InstalledPrinters;
                System.Drawing.Printing.PrinterSettings printerSettings = new System.Drawing.Printing.PrinterSettings();
                var pName = printerSettings.PrinterName;
                
                Resources.Add("DefaultPrinter",pName);
                
                //DocumentsGrid.DataContext = LoadDocuments();
                Resources.Add("doc", DocumentsGrid);
            }
            catch (Exception me)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}",me.Message,me.InnerException,me.StackTrace));
            }
            
        }

        private List<Doc> GetSavedDocs()
        {
            try
            {
                List<string> docsListText = System.IO.File.ReadAllLines("DocsList").ToList();
                List<Doc> docsList = new List<Doc>();
                foreach (var doc in docsListText)
                {
                    try
                    {
                        var t = doc.Split('\t');
                        var d = new Doc();
                        d.Sel = string.IsNullOrWhiteSpace(t[0]) ? false : bool.Parse(t[0]);
                        if (!string.IsNullOrWhiteSpace(t[1]))
                        {
                            try { d.SortOrder = int.Parse(t[1]); }
                            catch (Exception) { }
                        }
                        d.NickName = t[2];
                        d.FileName = t[3];
                        d.Path = t[4];
                        d.Tags = string.IsNullOrWhiteSpace(t[5]) ? new List<string>() : t[5].Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
                        d.LastPrinted = t[6];
                        docsList.Add(d);
                    }
                    catch (Exception)
                    {
                    }

                }
                return docsList;
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", e.Message, e.InnerException, e.StackTrace));
                return new List<Doc>();
            }
            
        }

        private ObservableCollection<Tags> GetSavedTagsFiltered()
        {
            try
            {
                List<string> tagsListText = System.IO.File.ReadAllLines("TagsList").ToList();
                var docsList = LoadDocuments();
                List<Tags> tagsList = new List<Tags>();
                foreach (var tag in tagsListText)
                {
                    try
                    {
                        var t = tag.Split('\t');
                        var doc = docsList.FirstOrDefault(d => d.Tags.Contains(t[1]) & d.Sel.Value);
                        if (doc != null)
                        {

                            tagsList.Add(new Tags(t[0], t[1]));
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                return new ObservableCollection<Tags>(tagsList);
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", e.Message, e.InnerException, e.StackTrace));
                return new ObservableCollection<Tags>();
            }
            
        }

        private static ObservableCollection<Tags> GetSavedTags()
        {
            try
            {
                List<string> tagsListText = System.IO.File.ReadAllLines("TagsList").ToList();
                List<Tags> tagsList = new List<Tags>();
                foreach (var tag in tagsListText)
                {
                    try
                    {
                        var t = tag.Split('\t');
                        tagsList.Add(new Tags(t[0], t[1]));
                    }
                    catch (Exception)
                    {
                    }
                }
                return new ObservableCollection<Tags>(tagsList);
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", e.Message, e.InnerException, e.StackTrace));
                return new ObservableCollection<Tags>();
            }
                    

        }

        private void ProcessDocument()
        {
            try
            {
                string printer = "";
                Microsoft.Office.Interop.Word.Application wap = new Microsoft.Office.Interop.Word.Application();
                wap.Visible = false;
                Microsoft.Office.Interop.Excel.Application xap = new Microsoft.Office.Interop.Excel.Application();

                xap.DisplayAlerts = false;
                xap.DeferAsyncQueries = true;
                foreach (Doc doc in DocumentsGrid.Items)
                {
                    if (doc.Sel.Value)
                    {
                        char ext = doc.FileName.ToUpper()[doc.FileName.LastIndexOf('.') + 1];
                        switch (ext)
                        {
                            case 'D':
                                wap.Visible = false;                                
                                Microsoft.Office.Interop.Word.Document edit = wap.Documents.Open(System.IO.Directory.GetCurrentDirectory() + "\\TemplatedDocuments\\" + doc.Path + doc.FileName, Type.Missing, false, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                                edit.Application.DisplayAlerts = Microsoft.Office.Interop.Word.WdAlertLevel.wdAlertsNone;
                                object replaceAll = Microsoft.Office.Interop.Word.WdReplace.wdReplaceAll;
                                foreach (Tags tag in TagsGrid.ItemsSource)
                                {
                                    if (edit.Sections.Count<1)
                                    {
                                        continue;
                                    }
                                    foreach (Microsoft.Office.Interop.Word.Section section in edit.Sections)
                                    {
                                        try
                                        {
                                            Microsoft.Office.Interop.Word.Find obh = section.Headers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range.Find;
                                            if (obh != null)
                                            {
                                                obh.ClearFormatting();
                                                obh.Text = tag.Tag;
                                                obh.Replacement.ClearFormatting();
                                                System.Windows.Clipboard.SetText(tag.Value);
                                                obh.Replacement.Text = "^c";
                                                obh.Execute(Type.Missing, false, false, Type.Missing, Type.Missing,
                            Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                            replaceAll, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                                            }



                                            Microsoft.Office.Interop.Word.Find obf = section.Footers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range.Find;
                                            if (obf != null)
                                            {
                                                obf.ClearFormatting();
                                                obf.Text = tag.Tag;
                                                obf.Replacement.ClearFormatting();
                                                System.Windows.Clipboard.SetText(tag.Value);
                                                obf.Replacement.Text = "^c";
                                                obf.Execute(Type.Missing, false, false, Type.Missing, Type.Missing,
                            Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                            replaceAll, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                                            }

                                        }
                                        catch (Exception e)
                                        {

                                        }
                                    }
                                    try
                                    {
                                        Microsoft.Office.Interop.Word.Find ob = wap.Selection.Find;
                                        if (ob != null)
                                        {
                                            ob.ClearFormatting();
                                            ob.Text = tag.Tag;
                                            ob.Replacement.ClearFormatting();
                                            if (tag.Value.Length > 200)
                                            {
                                                System.Windows.Clipboard.SetText(tag.Value);
                                                ob.Replacement.Text = "^c";
                                            }
                                            else
                                            {
                                                ob.Replacement.Text = tag.Value;
                                            }
                                            ob.Execute(Type.Missing, false, false, Type.Missing, Type.Missing,
                                            Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                                            replaceAll, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }


                                }
                                printer = string.IsNullOrWhiteSpace((string)PrinterSelector.SelectedItem) ? edit.Application.ActivePrinter : (string)PrinterSelector.SelectedItem;

                                edit.Application.ActivePrinter = printer;
                                edit.PrintOut(false, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, 1, Type.Missing, Microsoft.Office.Interop.Word.WdPrintOutPages.wdPrintAllPages, false, true, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                                doc.LastPrinted = DateTime.Now.ToString("dd.MMM");
                                edit.Close(Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges, Type.Missing, true);
                                
                                //wap.Quit(false, Type..Missing, true);

                                break;
                            case 'X':
                                xap.Visible = false;
                                //excel document
                                Microsoft.Office.Interop.Excel.Workbook editx = xap.Workbooks.Open(System.IO.Directory.GetCurrentDirectory() + "\\TemplatedDocuments\\" + doc.Path + doc.FileName, false, false, Type.Missing, Type.Missing, Type.Missing, true, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                                editx.Application.DisplayAlerts = false;
                                string filename = System.IO.Directory.GetCurrentDirectory() + "\\" + "Temp.xls";
                                editx.SaveAs(filename, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

                                foreach (Microsoft.Office.Interop.Excel.Worksheet sheet in editx.Sheets)
                                {
                                    foreach (Tags tag in TagsGrid.ItemsSource)
                                    {
                                        Microsoft.Office.Interop.Excel.Range r = sheet.get_Range("A1", Type.Missing);
                                        try
                                        {
                                            r.Replace(tag.Tag, tag.Value, Microsoft.Office.Interop.Excel.XlLookAt.xlPart, Microsoft.Office.Interop.Excel.XlSearchOrder.xlByRows, false, Type.Missing, Type.Missing, Type.Missing);
                                        }
                                        catch (Exception)
                                        {
                                            continue;
                                        }
                                    }
                                }
                                printer = string.IsNullOrWhiteSpace((string)PrinterSelector.SelectedItem) ? editx.Application.ActivePrinter : (string)PrinterSelector.SelectedItem;
                                editx.Sheets.PrintOutEx(Type.Missing, Type.Missing, 1, false, printer, false, true, Type.Missing,true);
                                //editx.Sheets.PrintOut(Type.Missing, Type.Missing, 1, false, printer, false, true, Type.Missing);
                                doc.LastPrinted = DateTime.Now.ToString("dd.MMM");
                                editx.Close(Microsoft.Office.Interop.Excel.XlSaveAction.xlDoNotSaveChanges, Type.Missing, false);                                
                                break;
                            default:
                                break;
                        }
                    }
                }
                while (wap.BackgroundPrintingStatus > 0)
                {
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.Thread.Join(500);
                }
                xap.Quit();
                wap.ActivePrinter = (string)Resources["DefaultPrinter"];
                wap.Quit(Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges, Type.Missing, Type.Missing);

                
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", e.Message, e.InnerException, e.StackTrace));
            }
            
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ProcessDocument();
        }

        private ObservableCollection<Doc> LoadDocuments()
        {
            try
            {
                string path = "TemplatedDocuments";
                List<Doc> documents = new List<Doc>();
                var files = System.IO.Directory.GetFiles(path, "*", System.IO.SearchOption.AllDirectories);
                List<Doc> storedDocs = GetSavedDocs();
                if (files.Count() > 0)
                {
                    foreach (var file in files)
                    {
                        var attr = System.IO.File.GetAttributes(file);
                        if (attr.HasFlag(System.IO.FileAttributes.Hidden))
                        {
                            continue;
                        }
                        int t = file.LastIndexOf('\\');
                        var fileName = file;
                        int sortOrder = 1;
                        if (t > 17)
                        {
                            fileName = file.Substring(t + 1);
                        }
                        var p = file.Substring(19, t - 18);
                        var sd = storedDocs.FirstOrDefault(d => d.FileName == fileName & d.Path == p);
                        if (sd != null)
                        {
                            documents.Add(sd);
                            sortOrder = sd.SortOrder + 1;
                        }
                        else
                        {
                            documents.Add(new Doc(true, fileName, p, new List<string>(), "", sortOrder, ""));
                            sortOrder++;
                        }
                    }
                }
                else
                {
                    //otvori file Explorer
                }
                // Otvori vsichki fajlove i tursi za tagove i gi vkaraj v spisak za da moje da se selektirat samo nujnite tagove v spisaka
                //UpdateTagsListFmDocuments(documents);
                return new ObservableCollection<Doc>(documents.OrderBy(d => d.Path).ThenBy(s => s.SortOrder));
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", e.Message, e.InnerException, e.StackTrace));
                return new ObservableCollection<Doc>();
            }
            
        }


        private void UpdateTagListFmTextFile(ObservableCollection<Doc> documents)
        {
            try
            {
                List<string> allActiveTags = new List<string>();
                foreach (Doc doc in documents)
                {
                    
                    foreach (string tag in doc.Tags)
                    {
                        if (!allActiveTags.Contains(tag) & doc.Sel.Value)
                        {
                            allActiveTags.Add(tag);
                        }
                    }
                }
                ObservableCollection<Tags> obtags = new ObservableCollection<Tags>();
                var tagsSource = GetSavedTags();
                foreach (string acTag in allActiveTags)
                {
                    var ts = tagsSource.FirstOrDefault(t => t.Tag == acTag);
                    if (ts == null)
                    {
                        obtags.Add(new Tags("", acTag));
                    }
                    else
                    {
                        obtags.Add(ts);
                    }
                }
                Resources["StoredTags"] = obtags;
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", e.Message, e.InnerException, e.StackTrace));                
            }            
        }
        private void UpdateTagsListFmDocuments(ObservableCollection<Doc> documents)
        {
            try
            {
                List<string> allActiveTags = new List<string>();
                Microsoft.Office.Interop.Word.Application wap = new Microsoft.Office.Interop.Word.Application();
                Microsoft.Office.Interop.Excel.Application exap = new Microsoft.Office.Interop.Excel.Application();
                foreach (Doc doc in documents)
                {
                    if (doc.Tags.Count < 1 & DocumentsGrid.SelectedIndex > -1)
                    {
                        char ext = doc.FileName.ToUpper()[doc.FileName.LastIndexOf('.') + 1];
                        switch (ext)
                        {
                            case 'D':
                                Microsoft.Office.Interop.Word.Document wd = wap.Documents.Open(System.IO.Directory.GetCurrentDirectory() + "\\TemplatedDocuments\\" + doc.Path + doc.FileName, Type.Missing, false, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                                Resources["dgdSelInd"] = DocumentsGrid.SelectedIndex;
                                Get_Update_Tags_fmWord(wd, ref DocumentsGrid);
                                SaveDocs_Click(null, new RoutedEventArgs());
                                wd.Close(Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges, Type.Missing, false);
                                break;
                            case 'E':
                                Microsoft.Office.Interop.Excel.Workbook wb = exap.Workbooks.Open(System.IO.Directory.GetCurrentDirectory() + "\\TemplatedDocuments\\" + doc.Path + doc.FileName, Type.Missing, false, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                                Resources["dgdSelInd"] = DocumentsGrid.SelectedIndex;
                                Get_Update_Tags_fmExcel(wb, ref DocumentsGrid);
                                wb.Close(Microsoft.Office.Interop.Excel.XlSaveAction.xlDoNotSaveChanges, Type.Missing, false);
                                break;
                            default:
                                break;
                        }
                    }
                    foreach (string tag in doc.Tags)
                    {
                        if (!allActiveTags.Contains(tag) & doc.Sel.Value)
                        {
                            allActiveTags.Add(tag);
                        }
                    }
                }
                wap.Quit(Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges,Type.Missing,Type.Missing);
                exap.Quit();
                ObservableCollection<Tags> obtags = new ObservableCollection<Tags>();
                var tagsSource = GetSavedTags();
                foreach (string acTag in allActiveTags)
                {
                    var ts = tagsSource.FirstOrDefault(t => t.Tag == acTag);
                    if (ts == null)
                    {
                        obtags.Add(new Tags("", acTag));
                    }
                    else
                    {
                        obtags.Add(ts);
                    }
                }
                Resources["StoredTags"] = obtags;
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", e.Message, e.InnerException, e.StackTrace));                
            }            
        }

        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            try
            {
                BrowseFolder bf = new BrowseFolder();
                bf.Owner = this;
                Doc doc = new Doc();
                bf.PathTB.Text = System.IO.Directory.GetCurrentDirectory();
                var bfsuc = bf.ShowDialog();
                if (!bfsuc.HasValue) { bf.Close(); return; }
                if (bfsuc.Value)
                {
                    var file = PathTBL.Text;
                    if (string.IsNullOrEmpty(file))
                    {
                        return;
                    }
                    char ext = file.ToUpper()[file.LastIndexOf('.') + 1];
                    var dir = System.IO.Directory.GetCurrentDirectory() + "\\";
                    doc.FileName = file.Substring(file.LastIndexOf('\\') + 1);
                    doc.Path = "";
                    doc.NickName = "";
                    doc.Sel = true;
                    doc.SortOrder = DocumentsGrid.Items.Count + 1;
                    doc.Tags = new List<string>();
                    doc.LastPrinted = "";
                    //check path then create directory if needed
                    var bfDest = new FolderSelect();
                    bfDest.Owner = this;
                    var hasPath = bfDest.ShowDialog();
                    if (hasPath.HasValue)
                    {
                        if (hasPath.Value)
                        {
                            doc.Path = (Resources["Folder"] as string);
                        }
                    }
                    if (!System.IO.Directory.Exists(System.IO.Directory.GetCurrentDirectory() + "\\TemplatedDocuments\\" + doc.Path))
                    {
                        System.IO.Directory.CreateDirectory(System.IO.Directory.GetCurrentDirectory() + "\\TemplatedDocuments\\" + doc.Path);
                    }
                    System.IO.File.Copy(file, System.IO.Directory.GetCurrentDirectory() + "\\TemplatedDocuments\\" + doc.Path + doc.FileName, true);
                    Resources["StoredDocuments"] = LoadDocuments();
                    //DocumentsGrid.ItemsSource = Resources["StoredDocuments"] as ObservableCollection<Doc>;
                    DocumentsGrid.Focus();
                    foreach (var item in DocumentsGrid.Items)
                    {
                        if ((item as Doc).Equals(doc))
                        {
                            DocumentsGrid.SelectedItem = item;
                        }
                    }
                    try
                    {
                        Button_Click_View(DocumentsGrid, new RoutedEventArgs());
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }            
        }


        private void TextBlock_MouseLeftButtonUp_Up(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (DocumentsGrid.SelectedIndex > 0)
                {

                    var currentItem = DocumentsGrid.SelectedItem as Doc;
                    var prevItem = DocumentsGrid.Items[DocumentsGrid.SelectedIndex -1] as Doc;
                    currentItem.SortOrder = prevItem.SortOrder - 1;
                    WriteDocsList();
                    var col = (Resources["StoredDocuments"] as ObservableCollection<Doc>).ToList().OrderBy(g => g.Path).ThenBy(f => f.SortOrder);
                    DocumentsGrid.ItemsSource = new ObservableCollection<Doc>(col);
                    //DocumentsGrid.DataContext = LoadDocuments();
                    DocumentsGrid.Focus();
                }
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }
        }

        private void TextBlock_MouseLeftButtonUp_Down(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if ((DocumentsGrid.SelectedIndex + 1) < DocumentsGrid.Items.Count)
                {   
                    var currentItem = DocumentsGrid.SelectedItem as Doc;
                    var nextItem = DocumentsGrid.Items[DocumentsGrid.SelectedIndex + 1] as Doc;
                    currentItem.SortOrder = nextItem.SortOrder + 1;
                    WriteDocsList();
                    var col = (Resources["StoredDocuments"] as ObservableCollection<Doc>).ToList().OrderBy(g=>g.Path).ThenBy(f=>f.SortOrder);
                    DocumentsGrid.ItemsSource = new ObservableCollection<Doc>(col);
                    //DocumentsGrid.DataContext = LoadDocuments();
                    DocumentsGrid.Focus();
                }
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }            
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                WriteDataToFiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Check file writing capability or stop Real Time anti virus protection. \r{0}",ex.Message));
            }
        }

        private void WriteDataToFiles()
        {
            WriteTagsList();
            WriteDocsList();
        }

        private void WriteDocsList(Doc docs)
        {
            try
            {
                StringBuilder docsSB = new StringBuilder();

                StringBuilder docTags = new StringBuilder();
                foreach (var t in docs.Tags)
                {
                    docTags.Append(t + ";");
                }

                docsSB.AppendLine(docs.Sel.ToString() + '\t' + docs.SortOrder.ToString() + '\t' + docs.NickName + '\t' + docs.FileName + '\t' + docs.Path + '\t' + docTags.ToString());

                System.IO.File.AppendAllText(System.IO.Directory.GetCurrentDirectory() + "\\DocsList", docsSB.ToString());
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }            
        }
        
        private void WriteDocsList()
        {
            try
            {
                StringBuilder docsSB = new StringBuilder();

                foreach (Doc docs in DocumentsGrid.Items)
                {
                    StringBuilder docTags = new StringBuilder();
                    foreach (var t in docs.Tags)
                    {
                        docTags.Append(t + ";");
                    }
                    docsSB.AppendLine(docs.Sel.ToString() + '\t' + docs.SortOrder.ToString() + '\t' + docs.NickName + '\t' + docs.FileName + '\t' + docs.Path + '\t' + docTags.ToString() + '\t' + docs.LastPrinted);
                }

                System.IO.File.WriteAllText(System.IO.Directory.GetCurrentDirectory() + "\\DocsList", docsSB.ToString());
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }            
        }


        private void WriteTagsList()
        {
            try
            {
                var tList = GetSavedTags();
                var tagsArray = new Tags[tList.Count];
                var oldTagsList = tList.ToList();
                oldTagsList.CopyTo(tagsArray);
                StringBuilder tagsSB = new StringBuilder();
                foreach (Tags ta in tagsArray)
                {
                    foreach (Tags tag in TagsGrid.Items)
                    {
                        if (!string.IsNullOrWhiteSpace(tag.Tag) & !string.IsNullOrEmpty(tag.Value))
                        {
                            if (oldTagsList.Contains(tag))
                            {
                                oldTagsList.Find(t => t.Tag == tag.Tag).Value = tag.Value;
                            }
                            else
                            {
                                oldTagsList.Add(tag);
                            }
                        }
                    }

                }
                foreach (Tags t in oldTagsList)
                {
                    tagsSB.AppendLine(t.Value + "\t" + t.Tag);
                }
                System.IO.File.WriteAllText(System.IO.Directory.GetCurrentDirectory() + "\\TagsList", tagsSB.ToString());
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }
            
        }

        private void Button_Click_View(object sender, RoutedEventArgs e)
        {
            
            //open document for edit
            var doc = DocumentsGrid.SelectedItem as Doc;
            var file = doc.FileName;
            char ext = file.ToUpper()[file.LastIndexOf('.') + 1];
            try
            {
                switch (ext)
                {
                    case 'D':
                        Microsoft.Office.Interop.Word.Application wap = new Microsoft.Office.Interop.Word.Application();
                        
                        Microsoft.Office.Interop.Word.Document edit = new Microsoft.Office.Interop.Word.Document();
                        try
                        {
                            edit = wap.Documents.Open(System.IO.Directory.GetCurrentDirectory() + "\\TemplatedDocuments\\" + doc.Path + doc.FileName, Type.Missing, false, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                            wap.Visible = true;
                            Resources["dgdSelInd"] = DocumentsGrid.SelectedIndex;
                            wap.DocumentBeforeSave += wap_DocumentBeforeSave;
                            wap.DocumentBeforeClose += wap_DocumentBeforeClose;
                        }
                        catch (Exception)
                        {
                            wap.Quit(Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges, Microsoft.Office.Interop.Word.WdSaveFormat.wdFormatDocumentDefault, Type.Missing);
                        }
                        break;
                    case 'X':
                        //excel document
                        Microsoft.Office.Interop.Excel.Application xap = new Microsoft.Office.Interop.Excel.Application();
                        try
                        {
                            Microsoft.Office.Interop.Excel.Workbook editx = xap.Workbooks.Open(System.IO.Directory.GetCurrentDirectory() + "\\TemplatedDocuments\\" + doc.Path + doc.FileName, Type.Missing, false, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                            Resources["dgdSelInd"] = DocumentsGrid.SelectedIndex;
                            xap.WorkbookBeforeSave += xap_WorkbookBeforeSave;
                            xap.WorkbookBeforeClose += xap_WorkbookBeforeClose;
                            xap.Visible = true;
                                    
                        }
                        catch (Exception)
                        {
                            xap.Quit();
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }
        }



        void wap_DocumentBeforeClose(Microsoft.Office.Interop.Word.Document edit, ref bool Cancel)
        {
            try
            {
                var doc = Resources["doc"] as DataGrid;
                Get_Update_Tags_fmWord(edit, ref doc);
                edit.Application.Quit(Microsoft.Office.Interop.Word.WdSaveOptions.wdSaveChanges,Type.Missing,Type.Missing);
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }
        }

        private void xap_WorkbookBeforeClose(Microsoft.Office.Interop.Excel.Workbook Wb, ref bool Cancel)
        {
            try
            {
                var doc = Resources["doc"] as DataGrid;
                Get_Update_Tags_fmExcel(Wb, ref doc);

                Wb.Application.Quit();
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }
        }

        void xap_WorkbookBeforeSave(Microsoft.Office.Interop.Excel.Workbook Wb, bool SaveAsUI, ref bool Cancel)
        {
            try
            {
                var doc = Resources["doc"] as DataGrid;
                Get_Update_Tags_fmExcel(Wb, ref doc);
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }
        }


        private void Get_Update_Tags_fmExcel(Microsoft.Office.Interop.Excel.Workbook editx,ref DataGrid ob)
        {
            try
            {
                int selInd = (int)Resources["dgdSelInd"];
                Doc doc = ob.Items[selInd] as Doc;
                List<string> nTags = new List<string>();
                foreach (Microsoft.Office.Interop.Excel.Worksheet section in editx.Sheets)
                {
                    try
                    {
                        var RNG = section.UsedRange;
                        Microsoft.Office.Interop.Excel.Range rg = RNG.Find("<*>", Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlLookAt.xlPart, Type.Missing, Microsoft.Office.Interop.Excel.XlSearchDirection.xlNext, false, Type.Missing, Type.Missing);
                        int rowFirst = rg.Row;
                        int colFirst = rg.Column;

                        while (true)
                        {
                            try
                            {
                                string st = rg.Text;
                                int startInd = 0;
                                int endInd = 0;
                                int ct = st.Count(l => l == '<');
                                for (int i = 0; i < ct; i++)
                                {
                                    startInd = rg.Text.IndexOf('<', endInd);
                                    endInd = rg.Text.IndexOf('>', startInd + 1);
                                    st = rg.Text.Substring(startInd, endInd - startInd + 1);
                                    if (!nTags.Contains(st) & !string.IsNullOrWhiteSpace(st))
                                    {
                                        nTags.Add(st);
                                    }
                                }
                                rg = RNG.FindNext(rg);
                                if (rg.Row == rowFirst & rg.Column == colFirst)
                                {
                                    break;
                                }
                            }
                            catch (Exception)
                            {

                            }

                        }
                        if (nTags.Count < 1)
                        {
                            MessageBox.Show(string.Format("The File:{0} has no tags in it. Please View the document and add tags.", doc.FileName));
                        }
                        else
                        {
                            doc.Tags = nTags;
                        }
                    }
                    catch (Exception)
                    {
                    }
                    
                }
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }            
        }

        private void Get_Update_Tags_fmExcel(Microsoft.Office.Interop.Excel.Workbook editx, Doc doc)
        {
            try
            {
                List<string> nTags = new List<string>();
                foreach (Microsoft.Office.Interop.Excel.Worksheet section in editx.Sheets)
                {
                    try
                    {
                        var RNG = section.UsedRange;
                        Microsoft.Office.Interop.Excel.Range rg = RNG.Find("<*>", Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlLookAt.xlPart, Type.Missing, Microsoft.Office.Interop.Excel.XlSearchDirection.xlNext, false, Type.Missing, Type.Missing);
                        int rowFirst = rg.Row;
                        int colFirst = rg.Column;

                        while (true)
                        {
                            try
                            {
                                string st = rg.Text;
                                int startInd = 0;
                                int endInd = 0;
                                int ct = st.Count(l => l == '<');
                                for (int i = 0; i < ct; i++)
                                {
                                    startInd = rg.Text.IndexOf('<', endInd);
                                    endInd = rg.Text.IndexOf('>', startInd + 1);
                                    st = rg.Text.Substring(startInd, endInd - startInd + 1);
                                    if (!nTags.Contains(st) & !string.IsNullOrWhiteSpace(st))
                                    {
                                        nTags.Add(st);
                                    }
                                }
                                rg = RNG.FindNext(rg);
                                if (rg.Row == rowFirst & rg.Column == colFirst)
                                {
                                    break;
                                }
                            }
                            catch (Exception)
                            {

                            }

                        }
                        if (nTags.Count < 1)
                        {
                            MessageBox.Show(string.Format("The File:{0} has no tags in it. Please View the document and add tags.", doc.FileName));
                        }
                        else
                        {
                            doc.Tags = nTags;
                        }
                    }
                    catch (Exception)
                    {
                        
                    }
                    
                }
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }
            
        }

        void editx_BeforeSave(bool SaveAsUI, ref bool Cancel)
        {
        }

        private void wap_DocumentBeforeSave(Microsoft.Office.Interop.Word.Document edit, ref bool SaveAsUI, ref bool Cancel)
        {
            try
            {
                var doc = Resources["doc"] as DataGrid;
                Get_Update_Tags_fmWord(edit, ref doc);
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }
        }


        private void Get_Update_Tags_fmWord(Microsoft.Office.Interop.Word.Document edit,ref DataGrid ob)
        {
            try
            {
                int selInd = (int)Resources["dgdSelInd"];
                Doc doc = ob.Items[selInd] as Doc;
                List<string> nTags = new List<string>();
                foreach (Microsoft.Office.Interop.Word.Section section in edit.Sections)
                {
                    int ind = 0;
                    int indend = 0;
                    var header = section.Headers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    while (header.Text.IndexOf("<", ind) > -1)
                    {
                        ind = header.Text.IndexOf("<", ind);
                        indend = header.Text.IndexOf(">", ind + 1);
                        var text = header.Text.Substring(ind, indend - ind + 1);
                        if (!nTags.Contains(text) & !string.IsNullOrWhiteSpace(text))
                        {
                            nTags.Add(text);
                        }
                        ind = indend + 1;
                    }
                    ind = 0;
                    indend = 0;
                    header = section.Footers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    while (header.Text.IndexOf("<", ind) > -1)
                    {
                        ind = header.Text.IndexOf("<", ind);
                        indend = header.Text.IndexOf(">", ind + 1);
                        var text = header.Text.Substring(ind, indend - ind + 1);
                        if (!nTags.Contains(text) & !string.IsNullOrWhiteSpace(text))
                        {
                            nTags.Add(text);
                        }
                        ind = indend + 1;
                    }
                    ind = 0;
                    indend = 0;
                    header = section.Range;
                    while (header.Text.IndexOf("<", ind) > -1)
                    {
                        ind = header.Text.IndexOf("<", ind);
                        indend = header.Text.IndexOf(">", ind + 1);
                        var text = header.Text.Substring(ind, indend - ind + 1);
                        if (!nTags.Contains(text) & !string.IsNullOrWhiteSpace(text))
                        {
                            nTags.Add(text);
                        }
                        ind = indend + 1;
                    }
                }
                if (nTags.Count < 1)
                {
                }
                else
                {
                    doc.Tags = nTags;
                }
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }
            
        }

        private void Get_Update_Tags_fmWord(Microsoft.Office.Interop.Word.Document edit, Doc doc)
        {
            try
            {
                List<string> nTags = new List<string>();
                foreach (Microsoft.Office.Interop.Word.Section section in edit.Sections)
                {
                    int ind = 0;
                    int indend = 0;
                    var header = section.Headers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    while (header.Text.IndexOf("<", ind) > -1)
                    {
                        ind = header.Text.IndexOf("<", ind);
                        indend = header.Text.IndexOf(">", ind + 1);
                        var text = header.Text.Substring(ind, indend - ind + 1);
                        if (!nTags.Contains(text) & !string.IsNullOrWhiteSpace(text))
                        {
                            nTags.Add(text);
                        }
                        ind = indend + 1;
                    }
                    ind = 0;
                    indend = 0;
                    header = section.Footers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    while (header.Text.IndexOf("<", ind) > -1)
                    {
                        ind = header.Text.IndexOf("<", ind);
                        indend = header.Text.IndexOf(">", ind + 1);
                        var text = header.Text.Substring(ind, indend - ind + 1);
                        if (!nTags.Contains(text) & !string.IsNullOrWhiteSpace(text))
                        {
                            nTags.Add(text);
                        }
                        ind = indend + 1;
                    }
                    ind = 0;
                    indend = 0;
                    header = section.Range;
                    while (header.Text.IndexOf("<", ind) > -1)
                    {
                        ind = header.Text.IndexOf("<", ind);
                        indend = header.Text.IndexOf(">", ind + 1);
                        var text = header.Text.Substring(ind, indend - ind + 1);
                        if (!nTags.Contains(text) & !string.IsNullOrWhiteSpace(text))
                        {
                            nTags.Add(text);
                        }
                        ind = indend + 1;
                    }
                }
                if (nTags.Count < 1)
                {
                    MessageBox.Show(string.Format("The File:{0} has no tags in it. Please View the document and add tags.", doc.FileName));
                }
                else
                {
                    doc.Tags = nTags;
                }
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }
            
        }
        

        private void Button_Click_Delete(object sender, RoutedEventArgs e)
        {
            try
            {
                var doc = DocumentsGrid.SelectedItem as Doc;
                System.IO.File.Delete(System.IO.Directory.GetCurrentDirectory() + "\\TemplatedDocuments\\" + doc.Path + doc.FileName);
                (Resources["StoredDocuments"] as ObservableCollection<Doc>).Remove(doc);
                DocumentsGrid.ItemsSource = Resources["StoredDocuments"] as ObservableCollection<Doc>;
                SaveDocs_Click(null, new RoutedEventArgs());
                UpdateTagsListFmDocuments(DocumentsGrid.ItemsSource as ObservableCollection<Doc>);

            }
            catch (System.IO.FileNotFoundException)
            {
                MessageBox.Show("File to delete not Fount!");
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var docsIS = Resources["StoredDocuments"] as ObservableCollection<Doc>;
                if (docsIS != null)
                {
                    UpdateTagListFmTextFile(docsIS);
                }
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }
            
            //uPDATE tAGS LIST
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            WriteTagsList();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            (Resources["StoredTags"] as ObservableCollection<Tags>).Add(new Tags("", "<Enter Tag Here>"));
        }

        private void SaveDocs_Click(object sender, RoutedEventArgs e)
        {
            WriteDocsList();
        }

        
        private void all_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (Doc item in DocumentsGrid.ItemsSource)
                {
                    item.Sel = (sender as System.Windows.Controls.CheckBox).IsChecked.Value;
                }
                CheckBox_Click_All(DocumentsGrid, new RoutedEventArgs());
                if ((sender as System.Windows.Controls.CheckBox).IsChecked.Value == true)
                {
                    WriteTagsList(Resources["StoredTags"] as ObservableCollection<Tags>);
                }
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }
            
        }

        private void CheckBox_Click_All(DataGrid DocumentsGrid, RoutedEventArgs routedEventArgs)
        {
            try
            {
                var docsIS = Resources["StoredDocuments"] as ObservableCollection<Doc>;
                if (docsIS != null)
                {
                    UpdateTagsListFmDocuments_All(docsIS);
                }
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }
            
        }

        private void UpdateTagsListFmDocuments_All(ObservableCollection<Doc> documents)
        {
            try
            {
                List<string> allActiveTags = new List<string>();
                foreach (Doc doc in documents)
                {
                    
                    foreach (string tag in doc.Tags)
                    {
                        if (!allActiveTags.Contains(tag) & doc.Sel.Value)
                        {
                            allActiveTags.Add(tag);
                        }
                    }
                }

                ObservableCollection<Tags> obtags = new ObservableCollection<Tags>();
                var tagsSource = GetSavedTagsFiltered();
                foreach (string acTag in allActiveTags)
                {
                    var ts = tagsSource.FirstOrDefault(t => t.Tag == acTag);
                    if (ts == null)
                    {
                        obtags.Add(new Tags("", acTag));
                    }
                    else
                    {
                        obtags.Add(ts);
                    }
                }
                Resources["StoredTags"] = obtags;
            
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }
            
        }

        private void WriteTagsList(ObservableCollection<Tags> obtags)
        {
            try
            {
                var tList = GetSavedTags();
                StringBuilder tagsSB = new StringBuilder();
                foreach (Tags ta in obtags)
                {
                    var tl = tList.FirstOrDefault(t => t.Tag.ToUpper() == ta.Tag.ToUpper());
                    if (tl != null)
                    {
                        ta.Value = tl.Value;
                    }
                }
                foreach (Tags t in obtags)
                {
                    tagsSB.AppendLine(t.Value + "\t" + t.Tag);
                }
                System.IO.File.WriteAllText(System.IO.Directory.GetCurrentDirectory() + "\\TagsList", tagsSB.ToString());
            }
            catch (Exception en)
            {
                MessageBox.Show(string.Format("Message: {0}; Stack Trace: {1}; Inner Exception: {2}", en.Message, en.InnerException, en.StackTrace));                
            }
        }

        private void TagsGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            WriteTagsList();
        }

        private void DocumentsGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            WriteDocsList();
        }

        private void DocumentsGrid_Drop(object sender, DragEventArgs e)
        {
            string[] f = (string[])e.Data.GetData(DataFormats.FileDrop, true);
            Doc doc = new Doc();
            foreach (string it in f)
            {
                if (it.ToUpper().Contains(".DOC") | it.ToUpper().Contains(".DOCX") | it.ToUpper().Contains(".XLS") | it.ToUpper().Contains(".XLSX"))
                {
                    string file = f[0];
                    if (string.IsNullOrEmpty(file))
                    {
                        return;
                    }
                    char ext = file.ToUpper()[file.LastIndexOf('.') + 1];
                    var dir = System.IO.Directory.GetCurrentDirectory() + "\\";
                    doc.FileName = file.Substring(file.LastIndexOf('\\') + 1);
                    doc.Path = "";
                    doc.NickName = "";
                    doc.Sel = true;
                    doc.SortOrder = DocumentsGrid.Items.Count + 1;
                    doc.Tags = new List<string>();
                    doc.LastPrinted = "";
                    //check path then create directory if needed
                    var bfDest = new FolderSelect();
                    bfDest.Owner = this;
                    var hasPath = bfDest.ShowDialog();
                    if (hasPath.HasValue)
                    {
                        if (hasPath.Value)
                        {
                            doc.Path = (Resources["Folder"] as string);
                        }
                    }
                    if (!System.IO.Directory.Exists(System.IO.Directory.GetCurrentDirectory() + "\\TemplatedDocuments\\" + doc.Path))
                    {
                        System.IO.Directory.CreateDirectory(System.IO.Directory.GetCurrentDirectory() + "\\TemplatedDocuments\\" + doc.Path);
                    }
                    System.IO.File.Copy(file, System.IO.Directory.GetCurrentDirectory() + "\\TemplatedDocuments\\" + doc.Path + doc.FileName, true);
                    Resources["StoredDocuments"] = LoadDocuments();
                    DocumentsGrid.Focus();
                    foreach (var item in DocumentsGrid.Items)
                    {
                        if ((item as Doc).Equals(doc))
                        {
                            DocumentsGrid.SelectedItem = item;
                        }
                    }
                    try
                    {
                        Button_Click_View(DocumentsGrid, new RoutedEventArgs());
                    }
                    catch (Exception)
                    {
                    }
                    return;
                }
            }
        }

        private void DocumentsGrid_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }


        







    }
}
