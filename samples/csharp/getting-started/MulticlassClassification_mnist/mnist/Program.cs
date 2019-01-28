using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Data;
using System;
using System.IO;
using mnist.DataStructures;



namespace mnist
{
    class Program
    {

        static readonly string TrainDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "optdigits-train.csv");
        static readonly string ValidationDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "optdigits-val.csv");
        static readonly string ModelPath = Path.Combine(Environment.CurrentDirectory, "MLModels", "Model.zip");

        static void Main(string[] args)
        {
            var env = new MLContext();
            Train(env);
            TestSomePredictions(env);
        }

        public static void Train(MLContext env)
        {
            try
            {
                var classification = new MulticlassClassificationContext(env);

                // STEP 1: Common data loading configuration
                var reader = env.Data.CreateTextReader(
                    new TextLoader.Arguments()
                    {
                        Column = new[] 
                        {
                            new TextLoader.Column("PixelValues", DataKind.R4, 0, 63),
                            new TextLoader.Column("Number", DataKind.R4, 64)
                        },
                        Separator = ",",
                        HasHeader = false
                    });

                var data = reader.Read(TrainDataPath);
                var testData = reader.Read(ValidationDataPath);

                // STEP 2: Common data process configuration with pipeline data transformations
                var dataProcessPipeline = env.Transforms.Concatenate("Features", "PixelValues").AppendCacheCheckpoint(env);

                // STEP 3: Set the training algorithm, then create and config the modelBuilder
                var trainer = env.MulticlassClassification.Trainers.StochasticDualCoordinateAscent(labelColumn: "Number", featureColumn: "Features");
                var trainingPipeline = dataProcessPipeline.Append(trainer);

                // STEP 4: Train the model fitting to the DataSet
                var watch = System.Diagnostics.Stopwatch.StartNew();
                Console.WriteLine("=============== Training the model ===============");

                ITransformer trainedModel = trainingPipeline.Fit(data);
                long elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine($"***** Training time: {elapsedMs / 1000} seconds *****");

                Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
                var predictions = trainedModel.Transform(testData);
                var metrics = env.MulticlassClassification.Evaluate(predictions, "Number", "Score");

                using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                    env.Model.Save(trainedModel, fs);

                Console.WriteLine("The model is saved to {0}", ModelPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //return null;
            }
        }


        private static void TestSomePredictions(MLContext mlContext)
        {
            ITransformer trainedModel;
            using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = mlContext.Model.Load(stream);
            }

            // Create prediction engine related to the loaded trained model
            var predEngine = trainedModel.CreatePredictionEngine<InputData, OutPutNum>(mlContext);

            //InputData data1 = SampleMNISTData.MNIST1;
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

            //InputData data2 = SampleMNISTData.MNIST2;
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

            //InputData data3 = SampleMNISTData.MNIST3;
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
