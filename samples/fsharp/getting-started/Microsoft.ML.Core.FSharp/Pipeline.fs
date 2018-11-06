namespace Microsoft.ML.Core.FSharp

module Pipeline =
    open Microsoft.ML.Core.Data
    open Microsoft.ML.Runtime.Data


    let textTransform (inputColumn : string) outputColumn env =
        TextTransform(env, inputColumn, outputColumn)

    let concatEstimator name source env =
        ConcatEstimator(env,name, source)

    let append (estimator : IEstimator<'a>) (pipeline : IEstimator<'b>)  = 
        match pipeline with
        | :? IEstimator<ITransformer> as p -> 
            p.Append estimator
        | _ -> failwith "The pipeline has to be an instance of IEstimator<ITransformer>."

    let fit (dataView : IDataView) (pipeline : EstimatorChain<'a>) =
        pipeline.Fit dataView