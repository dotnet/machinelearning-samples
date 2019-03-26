using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ObjectDetection.NewFolder
{
    public class ImageNetData
    {
        [LoadColumn(0)]
        public string ImagePath;

        [LoadColumn(1)]
        public string name;

        //public static IEnumerable<ImageNetData> ReadFromCsv(string file, string folder)
        //{
        //    return File.ReadAllLines(file)
        //     .Select(x => x.Split('\t'))
        //     .Select(x => new ImageNetData { ImagePath = Path.Combine(folder, x[0]), Label = x[1] });
        //}
    }
}
