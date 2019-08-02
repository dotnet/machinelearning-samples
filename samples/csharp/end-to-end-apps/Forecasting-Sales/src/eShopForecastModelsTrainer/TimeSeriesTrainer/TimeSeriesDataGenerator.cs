using System;
using System.Collections.Generic;
using System.Linq;

namespace eShopForecastModelsTrainer
{
    class TimeSeriesDataGenerator
    {
        /// <summary>
        /// Each ProductData object has a value for prev (the previous months units)
        /// and next (the next months units).  This method gets a little clever and
        /// generates two additional ProductData objects (one for the month prior and
        /// one for the month after) the given set of monts
        /// </summary>
        /// <param name="singleProductSeries">The initial 12 months of product data.</param>
        /// <returns></returns>
        public static IEnumerable<ProductData> SupplementDataWithPrevNextMonths(IEnumerable<ProductData> singleProductSeries)
        {
            var supplementedProductSeries = new List<ProductData>(singleProductSeries);

            float randomCountDelta = 4;
            float randomMaxDelta = 10;

            // Get the first month in series
            var firstMonth = singleProductSeries.FirstOrDefault(p => p.year == 2017 && p.month == singleProductSeries.Select(pp => pp.month).Min());

            if (firstMonth != null)
            {
                var month = firstMonth.month == 1 ? 12 : firstMonth.month - 1;

                var poorlyCalculatedCount = MathF.Round(singleProductSeries.Select(p => p.count).Average()) - randomCountDelta;
                var poorlyCalculatedMax = MathF.Round(singleProductSeries.Select(p => p.max).Average()) - randomMaxDelta;
                var poorlyCalculatedMin = MathF.Round(singleProductSeries.Select(p => p.min).Average());

                var previousMonth = new ProductData
                {
                    next = firstMonth.units,
                    productId = firstMonth.productId,
                    year = month == 12 ? 2016 : 2017,
                    month = month,
                    units = firstMonth.prev,
                    avg = MathF.Round(firstMonth.prev / poorlyCalculatedCount),
                    count = poorlyCalculatedCount,
                    max = poorlyCalculatedMax,
                    min = poorlyCalculatedMin,
                    prev = firstMonth.prev - (firstMonth.units - firstMonth.prev) // subtract the delta from the previous month to this month
                };

                supplementedProductSeries.Insert(0, previousMonth);

                Console.WriteLine(previousMonth);
            }
            else
            {
                Console.WriteLine("This really shouldn't ever happen");
            }

            // Get the last month in the series
            var lastMonth = singleProductSeries.FirstOrDefault(p => p.year == 2017 && p.month == singleProductSeries.Select(pp => pp.month).Max());

            if (lastMonth != null)
            {
                var month = lastMonth.month == 12 ? 1 : lastMonth.month + 1;

                var poorlyCalculatedCount = MathF.Round(singleProductSeries.Select(p => p.count).Average()) + randomCountDelta;
                var poorlyCalculatedMax = MathF.Round(singleProductSeries.Select(p => p.max).Average()) + randomMaxDelta;
                var poorlyCalculatedMin = MathF.Round(singleProductSeries.Select(p => p.min).Average());

                var nextMonth = new ProductData
                {
                    next = lastMonth.next + (lastMonth.next - lastMonth.units),  // add the delta from the previous month to this month
                    productId = lastMonth.productId,
                    year = month == 1 ? 2018 : 2017,
                    month = month,
                    units = lastMonth.next,
                    avg = MathF.Round(lastMonth.next / poorlyCalculatedCount),
                    count = poorlyCalculatedCount,
                    max = poorlyCalculatedMax,
                    min = poorlyCalculatedMin,
                    prev = lastMonth.units
                };

                Console.WriteLine(nextMonth);

                supplementedProductSeries.Add(nextMonth);
            }
            else
            {
                Console.WriteLine("This really shouldn't ever happen");
            }

            return supplementedProductSeries;
        }

        /// <summary>
        /// If we have 12 months worth of data, this will suppliment the data with an additional 12
        /// PREVIOUS months based on the growth exponent provided
        /// </summary>
        /// <param name="singleProductSeries">The initial 12 months of product data.</param>
        /// <param name="growth">The amount the values should grow year over year.</param>
        /// <returns></returns>
        public static IEnumerable<ProductData> SupplementDataWithYear(IEnumerable<ProductData> singleProductSeries, float growth = 0.5f)
        {
            if (singleProductSeries.Count() != 12)
            {
                throw new NotImplementedException("fix this, currently only handles if there's already a full 12 months of data.");
            }

            var supplementedProductSeries = new List<ProductData>();

            foreach (var product in singleProductSeries)
            {
                var newUnits = MathF.Floor(product.units * growth);
                var newCount = MathF.Floor(product.count * growth);
                var newMax = MathF.Floor(product.max * growth);
                var newMin = new Random().Next(1, 3);

                var newProduct = new ProductData
                {
                    next = MathF.Floor(product.next * growth),
                    productId = product.productId,
                    year = product.year - 1,
                    month = product.month,
                    units = newUnits,
                    avg = MathF.Round(newUnits / newCount),
                    count = newCount,
                    max = newMax,
                    min = newMin,
                    prev = MathF.Floor(product.prev * growth)
                };

                supplementedProductSeries.Add(newProduct);
            }

            supplementedProductSeries.AddRange(singleProductSeries);

            return supplementedProductSeries;
        }
    }
}
