using Microsoft.ML;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace eShopForecast
{
    public class TimeSeriesDataGenerator
    {
        /// <summary>
        /// Supplements the data and returns the orignial list of months with addtional months
        /// prepended to total a full 36 months.
        public static IEnumerable<ProductData> SupplementData(MLContext mlContext, IDataView productDataSeries)
        {
            return SupplementData(mlContext, mlContext.Data.CreateEnumerable<ProductData>(productDataSeries, false));
        }


        /// <summary>
        /// Supplements the data and returns the orignial list of months with addtional months
        /// prepended to total a full 36 months.
        public static IEnumerable<ProductData> SupplementData(MLContext mlContext, IEnumerable<ProductData> singleProductSeries)
        {
            var supplementedProductSeries = new List<ProductData>(singleProductSeries);

            // Get the first month in series
            var firstMonth = singleProductSeries.FirstOrDefault(p => p.year == 2017 && p.month == singleProductSeries.Select(pp => pp.month).Min());

            var referenceMonth = firstMonth;

            float randomCountDelta = 4;
            float randomMaxDelta = 10;

            if (singleProductSeries.Count() < 12)
            {
                var yearDelta = 12 - singleProductSeries.Count();

                for (int i = 1; i <= yearDelta; i++)
                {
                    var month = firstMonth.month - i < 1 ? 12 - MathF.Abs(firstMonth.month - i) : firstMonth.month - 1;

                    var year = month > firstMonth.month ? firstMonth.year - 1 : firstMonth.year;

                    var calculatedCount = MathF.Round(singleProductSeries.Select(p => p.count).Average()) - randomCountDelta;
                    var calculatedMax = MathF.Round(singleProductSeries.Select(p => p.max).Average()) - randomMaxDelta;
                    var calculatedMin = new Random().Next(1, 5);

                    var productData = new ProductData
                    {
                        next = referenceMonth.units,
                        productId = firstMonth.productId,
                        year = year,
                        month = month,
                        units = referenceMonth.prev,
                        avg = MathF.Round(referenceMonth.prev / calculatedCount),
                        count = calculatedCount,
                        max = calculatedMax,
                        min = calculatedMin,
                        prev = referenceMonth.prev - MathF.Round((referenceMonth.units - referenceMonth.prev) / 2) // subtract the delta from the previous month to this month
                    };

                    supplementedProductSeries.Insert(0, productData);

                    referenceMonth = productData;
                }
            }

            return SupplementDataWithYear(SupplementDataWithYear(supplementedProductSeries));
        }

        /// <summary>
        /// If we have 12 months worth of data, this will suppliment the data with an additional 12
        /// PREVIOUS months based on the growth exponent provided
        /// </summary>
        /// <param name="singleProductSeries">The initial 12 months of product data.</param>
        /// <param name="growth">The amount the values should grow year over year.</param>
        /// <returns></returns>
        static IEnumerable<ProductData> SupplementDataWithYear(IEnumerable<ProductData> singleProductSeries, float growth = 0.1f)
        {
            if (singleProductSeries.Count() < 12)
            {
                throw new NotImplementedException("fix this, currently only handles if there's already a full 12 months or more of data.");
            }

            var supplementedProductSeries = new List<ProductData>();

            var growthMultiplier = 1 - growth;

            var firstYear = singleProductSeries.Take(12);

            foreach (var product in firstYear)
            {
                var newUnits = MathF.Floor(product.units * growthMultiplier);
                var newCount = new Random().Next((int)MathF.Floor(product.count * growthMultiplier), (int)product.count);
                var newMax = MathF.Floor(product.max * growthMultiplier);
                var newMin = new Random().Next(1, 4);

                var newProduct = new ProductData
                {
                    next = MathF.Floor(product.next * growthMultiplier),
                    productId = product.productId,
                    year = product.year - 1,
                    month = product.month,
                    units = newUnits,
                    avg = MathF.Round(newUnits / newCount),
                    count = newCount,
                    max = newMax,
                    min = newMin,
                    prev = MathF.Floor(product.prev * growthMultiplier)
                };

                supplementedProductSeries.Add(newProduct);
            }

            supplementedProductSeries.AddRange(singleProductSeries);

            return supplementedProductSeries;
        }
    }
}
