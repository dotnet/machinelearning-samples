using System;
using System.Threading.Tasks;
using TensorFlowMLNETInceptionv3ModelScoring.Model;

namespace TensorFlowMLNETInceptionv3ModelScoring
{
    // IMPORTANT: This sample, needs ML.NET 0.6 NuGet packages, due to some bug fixes that happened after 0.5 release.
    // For now, you can get it from MyGet in this Feed: https://dotnet.myget.org/F/dotnet-core/api/v3/index.json
    //
    // - TensorFlow https://www.tensorflow.org/ is a popular machine learning toolkit that enables training deep neural networks(and general numeric computations).
    // - This sample show the usage of the ML.NET "TensorFlowScorer" transform that enables taking an existing TensorFlow model, either trained by you 
    //   or downloaded from somewhere else, and get the scores from the TensorFlow model in ML.NET code.
    // - For now (using LearningPipeline API), these scores can only be used within a LearningPipeline as inputs to a learner.
    //   However, with the upcoming ML.NET APIs, the scores from the TensorFlow model will be directly accessible.
    // - The implementation of this mentioned "TensorFlowScorer" transform is based on code from TensorFlowSharp.
    //
    // Sample code: Specifically, this sample code when training with the pipeline, it generates a numeric vector for each image that you have in the folder "images" 
    // and correlates those numeric vectors with the types of objects/things provided in the "tags.tsv" file. 
    // After that, the model is trained with an SDCA classifier (StochasticDualCoordinateAscentClassifier) that uses that relationship between numeric vectors and labels/tags. 
    // so when using the model in a test or final app, you can classify any given image that is similar to any of the images used in the pipeline.
    public class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var modelBuilder = new ModelTrainer(
                                           ModelHelpers.GetAssetsPath("data", "tags.tsv"),
                                           ModelHelpers.GetAssetsPath("images"),
                                           ModelHelpers.GetAssetsPath("model", "tensorflow_inception_graph.pb"),
                                           ModelHelpers.GetAssetsPath("model", "imageClassifier.zip"));

                await modelBuilder.BuildAndTrain();

                var modelEvaluator = new ModelEvaluator(
                                           ModelHelpers.GetAssetsPath("data", "tags.tsv"),
                                           ModelHelpers.GetAssetsPath("images"),
                                           ModelHelpers.GetAssetsPath("model", "imageClassifier.zip"));

                await modelEvaluator.Evaluate();

            } catch (Exception ex)
            {
                Console.WriteLine("InnerException: {0}", ex.InnerException.ToString());
                throw;
            }

            Console.WriteLine("End of process");
            Console.ReadKey();
        }
    }
}
