using System;
using System.IO;

using Microsoft.ML.Runtime.Learners;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML;

namespace MulticlassClassification_Iris
{
    public static partial class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string TrainDataPath => Path.Combine(AppPath, "datasets", "iris-train.txt");
        private static string TestDataPath => Path.Combine(AppPath, "datasets", "iris-test.txt");

        private static void Main(string[] args)
        {
            //1. Create ML.NET context/environment
            using (var env = new LocalEnvironment())
            {
                //2. Create DataReader with data schema mapped to file's columns
                var reader = new TextLoader(env,
                                            new TextLoader.Arguments()
                                            {
                                                Separator = "tab",
                                                HasHeader = true,
                                                Column = new[]
                                                {
                                                    new TextLoader.Column("Label", DataKind.R4, 0),
                                                    new TextLoader.Column("SepalLength", DataKind.R4, 1),
                                                    new TextLoader.Column("SepalWidth", DataKind.R4, 2),
                                                    new TextLoader.Column("PetalLength", DataKind.R4, 3),
                                                    new TextLoader.Column("PetalWidth", DataKind.R4, 4),
                                                }
                                            });
                
                //Load training data
                IDataView trainingDataView = reader.Read(new MultiFileSource(TrainDataPath));

                //3.Create a flexible pipeline (composed by a chain of estimators) for creating/traing the model.
                var pipeline = 
                    new ConcatEstimator(env, "Features", new[] { "SepalLength", "SepalWidth", "PetalLength", "PetalWidth" })
                           .Append(new SdcaMultiClassTrainer(env, new SdcaMultiClassTrainer.Arguments(),
                                                                   "Features",
                                                                   "Label"));


                //4. Create and train the model            
                Console.WriteLine("=============== Create and Train the Model ===============");

                var model = pipeline.Fit(trainingDataView);


                Console.WriteLine("=============== End of training ===============");
                Console.WriteLine();


                //5. Evaluate the model and show accuracy stats

                //Load evaluation/test data
                IDataView testDataView = reader.Read(new MultiFileSource(TestDataPath));

                Console.WriteLine("=============== Evaluating Model's accuracy with Test data===============");
                var predictions = model.Transform(testDataView);

                var multiClassificationCtx = new MulticlassClassificationContext(env);
                var metrics = multiClassificationCtx.Evaluate(predictions, "Label");

                Console.WriteLine("Metrics:");
                Console.WriteLine($"    AccuracyMacro = {metrics.AccuracyMacro:0.####}, a value between 0 and 1, the closer to 1, the better");
                Console.WriteLine($"    AccuracyMicro = {metrics.AccuracyMicro:0.####}, a value between 0 and 1, the closer to 1, the better");
                Console.WriteLine($"    LogLoss = {metrics.LogLoss:0.####}, the closer to 0, the better");
                Console.WriteLine($"    LogLoss for class 1 = {metrics.PerClassLogLoss[0]:0.####}, the closer to 0, the better");
                Console.WriteLine($"    LogLoss for class 2 = {metrics.PerClassLogLoss[1]:0.####}, the closer to 0, the better");
                Console.WriteLine($"    LogLoss for class 3 = {metrics.PerClassLogLoss[2]:0.####}, the closer to 0, the better");
                Console.WriteLine();


                //6. Test Sentiment Prediction with one sample text 
                var predictionFunct = model.MakePredictionFunction<IrisData, IrisPrediction>(env);



                var prediction = predictionFunct.Predict(TestIrisData.Iris1);
                Console.WriteLine($"Actual: setosa.     Predicted probability: setosa:      {prediction.Score[0]:0.####}");
                Console.WriteLine($"                                           versicolor:  {prediction.Score[1]:0.####}");
                Console.WriteLine($"                                           virginica:   {prediction.Score[2]:0.####}");
                Console.WriteLine();


                prediction = predictionFunct.Predict(TestIrisData.Iris2);
                Console.WriteLine($"Actual: virginica.  Predicted probability: setosa:      {prediction.Score[0]:0.####}");
                Console.WriteLine($"                                           versicolor:  {prediction.Score[1]:0.####}");
                Console.WriteLine($"                                           virginica:   {prediction.Score[2]:0.####}");
                Console.WriteLine();


                prediction = predictionFunct.Predict(TestIrisData.Iris3);
                Console.WriteLine($"Actual: versicolor. Predicted probability: setosa:      {prediction.Score[0]:0.####}");
                Console.WriteLine($"                                           versicolor:  {prediction.Score[1]:0.####}");
                Console.WriteLine($"                                           virginica:   {prediction.Score[2]:0.####}");
                Console.WriteLine();
            }
        }
    }
}