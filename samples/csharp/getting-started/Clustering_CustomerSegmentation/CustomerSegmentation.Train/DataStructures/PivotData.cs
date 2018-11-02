using CustomerSegmentation.Model;
using Microsoft.ML.Runtime.Api;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomerSegmentation.DataStructures
{
    public class PivotData
    {
        public float C1 { get; set; }
        public float C2 { get; set; }
        public float C3 { get; set; }
        public float C4 { get; set; }
        public float C5 { get; set; }
        public float C6 { get; set; }
        public float C7 { get; set; }
        public float C8 { get; set; }
        public float C9 { get; set; }
        public float C10 { get; set; }
        public float C11 { get; set; }
        public float C12 { get; set; }
        public float C13 { get; set; }
        public float C14 { get; set; }
        public float C15 { get; set; }
        public float C16 { get; set; }
        public float C17 { get; set; }
        public float C18 { get; set; }
        public float C19 { get; set; }
        public float C20 { get; set; }
        public float C21 { get; set; }
        public float C22 { get; set; }
        public float C23 { get; set; }
        public float C24 { get; set; }
        public float C25 { get; set; }
        public float C26 { get; set; }
        public float C27 { get; set; }
        public float C28 { get; set; }
        public float C29 { get; set; }
        public float C30 { get; set; }
        public float C31 { get; set; }
        public float C32 { get; set; }
        public string LastName { get; set; }

        public override string ToString()
        {
            return        $"{C1},{C2},{C3},{C4},{C5},{C6},{C7},{C8},{C9}," +
                   $"{C10},{C11},{C12},{C13},{C14},{C15},{C16},{C17},{C18},{C19}," +
                   $"{C20},{C21},{C22},{C23},{C24},{C25},{C26},{C27},{C28},{C29}," +
                   $"{C30},{C31},{C32},{LastName}";
        }

        public static void SaveToCsv(IEnumerable<PivotData> salesData, string file)
        {
            var columns = "C1,C2,C3,C4,C5,C6,C7,C8,C9," +
                          "C10,C11,C12,C13,C14,C15,C16,C17,C18,C19," +
                          "C20,C21,C22,C23,C24,C25,C26,C27,C28,C29," +
                          $"C30,C31,C32,{nameof(LastName)}";

            File.WriteAllLines(file, salesData
                .Select(s => s.ToString())
                .Prepend(columns));
        }
    }
}
