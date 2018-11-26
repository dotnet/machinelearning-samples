namespace Common

open System.IO
open Microsoft.ML
open Microsoft.ML.Core.Data
open Microsoft.ML.Transforms
open Microsoft.ML.Runtime.Data

module ModelBuilder =

    let create (mlContext : MLContext) (pipeline : IEstimator<ITransformer>) =
        (mlContext, pipeline)
        
    let append (estimator : IEstimator<'a>) (pipeline : IEstimator<'b>)  = 
        match pipeline with
        | :? IEstimator<ITransformer> as p -> 
            p.Append estimator
        | _ -> failwith "The pipeline has to be an instance of IEstimator<ITransformer>."

    let downcastPipeline (pipeline : IEstimator<'a>) =
        match pipeline with
        | :? IEstimator<ITransformer> as p -> p
        | _ -> failwith "The pipeline has to be an instance of IEstimator<ITransformer>."

    let addTrainer (trainer : IEstimator<'b>) (mlContext : MLContext, pipeline : IEstimator<'a>) =
        let newPipeline =
            pipeline 
            |> append trainer
        (mlContext, newPipeline)

    let addEstimator (estimator : IEstimator<'b>) (mlContext : MLContext, pipeline: IEstimator<'a>) =
        let newPipeline =
            pipeline 
            |> append estimator
            |> downcastPipeline
        (mlContext, newPipeline)

    let train (trainingData : IDataView) (mlContext : MLContext, pipeline : IEstimator<'a>) =
        pipeline.Fit trainingData :> ITransformer
        

    let private checkTrained (trainedModel : ITransformer) =
        if (trainedModel = null) then
            failwith "Cannot test before training. Call Train() first."

    let evaluateClusteringModel (dataView : IDataView) (trainedModel : ITransformer, (mlContext : MLContext, _)) =
        checkTrained trainedModel
        let predictions = trainedModel.Transform dataView
        mlContext.Clustering.Evaluate(predictions, score = "Score", features = "Features")

    let evaluateBinaryClassificationModel (testData : IDataView) label score (trainedModel : ITransformer, (mlContext : MLContext, _)) =
        checkTrained trainedModel
        let predictions = trainedModel.Transform testData
        mlContext.BinaryClassification.Evaluate(predictions, label, score)

    let evaluateMultiClassClassificationModel (testData : IDataView) label score (trainedModel : ITransformer, (mlContext : MLContext, _)) =
        checkTrained trainedModel
        let predictions = trainedModel.Transform testData
        mlContext.MulticlassClassification.Evaluate(predictions, label, score)

    let evaluateRegressionModel (testData : IDataView) label score (trainedModel : ITransformer, (mlContext : MLContext, _)) =
        checkTrained trainedModel
        let predictions = trainedModel.Transform testData
        mlContext.Regression.Evaluate(predictions, label, score)

    let crossValidateAndEvaluateMulticlassClassificationModel (data : IDataView) (numFolds : int) (labelColumn : string) (mlContext : MLContext, estimator : IEstimator<ITransformer>) = 
        //CrossValidation happens actually before training, so no check here.

        //Cross validate
        let crossValidationResults = mlContext.MulticlassClassification.CrossValidate(data, estimator, numFolds, labelColumn)
        //data, trainedModel, numFolds, labelColumn, null);

        //Another way to do it:
        //var context = new MulticlassClassificationContext(_mlcontext);
        //var crossValidationResults = context.CrossValidate(data, TrainingPipeline, numFolds, labelColumn, stratificationColumn);

        crossValidationResults


    let saveModelAsFile persistedModelPath (trainedModel : ITransformer, (mlContext : MLContext, _)) =
        checkTrained trainedModel

        use fs = new FileStream(persistedModelPath, FileMode.Create, FileAccess.Write, FileShare.Write)
        mlContext.Model.Save(trainedModel, fs);
        printfn "The model is saved to %s" persistedModelPath

