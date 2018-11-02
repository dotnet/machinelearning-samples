using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.Core.Data;
using Microsoft.ML;
using Octokit;
using System.IO;
using Microsoft.ML.Runtime.Data;

namespace GitHubLabeler
{
    internal class Labeler
    {
        private readonly GitHubClient _client;
        private readonly string _repoOwner;
        private readonly string _repoName;
        private readonly string _modelPath;
        private readonly MLContext _mlContext;
        private readonly ITransformer _loadedModel;
        private readonly PredictionFunction<GitHubIssue, GitHubIssuePrediction> _engine;

        public Labeler(string modelPath, string repoOwner, string repoName, string accessToken)
        {
            _modelPath = modelPath;
            _repoOwner = repoOwner;
            _repoName = repoName;

            
            _mlContext = new MLContext(seed:1);

            //Load model from .ZIP file
            using (var stream = new FileStream(_modelPath, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                _loadedModel = TransformerChain.LoadFrom(_mlContext, stream);
            }

            // Create prediction engine
            _engine = _loadedModel.MakePredictionFunction<GitHubIssue, GitHubIssuePrediction>(_mlContext);

            // Client to access GitHub
            var productInformation = new ProductHeaderValue("MLGitHubLabeler");
            _client = new GitHubClient(productInformation)
            {
                Credentials = new Credentials(accessToken)
            };
        }

        // Label all issues that are not labeled yet
        public async Task LabelAllNewIssues()
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

        private string PredictLabel(Issue issue)
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
            var prediction = _engine.Predict(issue);

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