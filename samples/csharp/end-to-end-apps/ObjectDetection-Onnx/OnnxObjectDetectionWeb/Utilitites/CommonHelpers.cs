using System.IO;

namespace OnnxObjectDetectionWeb.Utilities
{
    public static class CommonHelpers
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
