open System
open Microsoft.ML
open System.IO
open Microsoft.ML.Data
        
[<CLIMutable>]
type ShampooSalesData =
    {
        [<LoadColumn(0)>]
        Month : string 
        [<LoadColumn(1)>]
        numSales : float32
    }

[<CLIMutable>]
type ShampooSalesPrediction = {Prediction : double []}

let assemblyFolderPath = Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().Location)

let baseDatasetsRelativePath = @"../../../../Data"
let datasetRelativePath = Path.Combine(baseDatasetsRelativePath, "shampoo-sales.csv")

let datasetPath = Path.Combine(assemblyFolderPath, datasetRelativePath)

let baseModelsRelativePath = @"../../../../MLModels"
let modelRelativePath = Path.Combine(baseModelsRelativePath, "ShampooSalesModel.zip")

// Create MLContext to be shared across the model creation workflow objects 
let mlcontext = MLContext()

//assign the Number of records in dataset file to cosntant variable
let size = 36

//STEP 1: Common data loading configuration
let dataView = mlcontext.Data.LoadFromTextFile<ShampooSalesData>(path=datasetPath, hasHeader=true, separatorChar=',')

// To detect temporay changes in the pattern
do 
    printfn "Detect temporary changes in pattern"

    //STEP 2: Set the training algorithm    
    let trainingPipeLine =
        mlcontext.Transforms.DetectIidSpike("Prediction", "numSales", confidence=95, pvalueHistoryLength=size / 4)

    //STEP 3:Train the model by fitting the dataview
    printfn "=============== Training the model using Spike Detection algorithm ==============="
    let trainedModel = trainingPipeLine.Fit(dataView)
    printfn "=============== End of training process ==============="

    //Apply data transformation to create predictions.
    let transformedData = trainedModel.Transform(dataView)
    let predictions = mlcontext.Data.CreateEnumerable<ShampooSalesPrediction>(transformedData, reuseRowObject=false)
           
    printfn "Alert\tScore\tP-Value"

    predictions
    |> Seq.iter 
        (fun p ->
            if p.Prediction.[0] = 1.0 then
                Console.BackgroundColor <- ConsoleColor.DarkYellow
                Console.ForegroundColor <- ConsoleColor.Black
            printfn "%f\t%.2f\t%.2f" p.Prediction.[0] p.Prediction.[1] p.Prediction.[2]
            Console.ResetColor()
        )

//To detect persistent change in the pattern
do
    printfn "Detect Persistent changes in pattern"

    //STEP 2: Set the training algorithm    
    let trainingPipeLine = 
        mlcontext.Transforms.DetectIidChangePoint("Prediction", "numSales", confidence =95, changeHistoryLength = size / 4)

    //STEP 3:Train the model by fitting the dataview
    printfn "=============== Training the model Using Change Point Detection Algorithm==============="
    let trainedModel = trainingPipeLine.Fit(dataView)
    printfn "=============== End of training process ==============="

    //Apply data transformation to create predictions.
    let transformedData = trainedModel.Transform(dataView)
    let predictions = mlcontext.Data.CreateEnumerable<ShampooSalesPrediction>(transformedData, reuseRowObject=false)
                 
    printfn "Prediction column obtained post-transformation."
    printfn "Alert\tScore\tP-Value\tMartingale value"
      
    predictions
    |> Seq.iter 
        (fun p ->
            printfn "%f\t%.2f\t%.2f\t%.2f%s" p.Prediction.[0] p.Prediction.[1] p.Prediction.[2] p.Prediction.[3]
                (if p.Prediction.[0] = 1.0  then "  <-- alert is on, predicted changepoint" else "")
        )


printfn "=============== End of process, hit any key to finish ==============="

Console.ReadLine() |> ignore
