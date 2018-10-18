using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.ML.Legacy;
using Microsoft.ML.Legacy.Models;
using Microsoft.ML.Legacy.Data;
using Microsoft.ML.Legacy.Transforms;
using Microsoft.ML.Legacy.Trainers;

namespace BinaryClasification_TitanicSurvivalPrediction
{

    internal static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string TrainDataPath => Path.Combine(AppPath, "datasets", "titanic-train.csv");
        private static string TestDataPath => Path.Combine(AppPath, "datasets", "titanic-test.csv");
        private static string ModelPath => Path.Combine(AppPath, "TitanicModel.zip");

        private static async Task Main(string[] args)
        {
            // STEP 1: Create a model
            var model = await TrainAsync();

            // STEP2: Test accuracy
            Evaluate(model);

            // STEP 3: Make a prediction
            var prediction = model.Predict(TestTitanicData.Passenger);
            Console.WriteLine($"Did this passenger survive?   Actual: Yes   Predicted: {(prediction.Survived ? "Yes" : "No")} with {prediction.Probability*100}% probability");

            Console.ReadLine();
        }

        public static async Task<PredictionModel<TitanicData, TitanicPrediction>> TrainAsync()
        {
            // LearningPipeline holds all steps of the learning process: data, transforms, learners.  
            var pipeline = new LearningPipeline();

            // The TextLoader loads a dataset. The schema of the dataset is specified by passing a class containing
            // all the column names and their types.
            pipeline.Add(new TextLoader(TrainDataPath).CreateFrom<TitanicData>(useHeader: true, separator: ','));

            // Transform any text feature to numeric values
            pipeline.Add(new CategoricalOneHotVectorizer(
                "Sex",
                "Ticket",
                "Fare",
                "Cabin",
                "Embarked"));

            // Put all features into a vector
            pipeline.Add(new ColumnConcatenator(
                "Features",
                "Pclass",
                "Sex",
                "Age",
                "SibSp",
                "Parch",
                "Ticket",
                "Fare",
                "Cabin",
                "Embarked"));

            // FastTreeBinaryClassifier is an algorithm that will be used to train the model.
            // It has three hyperparameters for tuning decision tree performance. 
            pipeline.Add(new FastTreeBinaryClassifier() {NumLeaves = 5, NumTrees = 5, MinDocumentsInLeafs = 2});

            Console.WriteLine("=============== Training model ===============");
            // The pipeline is trained on the dataset that has been loaded and transformed.
            var model = pipeline.Train<TitanicData, TitanicPrediction>();

            // Saving the model as a .zip file.
            await model.WriteAsync(ModelPath);

            Console.WriteLine("=============== End training ===============");
            Console.WriteLine("The model is saved to {0}", ModelPath);

            return model;
        }

        private static void Evaluate(PredictionModel<TitanicData, TitanicPrediction> model)
        {
            // To evaluate how good the model predicts values, the model is ran against new set
            // of data (test data) that was not involved in training.
            var testData = new TextLoader(TestDataPath).CreateFrom<TitanicData>(useHeader: true, separator: ',');
            
            // BinaryClassificationEvaluator performs evaluation for Binary Classification type of ML problems.
            var evaluator = new BinaryClassificationEvaluator();

            Console.WriteLine("=============== Evaluating model ===============");

            var metrics = evaluator.Evaluate(model, testData);
            // BinaryClassificationMetrics contains the overall metrics computed by binary classification evaluators
            // The Accuracy metric gets the accuracy of a classifier which is the proportion 
            //of correct predictions in the test set.

            // The Auc metric gets the area under the ROC curve.
            // The area under the ROC curve is equal to the probability that the classifier ranks
            // a randomly chosen positive instance higher than a randomly chosen negative one
            // (assuming 'positive' ranks higher than 'negative').

            // The F1Score metric gets the classifier's F1 score.
            // The F1 score is the harmonic mean of precision and recall:
            //  2 * precision * recall / (precision + recall).

            Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"Auc: {metrics.Auc:P2}");
            Console.WriteLine($"F1Score: {metrics.F1Score:P2}");
            Console.WriteLine("=============== End evaluating ===============");
            Console.WriteLine();
        }
    }
}