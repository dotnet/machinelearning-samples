using Microsoft.ML;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.TensorFlow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TensorFlowMLNETInceptionv3ModelScoring.ImageData;

namespace TensorFlowMLNETInceptionv3ModelScoring.Model
{
    public class ModelTrainer
    {
        private readonly string dataLocation;
        private readonly string imagesFolder;
        private readonly string inputModelLocation;
        private readonly string outputModelLocation;

        public ModelTrainer(string dataLocation, string imagesFolder, string inputModelLocation, string outputModelLocation)
        {
            this.dataLocation = dataLocation;
            this.imagesFolder = imagesFolder;
            this.inputModelLocation = inputModelLocation;
            this.outputModelLocation = outputModelLocation;
        }

        private struct ImageNetSettings
        {
            public const int imageHeight = 224;
            public const int imageWidth = 224;
            public const float mean = 117;
            public const float scale = 1;
            public const bool channelsLast = true;
        }

        private struct InceptionSettings
        {
            // for checking tensor names, you can use tools like Netron,
            // which is installed by Visual Studio AI Tools

            // input tensor name
            public const string inputTensorName = "input";

            // output tensor name: in this case, is the node before the last one (not softmax, but softmax2_pre_activation).
            public const string outputTensorName = "softmax2_pre_activation";
        }

        public async Task BuildAndTrain()
        {
            var learningPipeline = BuildModel(dataLocation, imagesFolder, inputModelLocation);
            var model = Train(learningPipeline);
            var predictions = GetPredictions(dataLocation, imagesFolder, model).ToArray();
            ShowPredictions(predictions);

            if (outputModelLocation != null && outputModelLocation.Length > 0)
            {
                ModelHelpers.DeleteAssets(outputModelLocation);
                await model.WriteAsync(outputModelLocation);
            }
        }

        protected PredictionModel<ImageNetData, ImageNetPrediction> Train(LearningPipeline pipeline)
        {
            // Initialize TensorFlow engine
            TensorFlowUtils.Initialize();

            var model = pipeline.Train<ImageNetData, ImageNetPrediction>();
            return model;
        }

        protected LearningPipeline BuildModel(string dataLocation, string imagesFolder, string modelLocation)
        {
            const bool convertPixelsToFloat = true;
            const bool ignoreAlphaChannel = false;

            var pipeline = new LearningPipeline();

            // TextLoader loads tsv file, containing image file location and label 
            pipeline.Add(new Microsoft.ML.Data.TextLoader(dataLocation).CreateFrom<ImageNetData>(useHeader: false));

            // ImageLoader reads input images
            pipeline.Add(new ImageLoader((nameof(ImageNetData.ImagePath), "ImageReal"))
            {
                ImageFolder = imagesFolder
            });

            // ImageResizer is used to resize input image files
            // to the size used by the Neural Network
            pipeline.Add(new ImageResizer(("ImageReal", "ImageCropped"))
            {
                ImageHeight = ImageNetSettings.imageHeight,
                ImageWidth = ImageNetSettings.imageWidth,
                Resizing = ImageResizerTransformResizingKind.IsoCrop
            });

            // ImagePixelExtractor is used to process the input image files
            // according to the requirements of the Deep Neural Network
            // This step is the perfect place to make specific image transformations,
            // like normalizing pixel values (Pixel * scale / offset). 
            // This kind of image pre-processing is common when dealing with images used in DNN
            pipeline.Add(new ImagePixelExtractor(("ImageCropped", InceptionSettings.inputTensorName))
            {
                UseAlpha = ignoreAlphaChannel, // channel = (red, green, blue)
                InterleaveArgb = ImageNetSettings.channelsLast, // (width x height x channel)
                Convert = convertPixelsToFloat,
                Offset = ImageNetSettings.mean, // pixel normalization
                Scale = ImageNetSettings.scale // pixel normalization
            });

            // TensorFlowScorer is used to get the activation map before the last output of the Neural Network
            // This activation map is used as a image vector featurizer 
            pipeline.Add(new TensorFlowScorer()
            {
                ModelFile = modelLocation,
                InputColumns = new[] { InceptionSettings.inputTensorName },
                OutputColumns = new[] { InceptionSettings.outputTensorName }
            });


            pipeline.Add(new ColumnConcatenator(outputColumn: "Features", inputColumns: InceptionSettings.outputTensorName));
            pipeline.Add(new TextToKeyConverter(nameof(ImageNetData.Label)));

            // At this point, there are two inputs for the learner: 
            // * Features: input image vector feaures
            // * Label: label fom the input file
            // In this case, we use SDCA for classifying images using Label / Features columns. 
            // Other multi-class classifier may be used instead of SDCA
            pipeline.Add(new StochasticDualCoordinateAscentClassifier());

            return pipeline;
        }

        protected IEnumerable<ImageNetData> GetPredictions(string testLocation, string imagesFolder, PredictionModel<ImageNetData, ImageNetPrediction> model)
        {
            model.TryGetScoreLabelNames(out string[] labels);
            var testData = ImageNetData.ReadFromCsv(testLocation, imagesFolder);

            foreach (var sample in testData)
            {
                var probs = model.Predict(sample).PredictedLabels;
                yield return new ImageNetData() { ImagePath = sample.ImagePath, Label = ModelHelpers.GetLabel(labels, probs) };
            }

            var sampleNotInTraining = new ImageNetData() { ImagePath = Path.Combine(imagesFolder, "teddy5.jpg") };

            var sampleNotInTrainingProbs = model.Predict(sampleNotInTraining).PredictedLabels;
            yield return new ImageNetData() { ImagePath = sampleNotInTraining.ImagePath, Label = ModelHelpers.GetLabel(labels, sampleNotInTrainingProbs) };
        }

        protected void ShowPredictions (IEnumerable<ImageNetData> imageNetData)
        {
            var defaultForeground = Console.ForegroundColor;
            var labelColor = ConsoleColor.Green;
            Console.WriteLine($"**************************************************************");
            Console.WriteLine($"*       Predictions          ");
            Console.WriteLine($"*-------------------------------------------------------------");
            foreach (var item in imageNetData)
            {
                Console.Write($"      ImagePath: {item.ImagePath} predicted as ");
                Console.ForegroundColor = labelColor;
                Console.Write(item.Label);
                Console.ForegroundColor = defaultForeground;
                Console.WriteLine("");
            }
            Console.WriteLine($"**************************************************************");
        }
    }
}
