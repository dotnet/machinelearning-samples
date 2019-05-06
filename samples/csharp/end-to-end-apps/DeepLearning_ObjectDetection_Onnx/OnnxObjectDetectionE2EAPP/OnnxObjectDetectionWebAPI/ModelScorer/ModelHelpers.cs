using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OnnxObjectDetectionWebAPI.OnnxModelScorers
{
    public static class ModelHelpers
    {
        static FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}
