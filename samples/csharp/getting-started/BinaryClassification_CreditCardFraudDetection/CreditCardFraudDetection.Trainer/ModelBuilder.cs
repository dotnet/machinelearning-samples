using CreditCardFraudDetection.Common;
using CreditCardFraudDetection.Common.DataModels;

using Microsoft.ML;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.Data.IO;
using static Microsoft.ML.Transforms.Normalizers.NormalizingEstimator;
using static Microsoft.ML.Runtime.Api.GenerateCodeCommand;

using System;
using System.IO;
using System.Linq;
using Microsoft.ML.Runtime;

namespace CreditCardFraudDetection.Trainer
{
    public class ModelBuilder
    {
        private readonly string _assetsPath;
        private readonly string _dataSetFile;
        private readonly string _outputPath;

        private BinaryClassificationContext _context;
        private TextLoader _reader;
        private IDataView _trainData;
        private IDataView _testData;
        private MLContext _mlContext;

        public ModelBuilder(MLContext mlContext, string assetsPath, string dataSetFile)
        {
            _mlContext = mlContext;
            _assetsPath = assetsPath ?? throw new ArgumentNullException(nameof(assetsPath));
            _dataSetFile = dataSetFile ?? throw new ArgumentNullException(nameof(dataSetFile));
            _outputPath = Path.Combine(_assetsPath, "output");
        }


        public ModelBuilder PreProcessData(MLContext mlContext)
        {
             
            (_context, _reader, _trainData, _testData) = PrepareData(_mlContext);

            return this;
        }

        public void TrainFastTreeAndSaveModels( int cvNumFolds = 2, int numLeaves= 20 , int numTrees = 100,
                                                int minDocumentsInLeafs = 10, double learningRate = 0.2,
                                                Action<Arguments> advancedSettings = null)
        {

            //DELETE, code line not used
            //var logMeanVarNormalizer =   new Normalizer(_mlContext, Normalizer.NormalizerMode.MeanVariance ,("Features", "FeaturesNormalizedByMeanVar"));

            //.Append(mlContext.Transforms.Normalize(inputName: "PassengerCount", mode:NormalizerMode.MeanVariance))

            //Create a flexible pipeline (composed by a chain of estimators) for building/traing the model.

            var pipeline = _mlContext.Transforms.Concatenate("Features", new[] { "Amount", "V1", "V2", "V3", "V4", "V5", "V6",
                                                                          "V7", "V8", "V9", "V10", "V11", "V12",
                                                                          "V13", "V14", "V15", "V16", "V17", "V18",
                                                                          "V19", "V20", "V21", "V22", "V23", "V24",
                                                                          "V25", "V26", "V27", "V28" })
                            .Append(_mlContext.Transforms.Normalize(inputName: "Features", outputName: "FeaturesNormalizedByMeanVar", mode: NormalizerMode.MeanVariance))                       
                            .Append(_mlContext.BinaryClassification.Trainers.FastTree(label: "Label", 
                                                                                      features: "Features",
                                                                                      numLeaves: 20,
                                                                                      numTrees: 100,
                                                                                      minDatapointsInLeafs: 10,
                                                                                      learningRate: 0.2));

            var model = pipeline.Fit(_trainData);

            var metrics = _context.Evaluate(model.Transform(_testData), "Label");

            ConsoleHelpers.ConsoleWriteHeader($"Test Metrics:");
            Console.WriteLine("Acuracy: " + metrics.Accuracy);
            metrics.ToConsole();

            model.SaveModel(_mlContext, Path.Combine(_outputPath, "fastTree.zip"));
            Console.WriteLine("Saved model to " + Path.Combine(_outputPath, "fastTree.zip"));
        }

        private (BinaryClassificationContext context, TextLoader, IDataView trainData, IDataView testData) 
                    PrepareData(MLContext mlContext)
        {

            IDataView data = null;
            IDataView trainData = null;
            IDataView testData = null;

            // Step one: read the data as an IDataView.
            // Create the reader: define the data columns 
            // and where to find them in the text file.
            var reader = new TextLoader(mlContext, new TextLoader.Arguments
            {
                Column = new[] {
                    // A boolean column depicting the 'label'.
                    new TextLoader.Column("Label", DataKind.BL, 30),
                    // 29 Features V1..V28 + Amount
                    new TextLoader.Column("V1", DataKind.R4, 1 ),
                    new TextLoader.Column("V2", DataKind.R4, 2 ),
                    new TextLoader.Column("V3", DataKind.R4, 3 ),
                    new TextLoader.Column("V4", DataKind.R4, 4 ),
                    new TextLoader.Column("V5", DataKind.R4, 5 ),
                    new TextLoader.Column("V6", DataKind.R4, 6 ),
                    new TextLoader.Column("V7", DataKind.R4, 7 ),
                    new TextLoader.Column("V8", DataKind.R4, 8 ),
                    new TextLoader.Column("V9", DataKind.R4, 9 ),
                    new TextLoader.Column("V10", DataKind.R4, 10 ),
                    new TextLoader.Column("V11", DataKind.R4, 11 ),
                    new TextLoader.Column("V12", DataKind.R4, 12 ),
                    new TextLoader.Column("V13", DataKind.R4, 13 ),
                    new TextLoader.Column("V14", DataKind.R4, 14 ),
                    new TextLoader.Column("V15", DataKind.R4, 15 ),
                    new TextLoader.Column("V16", DataKind.R4, 16 ),
                    new TextLoader.Column("V17", DataKind.R4, 17 ),
                    new TextLoader.Column("V18", DataKind.R4, 18 ),
                    new TextLoader.Column("V19", DataKind.R4, 19 ),
                    new TextLoader.Column("V20", DataKind.R4, 20 ),
                    new TextLoader.Column("V21", DataKind.R4, 21 ),
                    new TextLoader.Column("V22", DataKind.R4, 22 ),
                    new TextLoader.Column("V23", DataKind.R4, 23 ),
                    new TextLoader.Column("V24", DataKind.R4, 24 ),
                    new TextLoader.Column("V25", DataKind.R4, 25 ),
                    new TextLoader.Column("V26", DataKind.R4, 26 ),
                    new TextLoader.Column("V27", DataKind.R4, 27 ),
                    new TextLoader.Column("V28", DataKind.R4, 28 ),
                    new TextLoader.Column("Amount", DataKind.R4, 29 ),
                },
                // First line of the file is a header, not a data row.
                HasHeader = true,
                Separator = ","
            });


            // We know that this is a Binary Classification task,
            // so we create a Binary Classification context:
            // it will give us the algorithms we need,
            // as well as the evaluation procedure.
            var classification = new BinaryClassificationContext(mlContext);

            if (!File.Exists(Path.Combine(_outputPath, "testData.idv")) &&
                !File.Exists(Path.Combine(_outputPath, "trainData.idv"))){
                // Split the data 80:20 into train and test sets, train and evaluate.

                data = reader.Read(new MultiFileSource(_dataSetFile));
                ConsoleHelpers.ConsoleWriteHeader("Show 4 transactions fraud (true) and 4 transactions not fraud (false) -  (source)");
                ConsoleHelpers.InspectData(mlContext, data, 4);



                // Can't do stratification when column type is a boolean, is this an issue?
                //(trainData, testData) = classification.TrainTestSplit(data, testFraction: 0.2, stratificationColumn: "Label");
                (trainData, testData) = classification.TrainTestSplit(data, testFraction: 0.2);

                // save test split
                IHostEnvironment env = (IHostEnvironment)mlContext;
                using (var ch = env.Start("SaveData"))
                using (var file = env.CreateOutputFile(Path.Combine(_outputPath, "testData.idv")))
                {
                    var saver = new BinarySaver(mlContext, new BinarySaver.Arguments());
                    DataSaverUtils.SaveDataView(ch, saver, testData, file);
                }

                // save train split
                using (var ch = ((IHostEnvironment)env).Start("SaveData"))
                using (var file = env.CreateOutputFile(Path.Combine(_outputPath, "trainData.idv")))
                {
                    var saver = new BinarySaver(mlContext, new BinarySaver.Arguments());
                    DataSaverUtils.SaveDataView(ch, saver, trainData, file);
                }

            }
            else {
                // Load splited data
                var binTrainData = new BinaryLoader(mlContext, new BinaryLoader.Arguments(), new MultiFileSource(Path.Combine(_outputPath, "trainData.idv")));
                var trainRoles = new RoleMappedData(binTrainData, roles: TransactionObservation.Roles());
                trainData = trainRoles.Data;


                var binTestData = new BinaryLoader(mlContext, new BinaryLoader.Arguments(), new MultiFileSource(Path.Combine(_outputPath, "testData.idv")));
                var testRoles = new RoleMappedData(binTestData, roles: TransactionObservation.Roles());
                testData = testRoles.Data;

            }

            ConsoleHelpers.ConsoleWriteHeader("Show 4 transactions fraud (true) and 4 transactions not fraud (false) -  (traindata)");
            ConsoleHelpers.InspectData(mlContext, trainData, 4);

            ConsoleHelpers.ConsoleWriteHeader("Show 4 transactions fraud (true) and 4 transactions not fraud (false) -  (testData)");
            ConsoleHelpers.InspectData(mlContext, testData, 4);

            return (classification, reader, trainData, testData);
        }
    }

}
