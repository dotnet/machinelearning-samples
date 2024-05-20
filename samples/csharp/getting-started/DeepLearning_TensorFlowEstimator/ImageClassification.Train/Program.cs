using System;
using System.IO;
using System.Threading.Tasks;
using ImageClassification.Model;
using static ImageClassification.Model.ConsoleHelpers;
using Common;
using System.Collections.Generic;
using ImageClassification.DataModels;

namespace ImageClassification.Train
{
    public class Program
    {
        static void Main(string[] args)
        {
            string assetsRelativePath = @"../../../assets";
            string assetsPath = GetAbsolutePath(assetsRelativePath);

            // SINGLE SMALL FLOWERS IMAGESET (200 files)
            const string imagesDatasetZip = "flower_photos_small_set.zip";
            const string imagesDatasetUrl = "https://bit.ly/3fkRKYy";

            // SINGLE FULL FLOWERS IMAGESET (3,600 files)
            // const string imagesDatasetZip = "flower_photos.tgz";
            // const string imagesDatasetUrl = "http://download.tensorflow.org/example_images/" + imagesDatasetZip;

            // https://storage.googleapis.com/download.tensorflow.org/models/inception5h.zip = inception-v1.zip
            // https://bit.ly/3KjEiCH v3 inception-v3.zip
            // Does not work (despite the instructions in the README.md,
            //  but works fine in the F# version):
            //const string inceptionFile = "inception5h"; //"inception-v1";
            //const string inceptionGraph = "tensorflow_inception_graph.pb";
            // Works fine:
            const string inceptionFile = "inception-v3";
            const string inceptionGraph = "inception_v3_2016_08_28_frozen.pb";

            const string inceptionGraphZip = inceptionFile + ".zip";
            //const string inceptionGraphUrl =
            //    "https://storage.googleapis.com/download.tensorflow.org/models/inception5h.zip"; // v1
            const string inceptionGraphUrl = "https://bit.ly/3KjEiCH"; // v3

            // Inception v1 or v3
            var inceptionFolder = Path.Combine(assetsPath, "inputs", "tensorflow-pretrained-models");
            var inceptionPb = Path.Combine(inceptionFolder, inceptionFile, inceptionGraph);
            var commonGraphsRelativePath = @"../../../../../../../../graphs";
            var commonGraphsPath = GetAbsolutePath(commonGraphsRelativePath);
            
            var fullIncepionFolderPath = Path.Combine(
                inceptionFolder, Path.GetFileNameWithoutExtension(inceptionGraphZip));
            List<string> destGraphFile = new List<string>() { inceptionPb };
            Web.DownloadBigFile(fullIncepionFolderPath, inceptionGraphUrl, inceptionGraphZip, commonGraphsPath, destGraphFile);

            var imageClassifierZip = Path.Combine(assetsPath, "outputs", "imageClassifier.zip");

            var tagsTsv = Path.Combine(assetsPath, "inputs", "data", "tags.tsv");

            string imagesDownloadFolderPath = Path.Combine(assetsPath, "inputs", "images");
            string imagesFolder = imagesDownloadFolderPath;
            //string finalImagesFolderName = DownloadImageSet(imagesDownloadFolderPath);
            //var fullImagesetFolderPath = Path.Combine(imagesDownloadFolderPath, finalImagesFolderName);
            var fullImagesetFolderPath = Path.Combine(
                imagesFolder, Path.GetFileNameWithoutExtension(imagesDatasetZip));
            Console.WriteLine($"Images folder: {fullImagesetFolderPath}");
            var commonDatasetsRelativePath = @"../../../../../../../../datasets";
            var commonDatasetsPath = GetAbsolutePath(commonDatasetsRelativePath);
            var imagePath1 = Path.Combine(fullImagesetFolderPath, 
                "daisy", "286875003_f7c0e1882d.jpg");
            List<string> destFiles = new List<string>() { imagePath1 };
            Web.DownloadBigFile(imagesFolder, imagesDatasetUrl, imagesDatasetZip, 
                commonDatasetsPath, destFiles);

            // Single full dataset
            IEnumerable<ImageData> allImages = LoadImagesFromDirectory(folder: fullImagesetFolderPath,
                                                                       useFolderNameasLabel: true);
            try
            {              
                var modelBuilder = new ModelBuilder(inceptionPb, imageClassifierZip);

                modelBuilder.BuildAndTrain(allImages);
            }
            catch (Exception ex)
            {
                ConsoleWriteException(ex.ToString());
            }

            ConsolePressAnyKey();
        }

        public static IEnumerable<ImageData> LoadImagesFromDirectory(string folder, bool useFolderNameasLabel = true)
        {
            var files = Directory.GetFiles(folder, "*",
                searchOption: SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if ((Path.GetExtension(file) != ".jpg") && (Path.GetExtension(file) != ".png"))
                    continue;

                var label = Path.GetFileName(file);
                if (useFolderNameasLabel)
                    label = Directory.GetParent(file).Name;
                else
                {
                    for (int index = 0; index < label.Length; index++)
                    {
                        if (!char.IsLetter(label[index]))
                        {
                            label = label.Substring(0, index);
                            break;
                        }
                    }
                }

                yield return new ImageData()
                {
                    ImagePath = file,
                    Label = label
                };

            }
        }


        //public static string DownloadImageSet(string imagesDownloadFolder)
        //{
        //    // Download a set of images to teach the network about the new classes

        //    //SMALL FLOWERS IMAGESET (200 files)
        //    string fileName = "flower_photos_small_set.zip";
        //    string url = $"https://aka.ms/mlnet-resources/datasets/flower_photos_small_set.zip";
        //    Web.Download(url, imagesDownloadFolder, fileName);
        //    Compress.UnZip(Path.Join(imagesDownloadFolder, fileName), imagesDownloadFolder);

        //    //FULL FLOWERS IMAGESET (3,600 files)
        //    //string fileName = "flower_photos.tgz";
        //    //string url = $"http://download.tensorflow.org/example_images/{fileName}";
        //    //Web.Download(url, imagesDownloadFolder, fileName);
        //    //Compress.ExtractTGZ(Path.Join(imagesDownloadFolder, fileName), imagesDownloadFolder);

        //    return Path.GetFileNameWithoutExtension(fileName);
        //}

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}
