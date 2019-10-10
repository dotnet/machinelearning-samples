using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImageClassification.DataModels
{
    public class InMemoryImageData
    {
        public byte[] Image;

        public string Label;

        public string ImageFileName;
    }
}
