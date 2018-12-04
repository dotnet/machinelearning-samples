using Microsoft.ML;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Trainers;
using System;
using System.Collections.Generic;
using System.IO;

namespace ProductRecommender
{
    class Program
    {
        //1. Do remember to replace amazon0302.txt with dataset from https://snap.stanford.edu/data/amazon0302.html
        //2. Replace column names with ProductID and CoPurchaseProductID. It should look like this:
        //   ProductID	ProductID_Copurchased
        //   0	1
        //   0  2
        private static string TrainingDataLocation = $"./Data/Amazon0302.txt";

        private static string ModelPath = $"./Model/model.zip";

        static void Main(string[] args)
        {
            //STEP 1: Create MLContext to be shared across the model creation workflow objects 
            var ctx = new MLContext();

            //STEP 2: Create a reader by defining the schema for reading the product co-purchase dataset
            //        Do remember to replace amazon0302.txt with dataset from https://snap.stanford.edu/data/amazon0302.html
            var reader = ctx.Data.TextReader(new TextLoader.Arguments()
            {
                Separator = "tab",
                HasHeader = true,
                Column = new[]
{
                    new TextLoader.Column("Label", DataKind.R4, 0),
                    new TextLoader.Column("ProductID", DataKind.U4, new [] { new TextLoader.Range(0) }, new KeyRange(0, 262110)),
                    new TextLoader.Column("CoPurchaseProductID", DataKind.U4, new [] { new TextLoader.Range(1) }, new KeyRange(0, 262110))
                }
            });

            //STEP 3: Read the training data which will be used to train the movie recommendation model
            var traindata = reader.Read(new MultiFileSource(TrainingDataLocation));


            //STEP 4: Your data is already encoded so all you need to do is call the MatrixFactorization Trainer with a few extra hyperparameters
            //        LossFunction, Alpa, Lambda and a few others like K and C as shown below. 
            var est = ctx.Recommendation().Trainers.MatrixFactorization("ProductID", "CoPurchaseProductID", labelColumn: "Label",
                                     advancedSettings: s =>
                                     {
                                         s.LossFunction = MatrixFactorizationTrainer.LossFunctionType.SquareLossOneClass;
                                         s.Alpha = 0.01;
                                         s.Lambda = 0.025;
                                         // For better results use the following parameters
                                         //s.K = 100;
                                         //s.C = 0.00001;
                                     });

            //STEP 5: Train the model fitting to the DataSet
            //Please add Amazon0302.txt dataset from https://snap.stanford.edu/data/amazon0302.html to Data folder if FileNotFoundException is thrown.
            var model = est.Fit(traindata);


            //STEP 6: Create prediction engine and predict the score for Product 63 being co-purchased with Product 3.
            //        The higher the score the higher the probability for this particular productID being co-purchased 
            var predictionengine = model.MakePredictionFunction<ProductEntry, Copurchase_prediction>(ctx);
            var prediction = predictionengine.Predict(
                new ProductEntry()
                {
                    ProductID = 3,
                    CoPurchaseProductID = 63
                });
        }

        public class Copurchase_prediction
        {
            public float Score { get; set; }
        }

        public class ProductEntry
        {
            [KeyType(Contiguous = true, Count = 262111, Min = 0)]
            public uint ProductID { get; set; }

            [KeyType(Contiguous = true, Count = 262111, Min = 0)]
            public uint CoPurchaseProductID { get; set; }
        }
    }
}
