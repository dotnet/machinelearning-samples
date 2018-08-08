[<AutoOpen>]
module Load

// First reference net standard and then load in all the assemblies used by ML .NET.
#r "netstandard"
#load @"..\.paket\load\netstandard2.0\main.group.fsx"

// This code below is required in order to make native assemblies used by ML .NET visible to F#'s interactive prompt for scripts.
// It's not required if you create a console / web application etc.
let private nativeDirectory = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable "userprofile", @".nuget\packages\microsoft.ml\0.2.0\runtimes\win-x64\native")
System.Environment.SetEnvironmentVariable("Path", System.Environment.GetEnvironmentVariable "Path" + ";" + nativeDirectory)

module Datasets =
    let private buildPath file = System.IO.Path.Combine(@"..\datasets\", file)
    let Imdb = buildPath "sentiment-imdb-train.txt"
    let Yelp = buildPath "sentiment-yelp-test.txt"