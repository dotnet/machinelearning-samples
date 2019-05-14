using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OnnxObjectDetectionE2EAPP;

namespace OnnxObjectDetectionE2EAPP.OnnxModelScorers
{
    public static class ModelHelpers
    {

        public static string GetAbsolutePath(string relativePath)
        {            
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);
            return fullPath;
        }
    }
}
