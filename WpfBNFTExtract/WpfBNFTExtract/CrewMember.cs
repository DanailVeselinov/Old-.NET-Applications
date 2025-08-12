using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfBNFTExtract
{
    class CrewMember : IComparable
    {
        private CWCREWSERV crew;
        private CWCREW crewData;
        private CWPRANK crewRank;
        private short? watch;

        public int? ID { get; set; }
        public string Name { get; set; }
        public string Rank { get; set; }
        public DateTime? EmbarkDate { get; set; }
        public DateTime? DisembarkDate { get; set; }

        public short? SortOrder { get; set; }
        public bool Watchkeeper { get; set; }
        
        public CrewMember( int? id , string firsName,string lastName, string rank, DateTime? dateEmb, DateTime? dateDisemb, short? sortOrder, short? watch)
        {
            ID = id;
            Name = string.Format("{1} {0}",firsName,lastName);
            Rank = rank;
            EmbarkDate = dateEmb;
            DisembarkDate = dateDisemb;
            SortOrder = sortOrder;
            Watchkeeper = watch.HasValue?(watch.Value == 1? true : false):false;
        }

        public CrewMember(CWCREWSERV crew, CWCREW crewData, CWPRANK crewRank, short? watch)
        {
            // TODO: Complete member initialization
            if (crew == null)
            {
                return;
            }
            ID = crew.CWCREW_ID;
            if (crewData!=null)
            {
                Name = string.Format("{1} {0}", crewData.FIRSTNAME, crewData.LASTNAME);                
            }
            else
            {
                Name = "Missing Crew Data";
            }
            Rank = crewRank !=null?crewRank.DESCR:"";
            EmbarkDate = crew.DATEE;
            DisembarkDate = crew.DATED;
            SortOrder = crewRank!=null? crewRank.AA:null;
            Watchkeeper = watch.HasValue ? (watch.Value == 1 ? true : false) : false;
        }


        public int CompareTo(object obj)
        {
            var o = obj as CrewMember;
            if (!SortOrder.HasValue)
            {
                return -1;
            }
            if (!o.SortOrder.HasValue)
            {
                return 1;
            }
            return SortOrder.Value.CompareTo(o.SortOrder.Value);
        }
    }
}
