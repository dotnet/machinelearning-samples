using Microsoft.ML.Runtime.Api;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomerSegmentation.DataStructures
{
    public class Transaction
    {
        //Customer Last Name,Offer #
        //Smith,2
        public string LastName { get; set; }
        public string OfferId { get; set; }

        public static IEnumerable<Transaction> ReadFromCsv(string file)
        {
            return File.ReadAllLines(file)
             .Skip(1) // skip header
             .Select(x => x.Split(','))
             .Select(x => new Transaction()
             {
                 LastName = x[0],
                 OfferId = x[1],
             });
        }
    }
}
