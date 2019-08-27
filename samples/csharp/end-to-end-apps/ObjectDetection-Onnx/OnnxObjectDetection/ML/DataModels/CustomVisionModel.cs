using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace OnnxObjectDetection
{
    public class CustomVisionModel : IOnnxModel
    {
        const string modelName = "model.onnx", labelsName = "labels.txt";

        private readonly string labelsPath;

        public string ModelPath { get; private set; }

        public string ModelInput { get; } = "data";
        public string ModelOutput { get; } = "model_outputs0";

        public string[] Labels { get; private set; }
        public (float,float)[] Anchors { get; } = { (0.573f,0.677f), (1.87f,2.06f), (3.34f,5.47f), (7.88f,3.53f), (9.77f,9.17f) };

        public CustomVisionModel(string modelPath)
        {
            var extractPath = Path.GetFullPath(modelPath.Replace(".zip", Path.DirectorySeparatorChar.ToString()));

            if (!Directory.Exists(extractPath))
                Directory.CreateDirectory(extractPath);

            ModelPath = Path.GetFullPath(Path.Combine(extractPath, modelName));
            labelsPath = Path.GetFullPath(Path.Combine(extractPath, labelsName));

            if (!File.Exists(ModelPath) || !File.Exists(labelsPath))
                ExtractArchive(modelPath);

            Labels = File.ReadAllLines(labelsPath);
        }

        void ExtractArchive(string modelPath)
        {
            using (ZipArchive archive = ZipFile.OpenRead(modelPath))
            {
                var modelEntry = archive.Entries.FirstOrDefault(e => e.Name.Equals(modelName, StringComparison.OrdinalIgnoreCase))
                    ?? throw new FormatException("The exported .zip archive is missing the model.onnx file");

                modelEntry.ExtractToFile(ModelPath);

                var labelsEntry = archive.Entries.FirstOrDefault(e => e.Name.Equals(labelsName, StringComparison.OrdinalIgnoreCase))
                    ?? throw new FormatException("The exported .zip archive is missing the labels.txt file");

                labelsEntry.ExtractToFile(labelsPath);
            }
        }
    }
}
