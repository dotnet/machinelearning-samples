using eShopForecast;

namespace eShopForecastModelsTrainer.Data
{
    public static class SampleProductData
    {
        public static ProductData[] MonthlyData { get; }

        static SampleProductData()
        {
            MonthlyData = new ProductData[] {
                new ProductData()
                {
                    productId = 263,
                    month = 11,
                    year = 2017,
                    avg = 29,
                    max = 221,
                    min = 1,
                    count = 35,
                    prev = 910,
                    units = 551
                },
                 new ProductData()
                {
                    productId = 988,
                    month = 11,
                    year = 2017,
                    avg = 41,
                    max = 225,
                    min = 4,
                    count = 26,
                    prev = 1094,
                    units = 1076
                }
            };
        }
    }
}
