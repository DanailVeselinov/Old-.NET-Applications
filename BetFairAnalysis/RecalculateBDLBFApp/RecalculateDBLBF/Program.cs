using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecalculateDBLBFConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //MultiplyDatabase();
            //MultiplyDatabase();
            DBLBF.BFDBLEntities context = new DBLBF.BFDBLEntities();            
            List<int> eventIdsone = new List<int>();
            List<int> eventIdstwo = new List<int>();
            List<int> eventIdsthree = new List<int>();
            List<int> eventIdsfour = new List<int>();
            List<int> eventIdsfive = new List<int>();
            List<int> eventIdssix = new List<int>();
            foreach (var item in context.PlacedBets)
            {
                try
                {
                    item.sizeMatched = 2;
                    bool contained = false;
                    switch (item.betlineId)
                    {
                        case 1:
                            contained = eventIdsone.Contains((int)item.eventId);
                            break;
                        case 2:
                            contained = eventIdstwo.Contains((int)item.eventId);
                            break;
                        case 3:
                            contained = eventIdsthree.Contains((int)item.eventId);
                            break;
                        case 4:
                            contained = eventIdsfour.Contains((int)item.eventId);
                            break;
                        case 5:
                            contained = eventIdsfive.Contains((int)item.eventId);
                            break;
                        case 6:
                            contained = eventIdssix.Contains((int)item.eventId);
                            break;
                        default:
                            break;
                    }
                    if (item.sortedOrder == 0 | contained | !ItemIsPlayable(item) | item.betlineId == null)
                    {
                        context.PlacedBets.Remove(item);
                        context.SaveChanges();
                    }
                    else
                    {
                        switch (item.betlineId)
                        {
                            case 1:
                                eventIdsone.Add((int)item.eventId);
                                break;
                            case 2:
                                eventIdstwo.Add((int)item.eventId);
                                break;
                            case 3:
                                eventIdsthree.Add((int)item.eventId);
                                break;
                            case 4:
                                eventIdsfour.Add((int)item.eventId);
                                break;
                            case 5:
                                eventIdsfive.Add((int)item.eventId);
                                break;
                            case 6:
                                eventIdssix.Add((int)item.eventId);
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error has Occured.{1} Inner Exception: {0}", e.InnerException, Environment.NewLine);
                }
                
            }
            var lostBets = from b in context.PlacedBets
                           where b.success == (b.Betline.betType == "B" ? false: true) & b.dateSettled != null
                           orderby b.dateSettled
                           select b;
            foreach (var previous in lostBets)
            {
                try
                {
                    var latterBets = from b in context.PlacedBets
                                     where (b.datePlaced > (DateTime)previous.dateSettled) & (b.betlineId == previous.betlineId)
                                     orderby b.datePlaced
                                     select b;
                    if (latterBets.Count() > 0)
                    {
                        var nextBet = latterBets.ToList()[0];
                        if (previous.Betline.betType == "B")
                        {
                            nextBet.sizeMatched += Math.Round(((previous.sizeMatched < 2 ? 2 : previous.sizeMatched) * previous.averagePrice) / (nextBet.averagePrice - 1), 2);
                        }
                        else
                        {
                            var nextSize = (nextBet.sizeMatched > 2 ? nextBet.sizeMatched : 0) + Math.Round((((((previous.sizeMatched < 2 ? 2 : previous.sizeMatched) * previous.averagePrice) / (previous.averagePrice - 1))) * (nextBet.averagePrice - 1)), 2);
                            nextBet.sizeMatched = nextSize < 2 ? 2 : nextSize;
                        }
                        context.SaveChanges();
                    }
                    else
                    {
                        if (previous.Betline.betType == "B")
                        {
                            previous.Betline.profitPerBet = Math.Round(((previous.sizeMatched < 2 ? 2 : previous.sizeMatched) * previous.averagePrice), 2);
                        }
                        else
                        {
                            previous.Betline.profitPerBet = Math.Round((((previous.sizeMatched < 2 ? 2 : previous.sizeMatched) * previous.averagePrice) / previous.averagePrice - 1), 2);
                        }
                        context.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error has Occured.{1} Inner Exception: {0}", e.InnerException, Environment.NewLine);
                }                                
            }
            var zeroSized = from bet in context.PlacedBets
                            where bet.sizeMatched == 0 & bet.Betline != null
                            select bet;
            foreach (var item in zeroSized)
            {
                try
                {
                    if (item.Betline.betType == "B")
                    {
                        item.sizeMatched = (item.Betline.initialProfitPerBet / (item.averagePrice - 1)) < 2 ? 2 : (item.Betline.initialProfitPerBet / (item.averagePrice - 1));
                    }
                    else
                    {
                        item.sizeMatched = (item.Betline.initialProfitPerBet * (item.averagePrice - 1)) < 2 ? 2 : (item.Betline.initialProfitPerBet * (item.averagePrice - 1));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error has Occured.{1} Inner Exception: {0}", e.InnerException, Environment.NewLine);
                }                
            }
            context.SaveChanges();
        }

        private static void MultiplyDatabase()
        {
            DBLBF.BFDBLEntities context = new DBLBF.BFDBLEntities();
            var betsList = context.PlacedBets.ToList<DBLBF.PlacedBet>();
            TimeSpan ts = betsList[betsList.Count - 1].datePlaced - betsList[0].datePlaced;
            Random rnd = new Random();
            foreach (DBLBF.PlacedBet bet in betsList)
            {
                DBLBF.PlacedBet newBet = new DBLBF.PlacedBet();
                newBet.averagePrice = bet.averagePrice;
                newBet.Betline = bet.Betline;
                newBet.betlineId = bet.betlineId;
                double minutesDifference = rnd.Next(-200,200);
                newBet.datePlaced = bet.datePlaced.AddMinutes(minutesDifference).AddDays(ts.Days);
                if (bet.dateSettled.HasValue)
	            {
                    newBet.dateSettled = (DateTime)bet.dateSettled.Value.AddMinutes(minutesDifference).AddDays(ts.Days);
	            }
                newBet.eventId = bet.eventId+newBet.datePlaced.Month;
                newBet.eventType = bet.eventType;
                newBet.marketId = bet.marketId+newBet.datePlaced.Month;
                newBet.marketMenuPath = bet.marketMenuPath;
                newBet.marketName = bet.marketName;
                newBet.resultCode = bet.resultCode;
                newBet.score = bet.score;
                newBet.Selection = bet.Selection;
                newBet.sizeMatched = bet.sizeMatched;
                newBet.sortedOrder = bet.sortedOrder;
                newBet.success = bet.success;
                context.PlacedBets.Add(newBet);
                context.SaveChanges();
            }
        }
        public static bool ItemIsPlayable(DBLBF.PlacedBet item)
        {
            try
            {
                var marketTypes = item.Betline.Filter.marketNames.Trim(':').Split(':').ToList();
                foreach (var marketT in marketTypes)
                {
                    switch (marketT)
                    {
                        case "Match Odds":
                            if (item.sortedOrder.HasValue)
                            {
                                if (!item.Betline.Filter.sortedOrdersMatchOdds.Split(':').ToList().Contains(item.sortedOrder.Value.ToString()))
                                {
                                    return false;
                                }
                            }
                            break;
                        case "Correct Score":
                            if (item.sortedOrder.HasValue)
                            {
                                if (!item.Betline.Filter.sortedOrdersCorrectScore.Split(':').ToList().Contains(item.sortedOrder.Value.ToString()))
                                {
                                    return false;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error has Occured.{1} Inner Exception: {0}", e.InnerException, Environment.NewLine);
                return false;
            }
            
        }
    }
}
