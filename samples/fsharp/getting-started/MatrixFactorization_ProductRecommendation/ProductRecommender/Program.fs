open Microsoft.ML
open Microsoft.ML.Data
open System
open System.IO
open Microsoft.ML.Trainers

[<CLIMutable>]
type ProductEntry = 
    {
        [<LoadColumn(0); KeyType(count=262111UL)>]
        ProductID : uint32
        [<LoadColumn(1); KeyType(count=262111UL)>]
        CoPurchaseProductID : uint32
        [<NoColumn>]
        Label : float32
    }
    
[<CLIMutable>]
type Prediction = {Score : float32}

let assemblyFolderPath = Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().Location)

let baseDatasetsRelativePath = @"../../../Data"
let trianDataRealtivePath = Path.Combine(baseDatasetsRelativePath, "Amazon0302.txt")
let trainDataPath = Path.Combine(assemblyFolderPath, trianDataRealtivePath)

let baseModelsRelativePath = @"../../../Model";
let modelRelativePath = Path.Combine(baseModelsRelativePath, "model.zip")
let modelPath = Path.Combine(assemblyFolderPath, modelRelativePath)

//STEP 1: Create MLContext to be shared across the model creation workflow objects 
let mlContext = new MLContext()

//STEP 2: Read the trained data using TextLoader by defining the schema for reading the product co-purchase dataset
//        Do remember to replace amazon0302.txt with dataset from https://snap.stanford.edu/data/amazon0302.html
let traindata = 
    let columns = 
        [|
            TextLoader.Column("Label", DataKind.Single, 0)
            TextLoader.Column("ProductID", DataKind.UInt32, source = [|TextLoader.Range(0)|], keyCount = KeyCount 262111UL) 
            TextLoader.Column("CoPurchaseProductID", DataKind.UInt32, source = [|TextLoader.Range(1)|], keyCount = KeyCount 262111UL) 
        |]
    mlContext.Data.LoadFromTextFile(trainDataPath, columns, hasHeader=true, separatorChar='\t')

//STEP 3: Your data is already encoded so all you need to do is specify options for MatrxiFactorizationTrainer with a few extra hyperparameters
//        LossFunction, Alpa, Lambda and a few others like K and C as shown below and call the trainer. 
let options = MatrixFactorizationTrainer.Options(MatrixColumnIndexColumnName = "ProductID", 
                                                 MatrixRowIndexColumnName = "CoPurchaseProductID",
                                                 LossFunction = MatrixFactorizationTrainer.LossFunctionType.SquareLossOneClass,
                                                 LabelColumnName = "Label",
                                                 Alpha = 0.01,
                                                 Lambda = 0.025)

// For better results use the following parameters
//options.K = 100;
//options.C = 0.00001;

//Step 4: Call the MatrixFactorization trainer by passing options.
let est = mlContext.Recommendation().Trainers.MatrixFactorization(options)
            
//STEP 5: Train the model fitting to the DataSet
//Please add Amazon0302.txt dataset from https://snap.stanford.edu/data/amazon0302.html to Data folder if FileNotFoundException is thrown.
let model = est.Fit(traindata)

//STEP 6: Create prediction engine and predict the score for Product 63 being co-purchased with Product 3.
//        The higher the score the higher the probability for this particular productID being co-purchased 
let predictionengine = mlContext.Model.CreatePredictionEngine<ProductEntry, Prediction>(model)
let prediction = predictionengine.Predict {ProductID = 3u; CoPurchaseProductID = 63u; Label = 0.f}

printfn ""
printfn "For ProductID = 3 and  CoPurchaseProductID = 63 the predicted score is %f" prediction.Score
printf "=============== End of process, hit any key to finish ==============="
Console.ReadKey() |> ignore
