using System;
using System.Collections.Generic;
using System.Text;

namespace eShopForecastModelsTrainer.Data
{
    public static class SampleProductData
    {
        public static ProductData[] Product1MonthlyData { get; }

        public static ProductData[] Product2MonthlyData { get;  }

        static SampleProductData()
        {
            Product1MonthlyData = new ProductData[] {
                new ProductData()
                {
                    productId = 263,
                    month = 10,
                    year = 2017,
                    avg = 91,
                    max = 370,
                    min = 1,
                    count = 10,
                    prev = 1675,
                    units = 910
                },
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
                }
            };

            Product2MonthlyData = new ProductData[]
            {
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
