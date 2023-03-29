using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace StyleTransfer.Web.ML.DataModels
{
    public class ImageInput
    {
        [LoadColumn(0)]
        public string ImagePath;
    }
}
