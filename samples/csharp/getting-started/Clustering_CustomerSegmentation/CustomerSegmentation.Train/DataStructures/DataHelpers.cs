using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomerSegmentation.DataStructures
{
    public class DataHelpers
    {
        public static IEnumerable<PivotData> PreProcessAndSave(string offersDataLocation, string transactionsDataLocation, string pivotDataLocation)
        {
            var preProcessData = PreProcess(offersDataLocation, transactionsDataLocation);
            PivotData.SaveToCsv(preProcessData, pivotDataLocation);
            return preProcessData;
        }

        public static IEnumerable<PivotData> PreProcess(string offersDataLocation, string transactionsDataLocation)
        {
            Common.ConsoleHelper.ConsoleWriteHeader("Preprocess input files");
            Console.WriteLine($"Offers file: {offersDataLocation}");
            Console.WriteLine($"Transactions file: {transactionsDataLocation}");

            var offers = Offer.ReadFromCsv(offersDataLocation);
            var transactions = Transaction.ReadFromCsv(transactionsDataLocation);

            // inner join datasets
            var clusterData = (from of in offers
                               join tr in transactions on of.OfferId equals tr.OfferId
                               select new
                               {
                                   of.OfferId,
                                   of.Campaign,
                                   of.Discount,
                                   tr.LastName,
                                   of.LastPeak,
                                   of.Minimum,
                                   of.Origin,
                                   of.Varietal,
                                   Count = 1,
                               }).ToArray();

            // pivot table (naive way)
            // based on code from https://stackoverflow.com/a/43091570
            var pivotDataArray =
                (from c in clusterData
                 group c by c.LastName into gcs
                 let lookup = gcs.ToLookup(y => y.OfferId, y => y.Count)
                 select new PivotData()
                 {
                     LastName = gcs.Key,
                     C1 = (float)lookup["1"].Sum(),
                     C2 = (float)lookup["2"].Sum(),
                     C3 = (float)lookup["3"].Sum(),
                     C4 = (float)lookup["4"].Sum(),
                     C5 = (float)lookup["5"].Sum(),
                     C6 = (float)lookup["6"].Sum(),
                     C7 = (float)lookup["7"].Sum(),
                     C8 = (float)lookup["8"].Sum(),
                     C9 = (float)lookup["9"].Sum(),
                     C10 = (float)lookup["10"].Sum(),
                     C11 = (float)lookup["11"].Sum(),
                     C12 = (float)lookup["12"].Sum(),
                     C13 = (float)lookup["13"].Sum(),
                     C14 = (float)lookup["14"].Sum(),
                     C15 = (float)lookup["15"].Sum(),
                     C16 = (float)lookup["16"].Sum(),
                     C17 = (float)lookup["17"].Sum(),
                     C18 = (float)lookup["18"].Sum(),
                     C19 = (float)lookup["19"].Sum(),
                     C20 = (float)lookup["20"].Sum(),
                     C21 = (float)lookup["21"].Sum(),
                     C22 = (float)lookup["22"].Sum(),
                     C23 = (float)lookup["23"].Sum(),
                     C24 = (float)lookup["24"].Sum(),
                     C25 = (float)lookup["25"].Sum(),
                     C26 = (float)lookup["26"].Sum(),
                     C27 = (float)lookup["27"].Sum(),
                     C28 = (float)lookup["28"].Sum(),
                     C29 = (float)lookup["29"].Sum(),
                     C30 = (float)lookup["30"].Sum(),
                     C31 = (float)lookup["31"].Sum(),
                     C32 = (float)lookup["32"].Sum()
                 }).ToArray();

            Console.WriteLine($"Total rows: {pivotDataArray.Length}");

            return pivotDataArray;
        }
    }
}
