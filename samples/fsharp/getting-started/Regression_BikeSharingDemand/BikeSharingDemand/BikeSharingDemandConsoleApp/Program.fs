// Learn more about F# at http://fsharp.org

open System
open Microsoft.ML
open Microsoft.ML.Core.Data
open Microsoft.ML.Runtime.Data

let modelsLocation = @"../../../../MLModels"

let datasetsLocation = @"../../../../Data"
let trainingDataLocation = sprintf @"%s/hour_train.csv" datasetsLocation
let testDataLocation = sprintf @"%s/hour_test.csv" datasetsLocation

let dataLoader (mlContext : MLContext) =
    mlContext.Data.TextReader(
        TextLoader.Arguments(
            Separator = ",",
            HasHeader = true,
            Column = 
                [|
                    TextLoader.Column("Season", Nullable DataKind.R4, 2)
                    TextLoader.Column("Year", Nullable DataKind.R4, 3)
                    TextLoader.Column("Month", Nullable DataKind.R4, 4)
                    TextLoader.Column("Hour", Nullable DataKind.R4, 5)
                    TextLoader.Column("Holiday", Nullable DataKind.R4, 6)
                    TextLoader.Column("Weekday", Nullable DataKind.R4, 7)
                    TextLoader.Column("WorkingDay", Nullable DataKind.R4, 8)
                    TextLoader.Column("Weather", Nullable DataKind.R4, 9)
                    TextLoader.Column("Temperature", Nullable DataKind.R4, 10)
                    TextLoader.Column("NormalizedTemperature", Nullable DataKind.R4, 11)
                    TextLoader.Column("Humidity", Nullable DataKind.R4, 12)
                    TextLoader.Column("Windspeed", Nullable DataKind.R4, 13)
                    TextLoader.Column("Count", Nullable DataKind.R4, 16)
                |]
        )
    )

let read (dataPath : string) (dataLoader : TextLoader) =
    dataLoader.Read dataPath


[<EntryPoint>]
let main argv =
    
    // Create MLContext to be shared across the model creation workflow objects 
    // Set a random seed for repeatable/deterministic results across multiple trainings.
    let mlContext = MLContext(seed = Nullable 0)

    // 1. Common data loading configuration
    let trainingDataView = 
        dataLoader mlContext
        |> read trainingDataLocation

    let testDataView = 
        dataLoader mlContext
        |> read testDataLocation


    // 2: Common data process configuration with pipeline data transformations
    let dataProcessPipeline =
        mlContext.Transforms.CopyColumns("Count", "Label")
        |> Common.ModelBuilder.append(
            mlContext.Transforms.Concatenate("Features", "Season", "Year", "Month",
                                            "Hour", "Holiday", "Weekday",
                                            "Weather", "Temperature", "NormalizedTemperature",
                                            "Humidity", "Windspeed"))
        |> Common.ConsoleHelper.downcastPipeline

    // (OPTIONAL) Peek data (such as 2 records) in training DataView after applying the ProcessPipeline's transformations into "Features" 
    Common.ConsoleHelper.peekDataViewInConsole<DataStructures.DemandObservation> mlContext trainingDataView dataProcessPipeline 10 |> ignore
    Common.ConsoleHelper.peekVectorColumnDataInConsole mlContext "Features" trainingDataView dataProcessPipeline 10 |> ignore

    // Definition of regression trainers/algorithms to use
    //var regressionLearners = new (string name, IEstimator<ITransformer> value)[]
    let regressionLearners : (string * IEstimator<ITransformer>) array =
        [|
            "FastTree", mlContext.Regression.Trainers.FastTree() |> Common.ConsoleHelper.downcastPipeline
            "Poisson", mlContext.Regression.Trainers.PoissonRegression() |> Common.ConsoleHelper.downcastPipeline
            "SDCA", mlContext.Regression.Trainers.StochasticDualCoordinateAscent() |> Common.ConsoleHelper.downcastPipeline
            "FastTreeTweedie", mlContext.Regression.Trainers.FastTreeTweedie() |> Common.ConsoleHelper.downcastPipeline
            //Other possible learners that could be included
            //...FastForestRegressor...
            //...GeneralizedAdditiveModelRegressor...
            //...OnlineGradientDescent... (Might need to normalize the features first)
        |]

    // 3. Phase for Training, Evaluation and model file persistence
    // Per each regression trainer: Train, Evaluate, and Save a different model
    for (learnerName, trainer) in regressionLearners do
        printfn "================== Training model =================="
        let modelBuilder = 
            Common.ModelBuilder.create mlContext dataProcessPipeline
            |> Common.ModelBuilder.addTrainer trainer

        let trainedModel = 
            modelBuilder
            |> Common.ModelBuilder.train trainingDataView
            
        printfn "===== Evaluating Model's accuracy with Test data ====="
        let metrics = 
            (trainedModel, modelBuilder)
            |> Common.ModelBuilder.evaluateRegressionModel testDataView "Count" "Score"

        Common.ConsoleHelper.printRegressionMetrics learnerName metrics

        //Save the model file that can be used by any application
        (trainedModel, modelBuilder)
            |> Common.ModelBuilder.saveModelAsFile (sprintf @"%s/%sModel.zip" modelsLocation learnerName)
 
    // 4. Try/test Predictions with the created models
    // The following test predictions could be implemented/deployed in a different application (production apps)
    // that's why it is seggregated from the previous loop
    // For each trained model, test 10 predictions           
    for (learnerName, _) in regressionLearners do
        //Load current model
        let modelScorer = 
            Common.ModelScorer.create mlContext
            |> Common.ModelScorer.loadModelFromZipFile (sprintf @"%s/%sModel.zip" modelsLocation learnerName)

        printfn "================== Visualize/test 10 predictions for model %sModel.zip ==================" learnerName
        //Visualize 10 tests comparing prediction with actual/observed values from the test dataset
        ModelScoringTester.visualizeSomePredictions testDataLocation modelScorer 10

    Common.ConsoleHelper.consolePressAnyKey ()

    0 // return an integer exit code
