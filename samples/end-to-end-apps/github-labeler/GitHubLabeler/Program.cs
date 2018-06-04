using System;
using System.Configuration;
using System.Threading.Tasks;

namespace GitHubLabeler
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            await Predictor.TrainAsync();

            await Label();
        }

        private static async Task Label()
        {
            var token = ConfigurationManager.AppSettings["GitHubToken"];
            var userName = ConfigurationManager.AppSettings["GitHubUserName"];
            var repoName = ConfigurationManager.AppSettings["GitHubRepoName"];

            if (string.IsNullOrEmpty(token) ||
                string.IsNullOrEmpty(userName) ||
                string.IsNullOrEmpty(repoName))
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("Error: please configure the credentials in the app.config");
                Console.ReadLine();
                return;
            }

            var labeler = new Labeler(userName, repoName, token);

            await labeler.LabelAllNewIssues();

            Console.WriteLine("Labeling completed");
            Console.ReadLine();
        }
    }
}