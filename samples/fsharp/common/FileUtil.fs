module Common.FileUtil
    
open System.IO // Path
open System.Globalization // StringInfo
            
let reverseString s =
    seq {
        let rator = StringInfo.GetTextElementEnumerator(s)
        while rator.MoveNext() do
        yield rator.GetTextElement()
    }
    |> Array.ofSeq
    |> Array.rev
    |> String.concat ""

let GetExtensions (filePath:string) =
    let longStr = filePath.Length
    let longStrWithoutDot = filePath.Replace(".", "").Length
    let nbDots = longStr - longStrWithoutDot
            
    if nbDots = 1 then 
        Path.GetExtension(filePath)
    else
        let reverseFilePath = reverseString filePath
        let firstDotPos = reverseFilePath.IndexOf(".")
        let secondDotPos = reverseFilePath.IndexOf(".", firstDotPos+1)
        let doubleExtRev = reverseFilePath.Substring(0, secondDotPos + 1)
        let doubleExt = reverseString doubleExtRev
        if (doubleExt.Length <= 10) then
            doubleExt
        else
            Path.GetExtension(filePath)

let CreateParentDirectoryIfNotExists (filename:string) =
    let parentDir = System.IO.Path.GetDirectoryName(filename)
    if not (Directory.Exists(parentDir)) then 
        Directory.CreateDirectory(parentDir) |> ignore

let CreateDirectoryIfNotExists (folder:string) =
    if not (Directory.Exists(folder)) then 
        Directory.CreateDirectory(folder) |> ignore

let CopyAndCreateDirectoryIfNotExists (sourceFile:string) (destFile:string) =
    CreateParentDirectoryIfNotExists sourceFile
    System.IO.File.Copy(sourceFile, destFile)