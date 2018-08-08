## Getting Started with F# ML .NET samples

1. Run build.cmd or build.sh to download all dependencies required and generate any load scripts.
2. Open this folder (fsharp-samples) **as the root folder** in an IDE of your choice e.g. VS2017 or VSCode (if using VS Code, make sure that you have installed the [Ionide](http://ionide.io/) extension to use F#). This folder is necessary as the root in order for the folder paths to the datasets to work (otherwise you may need to modify the `buildPath` function in `load.fsx` to correct the path).
3. Open any of the samples e.g. Sentiment Analysis.
4. You can highlight individual lines of code and "send them" to the F# Interactive prompt (a REPL) to get rapid feedback. In Visual Studio 2017 or VS Code, this should be set to the `ALT` + `Enter` key combo (if not, you can bind it easily enough through settings of either IDE).