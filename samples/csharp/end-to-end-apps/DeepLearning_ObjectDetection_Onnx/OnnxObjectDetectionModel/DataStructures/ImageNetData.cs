using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OnnxObjectDetectionModel
{
    public class ImageNetData
    {
        [LoadColumn(0)]
        public string ImagePath;
    }
}
