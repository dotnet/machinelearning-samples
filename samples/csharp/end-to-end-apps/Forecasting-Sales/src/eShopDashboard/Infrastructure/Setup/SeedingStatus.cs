using System;
using System.Linq;

namespace eShopDashboard.Infrastructure.Setup
{
    public class SeedingStatus
    {
        private readonly bool _needsSeeding;

        public SeedingStatus(bool needsSeeding)
        {
            _needsSeeding = needsSeeding;
        }

        public SeedingStatus(int recordsToLoad)
        {
            if (recordsToLoad <= 0) throw new ArgumentOutOfRangeException(nameof(recordsToLoad), "Must be greater than zero!");

            _needsSeeding = true;
            RecordsToLoad = recordsToLoad;
        }

        public SeedingStatus(params SeedingStatus[] seedingStatuses)
        {
            RecordsToLoad = seedingStatuses.Where(s => s.NeedsSeeding).Sum(s => s.RecordsToLoad);

            _needsSeeding = RecordsToLoad > 0;
        }

        public bool NeedsSeeding => _needsSeeding && RecordsLoaded < RecordsToLoad;

        public int PercentComplete => (int)decimal.Round(RecordsToLoad == 0 ? 100 : RecordsLoaded / (decimal) RecordsToLoad * 100);

        public int RecordsLoaded { get; set; }

        public int RecordsToLoad { get; }

        public void SetAsComplete()
        {
            RecordsLoaded = RecordsToLoad;
        }
    }
}