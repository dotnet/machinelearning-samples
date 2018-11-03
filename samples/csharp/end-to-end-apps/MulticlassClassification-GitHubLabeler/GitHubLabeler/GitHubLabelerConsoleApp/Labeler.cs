using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.Core.Data;
using Microsoft.ML;
using Octokit;
using System.IO;
using Microsoft.ML.Runtime.Data;
using GitHubLabeler.DataStructures;
using Common;

namespace GitHubLabeler
{
    //This "Labeler" class could be used in a different End-User application (Web app, other console app, desktop app, etc.)
    internal class Labeler
    {
        private readonly GitHubClient _client;
        private readonly string _repoOwner;
        private readonly string _repoName;
        private readonly string _modelPath;
        private readonly MLContext _mlContext;

        private readonly ModelScorer<GitHubIssue, GitHubIssuePrediction> _modelScorer;

        public Labeler(string modelPath, string repoOwner = "", string repoName = "", string accessToken = "")
        {
            _modelPath = modelPath;
            _repoOwner = repoOwner;
            _repoName = repoName;
           
            _mlContext = new MLContext(seed:1);

            //Load file model into ModelScorer
            _modelScorer = new ModelScorer<GitHubIssue, GitHubIssuePrediction>(_mlContext);
            _modelScorer.LoadModelFromZipFile(_modelPath);

            //Configure Client to access a GitHub repo
            if (accessToken != string.Empty)
            {
                var productInformation = new ProductHeaderValue("MLGitHubLabeler");
                _client = new GitHubClient(productInformation)
                {
                    Credentials = new Credentials(accessToken)
                };
            }
        }

        public void TestPredictionForSingleIssue()
        {
            GitHubIssue singleIssue = new GitHubIssue() { ID = "Any-ID", Title = "Entity Framework crashes", Description = "When connecting to the database, EF is crashing" };

            //Predict label for single hard-coded issue
            var prediction = _modelScorer.PredictSingle(singleIssue);
            Console.WriteLine($"=============== Single Prediction - Result: {prediction.Area} ===============");
        }

        // Label all issues that are not labeled yet
        public async Task LabelAllNewIssuesInGitHubRepo()
        {
            var newIssues = await GetNewIssues();
            foreach (var issue in newIssues.Where(issue => !issue.Labels.Any()))
            {
                var label = PredictLabel(issue);
                ApplyLabel(issue, label);
            }
        }

        private async Task<IReadOnlyList<Issue>> GetNewIssues()
        {
            var issueRequest = new RepositoryIssueRequest
            {
                State = ItemStateFilter.Open,
                Filter = IssueFilter.All,
                Since = DateTime.Now.AddMinutes(-10)
            };

            var allIssues = await _client.Issue.GetAllForRepository(_repoOwner, _repoName, issueRequest);

            // Filter out pull requests and issues that are older than minId
            return allIssues.Where(i => !i.HtmlUrl.Contains("/pull/"))
                            .ToList();
        }

        private string PredictLabel(Octokit.Issue issue)
        {
            var corefxIssue = new GitHubIssue
            {
                ID = issue.Number.ToString(),
                Title = issue.Title,
                Description = issue.Body
            };

            var predictedLabel = Predict(corefxIssue);

            return predictedLabel;
        }

        public string Predict(GitHubIssue issue)
        {          
            var prediction = _modelScorer.PredictSingle(issue);

            return prediction.Area;
        }

        private void ApplyLabel(Issue issue, string label)
        {
            var issueUpdate = new IssueUpdate();
            issueUpdate.AddLabel(label);

            _client.Issue.Update(_repoOwner, _repoName, issue.Number, issueUpdate);

            Console.WriteLine($"Issue {issue.Number} : \"{issue.Title}\" \t was labeled as: {label}");
        }
    }
}