using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OnnxObjectDetectionWebAPI
{
    public class ImageNetData
    {
        [LoadColumn(0)]
        public string ImagePath;
    }
}
