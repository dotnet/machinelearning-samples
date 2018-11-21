module GitHubClientFacade


open System
open Octokit

let getAllIssuesForRepository (client : GitHubClient) repoName repoOwner (issueRequest : RepositoryIssueRequest) =
    client.Issue.GetAllForRepository(repoOwner, repoName, issueRequest)
    |> Async.AwaitTask
    |> Async.RunSynchronously
    |> Seq.toList


let updateIssue (client : GitHubClient) repoName repoOwner (issue : Issue) issueUpdate =
    client.Issue.Update(repoOwner, repoName, issue.Number, issueUpdate)
    |> Async.AwaitTask
    |> Async.RunSynchronously
    |> ignore