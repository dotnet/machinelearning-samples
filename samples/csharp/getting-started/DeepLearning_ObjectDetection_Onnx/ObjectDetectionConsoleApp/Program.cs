#region MainUsings
using System;
using System.IO;
#endregion

namespace ObjectDetection
{
    class Program
    {
        public static void Main()
        {
            #region MainDefinePaths
            var assetsRelativePath = @"../../../assets";
            string assetsPath = GetAbsolutePath(assetsRelativePath);
            var modelFilePath = Path.Combine(assetsPath, "Model", "TinyYolo2_model.onnx");
            var imagesFolder = Path.Combine(assetsPath,"images");
            #endregion

            #region MainProgram
            try
            {
                var modelScorer = new OnnxModelScorer(imagesFolder, modelFilePath);
                modelScorer.Score();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            Console.WriteLine("========= End of Process..Hit any Key ========");
            Console.ReadLine();
            #endregion 
        }

        #region GetAbsolutePathMethod
        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
        #endregion
    }
}



