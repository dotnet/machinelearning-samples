# Structure Guidelines for ML.NET Samples

This section provides guidelines for creating or updating the samples at the [ML.NET samples GitHub repo](https://github.com/dotnet/machinelearning-samples).

## Why use these guidelines?
 
The main goal is to have consistent/similar code across the provided ML.NET samples which can help on faster understanding of each sample as most of them should look familiar in structure, code style and conventions.

*Initial inconsistency in samples structure*: Note that the initial ML.NET samples were created by different developers without following these guidelines, so it'll need some time until most examples are very similar in structure. We're willing to make them consistent while refactoring the samples on every ML.NET minor version release, though.
Feel free to create PRs in order to help on this work, of course.

A well-defined, standard project structure means that a newcomer can get started undertanding an exploring a project without having to read a lot of detailed documentation. In addition, it also means that you don't necessarily need to review 100% of the code before knowing where to investigate when you are looking for a particular area (e.g. data transformations, model training, model deployment, etc.)

However, machine learning projects for model creation/training/evaluation/test (including when using ML.NET in particular) are very similar in regard to the needed phases, folder structure and iterative workflow when building, training and deploying a model, so most of these guidelines can also be applicable to your own ML.NET projects. 

## Nothing here is mandatory

Style, best practices and conventions are arguable and ultimately they also depend on your context. These guidelines are oppinionated with the main purpose of providing a faster getting started experience due to a familiar project structure that has been discussed and analyzed.

Do you disagree on some of the folder names or structure? Sure! This is an initial lightweight baseline structure intended to be a good starting point for average ML.NET projects, but of course, depending on your context, it could be different. Style is in many cases a matter of preferrence. The important point is to try to be consistent.

The following are simple principles about consistency:

- Consistency across multiple projects is important for a collection of applications or samples (such as in ML.NET samples repo)
- Consistency within a single project is more important
- Consistency within one module, class or method is the most important.

However, know when to be inconsistent. Depending on your project some styles and recommendations might not be applicable for you.

Feel free to ask in the issues section and to contribute while trying to follow these commond guidelines.

## Baseline guidelines from .NET and your language of choice

This guidelines in this document are restricted to what's special in machine learning projects.
For any other framework and language guidelines on how to design your .NET code, read the following guidelines: 

- [.NET code guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/) (C# focus)
- [F# code guidelines](https://docs.microsoft.com/en-us/dotnet/fsharp/style-guide/index#five-principles-of-good-f-code)
- [Visual Basic guidelines](https://docs.microsoft.com/en-us/dotnet/visual-basic/programming-guide/program-structure/program-structure-and-code-conventions)

## Project structure guidelines

The project structure is a combination of the following topics:

- Directory/folder structure
- Opinionated code style practices

## Application types

For the following content, the following concepts appearing in the directory/folder structure must be defined:

- **Model-builder app**: Can also be called "model training" app. Usually a console app where you create/train/validate/test an ML.NET model
- **End-user app**: The "production" app that simply uses the trained model. Usually it can be an ASP.NET web app, a service (Web API), a desktop app or a mobile Xamarin app.
Note that Xamarin apps usually run on ARM devices (iOS or Android) and ML.NET as of 2018 just supports x64 and x86, but the mobile Xamarin app could be invoking remote Web API services, though.

Before getting into the specific directory/folde structure, let's review a few premises driving that structure.

## Principles

### A sample can be composed by multiple apps

An important characteristic of the ML.NET samples is that some/many of them might have multiple related apps such as a console app for model creation/training and a different app for deploying the trained model, such as a Web app or desktop app. Each of those apps might be very different in purpose (ML model workflow vs. production app predicting with a trained model), therefore, the folder structure can and must be different depending on the type of app.

### Application projects should be autonomous in structure

The main goal here is that you should be able to copy the app's folder (such a web app folder) to any server or dev machine and still have everything it needs to run. That includes:

 - For an "end-user" app (app predicting with a trained model), its folder must include the ML.NET model file it needs for predicting.
 - For an "ML model builder/training" app, its folder must include the training and test datasets or if those files are too big for GitHub, then it should implement an automated way for download those files. 

Essentially, the rule is that everything you need to run each app must be available within the app's project folder, without any absolute filepaths or references to other folders in the filesystem out of the scope of the app. 

This rule can be arguable for your own ML workflow if you want to be more agile in your own apps by accessing common folders for generated models. However, for samples that must be able to run out-of-the-box when you just copy a single app (i.e. only the "production" app) to a different machine/server, this rule should be followed when implementing the ML.NET samples.

## Directory/folder structure

### Directory structure for a sample with a single training/test app (e.g. ML Model builder console app)

If the ML.NET sample is basic you might just have a single console-app, then your folder structure won't have additional end-user apps of the shared library mentioned later, because all the data-structure classes can also be positioned within this single console-app, like in the following directory structure:

```
ML.NET Sample
├── Solution.sln               <- Top-level VS solution targeting all apps in the sample
├── README.md                  <- Top-level README for developers
├── docs                       <- Global docs folder documenting the whole sample
├── images                     <- Images used by the .md documents
│
└── MLModelBuilderApp          <- Usually a console app for building/training/evaluating/testing a model
     ├── Data
     │   ├── Raw               <- The original, immutable data dump.
     │   └── Processed         <- The final, canonical datasets for training/evaluating/testing.
     ├── MLModels              <- The trained, generated saved/serialized ML.NET model files
     ├── MyConsoleApp          <- Source code for this particular app
     │   ├── MLDataStructures  <- Data classes for observations and predictions (Remove if using shared library)
     │   ├── .csproj           <- App's .csproj project file
     │   └── .cs files         <- Main code files
     └── MyTestProject         <- Unit tests project 
```
### TO DISCUSS: Alternative choices for data and models folders

Input/Ouput approach for the Model Builder app (building/training/evaluating/testing a model):

```
ML.NET Sample
├── Solution.sln            
├── README.md               
├── docs                    
├── images                  
└── MLModelBuilderApp       
     ├── assets (or "io")
     │   ├── input
     │   │     └── data
     │   │           ├── raw                   
     │   │           └── processed      
     │   └── output  
     │         ├── models           
     │         └── data             
     ├── MyConsoleApp                
     │   ├── DataStructures 
     │   ├── .csproj        
     │   └── .cs files      
     └── MyTestProject            
```

### Directory structure for a sample with multiple apps

Note that "Model-Builder-App" or "Production-App" would have your own naming. It is the concept the important point.

```
ML.NET Sample
├── Solution.sln               <- Top-level VS solution targeting all apps in the sample
├── README.md                  <- Top-level README for developers
├── docs                       <- Global docs folder documenting the whole sample
├── images                     <- Images used by the .md documents
│
├── MLModelBuilderApp          <- Usually a console app for building/training/evaluating/testing a model
│    ├── Data
│    │   ├── Raw               <- The original, immutable data dump.
│    │   └── Processed         <- The final, canonical datasets for training/evaluating/testing.
│    ├── MLModels              <- The trained, generated saved/serialized ML.NET model files
│    ├── MyConsoleApp          <- Source code for this particular app
│    │   ├── MLDataStructures  <- Data classes for observations and predictions (Remove if using shared library)
│    │   ├── .csproj           <- App's .csproj project file
│    │   └── .cs files         <- Main code files
│    └── MyTestProject         <- Unit tests project for this app's code
│
├── EndUserApp                 <- Actual final-user's app using the trained model just for predicting/scoring. 
│    ├── MyWebApp              <- Source code for this particular app (WebApp, ConsoleApp, DesktopApp, etc.)
│    │   ├── MLDataStructures  <- Data classes for observations and predictions (Remove if using shared library)
│    │   ├── MLModels          <- ML.NET serialized .ZIP model files 
│    │   ├── EntityModel       <- Could also be "Model" or "DomainModel": Entity classes when using databases, EF, etc. 
│    │   └── Other files       <- App's files and folders
│    └── MyTestProject         <- Unit tests project for this app's code 
│
└── Shared-Library             <- Shared Library with common classes across apps 
     ├── MySharedLibrary       <- Source code 
     │   ├── MLDataStructures  <- Data classes for observations and predictions
     │   └── Other files       <- App's files and folders
     └── MyTestProject         <- Unit tests project for this class library code 

```

Clearly, the app's folder structure for the "End-user" application is the one that probably can be very different depending on the app type (ASP.NET Core web app, WPF desktop app, Xamarin mobile app, etc.) The point is to simply highlight possible folder names for the serialized ML.NET models and data structure classes used by the model.

#### Reason why datasets are distributed across the samples which use them instead of having a single folder for ALL datasets from ALL samples in the ML.NET-Samples repository

Because of the same "autonomous samples" argument explained above. Essentially, the rule is that everything you need to run each app must be available within the app's project folder, without any absolute filepaths or references to other folders in the filesystem out of the scope of the app.

#### Reason why 'model' and 'data' folders are out of the app's source-code folder

Even when it might be tempting to put it within the project's code folder so you see the datasets and generated models within the Visual Studio explorer window, this approach would cause:
- Large datasource files would bloat the source code folder, making it too large if you just want to copy/extract the code of a sample.
- .ZIP files generated by ML.NET would always appear as files to be added to the repo by Git. However, the code to be uploaded to GitHub doesn't need those .ZIP files as they will be generated when running the training/testing app.   

#### Reason why solution.sln is at the root folder

Since any given ML.NET sample could be comprised by multiple apps/projects (e.g. a console training-app and a web app), the solution has to be placed in the root level of the sample so when opening the solution in Visual Studio you see all the related app's projects.


#### Additional references of .NET project structure

- [Cookiecutter Data Science](https://drivendata.github.io/cookiecutter-data-science/)

- [A Quick Guide to Organizing [Data Science] Projects](https://medium.com/outlier-bio-blog/a-quick-guide-to-organizing-data-science-projects-updated-for-2016-4cbb1e6dac71)

- [Data Science Project Folder Structure](https://dzone.com/articles/data-science-project-folder-structure)

- [Organizing and testing projects with .NET Core](https://docs.microsoft.com/en-us/dotnet/core/tutorials/testing-with-cli)

- [David Fowler's project structure (ASP.NET team)](https://gist.github.com/davidfowl/ed7564297c61fe9ab814)



## Internal code structure for the Model Builder app
 

### Phase A: Iterative process building the model

	1. Input dataset (where is my data)
	2. Loader function (define columns)
	3. Feature engineering (process raw input data)
	4. Learner (define my pipeline's model type & hyperparameters)
	5. TrainTest / CV (get metrics for my pipeline)
	6. (Look at metrics)
	7. (Iterate to improve metrics -- goto step 3 or 4)
	8. Re-train a model for production on 100% of data

**Train with 100% of the data once the algorithm and config are decided**:
 
You should do a full re-train after your iterative train/evaluate/test cycles because of the following reasons:

- The full re-training on the entire dataset gives you a better model because it is trained with more data. 
- The CV/TrainTest gives your pipeline's metrics { accuracy, AUC, NDCG, etc } so you can compare different approaches (algorithm/hyper-parameters). 
- The full retraining, on 100% of the dataset, is the model to deploy in production.

### B. Deploy the model to production application (end-user application)

Point to different samples on how to host trained models in an different application types:
- Web App (ASP.NET Core app, MVC, Razor)
- Web API service (ASP.NET Core Web API)
- Desktop app (UWP, WPF, WinForms)
- Mobile app (Xamarin app, when ARM is supported by ML.NET)
- Edge/IoT (When ARM is supported by ML.NET)


#### Additional references for model builder app

- [Machine Learning Project Structure: Stages, Roles, and Tools](https://www.altexsoft.com/blog/datascience/machine-learning-project-structure-stages-roles-and-tools/)

## Opinionated code style practices

TBD
