using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Common;

namespace CustomerSegmentation.DataStructures
{
    public class Offer
    {
        //Offer #,Campaign,Varietal,Minimum Qty (kg),Discount (%),Origin,Past Peak
        public string OfferId { get; set; }
        public string Campaign { get; set; }
        public string Varietal { get; set; }
        public float Minimum { get; set; }
        public float Discount { get; set; }
        public string Origin { get; set; }
        public string LastPeak { get; set; }

        public static IEnumerable<Offer> ReadFromCsv(string file)
        {
            return File.ReadAllLines(file)
             .Skip(1) // skip header
             .Select(x => x.Split(','))
             .Select(x => new Offer()
             {
                 OfferId = x[0],
                 Campaign = x[1],
                 Varietal = x[2],
                 Minimum = x[3].ToFloatWithInvariantCulture(),
                 Discount = x[4].ToFloatWithInvariantCulture(),
                 Origin = x[5],
                 LastPeak = x[6]
             });
        }
    }
}
