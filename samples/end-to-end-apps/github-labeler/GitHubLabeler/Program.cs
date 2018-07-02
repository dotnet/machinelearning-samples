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
        public static IConfiguration Configuration { get; set; }
        private static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                                        .SetBasePath(Directory.GetCurrentDirectory())
                                        .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            await Predictor.TrainAsync();

            await Label();
        }

        private static async Task Label()
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

            var labeler = new Labeler(repoOwner, repoName, token);

            await labeler.LabelAllNewIssues();

            Console.WriteLine("Labeling completed");
            Console.ReadLine();
        }
    }
}