using System;
using System.IO;
using Microsoft.ML.Runtime.Learners;
using Microsoft.ML.Runtime;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Training;
using Microsoft.ML;
using Microsoft.ML.StaticPipe;
using Microsoft.ML.Trainers;

namespace BinaryClassification_SentimentAnalysis
{

    internal static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string TrainDataPath => Path.Combine(AppPath, "datasets", "wikipedia-detox-250-line-data.tsv");
        private static string TestDataPath => Path.Combine(AppPath, "datasets", "wikipedia-detox-250-line-test.tsv");
        private static string ModelPath => Path.Combine(AppPath, "SentimentModel.zip");


        static void Main(string[] args)
        {
            //1. Create ML.NET context/environment
            var env = new LocalEnvironment();

            //2. Create DataReader with data schema mapped to file's columns
            var reader = TextLoader.CreateReader(env, ctx => (label: ctx.LoadBool(0),
                                                              text: ctx.LoadText(1)));

            //3. Create an estimator to use afterwards for creating/traing the model.

            var bctx = new BinaryClassificationContext(env);

            var est = reader.MakeNewEstimator().Append(row => (label: row.label,
                                                               text: row.text.FeaturizeText()))  //Convert text to numeric vectors 
                                               //Specify SDCA trainer based on the 'label' column
                                               .Append(row => (label: row.label,
                                                               prediction: bctx.Trainers.Sdca(row.label, row.text)))
                                               //Specify 'predictedlabel' as the predicted value
                                               .Append(row => (label: row.label,
                                                               prediction: row.prediction,
                                                               predictedlabel: row.prediction.predictedLabel));

            //4. Build and train the model

            //Load training data
            var traindata = reader.Read(new MultiFileSource(TrainDataPath));

            Console.WriteLine("=============== Create and Train the Model ===============");
            var model = est.Fit(traindata);
            Console.WriteLine("=============== End of training ===============");
            Console.WriteLine();


            //5. Evaluate the model
            
            //Load test data
            var testdata = reader.Read(new MultiFileSource(TestDataPath));

            Console.WriteLine("=============== Evaluating Model's accuracy with Test data===============");
            var predictions = model.Transform(testdata);
            var metrics = bctx.Evaluate(predictions, row => row.label, row => row.prediction);

            Console.WriteLine();
            Console.WriteLine("PredictionModel quality metrics evaluation");
            Console.WriteLine("------------------------------------------");
            Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"Auc: {metrics.Auc:P2}");
            Console.WriteLine($"F1Score: {metrics.F1Score:P2}");
            Console.WriteLine("=============== End of Model's evaluation ===============");
            Console.WriteLine();

            //6. Test Sentiment Prediction with one sample text 
            var pdf = model.AsDynamic.MakePredictionFunction<SentimentIssue, SentimentPrediction>(env);

            SentimentIssue textToTest = new SentimentIssue
                                        {
                                            text = "This is a very rude movie"
                                        };

            var resultprediction = pdf.Predict(textToTest);

            Console.WriteLine();
            Console.WriteLine("=============== Test of model with a sample ===============");
            Console.WriteLine($"Text: {textToTest.text} | Prediction: {(resultprediction.predictedlabel ? "Positive" : "Negative")} sentiment");

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }


        //private static async Task Main(string[] args)
        //{
        //    // STEP 1: Create a model
        //    var model = await TrainAsync();

        //    // STEP2: Test accuracy
        //    Evaluate(model);

        //    // STEP 3: Make a prediction
        //    var predictions = model.Predict(TestSentimentData.Sentiments);

        //    var sentimentsAndPredictions =
        //        TestSentimentData.Sentiments.Zip(predictions, (sentiment, prediction) => (sentiment, prediction));
        //    foreach (var item in sentimentsAndPredictions)
        //    {
        //        Console.WriteLine(
        //            $"Sentiment: {item.sentiment.SentimentText} | Prediction: {(item.prediction.Sentiment ? "Positive" : "Negative")} sentiment");
        //    }

        //    Console.ReadLine();
        //}

        //public static async Task<PredictionModel<SentimentData, SentimentPrediction>> TrainAsync()
        //{
        //    // LearningPipeline holds all steps of the learning process: data, transforms, learners.  
        //    var pipeline = new LearningPipeline();

        //    // The TextLoader loads a dataset. The schema of the dataset is specified by passing a class containing
        //    // all the column names and their types.
        //    pipeline.Add(new TextLoader(TrainDataPath).CreateFrom<SentimentData>());

        //    // TextFeaturizer is a transform that will be used to featurize an input column to format and clean the data.
        //    pipeline.Add(new TextFeaturizer("Features", "SentimentText"));

        //    // FastTreeBinaryClassifier is an algorithm that will be used to train the model.
        //    // It has three hyperparameters for tuning decision tree performance. 
        //    pipeline.Add(new FastTreeBinaryClassifier() {NumLeaves = 5, NumTrees = 5, MinDocumentsInLeafs = 2});

        //    Console.WriteLine("=============== Training model ===============");
        //    // The pipeline is trained on the dataset that has been loaded and transformed.
        //    var model = pipeline.Train<SentimentData, SentimentPrediction>();

        //    // Saving the model as a .zip file.
        //    await model.WriteAsync(ModelPath);

        //    Console.WriteLine("=============== End training ===============");
        //    Console.WriteLine("The model is saved to {0}", ModelPath);

        //    return model;
        //}

        //private static void Evaluate(PredictionModel<SentimentData, SentimentPrediction> model)
        //{
        //    // To evaluate how good the model predicts values, the model is ran against new set
        //    // of data (test data) that was not involved in training.
        //    var testData = new TextLoader(TestDataPath).CreateFrom<SentimentData>();

        //    // BinaryClassificationEvaluator performs evaluation for Binary Classification type of ML problems.
        //    var evaluator = new BinaryClassificationEvaluator();

        //    Console.WriteLine("=============== Evaluating model ===============");

        //    var metrics = evaluator.Evaluate(model, testData);
        //    // BinaryClassificationMetrics contains the overall metrics computed by binary classification evaluators
        //    // The Accuracy metric gets the accuracy of a classifier which is the proportion 
        //    //of correct predictions in the test set.

        //    // The Auc metric gets the area under the ROC curve.
        //    // The area under the ROC curve is equal to the probability that the classifier ranks
        //    // a randomly chosen positive instance higher than a randomly chosen negative one
        //    // (assuming 'positive' ranks higher than 'negative').

        //    // The F1Score metric gets the classifier's F1 score.
        //    // The F1 score is the harmonic mean of precision and recall:
        //    //  2 * precision * recall / (precision + recall).

        //    Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
        //    Console.WriteLine($"Auc: {metrics.Auc:P2}");
        //    Console.WriteLine($"F1Score: {metrics.F1Score:P2}");
        //    Console.WriteLine("=============== End evaluating ===============");
        //    Console.WriteLine();
        //}
    }
}