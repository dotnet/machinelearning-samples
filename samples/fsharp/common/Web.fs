module Common.Web

#nowarn "0044" // FS0044 WebClient: Type or member is obsolete

open System
open System.IO // Path
open System.IO.Compression // ZipFile
open System.Linq // List
open System.Net // WebClient
open System.Net.Http // HttpClient
open ICSharpCode.SharpZipLib.GZip // GZipInputStream
open ICSharpCode.SharpZipLib.Tar // TarArchive
open ICSharpCode.SharpZipLib.Zip // FastZip
open Common.FileUtil
open Flurl.Http // url.DownloadFileAsync

// https://gitlab.com/epiccash/epic-updater/-/blob/master/EpicUpdater/ServiceFunctions.fs
let tgzipDecompress downloadDir package (file : FileInfo) =
    use originalFileStream = file.OpenRead()
    let currentFileName = file.FullName
    let newFileName = Path.Combine(downloadDir, package)
    use gzipStream = new GZipInputStream(originalFileStream)
    use tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Text.Encoding.Default)
    tarArchive.ExtractContents(newFileName)

// Fixing redirection for NetCore application
// Unlike .Net 4, Net Core does not follow redirection from https to http, for new, higher security reasons
// However, it is still possible to follow the redirection explicitly, this is finally reasonable behavior
// Message=The remote server returned an error: (301) Moved Permanently.
// System.Net.WebException HResult=0x80131509 Source=System.Net.Requests
// https://github.com/dotnet/runtime/issues/23697

type MyHttpClient() =

    // HttpClient is intended to be instantiated once per application, rather than per-use
    // https://docs.microsoft.com/fr-fr/dotnet/api/system.net.http.httpclient?view=net-6.0
    let _httpClient = new HttpClient ()

    member public x.DownloadFileRedirect (address: string) (fileName: string) =
            
        //printfn "DownloadFile %s -> %s" address fileName
        //use hClient = new HttpClient ()

        use requestMessage = new HttpRequestMessage(HttpMethod.Get, address)

        use responseMessage = _httpClient.Send(requestMessage) 

        let checkEmpty x y =
            if x = null then y
            else x.ToString()
        let finalUrl = checkEmpty responseMessage.Headers.Location address
        //printfn "finalUrl %s" finalUrl
                            
        let destFolder = Path.GetDirectoryName(fileName)
        let destFilename = Path.GetFileName(fileName)
        // DownloadFileAsync is not a member of String: install Flurl.Http from NuGet
        finalUrl.DownloadFileAsync(destFolder, destFilename) |> Async.AwaitTask

    // Does not work?
    member public x.DownloadFileRedirectAsync (address: string) (fileName: string) =
        async {

            use requestMessage = new HttpRequestMessage(HttpMethod.Get, address)
                
            use! responseMessage = _httpClient.SendAsync(requestMessage) |> Async.AwaitTask
                
            let checkEmpty x y =
                if x = null then y
                else x.ToString()
            let finalUrl = checkEmpty responseMessage.Headers.Location address
                                            
            let destFolder = Path.GetDirectoryName(fileName)
            let destFilename = Path.GetFileName(fileName)
            // DownloadFileAsync is not a member of String: install Flurl.Http from NuGet
            let! destFullPath = finalUrl.DownloadFileAsync(destFolder, destFilename) |> Async.AwaitTask
            return destFullPath
        }

type MyWebClient() =
    inherit WebClient()

    // HttpClient is intended to be instantiated once per application, rather than per-use
    // https://docs.microsoft.com/fr-fr/dotnet/api/system.net.http.httpclient?view=net-6.0
    let _httpClient = new HttpClient ()

    member public x.DownloadFileRedirect (address: string) (fileName: string) =
            
        //printfn "DownloadFile %s -> %s" address fileName

        use requestMessage = new HttpRequestMessage(HttpMethod.Get, address)

        use responseMessage = _httpClient.Send(requestMessage) 

        let checkEmpty x y =
            if x = null then y
            else x.ToString()
        let finalUrl = checkEmpty responseMessage.Headers.Location address
        //printfn "finalUrl %s" finalUrl
                            
        use wClient = new WebClient() 
                            
        wClient.DownloadFile(Uri(finalUrl), fileName)

    // Does not work?
    member public x.DownloadFileRedirectAsync (address: string) (fileName: string) =
        async {

            use requestMessage = new HttpRequestMessage(HttpMethod.Get, address)
                
            use! responseMessage = _httpClient.SendAsync(requestMessage) |> Async.AwaitTask
                
            let checkEmpty x y =
                if x = null then y
                else x.ToString()
            let finalUrl = checkEmpty responseMessage.Headers.Location address
                                            
            use wClient = new WebClient() 
                                            
            wClient.DownloadFile(Uri(finalUrl), fileName)
            return fileName
        }

let DownloadBigFile (bigFileFolder:string) (bigFileUrl:string) (bigFileDest:string) (commonDatasetsPath:string) (destFiles:string list) (destFolder:string) (imageDownloadFolder:string) =

    let destPath = Path.Combine(bigFileFolder, bigFileDest)
    let commonPath = Path.Combine(commonDatasetsPath, bigFileDest)
    
    let destFullPath = Path.GetFullPath(destPath) // To simplify debugging
    let checkEmpty x =
        if String.IsNullOrEmpty(x) then ""
        else Path.GetFullPath(x)
    let destFolderFullPath = checkEmpty destFolder

    let commonFullPath = Path.GetFullPath(commonPath)

    let checkCopy destPathExists commonPathExists =
        if not (destPathExists) && commonPathExists then "copyFromCommon"
        else if destPathExists && not (commonPathExists) then "copyToCommon"
        else ""
        
    let destPathExists = File.Exists(destFullPath)
    let commonPathExists = File.Exists(commonFullPath)
    let result = (checkCopy destPathExists commonPathExists)

    if result = "copyFromCommon" then 
        CreateParentDirectoryIfNotExists destFullPath
        System.IO.File.Copy(commonFullPath, destFullPath)
    else if result = "copyToCommon" then System.IO.File.Copy(destFullPath, commonFullPath)

    let extensions = GetExtensions(destFullPath)
    let checkExtensions ext =
        if (ext = ".zip" || ext = ".tgz" || ext = ".tar.gz") then true
        else false
    let compressedFile = (checkExtensions extensions)

    let checkDir dest =
        if String.IsNullOrEmpty(dest) then true
        else Directory.Exists(dest)
    let destFolderExists = (checkDir destFolderFullPath)

    let checkDest (dest:string list) =
        if dest.Count() = 0 then false
        else true
    let destFilesExists = (checkDest destFiles)

    let chkDone destFilesExists destFiles destFullPath destFolderExists compressedFile = 
        if destFilesExists then
            let isAllFound list = List.forall (fun elem -> File.Exists(elem)) list
            ((isAllFound destFiles) && destFolderExists)
        else 
            File.Exists(destFullPath) && destFolderExists && not compressedFile

    let result = chkDone destFilesExists destFiles destFullPath destFolderExists compressedFile 
    if result then
        printfn "==== Data found ===="
    else

        if not (File.Exists destFullPath) then
            printfn "==== Downloading... ===="

            // The code below will download a dataset from a third-party,
            //  and may be governed by separate third-party terms
            // By proceeding, you agree to those separate terms
                
            // Works fine, but with .Net6, WebClient is now obsolete
            //let client = new MyWebClient()
            //client.DownloadFileRedirect bigFileUrl destFullPath
            // Does not work?
            //client.DownloadFileRedirectAsync bigFileUrl destFullPath |> ignore

            // Downloading a file is now very easy with .Net6... using Flurl.Http!
            let client = new MyHttpClient()
            let destFullPath = client.DownloadFileRedirect bigFileUrl destFullPath |> Async.RunSynchronously
            // Does not work?
            //client.DownloadFileRedirectAsync bigFileUrl destFullPath |> ignore

            if (File.Exists destFullPath) then
                printfn "==== Downloading is completed ===="
                if not (compressedFile) && not (File.Exists(commonFullPath)) then System.IO.File.Copy(destFullPath, commonFullPath)
            else
                printfn "==== Downloading: Fail! ===="
                Environment.Exit(0)
        
        if (compressedFile && File.Exists destFullPath) then
            printfn "==== Extracting data... ===="
            let checkExt extensions destFullPath bigFileFolder =
                try
                    if extensions = ".tar.gz" || extensions = ".tgz" then
                        let fi = new FileInfo(destFullPath)
                        let destDirectory = ""
                        tgzipDecompress bigFileFolder destDirectory fi
                        true
                    else if extensions = ".zip" then
                        let myFastZip = new FastZip()
                        myFastZip.ExtractZip(destFullPath, bigFileFolder, "")
                        true
                    else
                        false
                with
                    | ex -> printfn "Can't unzip, the zip file may be corrupted: %s " ex.Message; false

            let zipSuccess = (checkExt extensions destFullPath bigFileFolder)
            if zipSuccess then
                if not (File.Exists(commonFullPath)) then System.IO.File.Copy(destFullPath, commonFullPath)
            else
                // If the file is corrupted then download it again
                File.Delete(destFullPath);
                printfn "==== Downloading: Fail! ===="
                Environment.Exit(0)
            zipSuccess |> ignore

    let zippedPath = Path.Join(imageDownloadFolder, bigFileDest)
    let unzippedPath = Path.Join(imageDownloadFolder, Path.GetFileNameWithoutExtension(zippedPath))
    Path.Join(unzippedPath, Path.GetFileNameWithoutExtension(bigFileDest))    

let downloadZippedImageSetAsync (fileName:string) (downloadUrl:string) (imageDownloadFolder:string) = 
    async {
        let zippedPath = Path.Join(imageDownloadFolder, fileName)
        let unzippedPath = Path.Join(imageDownloadFolder, Path.GetFileNameWithoutExtension(zippedPath))
            
        if not (File.Exists zippedPath) then
            let client = new WebClient()
            client.DownloadFile(Uri(downloadUrl), zippedPath)
            
        if not (Directory.Exists unzippedPath) then
            ZipFile.ExtractToDirectory(zippedPath, unzippedPath)
            
        return Path.Join(unzippedPath, Path.GetFileNameWithoutExtension(fileName))
    }

let downloadZippedImageSet (fileName:string) (downloadUrl:string) (imageDownloadFolder:string) = 
    let zippedPath = Path.Join(imageDownloadFolder, fileName)
    let unzippedPath = Path.Join(imageDownloadFolder, Path.GetFileNameWithoutExtension(zippedPath))

    if not (File.Exists zippedPath) then
        let client = new WebClient()
        client.DownloadFile(Uri(downloadUrl), zippedPath)
                
    if not (Directory.Exists unzippedPath) then
        ZipFile.ExtractToDirectory(zippedPath, unzippedPath)
                
    Path.Join(unzippedPath, Path.GetFileNameWithoutExtension(fileName))