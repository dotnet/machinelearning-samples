using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using Octokit;
using System.IO;
using GitHubLabeler.DataStructures;
using Microsoft.ML.Data;

namespace GitHubLabeler
{
    // This "Labeler" class could be used in a different End-User application (Web app, other console app, desktop app, etc.)
    internal class Labeler
    {
        private readonly GitHubClient _client;
        private readonly string _repoOwner;
        private readonly string _repoName;
        private readonly string _modelPath;
        private readonly MLContext _mlContext;

        private readonly PredictionEngine<GitHubIssue, GitHubIssuePrediction> _predEngine;
        private readonly ITransformer _trainedModel;

        private FullPrediction[] _fullPredictions;

        public Labeler(string modelPath, string repoOwner = "", string repoName = "", string accessToken = "")
        {
            _modelPath = modelPath;
            _repoOwner = repoOwner;
            _repoName = repoName;
           
            _mlContext = new MLContext();

            // Load model from file.
            _trainedModel = _mlContext.Model.Load(_modelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model.
            _predEngine = _mlContext.Model.CreatePredictionEngine<GitHubIssue, GitHubIssuePrediction>(_trainedModel);

            // Configure Client to access a GitHub repo.
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
            var singleIssue = new GitHubIssue()
			{
                ID = "Any-ID",
                Title = "Crash in SqlConnection when using TransactionScope",
                Description = "I'm using SqlClient in netcoreapp2.0. Sqlclient.Close() crashes in Linux but works on Windows"
            };

            // Predict labels and scores for single hard-coded issue.
            var prediction = _predEngine.Predict(singleIssue);

            _fullPredictions = GetBestThreePredictions(prediction);

            Console.WriteLine($"==== Displaying prediction of Issue with Title = {singleIssue.Title} and Description = {singleIssue.Description} ====");

            Console.WriteLine("1st Label: " + _fullPredictions[0].PredictedLabel + " with score: " + _fullPredictions[0].Score);
            Console.WriteLine("2nd Label: " + _fullPredictions[1].PredictedLabel + " with score: " + _fullPredictions[1].Score);
            Console.WriteLine("3rd Label: " + _fullPredictions[2].PredictedLabel + " with score: " + _fullPredictions[2].Score);

            Console.WriteLine($"=============== Single Prediction - Result: {prediction.Area} ===============");
        }

        private FullPrediction[] GetBestThreePredictions(GitHubIssuePrediction prediction)
        {
            float[] scores = prediction.Score;
            int size = scores.Length;
            int index0, index1, index2 = 0;

            VBuffer<ReadOnlyMemory<char>> slotNames = default;
            _predEngine.OutputSchema[nameof(GitHubIssuePrediction.Score)].GetSlotNames(ref slotNames);

            GetIndexesOfTopThreeScores(scores, size, out index0, out index1, out index2);

            _fullPredictions = new FullPrediction[]
                {
                    new FullPrediction(slotNames.GetItemOrDefault(index0).ToString(),scores[index0],index0),
                    new FullPrediction(slotNames.GetItemOrDefault(index1).ToString(),scores[index1],index1),
                    new FullPrediction(slotNames.GetItemOrDefault(index2).ToString(),scores[index2],index2)
                };

            return _fullPredictions;
        }

        private void GetIndexesOfTopThreeScores(float[] scores, int n, out int index0, out int index1, out int index2)
        {
            int i;
            float first, second, third;
            index0 = index1 = index2 = 0;
            if (n < 3)
            {
                Console.WriteLine("Invalid Input");
                return;
            }
            third = first = second = 000;
            for (i = 0; i < n; i++)
            {
                // If current element is  
                // smaller than first 
                if (scores[i] > first)
                {
                    third = second;
                    second = first;
                    first = scores[i];
                }
                // If arr[i] is in between first 
                // and second then update second 
                else if (scores[i] > second)
                {
                    third = second;
                    second = scores[i];
                }

                else if (scores[i] > third)
                    third = scores[i];
            }
            var scoresList = scores.ToList();
            index0 = scoresList.IndexOf(first);
            index1 = scoresList.IndexOf(second);
            index2 = scoresList.IndexOf(third);
        }

        // Label all issues that are not labeled yet
        public async Task LabelAllNewIssuesInGitHubRepo()
        {
            var newIssues = await GetNewIssues();
            foreach (var issue in newIssues.Where(issue => !issue.Labels.Any()))
            {
                var label = PredictLabels(issue);
                ApplyLabels(issue, label);
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

        private FullPrediction[] PredictLabels(Octokit.Issue issue)
        {
            var corefxIssue = new GitHubIssue
            {
                ID = issue.Number.ToString(),
                Title = issue.Title,
                Description = issue.Body
            };

            _fullPredictions = Predict(corefxIssue);

            return _fullPredictions;
        }

        public FullPrediction[] Predict(GitHubIssue issue)
        {
            var prediction = _predEngine.Predict(issue);

            var fullPredictions = GetBestThreePredictions(prediction);

            return fullPredictions;
        }

        private void ApplyLabels(Issue issue, FullPrediction[] fullPredictions)
        {
            var issueUpdate = new IssueUpdate();

            //assign labels in GITHUB only if predicted score of all predictions is > 30%
            foreach (var fullPrediction in fullPredictions)
            {
                if (fullPrediction.Score >= 0.3)
                {
                    issueUpdate.AddLabel(fullPrediction.PredictedLabel);
                    _client.Issue.Update(_repoOwner, _repoName, issue.Number, issueUpdate);

                    Console.WriteLine($"Issue {issue.Number} : \"{issue.Title}\" \t was labeled as: {fullPredictions[0].PredictedLabel}");
                }
            }
        }
     }
}