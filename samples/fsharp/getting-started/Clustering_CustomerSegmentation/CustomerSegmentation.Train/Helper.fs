namespace CustomerSegmentation

open Microsoft.ML
open Microsoft.ML.Core.Data
open Microsoft.ML.Runtime.Data
open Microsoft.ML.Transforms
open System
open System.IO
open System.Text
open System.Linq
open System.Collections.Generic


module CustomerSegmentationTextLoaderFactory = 
    let CreateTextLoader (mlContext : MLContext) = 
        mlContext.Data.TextReader(TextLoader.Arguments(
                                            Separator = ",",
                                            HasHeader = true,
                                            Column = [|
                                                        TextLoader.Column("Features", Nullable DataKind.R4, [|TextLoader.Range(0,Nullable 31)|])
                                                        TextLoader.Column("LastName", Nullable DataKind.Text, 32)   
                                                    |]
                                        ))

module ModelHelpers =
    type internal Marker = interface end
    let private _dataRoot = FileInfo(typeof<Marker>.Assembly.Location)

    let GetAssetsPath ( [<ParamArray>]paths : string []) =
        if isNull paths || paths.Length = 0 
        then null 
        else Path.Combine(paths.Prepend(_dataRoot.Directory.FullName).ToArray())

    let DeleteAssets ( [<ParamArray>]paths : string []) =
        let location = GetAssetsPath paths
        
        if (String.IsNullOrWhiteSpace(location) |> not) && File.Exists(location)
        then 
            File.Delete(location)
            location
        else location