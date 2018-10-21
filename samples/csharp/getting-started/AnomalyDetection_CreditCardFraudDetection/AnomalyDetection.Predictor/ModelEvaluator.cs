using AnomalyDetection.Common;
using AnomalyDetection.Common.DataModels;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Runtime.Data.IO;
using System;
using System.Linq;

namespace AnomalyDetection.Predictor
{
    public class ModelsEvaluator
    {
        private readonly string _modelfile;
        private readonly string _dasetFile;

        public ModelsEvaluator(string modelfile, string dasetFile) {
            _modelfile = modelfile ?? throw new ArgumentNullException(nameof(modelfile));
            _dasetFile = dasetFile ?? throw new ArgumentNullException(nameof(dasetFile));
        }

        public void EvaluateModel(int? seed = 1) {

            var env = new LocalEnvironment(seed);

            var binTestData = new BinaryLoader(env, new BinaryLoader.Arguments(), new MultiFileSource(_dasetFile));
            var testRoles = new RoleMappedData(binTestData, roles: TransactionVectorModel.Roles());
            var dataTest = testRoles.Data;


            ConsoleHelpers.ConsoleWriterSection($"Data from source:");
            ConsoleHelpers.InspectData(env, dataTest);

            ConsoleHelpers.ConsoleWriteHeader($"Predictions from saved model:");
                ITransformer model = env.ReadModel(_modelfile);
                var predictionFunc = model.MakePredictionFunction<TransactionVectorModel, TransactionEstimatorModel>(env);
                ConsoleHelpers.ConsoleWriterSection($"Evaluate Data (should be predicted true):");
                dataTest.AsEnumerable<TransactionVectorModel>(env, reuseRowObject: false)
                        .Where(x => x.Label == true)
                        .Take(4)
                        .Select(testData => testData)
                        .ToList()
                        .ForEach(testData => {
                            predictionFunc.Predict(testData).PrintToConsole();
                        });


                ConsoleHelpers.ConsoleWriterSection($"Evaluate Data (should be predicted false):");
                dataTest.AsEnumerable<TransactionVectorModel>(env, reuseRowObject: false)
                        .Where(x => x.Label == false)
                        .Take(4)
                        .ToList()
                        .ForEach(testData => {
                            predictionFunc.Predict(testData).PrintToConsole();
                        });
        }
     
    }
}
