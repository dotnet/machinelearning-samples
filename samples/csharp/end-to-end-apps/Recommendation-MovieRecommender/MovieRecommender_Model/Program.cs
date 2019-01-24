using System;
using Microsoft.ML.Runtime.Learners;
using Microsoft.ML.Runtime;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Training;
using Microsoft.ML;
using Microsoft.ML.StaticPipe;
using Microsoft.ML.Trainers;
using System.IO;
using System.Linq;
using static Microsoft.ML.Core.Data.SchemaShape;
using Microsoft.ML.Runtime.Api;

namespace MovieRecommenderModel
{
    /* This movie recommendation model is built on the http://files.grouplens.org/datasets/movielens/ml-latest-small.zip dataset
       for improved model performance use the https://grouplens.org/datasets/movielens/1m/ dataset instead. */

    class Program
    {
        private static string TrainingDataLocation = @".\Data\ratings_train.csv";
        private static string TestDataLocation = @".\Data\ratings_test.csv";
        private static string ModelPath = @"..\..\..\Model\model.zip";

        static void Main(string[] args)
        {

            //Call the following piece of code for splitting the ratings.csv into ratings_train.csv and ratings.test.csv.
            // Program.dataprep();

            //STEP 1: Create MLContext to be shared across the model creation workflow objects 
            var ctx = new MLContext();

            //STEP 2: Create a reader by defining the schema for reading the movie recommendation datasets
            var reader = ctx.Data.TextReader(new TextLoader.Arguments()
            {
                Separator = ",",
                HasHeader = true,
                Column = new[]
                {
                    new TextLoader.Column("userId", DataKind.Text, 0),
                    new TextLoader.Column("movieId", DataKind.Text, 1),
                    new TextLoader.Column("Label", DataKind.R4, 2)
                }
            });

            //STEP 3: Read the training data and test data which will be used to train and test the movie recommendation model
            IDataView trainingDataView = reader.Read(TrainingDataLocation);
            IDataView testDataView = reader.Read(TestDataLocation);

            //STEP 4: Transform your data by encoding the two features userId and movieID. 
            //        These encoded features will be provided as input to FieldAwareFactorizationMachine learner
            var pipeline = ctx.Transforms.Categorical.OneHotEncoding("userId", "userIdEncoded").
                                          Append(ctx.Transforms.Categorical.OneHotEncoding("movieId", "movieIdEncoded").
                                          Append(ctx.Transforms.Concatenate("Features", "userIdEncoded", "movieIdEncoded")).
                                          Append(ctx.BinaryClassification.Trainers.FieldAwareFactorizationMachine(label:"Label", features:new string[] {
                                                                                                                                      "Features"})));
            //STEP 5: Train the model fitting to the DataSet  
            Console.WriteLine("=============== Training the model ===============");
            var model = pipeline.Fit(trainingDataView);

            //STEP 6: Evaluate the model performance 
            Console.WriteLine("=============== Evaluating the model ===============");
            var prediction = model.Transform(testDataView);
            var metrics = ctx.BinaryClassification.Evaluate(prediction, label: "Label", score:"Score", predictedLabel:"PredictedLabel");
            Console.WriteLine("Evaluation Metrics: acc:" + Math.Round(metrics.Accuracy, 2) + " auc:" + Math.Round(metrics.Auc,2));
            
            //STEP 7:  Try/test a single prediction by predicting a single movie rating for a specific user
            var predictionengine = model.MakePredictionFunction<RatingData, RatingPrediction>(ctx);
            var movieratingprediction = predictionengine.Predict(
                            new RatingData()
                            {
                                //Example rating prediction for userId = 6, movieId = 10 (GoldenEye)
                                userId = "6",
                                movieId = "10"
                            }
                        );

            //STEP 8:  Save model to disk 
            Console.WriteLine("=============== Writing model to disk ===============");
            using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                ctx.Model.Save(model, fs);
      }

        /*
         * FieldAwareFactorizationMachine the learner used in this example requires the problem to setup as a binary classification problem.
         * The dataprep method performs two tasks:
         * 1. It goes through all the ratings and replaces the ratings > 3 as 1, suggesting a movie is recommended and ratings < 3 as 0, suggesting
              a movie is not recommended
           2. This piece of code also splits the ratings.csv into rating-train.csv and ratings-test.csv used for model training and testing respectively. 
         */
        //public static void dataprep()
        //{
            
        //    string[] dataset = File.ReadAllLines(@".\Data\ratings.csv");

        //    string[] new_dataset = new string[dataset.Length];
        //    new_dataset[0] = dataset[0];
        //    for (var i = 1; i < dataset.Length; i++)
        //    {
        //        var line = dataset[i];
        //        var lineSplit = line.Split(',');
        //        var rating = Double.Parse(lineSplit[2]);
        //        rating = rating > 3 ? 1 : 0;
        //        lineSplit[2] = rating.ToString();
        //        var new_line = string.Join(',', lineSplit);
        //        new_dataset[i] = new_line;

        //    }
        //    dataset = new_dataset;
        //    var numLines = dataset.Length;
        //    var body = dataset.Skip(1);
        //    var sorted = body.Select(line => new { SortKey = Int32.Parse(line.Split(',')[3]), Line = line })
        //                     .OrderBy(x => x.SortKey)
        //                     .Select(x => x.Line);
        //    File.WriteAllLines(@"..\..\..\Data\ratings_train.csv", dataset.Take(1).Concat(sorted.Take((int)(numLines * 0.9))));
        //    File.WriteAllLines(@"..\..\..\Data\ratings_test.csv", dataset.Take(1).Concat(sorted.TakeLast((int)(numLines * 0.1))));
        //}
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
