using Common;
using DatabaseLoaderConsoleApp.DataModels;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace DatabaseLoaderConsoleApp
{
    public class Program
    {  
        public static void Main()
        {
            var mlContext = new MLContext();

            // localdb SQL database connection string using a filepath to attach the database file into localdb
            string dbFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SqlLocalDb", "Criteo-100k-rows.mdf");
            string connectionString = $"Data Source = (LocalDB)\\MSSQLLocalDB;AttachDbFilename={dbFilePath};Database=Criteo-100k-rows;Integrated Security = True";

            // ConnString Example: localdb SQL database connection string for 'localdb default location' (usually files located at /Users/YourUser/)
            //string connectionString = @"Data Source=(localdb)\MSSQLLocalDb;Initial Catalog=YOUR_DATABASE;Integrated Security=True;Pooling=False";
            //
            // ConnString Example: on-premises SQL Server Database (Integrated security)
            //string connectionString = @"Data Source=YOUR_SERVER;Initial Catalog=YOUR_DATABASE;Integrated Security=True;Pooling=False";
            //
            // ConnString Example:  Azure SQL Database connection string
            //string connectionString = @"Server=tcp:yourserver.database.windows.net,1433; Initial Catalog = YOUR_DATABASE; Persist Security Info = False; User ID = YOUR_USER; Password = YOUR_PASSWORD; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 60; ConnectRetryCount = 5; ConnectRetryInterval = 10;";

            string commandText = "SELECT * from URLClicks";

            DatabaseLoader loader = mlContext.Data.CreateDatabaseLoader<UrlClick>();
            
            DatabaseSource dbSource = new DatabaseSource(SqlClientFactory.Instance, 
                                                         connectionString, 
                                                         commandText);
            
            IDataView dataView = loader.Load(dbSource);

            var trainTestData = mlContext.Data.TrainTestSplit(dataView);

            //do the transformation in IDataView
            //Transform categorical features into binary
            var CatogoriesTranformer = mlContext.Transforms.Conversion.ConvertType(nameof(UrlClick.Label), outputKind:Microsoft.ML.Data.DataKind.Boolean).
                Append(mlContext.Transforms.Categorical.OneHotEncoding(new[] {
                new InputOutputColumnPair("Cat14Encoded", "Cat14"),
                new InputOutputColumnPair("Cat15Encoded", "Cat15"),
                new InputOutputColumnPair("Cat16Encoded", "Cat16"),
                new InputOutputColumnPair("Cat17Encoded", "Cat17"),
                new InputOutputColumnPair("Cat18Encoded", "Cat18"),
                new InputOutputColumnPair("Cat19Encoded", "Cat19"),
                new InputOutputColumnPair("Cat20Encoded", "Cat20"),
                new InputOutputColumnPair("Cat21Encoded", "Cat21"),
                new InputOutputColumnPair("Cat22Encoded", "Cat22"),
                new InputOutputColumnPair("Cat23Encoded", "Cat23"),
                new InputOutputColumnPair("Cat24Encoded", "Cat24"),
                new InputOutputColumnPair("Cat25Encoded", "Cat25"),
                new InputOutputColumnPair("Cat26Encoded", "Cat26"),
                new InputOutputColumnPair("Cat27Encoded", "Cat27"),
                new InputOutputColumnPair("Cat28Encoded", "Cat28"),
                new InputOutputColumnPair("Cat29Encoded", "Cat29"),
                new InputOutputColumnPair("Cat30Encoded", "Cat30"),
                new InputOutputColumnPair("Cat31Encoded", "Cat31"),
                new InputOutputColumnPair("Cat32Encoded", "Cat32"),
                new InputOutputColumnPair("Cat33Encoded", "Cat33"),
                new InputOutputColumnPair("Cat34Encoded", "Cat34"),
                new InputOutputColumnPair("Cat35Encoded", "Cat35"),
                new InputOutputColumnPair("Cat36Encoded", "Cat36"),
                new InputOutputColumnPair("Cat37Encoded", "Cat37"),
                new InputOutputColumnPair("Cat38Encoded", "Cat38"),
                new InputOutputColumnPair("Cat39Encoded", "Cat39")
            }, OneHotEncodingEstimator.OutputKind.Binary));
            
            var featuresTransformer = CatogoriesTranformer.Append(
                mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat01Featurized", inputColumnName: nameof(UrlClick.Feat01)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat02Featurized", inputColumnName: nameof(UrlClick.Feat02)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat03Featurized", inputColumnName: nameof(UrlClick.Feat03)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat04Featurized", inputColumnName: nameof(UrlClick.Feat04)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat05Featurized", inputColumnName: nameof(UrlClick.Feat05)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat06Featurized", inputColumnName: nameof(UrlClick.Feat06)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat07Featurized", inputColumnName: nameof(UrlClick.Feat07)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat08Featurized", inputColumnName: nameof(UrlClick.Feat08)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat09Featurized", inputColumnName: nameof(UrlClick.Feat09)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat10Featurized", inputColumnName: nameof(UrlClick.Feat10)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat11Featurized", inputColumnName: nameof(UrlClick.Feat11)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat12Featurized", inputColumnName: nameof(UrlClick.Feat12)))
                .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Feat13Featurized", inputColumnName: nameof(UrlClick.Feat13)));

            var finalTransformerPipeLine = featuresTransformer.Append(mlContext.Transforms.Concatenate("Features",
                            "Feat01Featurized", "Feat02Featurized", "Feat03Featurized", "Feat04Featurized", "Feat05Featurized",
                            "Feat06Featurized", "Feat07Featurized", "Feat08Featurized", "Feat09Featurized", "Feat10Featurized",
                            "Feat11Featurized", "Feat12Featurized", "Feat12Featurized",
                            "Cat14Encoded", "Cat15Encoded", "Cat16Encoded", "Cat17Encoded", "Cat18Encoded", "Cat19Encoded",
                            "Cat20Encoded", "Cat21Encoded", "Cat22Encoded", "Cat23Encoded", "Cat24Encoded", "Cat25Encoded",
                            "Cat26Encoded", "Cat27Encoded", "Cat28Encoded", "Cat29Encoded", "Cat30Encoded", "Cat31Encoded",
                            "Cat32Encoded", "Cat33Encoded", "Cat34Encoded", "Cat35Encoded", "Cat36Encoded", "Cat37Encoded",
                            "Cat38Encoded", "Cat39Encoded"));

            // Apply the ML algorithm
            var trainingPipeLine = finalTransformerPipeLine.Append(mlContext.BinaryClassification.Trainers.FieldAwareFactorizationMachine(labelColumnName: "Label", featureColumnName: "Features"));
            
            Console.WriteLine("Training the ML model while streaming data from a SQL database...");
            Stopwatch watch = new Stopwatch();
            watch.Start();

            var model = trainingPipeLine.Fit(trainTestData.TrainSet);            

            watch.Stop();
            Console.WriteLine("Elapsed time for training the model = {0} seconds", watch.ElapsedMilliseconds/1000);

            Console.WriteLine("Evaluating the model...");
            Stopwatch watch2 = new Stopwatch();
            watch2.Start();

            var predictions = model.Transform(trainTestData.TestSet);            
            // Now that we have the test predictions, calculate the metrics of those predictions and output the results.
            var metrics = mlContext.BinaryClassification.Evaluate(predictions);

            watch2.Stop();
            Console.WriteLine("Elapsed time for evaluating the model = {0} seconds", watch2.ElapsedMilliseconds / 1000);

            ConsoleHelper.PrintBinaryClassificationMetrics("==== Evaluation Metrics training from a Database ====", metrics);

            // 
            Console.WriteLine("Trying a single prediction:");

            var predictionEngine = mlContext.Model.CreatePredictionEngine<UrlClick, ClickPrediction>(model);

            UrlClick sampleData = new UrlClick() { 
                                        Label = String.Empty,
                                        Feat01 = "32", Feat02 = "3", Feat03 = "5", Feat04 = "NULL", Feat05 = "1",
                                        Feat06 = "0", Feat07 = "0", Feat08 = "61", Feat09 = "5", Feat10 = "0",
                                        Feat11 = "1", Feat12 = "3157", Feat13 = "5", 
                                        Cat14 = "e5f3fd8d", Cat15 = "a0aaffa6", Cat16 = "aa15d56f", Cat17 = "da8a3421", 
                                        Cat18 = "cd69f233", Cat19 = "6fcd6dcb", Cat20 = "ab16ed81", Cat21 = "43426c29", 
                                        Cat22 = "1df5e154", Cat23 = "00c5ffb7", Cat24 = "be4ee537", Cat25 = "f3bbfe99",         
                                        Cat26 = "7de9c0a9", Cat27 = "6652dc64", Cat28 = "99eb4e27", Cat29 = "4cdc3efa",                       
                                        Cat30 = "d20856aa", Cat31 = "a1eb1511", Cat32 = "9512c20b", Cat33 = "febfd863", 
                                        Cat34 = "a3323ca1", Cat35 = "c8e1ee56", Cat36 = "1752e9e8", Cat37 = "75350c8a", 
                                        Cat38 = "991321ea", Cat39 = "b757e957" 
                                        };

            var clickPrediction = predictionEngine.Predict(sampleData);

            Console.WriteLine($"Predicted Label: {clickPrediction.PredictedLabel} - Score:{Sigmoid(clickPrediction.Score)}", Color.YellowGreen);
            Console.WriteLine();

            //*** Detach database from localdb only if you used a conn-string with a filepath to attach the database file into localdb ***
            Console.WriteLine("... Detaching database from SQL localdb ...");
            DetachDatabase(connectionString);

            Console.WriteLine("=============== Press any key ===============");
            Console.ReadKey();
        }

        public static float Sigmoid(float x)
        {
            return (float)(100 / (1 + Math.Exp(-x)));
        }

        public static void DetachDatabase(string userConnectionString) //DELETE PARAM *************
        {
            string dbName = string.Empty;
            using (SqlConnection userSqlDatabaseConnection = new SqlConnection(userConnectionString))
            {
                userSqlDatabaseConnection.Open();
                dbName = userSqlDatabaseConnection.Database;
            }

            string masterConnString = $"Data Source = (LocalDB)\\MSSQLLocalDB;Integrated Security = True";
            using (SqlConnection sqlDatabaseConnection = new SqlConnection(masterConnString))
            {
                sqlDatabaseConnection.Open();

                string prepareDbcommandString = $"ALTER DATABASE [{dbName}] SET OFFLINE WITH ROLLBACK IMMEDIATE ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                //(ALTERNATIVE) string prepareDbcommandString = $"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                SqlCommand sqlPrepareCommand = new SqlCommand(prepareDbcommandString, sqlDatabaseConnection);
                sqlPrepareCommand.ExecuteNonQuery();

                string detachCommandString = "sp_detach_db";
                SqlCommand sqlDetachCommand = new SqlCommand(detachCommandString, sqlDatabaseConnection);
                sqlDetachCommand.CommandType = CommandType.StoredProcedure;
                sqlDetachCommand.Parameters.AddWithValue("@dbname", dbName);
                sqlDetachCommand.ExecuteNonQuery();
            }
        }
    }
}