using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImageClassification.DataModels
{
    public class ImageData
    {
        [LoadColumn(0)]
        public string ImagePath;

        [LoadColumn(1)]
        public string Label;
    }

}
