# GitHub Labeler

| ML.NET version | API type          | Status                        | App Type    | Data sources | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v1.3.1 | Dynamic API | Up-to-date | Console app | .csv file and GitHub issues | Issues classification | Multi-class  classification | SDCA multi-class classifier |


This is a simple prototype application to demonstrate how to use [ML.NET](https://www.nuget.org/packages/Microsoft.ML/) APIs. The main focus is on creating, training, and using ML (Machine Learning) model that is implemented in Predictor.cs class.

## Overview
GitHubLabeler is a .NET Core console application that:
* trains ML model on your labeled GitHub issues to teach the model what label should be assigned for a new issue. (As an example, you can use `corefx-issues-train.tsv` file that contains issues from public [corefx](https://github.com/dotnet/corefx) repository)
* labeles a new issue. The application will get all unlabeled open issues from the GitHub repository specified at the `appsettings.json` file and label them using the trained ML model created on the step above.  

This ML model is using multi-class classification algorithm (`SdcaMultiClassTrainer`) from [ML.NET](https://www.nuget.org/packages/Microsoft.ML/).

## Enter you GitHub configuration data
1. **Provide your GitHub data** in the `appsettings.json` file:

    To allow the app to label issues in your GitHub repository you need to provide the folloving data into the appsettings.json file.
    ```csharp
        {
          "GitHubToken": "YOUR-GUID-GITHUB-TOKEN",
          "GitHubRepoOwner": "YOUR-REPO-USER-OWNER-OR-ORGANIZATION",
          "GitHubRepoName": "YOUR-REPO-SINGLE-NAME"
        }
    ```
    Your user account (`GitHubToken`) should have write rights to the repository (`GitHubRepoName`).

    Check out here [how to create a Github Token](https://help.github.com/articles/creating-a-personal-access-token-for-the-command-line/).

    `GitHubRepoOwner` can be a GitHub user ID (i.e. "MyUser") or it can also be a GitHub Organization (i.e. "dotnet")

2. **Provide training file**

    a.  You can use existing `corefx_issues.tsv` data file for experimenting  with the program. In this case the predicted labels will be chosen among labels from [corefx](https://github.com/dotnet/corefx) repository. No changes required.
    
    b. To work with labels from your GitHub repository, you will need to train the model on your data. To do so, export GitHub issues from your repository in `.tsv` file with the following columns:
    * ID - issue's ID
    * Area - issue's label (named this way to avoid confusion with the Label concept in ML.NET)
    * Title - issue's title
    * Description - issue's description
    
    and add the file in `Data` folder. Update `DataSetLocation` field to match your file's name:
```fsharp
let dataSetLocation = sprintf @"%s/corefx-issues-train.tsv" baseDatasetsLocation
```

## Training 
Training is a process of running an ML model through known examples (in our case - issues with labels) and teaching it how to label new issues. In this sample it is done by calling this method at the console app:
```fsharp
buildAndTrainModel dataSetLocation modelFilePathName MyTrainerStrategy.SdcaMultiClassTrainer
```
After the training is completed, the model is saved as a .zip file in `MLModels\GitHubLabelerModel.zip`.

## Labeling
When the model is trained, it can be used for predicting new issue's label. 

For a single test/demo without connecting to a real GitHub repo, call this method from the console app:
```fsharp
testSingleLabelPrediction modelFilePathName
```

For accessing the real issues of a GitHub repo, you call this other method from the console app:
```fsharp
predictLabelsAndUpdateGitHub configuration modelFilePathName
```

For testing convenience when reading issues from your GitHub repo, it will only load not labeled issues that were created in the past 10 minutes and are subject to be labeled. You can chenge that config, though:
```fsharp
Since = Nullable (DateTimeOffset(DateTime.Now.AddMinutes(-10.)))
```
You can modify those settings. After predicting the label, the program updates the issue with the predicted label on your GitHub repo.