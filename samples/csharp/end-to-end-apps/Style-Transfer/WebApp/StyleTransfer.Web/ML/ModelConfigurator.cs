using System.Collections.Generic;
using System.Linq;
using Microsoft.ML;
using StyleTransfer.Web.ML.DataModels;



namespace StyleTransfer.Web.ML
{
    public class ModelConfigurator
    {
        public MLContext MlContext { get; private set; }

        public ITransformer Model { get; }

        public ModelConfigurator(string tensorFlowModelFilePath)
        {
            MlContext = new MLContext();

            // Model creation and pipeline definition for images needs to run just once, so calling it from the constructor.
            Model = SetupMlnetModel(tensorFlowModelFilePath);
        }

        // For checking tensor names, you can open the TF model .pb file with tools like Netron: https://github.com/lutzroeder/netron
        public struct TensorFlowModelSettings
        {
            // Input tensor name.
            public const string inputTensorName = "Placeholder";

            // Output tensor name.
            public const string outputTensorName = "add_37";
        }

        private ITransformer SetupMlnetModel(string tensorFlowModelFilePath)
        {
            // Create pipeline to execute our model

            return mlModel;
        }

        private IDataView CreateEmptyDataView()
        {
            // Create empty DataView ot Images. We just need the schema to call fit().

        }
    }
}
