using Common;
using Microsoft.ML;
using System;
using System.Threading;
using System.Threading.Tasks;

using TestObjectPoolingConsoleApp.DataStructures;

namespace TestObjectPoolingConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            // Create an opportunity for the user to cancel.
            Task.Run(() =>
            {
                if (Console.ReadKey().KeyChar == 'c' || Console.ReadKey().KeyChar == 'C')
                    cts.Cancel();
            });

            MLContext mlContext = new MLContext(seed:1);
            string modelFolder = $"Forecast/ModelFiles";
            string modelFilePathName = $"ModelFiles/country_month_fastTreeTweedie.zip";
            var countrySalesModel = new MLModelEngine<CountryData, CountrySalesPrediction>(mlContext, 
                                                                                           modelFilePathName,
                                                                                           minPredictionEngineObjectsInPool: 50,
                                                                                           maxPredictionEngineObjectsInPool: 2000,
                                                                                           expirationTime:30000);

            Console.WriteLine("Current number of objects in pool: {0:####.####}", countrySalesModel.CurrentPredictionEnginePoolSize);

            //Single Prediction
            var singleCountrySample = new CountryData("Australia", 2017, 1, 477, 164, 2486, 9, 10345, 281, 1029);
            var singleNextMonthPrediction = countrySalesModel.Predict(singleCountrySample);

            Console.WriteLine("Prediction: {0:####.####}", singleNextMonthPrediction.Score);

            // Create a high demand for the modelEngine objects.
            Parallel.For(0, 1000000, (i, loopState) =>
            {
                //Sample country data
                //next,country,year,month,max,min,std,count,sales,med,prev
                //4.23056080166201,Australia,2017,1,477.34,164.916,2486.1346772137,9,10345.71,281.7,1029.11

                var countrySample = new CountryData("Australia", 2017, 1, 477, 164, 2486, 9, 10345, 281, i);

                // This is the bottleneck in our application. All threads in this loop
                // must serialize their access to the static Console class.
                Console.CursorLeft = 0;
                var nextMonthPrediction = countrySalesModel.Predict(countrySample);

                //(Wait for a 1/10 second)
                //System.Threading.Thread.Sleep(1000);

                Console.WriteLine("Prediction: {0:####.####}", nextMonthPrediction.Score);
                Console.WriteLine("-----------------------------------------");
                Console.WriteLine("Current number of objects in pool: {0:####.####}", countrySalesModel.CurrentPredictionEnginePoolSize);

                if (cts.Token.IsCancellationRequested)
                    loopState.Stop();

            });

            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("Current number of objects in pool: {0:####.####}", countrySalesModel.CurrentPredictionEnginePoolSize);


            Console.WriteLine("Press the Enter key to exit.");
            Console.ReadLine();
            cts.Dispose();
        }

    }


}
