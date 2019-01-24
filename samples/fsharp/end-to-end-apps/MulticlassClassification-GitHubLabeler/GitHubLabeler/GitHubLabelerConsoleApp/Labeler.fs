module Labeler

open System
open Microsoft.ML
open Octokit
open DataStructures

open Common

type GitHubClientFacade =
    {
        getAllIssuesForRepository : RepositoryIssueRequest -> Issue list
        updateIssue : Issue -> IssueUpdate -> unit    
    } with
    static member init (client : GitHubClient) (repoOwner : string) (repoName : string) =
        let getAll (client : GitHubClient) (repoOwner : string) (repoName : string) (issueRequest : RepositoryIssueRequest) = 
            client.Issue.GetAllForRepository(repoOwner, repoName, issueRequest)
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> Seq.toList
        let getAll' (issueRequest : RepositoryIssueRequest) = getAll client repoName repoOwner issueRequest

        let updateIssue (client : GitHubClient) (repoOwner : string) (repoName : string) (issue : Issue) (issueUpdate : IssueUpdate) =
            client.Issue.Update(repoOwner, repoName, issue.Number, issueUpdate)
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
        let updateIssue' (issue : Issue) (issueUpdate : IssueUpdate) = updateIssue client repoOwner repoName issue issueUpdate

        {
            getAllIssuesForRepository = getAll'
            updateIssue = updateIssue'
        }

type Labeler = ((MLContext * Runtime.Data.TransformerChain<Core.Data.ITransformer> * Runtime.Data.PredictionFunction<GitHubIssue, GitHubIssuePrediction>)) * GitHubClientFacade

let initialise modelPath repoOwner repoName accessToken : Labeler =
    let mlContext = MLContext(seed = Nullable 1)

    let modelScorer = 
        Common.ModelScorer.create mlContext
        |> Common.ModelScorer.loadModelFromZipFile modelPath

    let productInformation = ProductHeaderValue "MLGitHubLabeler"
    let client = GitHubClient(productInformation, Credentials = Credentials(accessToken))
    let gitHubClient = GitHubClientFacade.init client repoOwner repoName
    modelScorer, gitHubClient

/// Predict single, hard coded issue
let testPredictionForSingleIssue ((modelScorer, _) : Labeler) =

    let singleIssue = { ID = "Any-ID"; Area = ""; Title = "Entity Framework crashes"; Description = "When connecting to the database, EF is crashing" }

    //Predict label for single hard-coded issue
    let prediction = 
        modelScorer
        |> Common.ModelScorer.predictSingle singleIssue
    printfn "=============== Single Prediction - Result: %s ===============" prediction.Area

let private getNewIssues (client : GitHubClientFacade) = 
    let issueRequest =
        new RepositoryIssueRequest(
            State = ItemStateFilter.Open,
            Filter = IssueFilter.All,
            Since = Nullable (DateTimeOffset(DateTime.Now.AddMinutes(-10.)))
        )

    client.getAllIssuesForRepository issueRequest
    // Filter out pull requests and issues that are older than minId
    |> List.filter(fun (i : Issue) -> not(i.HtmlUrl.Contains("/pull/")))
    
let private predict (issue : GitHubIssue) modelScorer =
    let prediction = 
        modelScorer
        |> Common.ModelScorer.predictSingle issue
    prediction.Area

let private predictLabel (issue : Issue) =
    let corefxIssue = 
        {
            ID = issue.Number.ToString()
            Area = ""
            Title = issue.Title
            Description = issue.Body
        }

    let predictedLabel = predict corefxIssue
    predictedLabel

let private applyLabel (issue : Issue) label client =
    let issueUpdate = new IssueUpdate()
    issueUpdate.AddLabel label

    client.updateIssue issue issueUpdate

    printfn "Issue %d : \"%s\" \t was labeled as: %s" issue.Number issue.Title label

/// Label all issues that are not labeled yet
let LabelAllNewIssuesInGitHubRepo ((modelScorer, client) : Labeler) =
    let newIssues = client |> getNewIssues


    for issue in newIssues do
        let label = predictLabel issue modelScorer
        client |> applyLabel issue label
