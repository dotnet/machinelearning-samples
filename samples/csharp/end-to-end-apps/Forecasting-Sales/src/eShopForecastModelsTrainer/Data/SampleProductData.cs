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
                    productId = 988,
                    month = 10,
                    year = 2017,
                    avg = 43,
                    max = 220,
                    min = 1,
                    count = 25,
                    prev = 1036,
                    next = 1076,
                    units = 1094
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
