using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.IO;
using mnist.DataStructures;
using Microsoft.ML.Transforms;

namespace mnist
{
    class Program
    {
        private static string BaseDatasetsRelativePath = @"../../../Data";
        private static string TrianDataRealtivePath = $"{BaseDatasetsRelativePath}/optdigits-train.csv";
        private static string TestDataRealtivePath = $"{BaseDatasetsRelativePath}/optdigits-val.csv";

        private static string TrainDataPath = GetAbsolutePath(TrianDataRealtivePath);
        private static string TestDataPath = GetAbsolutePath(TestDataRealtivePath);

        private static string BaseModelsRelativePath = @"../../../MLModels";
        private static string ModelRelativePath = $"{BaseModelsRelativePath}/Model.zip";

        private static string ModelPath = GetAbsolutePath(ModelRelativePath);

        static void Main(string[] args)
        {
            MLContext mlContext = new MLContext();
            Train(mlContext);
            TestSomePredictions(mlContext);

            Console.WriteLine("Hit any key to finish the app");
            Console.ReadKey();
        }

        public static void Train(MLContext mlContext)
        {
            try
            {
                // STEP 1: Common data loading configuration
                var trainData = mlContext.Data.LoadFromTextFile(path: TrainDataPath,
                        columns : new[]
                        {
                            new TextLoader.Column(nameof(InputData.PixelValues), DataKind.Single, 0, 63),
                            new TextLoader.Column("Number", DataKind.Single, 64)
                        },
                        hasHeader : false,
                        separatorChar : ','
                        );


                var testData = mlContext.Data.LoadFromTextFile(path: TestDataPath,
                        columns: new[]
                        {
                            new TextLoader.Column(nameof(InputData.PixelValues), DataKind.Single, 0, 63),
                            new TextLoader.Column("Number", DataKind.Single, 64)
                        },
                        hasHeader: false,
                        separatorChar: ','
                        );

                // STEP 2: Common data process configuration with pipeline data transformations
                // Use in-memory cache for small/medium datasets to lower training time. Do NOT use it (remove .AppendCacheCheckpoint()) when handling very large datasets.
                var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey("Label", "Number", keyOrdinality: ValueToKeyMappingEstimator.KeyOrdinality.ByValue).
                    Append(mlContext.Transforms.Concatenate("Features", nameof(InputData.PixelValues)).AppendCacheCheckpoint(mlContext));

                // STEP 3: Set the training algorithm, then create and config the modelBuilder
                var trainer = mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName: "Label", featureColumnName: "Features");
                var trainingPipeline = dataProcessPipeline.Append(trainer).Append(mlContext.Transforms.Conversion.MapKeyToValue("Number","Label"));

                // STEP 4: Train the model fitting to the DataSet

                Console.WriteLine("=============== Training the model ===============");
                ITransformer trainedModel = trainingPipeline.Fit(trainData);

                Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
                var predictions = trainedModel.Transform(testData);
                var metrics = mlContext.MulticlassClassification.Evaluate(data:predictions, labelColumnName:"Number", scoreColumnName:"Score");

                Common.ConsoleHelper.PrintMultiClassClassificationMetrics(trainer.ToString(), metrics);

                mlContext.Model.Save(trainedModel, trainData.Schema, ModelPath);

                Console.WriteLine("The model is saved to {0}", ModelPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                //return null;
            }
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }

        private static void TestSomePredictions(MLContext mlContext)
        {
            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model
            var predEngine = mlContext.Model.CreatePredictionEngine<InputData, OutPutData>(trainedModel);

            var resultprediction1 = predEngine.Predict(SampleMNISTData.MNIST1);

            Console.WriteLine($"Actual: 1     Predicted probability:       zero:  {resultprediction1.Score[0]:0.####}");
            Console.WriteLine($"                                           One :  {resultprediction1.Score[1]:0.####}");
            Console.WriteLine($"                                           two:   {resultprediction1.Score[2]:0.####}");
            Console.WriteLine($"                                           three: {resultprediction1.Score[3]:0.####}");
            Console.WriteLine($"                                           four:  {resultprediction1.Score[4]:0.####}");
            Console.WriteLine($"                                           five:  {resultprediction1.Score[5]:0.####}");
            Console.WriteLine($"                                           six:   {resultprediction1.Score[6]:0.####}");
            Console.WriteLine($"                                           seven: {resultprediction1.Score[7]:0.####}");
            Console.WriteLine($"                                           eight: {resultprediction1.Score[8]:0.####}");
            Console.WriteLine($"                                           nine:  {resultprediction1.Score[9]:0.####}");
            Console.WriteLine();

            var resultprediction2 = predEngine.Predict(SampleMNISTData.MNIST2);

            Console.WriteLine($"Actual: 7     Predicted probability:       zero:  {resultprediction2.Score[0]:0.####}");
            Console.WriteLine($"                                           One :  {resultprediction2.Score[1]:0.####}");
            Console.WriteLine($"                                           two:   {resultprediction2.Score[2]:0.####}");
            Console.WriteLine($"                                           three: {resultprediction2.Score[3]:0.####}");
            Console.WriteLine($"                                           four:  {resultprediction2.Score[4]:0.####}");
            Console.WriteLine($"                                           five:  {resultprediction2.Score[5]:0.####}");
            Console.WriteLine($"                                           six:   {resultprediction2.Score[6]:0.####}");
            Console.WriteLine($"                                           seven: {resultprediction2.Score[7]:0.####}");
            Console.WriteLine($"                                           eight: {resultprediction2.Score[8]:0.####}");
            Console.WriteLine($"                                           nine:  {resultprediction2.Score[9]:0.####}");
            Console.WriteLine();

            var resultprediction3 = predEngine.Predict(SampleMNISTData.MNIST3);

            Console.WriteLine($"Actual: 9     Predicted probability:       zero:  {resultprediction3.Score[0]:0.####}");
            Console.WriteLine($"                                           One :  {resultprediction3.Score[1]:0.####}");
            Console.WriteLine($"                                           two:   {resultprediction3.Score[2]:0.####}");
            Console.WriteLine($"                                           three: {resultprediction3.Score[3]:0.####}");
            Console.WriteLine($"                                           four:  {resultprediction3.Score[4]:0.####}");
            Console.WriteLine($"                                           five:  {resultprediction3.Score[5]:0.####}");
            Console.WriteLine($"                                           six:   {resultprediction3.Score[6]:0.####}");
            Console.WriteLine($"                                           seven: {resultprediction3.Score[7]:0.####}");
            Console.WriteLine($"                                           eight: {resultprediction3.Score[8]:0.####}");
            Console.WriteLine($"                                           nine:  {resultprediction3.Score[9]:0.####}");
            Console.WriteLine();
        }
    }
}
