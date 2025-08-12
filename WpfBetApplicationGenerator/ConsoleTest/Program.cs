using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfBetApplication;

namespace ConsoleTest
{
    class Program
    {
        public static void newList(List<int> list, int number)
        {
            List<int> secondList = new List<int>();
            foreach (var item in list)
            {
                if (item<number)
                {
                    secondList.Add(item);
                }
            }
            list = secondList;
            foreach (int integer in list)
            {

                Console.WriteLine(integer);
            }
        }
        static void Main(string[] args)
        {
            

        }
        
    }
}
