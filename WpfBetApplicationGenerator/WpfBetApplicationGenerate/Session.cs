using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Data.Entity;
using WpfBetApplication.BFExchangeService;

namespace WpfBetApplication
{
    public static class Session 
    {
        //private static DBLBettingApp.Email email;

        //public static DBLBettingApp.Email Email
        //{
        //    get { return Session.email; }
        //    set { Session.email = value; }
        //}
        private static WpfBetApplication.BettingLine betline = new BettingLine();
        public static WpfBetApplication.BettingLine Betline
        {
            get { return Session.betline; }
            set { Session.betline = value; }
        }
        static DateTime lastSleepDate;

        public static DateTime LastSleepDate
        {
            get { return lastSleepDate; }
            set { lastSleepDate = value; }
        }
        public static Random RND = new Random();
        public static bool Running = true;
        public static DBLBettingApp.BFBDBEntities databaseContext = new DBLBettingApp.BFBDBEntities();
        public static List<BettingLine> activeBettingLines = new List<BettingLine>();
        public static DateTime baseDate = new DateTime(1970, 1, 1, 00, 00, 00, DateTimeKind.Utc);
        private static DateTime dateTimeNow = new DateTime();

        public static DateTime DateTimeNow
        {
            get { return Session.dateTimeNow; }
            set { Session.dateTimeNow = DateTime.Parse(value.ToString(), System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat); }
        }
        public static List<Market> MarketsList = new List<Market>(32);
        
        //working
         //Filling array of marketData to market(Object) and returns bool
        
        
    }
}
