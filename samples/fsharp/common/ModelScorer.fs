namespace Common

open System.IO
open Microsoft.ML
open Microsoft.ML.Core.Data
open Microsoft.ML.Transforms
open Microsoft.ML.Runtime.Data

module ModelScorer =

    let create (mlContext : MLContext) =
        mlContext

    let loadModelFromZipFile<'TObservation, 'TPrediction when 'TPrediction : (new : unit -> 'TPrediction) and 'TPrediction : not struct and 'TObservation : not struct> modelPath (mlContext : MLContext) =
        use stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read)
        let trainedModel = TransformerChain.LoadFrom(mlContext, stream)
        let predictionFunction = trainedModel.MakePredictionFunction<'TObservation, 'TPrediction>(mlContext);
       
        mlContext, trainedModel, predictionFunction

    let setTrainedModel<'TObservation, 'TPrediction when 'TPrediction : (new : unit -> 'TPrediction) and 'TPrediction : not struct and 'TObservation : not struct> (trainedModel : ITransformer) (mlContext : MLContext) =
        let predictionFunction = trainedModel.MakePredictionFunction<'TObservation, 'TPrediction>(mlContext);
       
        mlContext, trainedModel, predictionFunction

    let private checkTrainedModelIsLoaded (trainedModel) =
        if trainedModel = null then
            failwith "Need to have a model before scoring. Call LoadModelFromZipFile(modelPath) first or provided a model through the constructor."

    let predictSingle (input : 'TObservation) (mlContext : MLContext, trainedModel, predictionFunction : PredictionFunction<'TObservation, 'TPrediction>) =
        checkTrainedModelIsLoaded trainedModel
        predictionFunction.Predict(input);
        