using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace WpfBNFTExtract
{
    public class TableRow : IComparable 
    {
        public TableRow(string name, string rank, short? order, string h1a, string h1b, string h2a, string h2b, string h3a, string h3b, string h4a, string h4b, string h5a, string h5b, string h6a, string h6b, string h7a, string h7b, string h8a, string h8b, string h9a, string h9b, string h10a, string h10b, string h11a, string h11b, string h12a, string h12b, string h13a, string h13b, string h14a, string h14b, string h15a, string h15b, string h16a, string h16b, string h17a, string h17b, string h18a, string h18b, string h19a, string h19b, string h20a, string h20b, string h21a, string h21b, string h22a, string h22b, string h23a, string h23b, string h24a, string h24b, double? ot)
        {
            this.Name = name; this.Rank = rank; this.SortOrder = order; H01A = h1a; H01B = h1b; ; H02A = h2a; H02B = h2b; H03A = h3a; H03B = h3b; H04A = h4a; H04B = h4b; H05A = h5a; H05B = h5b; H06A = h6a; H06B = h6b; H07A = h7a; H07B = h7b; H08A = h8a; H08B = h8b; H09A = h9a; H09B = h9b; H10A = h10a; H10B = h10b; H11A = h11a; H11B = h11b; H12A = h12a; H12B = h12b; H13A = h13a; H13B = h13b; H14A = h14a; H14B = h14b; H15A = h15a; H15B = h15b; H16A = h16a; H16B = h16b; H17A = h17a; H17B = h17b; H18A = h18a; H18B = h18b; H19A = h19a; H19B = h19b; H20A = h20a; H20B = h20b; H21A = h21a; H21B = h21b; H22A = h22a; H22B = h22b; H23A = h23a; H23B = h23b; H24A = h24a; H24B = h24b; overtime = (double)ot;
        }
        public TableRow()
        {
        }
        public short? SortOrder { get; set; }
        public string Name { get; set; }
        public string Rank { get; set; }
        public string H01A { get; set; }
        public string H01B { get; set; }
        public string H02A { get; set; }
        public string H02B { get; set; }
        public string H03A { get; set; }
        public string H03B { get; set; }
        public string H04A { get; set; }
        public string H04B { get; set; }
        public string H05A { get; set; }
        public string H05B { get; set; }
        public string H06A { get; set; }
        public string H06B { get; set; }
        public string H07A { get; set; }
        public string H07B { get; set; }
        public string H08A { get; set; }
        public string H08B { get; set; }
        public string H09A { get; set; }
        public string H09B { get; set; }
        public string H10A { get; set; }
        public string H10B { get; set; }
        public string H11A { get; set; }
        public string H11B { get; set; }
        public string H12A { get; set; }
        public string H12B { get; set; }
        public string H13A { get; set; }
        public string H13B { get; set; }
        public string H14A { get; set; }
        public string H14B { get; set; }
        public string H15A { get; set; }
        public string H15B { get; set; }
        public string H16A { get; set; }
        public string H16B { get; set; }
        public string H17A { get; set; }
        public string H17B { get; set; }
        public string H18A { get; set; }
        public string H18B { get; set; }
        public string H19A { get; set; }
        public string H19B { get; set; }
        public string H20A { get; set; }
        public string H20B { get; set; }
        public string H21A { get; set; }
        public string H21B { get; set; }
        public string H22A { get; set; }
        public string H22B { get; set; }
        public string H23A { get; set; }
        public string H23B { get; set; }
        public string H24A { get; set; }
        public string H24B { get; set; }
        public double overtime { get; set; }

        public int CompareTo(object obj)
        {
            return (int)(this.SortOrder - (obj as TableRow).SortOrder);
        }
    }

    public class MonthlyTableRow : TableRow
    {
        public DateTime Date { get; set; }
        public double RestHoursIn24Hrs { get; set; }
        public string Comments { get; set; }
        public double MinRestIn24Hrs { get; set; }
        public double MinRestIn7Days { get; set; }
        public MonthlyTableRow(DateTime date, string h1a, string h1b, string h2a, string h2b, string h3a, string h3b, string h4a, string h4b, string h5a, string h5b, string h6a, string h6b, string h7a, string h7b, string h8a, string h8b, string h9a, string h9b, string h10a, string h10b, string h11a, string h11b, string h12a, string h12b, string h13a, string h13b, string h14a, string h14b, string h15a, string h15b, string h16a, string h16b, string h17a, string h17b, string h18a, string h18b, string h19a, string h19b, string h20a, string h20b, string h21a, string h21b, string h22a, string h22b, string h23a, string h23b, string h24a, string h24b, double rest24,double restin24, double restin7d, string comments )
        {
            Date = date; H01A = h1a; H01B = h1b; ; H02A = h2a; H02B = h2b; H03A = h3a; H03B = h3b; H04A = h4a; H04B = h4b; H05A = h5a; H05B = h5b; H06A = h6a; H06B = h6b; H07A = h7a; H07B = h7b; H08A = h8a; H08B = h8b; H09A = h9a; H09B = h9b; H10A = h10a; H10B = h10b; H11A = h11a; H11B = h11b; H12A = h12a; H12B = h12b; H13A = h13a; H13B = h13b; H14A = h14a; H14B = h14b; H15A = h15a; H15B = h15b; H16A = h16a; H16B = h16b; H17A = h17a; H17B = h17b; H18A = h18a; H18B = h18b; H19A = h19a; H19B = h19b; H20A = h20a; H20B = h20b; H21A = h21a; H21B = h21b; H22A = h22a; H22B = h22b; H23A = h23a; H23B = h23b; H24A = h24a; H24B = h24b; RestHoursIn24Hrs = rest24; MinRestIn24Hrs = restin24; MinRestIn7Days = restin7d; Comments = comments;            
        }

    }
    public class DailyUserPrint
    {
        public DailyUserPrint()
        {
            Name = "";
            Rank = "";
            Overtime = 0;
            watchkeepingPeriods = new List<Period>();
            restTimePeriods = new List<Period>();
            otherWorkPeriods = new List<Period>();
        }
        public string Name{get;set;}
        public string Rank{get;set;}
        public double Overtime{get;set;}

        public List<Period> watchkeepingPeriods{get;set;}
        public List<Period> otherWorkPeriods{get;set;}
        public List<Period> restTimePeriods{get;set;}


        internal int GetRowsNumber()
        {
            int i = 0;
            if (otherWorkPeriods.Count > i) { i = otherWorkPeriods.Count; }
            if (watchkeepingPeriods.Count > i) { i = watchkeepingPeriods.Count; }
            if (restTimePeriods.Count > i) { i = restTimePeriods.Count; }
            return i;
        }
    }

    public class BunkerUserPrint : INotifyPropertyChanged
    {
        public BunkerUserPrint()
        {
            Name = "";
            Rank = "";
            Overtime = 0;
            Overtime1 = 0;
            Overtime2 = 0;
            restTimePeriods = new List<Period>();
            otherWorkPeriods = new List<Period>();
            restTime1Periods = new List<Period>();
            otherWork1Periods = new List<Period>();
            restTime2Periods = new List<Period>();
            otherWork2Periods = new List<Period>();
            Selected = false;            
        }

        public short? SortOrder { get; set; }
        private bool? _sel;
        public bool? Selected { get { return _sel; } set { _sel = value; NotifyPropertyChanged("Selected"); } }

        public string Name { get; set; }
        public string Rank { get; set; }
        public double Overtime { get; set; }
        public double Overtime1 { get; set; }
        public double Overtime2 { get; set; }

        public List<Period> otherWorkPeriods { get; set; }
        public List<Period> restTimePeriods { get; set; }
        public List<Period> otherWork1Periods { get; set; }
        public List<Period> restTime1Periods { get; set; }
        public List<Period> otherWork2Periods { get; set; }
        public List<Period> restTime2Periods { get; set; }

        internal int GetRowsNumber()
        {
            int i = 0;
            if (otherWorkPeriods.Count > i) { i = otherWorkPeriods.Count; }
            if (restTimePeriods.Count > i) { i = restTimePeriods.Count; }
            if (otherWork1Periods.Count > i) { i = otherWork1Periods.Count; }
            if (restTime1Periods.Count > i) { i = restTime1Periods.Count; }
            if (otherWork2Periods.Count > i) { i = otherWork2Periods.Count; }
            if (restTime2Periods.Count > i) { i = restTime2Periods.Count; }
            return i;
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
    public class Period
    {
        public Period(string fm, string to, double hrs)
        {
            From = fm;
            To = to;
            Hours = hrs;
        }

        public Period() { }
        public string From{get;set;}
        public string To{get;set;}
        public double Hours{get;set;}
    }

}
