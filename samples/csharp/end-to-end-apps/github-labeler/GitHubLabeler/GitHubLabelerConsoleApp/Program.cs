using System;
using System.Configuration;
using System.Threading.Tasks;

// Requires following NuGet packages
// NuGet: Microsoft.Extensions.Configuration
// NuGet: Microsoft.Extensions.Configuration.Json
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GitHubLabeler
{
    internal static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        private static string BaseDatasetsLocation = @"../../../../Data";
        private static string DataSetLocation = $"{BaseDatasetsLocation}/corefx-issues-train.tsv";
        //private static string DataPath => Path.Combine(AppPath, "Data", "corefx-issues-train.tsv");

        private static string BaseModelsLocation = @"../../../../MLModels";
        private static string ModelPath => Path.Combine(AppPath, "GitHubLabelerModel.zip");

        public static IConfiguration Configuration { get; set; }
        private static async Task Main(string[] args)
        {
            SetupAppConfiguration();

            //1. ChainedBuilderExtensions and Train the model
            Predictor.Train(DataSetLocation, ModelPath);

            //2. Predict Issue Labels and apply into a real GitHub repo
            await PredictLabelsAndUpdateGitHub(ModelPath);
        }

        private static async Task PredictLabelsAndUpdateGitHub(string ModelPath)
        {
            var token = Configuration["GitHubToken"];
            var repoOwner = Configuration["GitHubRepoOwner"];
            var repoName = Configuration["GitHubRepoName"];

            if (string.IsNullOrEmpty(token) ||
                string.IsNullOrEmpty(repoOwner) ||
                string.IsNullOrEmpty(repoName))
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("Error: please configure the credentials in the appsettings.json file");
                Console.ReadLine();
                return;
            }

            var labeler = new Labeler(ModelPath, repoOwner, repoName, token);

            await labeler.LabelAllNewIssues();

            Console.WriteLine("Labeling completed");
            Console.ReadLine();
        }

        private static void SetupAppConfiguration()
        {
            var builder = new ConfigurationBuilder()
                                        .SetBasePath(Directory.GetCurrentDirectory())
                                        .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }
    }
}