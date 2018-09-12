using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TensorFlowMLNETInceptionv3ModelScoring.ImageData;

namespace TensorFlowMLNETInceptionv3ModelScoring.Model
{
    public class ModelEvaluator
    {
        private readonly string dataLocation;
        private readonly string imagesFolder;
        private readonly string modelLocation;

        public ModelEvaluator(string dataLocation, string imagesFolder, string modelLocation)
        {
            this.dataLocation = dataLocation;
            this.imagesFolder = imagesFolder;
            this.modelLocation = modelLocation;
        }

        public async Task Evaluate()
        {
            var model = await PredictionModel.ReadAsync<ImageNetData, ImageNetPrediction>(modelLocation);
            var predictions = GetPredictions(dataLocation, imagesFolder, model).ToArray();
            ShowMetrics(dataLocation, model);
        }

        protected IEnumerable<ImageNetPrediction> GetPredictions(string testLocation, string imagesFolder, PredictionModel<ImageNetData, ImageNetPrediction> model)
        {
            var testData = ImageNetData.ReadFromCsv(testLocation, imagesFolder);

            foreach (var sample in testData)
            {
                yield return model.Predict(sample);
            }
        }

        protected void ShowMetrics(string testLocation, PredictionModel<ImageNetData, ImageNetPrediction> model)
        {
            var evaluator = new ClassificationEvaluator();
            var testDataSource = new TextLoader(testLocation).CreateFrom<ImageData.ImageNetData>();
            ClassificationMetrics metrics = evaluator.Evaluate(model, testDataSource);
            PrintMetrics(metrics);
        }

        protected static void PrintMetrics(ClassificationMetrics metrics)
        {
            Console.WriteLine($"**************************************************************");
            Console.WriteLine($"*       Metrics for Image Classification          ");
            Console.WriteLine($"*-------------------------------------------------------------");
            Console.WriteLine($"*       Log Loss: {metrics.LogLoss:0.##}");
            Console.WriteLine($"**************************************************************");
        }
    }
}
