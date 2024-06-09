using Flurl.Http; // url.DownloadFileAsync
using ICSharpCode.SharpZipLib.GZip; // GZipInputStream
using ICSharpCode.SharpZipLib.Tar; // TarArchive
using ICSharpCode.SharpZipLib.Zip; // FastZip
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http; // HttpClient
using System.Threading.Tasks;

namespace Common
{

    // Fixing redirection for NetCore application
    // Unlike .Net 4, Net Core does not follow redirection from https to http, for new, higher security reasons
    // However, it is still possible to follow the redirection explicitly, this is finally reasonable behavior
    // Message=The remote server returned an error: (301) Moved Permanently.
    // System.Net.WebException HResult=0x80131509 Source=System.Net.Requests
    // https://github.com/dotnet/runtime/issues/23697

    public static class HttpHelper
    {
        // HttpClient is intended to be instantiated once per application, rather than per-use
        // https://docs.microsoft.com/fr-fr/dotnet/api/system.net.http.httpclient?view=net-6.0
        private static readonly HttpClient _httpClient = new HttpClient();

        public static void DownloadFile(string url, string destPath)
        {
            var success = Task.WhenAny(DownloadFileAsyncRedirect(url, destPath)).Result;
        }

        private static async Task<bool> DownloadFileAsyncRedirect(string url, string destPath)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var httpResponseMessage =
                await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            string finalUrl;
            if (httpResponseMessage.Headers.Location == null)
                finalUrl = url; // Direct download without redirection
            else
                finalUrl = httpResponseMessage.Headers.Location.ToString();

            // IMPORTANT
            // .NET Core does not allow redirection from HTTPs -> HTTP and won't give a proper exception message
            // We have to account for that ourselves by not following redirections and instead creating a new request
            // Note that this behaviour doesn't exist in .NET Framework

            var destFolder = Path.GetDirectoryName(destPath);
            var destFilename = Path.GetFileName(destPath);
            // DownloadFileAsync is not a member of String: install Flurl.Http from NuGet
            var destFullPath = await finalUrl.DownloadFileAsync(destFolder, destFilename);

            return true;
        }
    }

    public class Web
    {
        public static void DownloadBigFile(string bigFileFolder, string bigFileUrl,
            string bigFileDest, string commonDatasetsPath,
            List<string> destFiles = null, string destFolder = null, 
            bool doNotUnzip = false)
        {
            string destPath = Path.Combine(bigFileFolder, bigFileDest);
            string commonPath = Path.Combine(commonDatasetsPath, bigFileDest);
            // To simplify debugging
            string destFullPath = Path.GetFullPath(destPath);
            string destFolderFullPath = "";
            if (!string.IsNullOrEmpty(destFolder))
                destFolderFullPath = Path.GetFullPath(destFolder);
            var commonFullPath = Path.GetFullPath(commonPath);

            if (!File.Exists(destFullPath) && File.Exists(commonFullPath)) {
                string parentDir = System.IO.Path.GetDirectoryName(destFullPath);
                if (!Directory.Exists(parentDir)) Directory.CreateDirectory(parentDir);
                System.IO.File.Copy(commonFullPath, destFullPath);
            }
            else if (File.Exists(destFullPath) && !File.Exists(commonFullPath))
                System.IO.File.Copy(destFullPath, commonFullPath);

            bool compressedFile = false;
            string extension = GetExtensions(destFullPath);
            if (extension == ".zip" || extension == ".tgz" || extension == ".tar.gz")
                compressedFile = true;

            bool dirExists = true;
            if (!string.IsNullOrEmpty(destFolderFullPath))
                dirExists = Directory.Exists(destFolderFullPath);

            if (destFiles == null) {
                if (File.Exists(destFullPath)) {
                    if (!compressedFile && dirExists) return;
                }
            }
            else {
                bool allFilesExist = true;
                foreach (var oneFile in destFiles) {
                    if (!File.Exists(oneFile)) {
                        allFilesExist = false; break;
                    }
                }
                if (allFilesExist && dirExists) return;
            }

            if (!File.Exists(destFullPath))
            {
                Console.WriteLine("==== Downloading... ====");
                string directoryPath = Path.GetDirectoryName(destFullPath);
                string directoryPath2 = Path.GetDirectoryName(directoryPath);
                if (!Directory.Exists(directoryPath2)) Directory.CreateDirectory(directoryPath2);
                if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

                // The code below will download a dataset from a third-party,
                //  and may be governed by separate third-party terms
                // By proceeding, you agree to those separate terms

                // Works fine, but with .Net6, WebClient is now obsolete
                //using (var client = new MyWebClient()) {
                //    client.DownloadFile(bigFileUrl, destFullPath);
                //}

                // Downloading a file is now very easy with .Net6... using Flurl.Http!
                HttpHelper.DownloadFile(bigFileUrl, destFullPath);
                
                if (File.Exists(destFullPath)) {
                    Console.WriteLine("==== Downloading is completed ====");
                }
                else {
                    Console.WriteLine("==== Downloading: Fail! ====");
                    Environment.Exit(0);
                }
            }

            if (File.Exists(destFullPath)) {
                Console.WriteLine("==== Extracting data... ====");
                bool unzipSuccess = true;
                if (!doNotUnzip) 
                try
                {
                    switch (extension)
                    {
                        case ".zip":
                            {
                                unzipSuccess = false;
                                FastZip myFastZip = new FastZip();
                                myFastZip.ExtractZip(destFullPath, bigFileFolder, fileFilter: string.Empty);
                                unzipSuccess = true;
                                break;
                            }

                        case ".tgz":
                        case ".tar.gz":
                            {
                                unzipSuccess = false;
                                using (var inputStream = File.OpenRead(destFullPath))
                                {
                                    using (var gzipStream = new GZipInputStream(inputStream))
                                    {
                                        using (TarArchive tarArchive = TarArchive.CreateInputTarArchive(
                                            gzipStream, nameEncoding: System.Text.Encoding.Default))
                                        {
                                            tarArchive.ExtractContents(bigFileFolder);
                                        }
                                    }
                                }
                                unzipSuccess = true;
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Can't unzip " +
                        Path.GetFileName(destFullPath) + ":");
                    Console.WriteLine(ex.Message);
                    // If the file is corrupted then download it again
                    File.Delete(destFullPath);
                }

                if (unzipSuccess && !File.Exists(commonFullPath))
                    System.IO.File.Copy(destFullPath, commonFullPath);

                var success = false;
                if (!string.IsNullOrEmpty(destFolderFullPath))
                    success = Directory.Exists(destFolderFullPath);
                else if (destFiles == null) {
                    if (File.Exists(destFullPath)) success = true;
                }
                else {
                    success = true;
                    foreach (var oneFile in destFiles) {
                        if (!File.Exists(oneFile)) success = false;
                    }
                }

                if (unzipSuccess && success)
                    Console.WriteLine("==== Extracting: Done. ====");
                else {
                    Console.WriteLine("==== Extracting: Fail! ====");
                    Environment.Exit(0);
                }
            }
        }

        public static string GetExtensions(string filePath)
        {
            int longStr = filePath.Length;
            int longStrWithoutDot = filePath.Replace(".", "").Length;
            int nbDots = longStr - longStrWithoutDot;
            if (nbDots == 2)
            {
                // Example: .tar.gz
                string reverseFilePath = Reverse(filePath);
                int firstDotPos = reverseFilePath.IndexOf(".");
                int secondDotPos = reverseFilePath.IndexOf(".", firstDotPos + 1);
                string doubleExtRev = reverseFilePath.Substring(0, secondDotPos + 1);
                string doubleExt = Reverse(doubleExtRev);
                int longStrDbleExt = doubleExt.Length;
                if (longStrDbleExt <= 10) return doubleExt;
            }
            return Path.GetExtension(filePath);
        }

        public static string Reverse(string s)
        {
            if (s == null) return null;
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }

#pragma warning disable SYSLIB0014 // WebClient: Type or member is obsolete

    class MyWebClient : WebClient
    {
        // HttpClient is intended to be instantiated once per application, rather than per-use
        // https://docs.microsoft.com/fr-fr/dotnet/api/system.net.http.httpclient?view=net-6.0
        private static readonly HttpClient _httpClient = new HttpClient();

        public new void DownloadFile(string url, string destPath)
        {
            var success = Task.WhenAny(DownloadFileAsyncRedirect(url, destPath)).Result;
        }

        private static async Task<bool> DownloadFileAsyncRedirect(string url, string destPath)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var httpResponseMessage = 
                await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            string finalUrl;
            if (httpResponseMessage.Headers.Location == null)
                finalUrl = url; // Direct download without redirection
            else
                finalUrl = httpResponseMessage.Headers.Location.ToString();

            // IMPORTANT
            // .NET Core does not allow redirection from HTTPs -> HTTP and won't give a proper exception message
            // We have to account for that ourselves by not following redirections and instead creating a new request
            // Note that this behaviour doesn't exist in .NET Framework
            using (var wClient = new WebClient())
            {
                wClient.DownloadFile(finalUrl, destPath);
            }

            return true;
        }
    }

#pragma warning restore SYSLIB0014 // WebClient: Type or member is obsolete

}
