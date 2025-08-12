using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiefPortDocuments
{
    class Doc : INotifyPropertyChanged
    {
        private bool? _sel;
        private int _sortOrder;
        private string _lastPrint;
        public string LastPrinted {
            get { return _lastPrint; }
            set
            {
                _lastPrint = value;
                NotifyPropertyChanged("LastPrinted");
            }
        }
        public int SortOrder {
            get { return _sortOrder;}
            set { 
                _sortOrder = value;
                NotifyPropertyChanged("SortOrder");
            } 
        }
        public string NickName { get; set; }
        public bool? Sel { get { return _sel; } set { _sel = value; NotifyPropertyChanged("Sel"); } }
        public string FileName { get; set; }
        public string Path { get; set; }
        public List<string> Tags { get; set; }

        public Doc( bool sel , string name, string path ,List<string> tags, string nickName = "", int sort = 100, string lastDate="")
        {
            NickName = nickName;
            SortOrder = sort;
            Sel = sel;
            FileName = name;
            Path = path;
            Tags = new List<string>();
            LastPrinted = lastDate;            
        }

        public Doc()
        {
            NickName = "";
            SortOrder = 100;
            Sel = true;
            FileName = "";
            Path = "";
            Tags = new List<string>();
            LastPrinted = "";
        }
        public override bool Equals(object obj)
        {
            try
            {
                Doc doc = obj as Doc;
                if (this.FileName == doc.FileName & this.Path == doc.Path)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
	    {
		 
	}
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
