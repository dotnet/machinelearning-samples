using System;
using Microsoft.ML;
using System.IO;
using System.Linq;
using Microsoft.ML.Data;
using System.Collections.Generic;
using Console = Colorful.Console;
using System.Drawing;
using Microsoft.ML.FactorizationMachine;

namespace MovieRecommenderModel
{
    /* This movie recommendation model is built on the http://files.grouplens.org/datasets/movielens/ml-latest-small.zip dataset
       for improved model performance use the https://grouplens.org/datasets/movielens/1m/ dataset instead. */

    class Program
    {
        private static string TrainingDataLocation = @".\Data\ratings_train.csv";
        private static string TestDataLocation = @".\Data\ratings_test.csv";
        private static string ModelPath = @"..\..\..\Model\model.zip";

        private static string userId = nameof(userId);
        private static string userIdEncoded = nameof(userIdEncoded);

        private static string movieId = nameof(movieId);
        private static string movieIdEncoded = nameof(movieIdEncoded);

        private static string Label = nameof(Label);
        private static string Features = nameof(Features);

        private static string Score = nameof(Score);
        private static string PredictedLabel = nameof(PredictedLabel);

        static void Main(string[] args)
        {
            Color color = Color.FromArgb(130,150,115);

            //Call the following piece of code for splitting the ratings.csv into ratings_train.csv and ratings.test.csv.
            // Program.DataPrep();

            //STEP 1: Create MLContext to be shared across the model creation workflow objects
            MLContext mlContext = new MLContext();

            //STEP 2: Create a TextLoader by defining the schema for reading the movie recommendation datasets
            TextLoader reader = mlContext.Data.CreateTextReader(new[]
                {
                    new TextLoader.Column(userId, DataKind.Text, 0),
                    new TextLoader.Column(movieId, DataKind.Text, 1),
                    new TextLoader.Column(Label, DataKind.R4, 2)
                },
                separatorChar: ',',
                hasHeader:true);

            Console.WriteLine("=============== Reading Input Files ===============", color);
            Console.WriteLine();

            //STEP 3: Read the training data and test data which will be used to train and test the movie recommendation model
            IDataView trainingDataView = reader.Read(TrainingDataLocation);
            IDataView testDataView = reader.Read(TestDataLocation);

            Console.WriteLine("=============== Transform Data And Preview ===============", color);
            Console.WriteLine();

            //STEP 4: Transform your data by encoding the two features userId and movieID.
            //        These encoded features will be provided as input to FieldAwareFactorizationMachine learner

            EstimatorChain<FieldAwareFactorizationMachinePredictionTransformer> pipeline =
                mlContext.Transforms.Categorical.OneHotEncoding(userId, userIdEncoded).
                    Append(mlContext.Transforms.Categorical.OneHotEncoding(movieId, movieIdEncoded)).
                    Append(mlContext.Transforms.Concatenate(Features, userIdEncoded, movieIdEncoded)).
                    Append(mlContext.BinaryClassification.Trainers.FieldAwareFactorizationMachine(featureColumns: new string[] { Features}));

            DataDebuggerPreview preview = pipeline.Preview(trainingDataView, maxRows: 10);

            // STEP 5: Train the model fitting to the DataSet
            Console.WriteLine("=============== Training the model ===============", color);
            Console.WriteLine();

            TransformerChain<FieldAwareFactorizationMachinePredictionTransformer> model = pipeline.Fit(trainingDataView);

            //STEP 6: Evaluate the model performance
            Console.WriteLine("=============== Evaluating the model ===============", color);
            Console.WriteLine();
            IDataView prediction = model.Transform(testDataView);

            CalibratedBinaryClassificationMetrics metrics = mlContext.BinaryClassification.Evaluate(prediction, label: Label, score: Score, predictedLabel: PredictedLabel);
            Console.WriteLine($"Evaluation Metrics: acc:{Math.Round(metrics.Accuracy, 2)} auc:{Math.Round(metrics.Auc, 2)}", Color.YellowGreen);
            Console.WriteLine();

            //STEP 7:  Try/test a single prediction by predicting a single movie rating for a specific user
            Console.WriteLine("=============== Test a single prediction ===============", color);
            Console.WriteLine();
            PredictionEngine<RatingData, RatingPrediction> predictionEngine = model.CreatePredictionEngine<RatingData, RatingPrediction>(mlContext);
            RatingData testData = new RatingData() { userId = "6", movieId = "10" };

            RatingPrediction movieRatingPrediction = predictionEngine.Predict(testData);
            Console.WriteLine($"UserId:{testData.userId} with movieId: {testData.movieId} {movieRatingPrediction.Score}-{movieRatingPrediction.PredictedLabel}", Color.YellowGreen);
            Console.WriteLine();

            //STEP 8:  Save model to disk
            Console.WriteLine("=============== Writing model to disk ===============", color);
            Console.WriteLine();

            using (FileStream fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                mlContext.Model.Save(model, fs);
            }

            Console.WriteLine("Press any key ...");
            Console.Read();
        }

        /*
         * FieldAwareFactorizationMachine the learner used in this example requires the problem to setup as a binary classification problem.
         * The DataPrep method performs two tasks:
         * 1. It goes through all the ratings and replaces the ratings > 3 as 1, suggesting a movie is recommended and ratings < 3 as 0, suggesting
              a movie is not recommended
           2. This piece of code also splits the ratings.csv into rating-train.csv and ratings-test.csv used for model training and testing respectively.
         */
        public static void DataPrep()
        {

            string[] dataset = File.ReadAllLines(@".\Data\ratings.csv");

            string[] new_dataset = new string[dataset.Length];
            new_dataset[0] = dataset[0];
            for (int i = 1; i < dataset.Length; i++)
            {
                string line = dataset[i];
                string[] lineSplit = line.Split(',');
                double rating = Double.Parse(lineSplit[2]);
                rating = rating > 3 ? 1 : 0;
                lineSplit[2] = rating.ToString();
                string new_line = string.Join(',', lineSplit);
                new_dataset[i] = new_line;
            }
            dataset = new_dataset;
            int numLines = dataset.Length;
            IEnumerable<string> body = dataset.Skip(1);
            IEnumerable<string> sorted = body.Select(line => new { SortKey = Int32.Parse(line.Split(',')[3]), Line = line })
                             .OrderBy(x => x.SortKey)
                             .Select(x => x.Line);
            File.WriteAllLines(@"..\..\..\Data\ratings_train.csv", dataset.Take(1).Concat(sorted.Take((int)(numLines * 0.9))));
            File.WriteAllLines(@"..\..\..\Data\ratings_test.csv", dataset.Take(1).Concat(sorted.TakeLast((int)(numLines * 0.1))));
        }
    }


    public class RatingData
    {
        public string userId;

        public string movieId;

        public float Label;
    }

    public class RatingPrediction
    {
        public bool PredictedLabel;

        public float Score;
    }
}
