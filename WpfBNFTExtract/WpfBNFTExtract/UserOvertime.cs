using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfBNFTExtract
{
    class UserOvertime : IComparable
    {
        public string Name { get; set; }
        public string Rank { get; set; }
        public int id { get; set; }
        public double totalOvertime { get; set; }
        public short sortOrder { get; set; }
        
        public int CompareTo(object obj)
        {
            return (obj as UserOvertime).sortOrder - this.sortOrder;
        }
    }
}
