using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reviseDatabases
{
    class Program
    {
        static void Main(string[] args)
        {
            BFDBLRealDrawEntities context = new BFDBLRealDrawEntities();
            foreach (var pb in context.PlacedBets)
            {
                if (pb.dateSettled != null)
                {
                    var splitMp = pb.marketMenuPath.Split('/');
                    pb.marketName = splitMp[splitMp.Length - 1].Trim();
                    var teams = splitMp[splitMp.Length - 2].Split('v');
                    if (teams[0].Contains(pb.Selection))
                    {
                        pb.sortedOrder = 1;
                    }
                    else if(teams[1].Contains(pb.Selection))
                    {
                        pb.sortedOrder = 2;
                    }
                    else
                    {
                        pb.sortedOrder = 3;
                    }
                    pb.eventType = 1;
                    if (pb.resultCode == "Settled")
                    {
                        pb.resultCode = "FT";
                    }
                }
                else
                {
                    context.PlacedBets.Remove(pb);
                }
                context.SaveChanges();
            }
        }
    }
}
