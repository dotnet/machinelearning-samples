using Microsoft.ML.Runtime.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImageClassification.ImageData
{
    public class ImageNetData
    {
        public string ImagePath;

        public string Label;

        public static IEnumerable<ImageNetData> ReadFromCsv(string file, string folder)
        {
            return File.ReadAllLines(file)
             .Select(x => x.Split('\t'))
             .Select(x => new ImageNetData()
             {
                 ImagePath = Path.Combine(folder,x[0]),
                 Label = x[1],
             });
        }
    }

    public class ImageNetDataProbability : ImageNetData
    {
        public float Probability { get; set; }


        public void ConsoleWriteLine()
        {
            var defaultForeground = Console.ForegroundColor;
            var labelColor = ConsoleColor.Green;

            Console.Write($"ImagePath: {ImagePath} predicted as ");
            Console.ForegroundColor = labelColor;
            Console.Write(Label);
            Console.ForegroundColor = defaultForeground;
            Console.Write(" with probability ");
            Console.ForegroundColor = labelColor;
            Console.Write(Probability);
            Console.ForegroundColor = defaultForeground;
            Console.WriteLine("");
        }
    }

    public class ImageNetPipeline
    {
        public string ImagePath;
        public string Label;
        public string PredictedLabelValue;
        public float[] Score;
        public float[] softmax2_pre_activation;
    }
}
