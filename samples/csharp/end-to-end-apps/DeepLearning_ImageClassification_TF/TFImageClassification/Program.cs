using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TFImageClassification
{
    public class Program
    {

        private const string graphFile = "model.pb";
        private const string graphUrl = "https://bit.ly/39Dvv0m";
        private static string commonGraphsRelativePath = @"../../../../../graphs";
        private static string commonGraphsPath = Path.GetFullPath(commonGraphsRelativePath);
        private static string graphsPath = Path.GetFullPath(@"ML");
        private static string modelDirectoryPath = Path.GetFullPath(
            Path.Combine(graphsPath, "TensorFlowModel"));
        private static string modelFilePath = Path.Combine(modelDirectoryPath, graphFile);

        public static void Main(string[] args)
        {
            List<string> destGraphFile = new List<string>() { modelFilePath };
            Web.DownloadBigFile(modelDirectoryPath, graphUrl, graphFile,
                commonGraphsPath, destGraphFile );

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
