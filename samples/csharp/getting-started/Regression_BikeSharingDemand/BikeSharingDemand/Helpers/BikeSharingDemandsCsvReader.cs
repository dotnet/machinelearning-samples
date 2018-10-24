using System.Collections.Generic;
using System.IO;
using System.Linq;
using BikeSharingDemand.BikeSharingDemandData;

namespace BikeSharingDemand.Helpers
{
    public class BikeSharingDemandsCsvReader
    {
        public IEnumerable<BikeSharingDemandSample> GetDataFromCsv(string dataLocation)
        {
            return File.ReadAllLines(dataLocation)
                .Skip(1)
                .Select(x => x.Split(','))
                .Select(x => new BikeSharingDemandSample()
                {
                    Season = float.Parse(x[2]),
                    Year = float.Parse(x[3]),
                    Month = float.Parse(x[4]),
                    Hour = float.Parse(x[5]),
                    Holiday = float.Parse(x[6]),
                    Weekday = float.Parse(x[7]),
                    WorkingDay = float.Parse(x[8]),
                    Weather = float.Parse(x[9]),
                    Temperature = float.Parse(x[10]),
                    NormalizedTemperature = float.Parse(x[11]),
                    Humidity = float.Parse(x[12]),
                    Windspeed = float.Parse(x[13]),
                    Count = float.Parse(x[16])
                });
        }
    }
}
