namespace Common

module Pipeline =

    open Microsoft.ML.Core.Data
    open Microsoft.ML.Transforms
    open Microsoft.ML.Runtime.Data

    //let textTransform (inputColumn : string) outputColumn env =
    //    Microsoft.ML.Transforms.Text.TextTransform(env, inputColumn, outputColumn)

    let copyColumnsEstimator input output env =
        CopyColumnsEstimator(env, input, output)

    //let concatEstimator input output env =
    //    ConcatEstimator(env, input, output)

    //let append' (estimator : IEstimator<'b>) (pipeline : IEstimator<ITransformer>)  = 
    //    pipeline.Append estimator

    let append (estimator : IEstimator<'a>) (pipeline : IEstimator<'b>)  = 
        match pipeline with
        | :? IEstimator<ITransformer> as p -> 
            p.Append estimator
        | _ -> failwith "The pipeline has to be an instance of IEstimator<ITransformer>."


    let fit (dataView : IDataView) (pipeline : EstimatorChain<'a>) =
        pipeline.Fit dataView


    
    let downcast' (b : IEstimator<'a>) =
        match b with
        | :? IEstimator<ITransformer> as b -> b
        | _ -> failwith "qwe"
