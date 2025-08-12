using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfBNFTExtract
{
    public class Server
    {
        public Server( string s , string i, string v) 
        {
            ServerName = s;
            InstanceName = i;
            Version = v;
        }
        public string ServerName { get; set; }
        public string InstanceName { get; set; }
        public string Version { get; set; }

    }
}
