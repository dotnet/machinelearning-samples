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

            //Inception v3
            var inceptionPb = Path.Combine(assetsPath, "inputs", "tensorflow-pretrained-models", "inception-v3", "inception_v3_2016_08_28_frozen.pb");

            //Inception v1 (Second alternative)
            //var inceptionPb = Path.Combine(assetsPath, "inputs", "tensorflow-pretrained-models", "inception-v1", "tensorflow_inception_graph.pb");

            var imageClassifierZip = Path.Combine(assetsPath, "outputs", "imageClassifier.zip");

            var tagsTsv = Path.Combine(assetsPath, "inputs", "data", "tags.tsv");

            string imagesDownloadFolderPath = Path.Combine(assetsPath, "inputs", "images");
            string finalImagesFolderName = DownloadImageSet(imagesDownloadFolderPath);
            var fullImagesetFolderPath = Path.Combine(imagesDownloadFolderPath, finalImagesFolderName);
            Console.WriteLine($"Images folder: {fullImagesetFolderPath}");

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


        public static string DownloadImageSet(string imagesDownloadFolder)
        {
            // Download a set of images to teach the network about the new classes

            //SMALL FLOWERS IMAGESET (200 files)
            string fileName = "flower_photos_small_set.zip";
            string url = $"https://mlnetfilestorage.file.core.windows.net/imagesets/flower_images/flower_photos_small_set.zip?st=2019-08-07T21%3A27%3A44Z&se=2030-08-08T21%3A27%3A00Z&sp=rl&sv=2018-03-28&sr=f&sig=SZ0UBX47pXD0F1rmrOM%2BfcwbPVob8hlgFtIlN89micM%3D";
            Web.Download(url, imagesDownloadFolder, fileName);
            Compress.UnZip(Path.Join(imagesDownloadFolder, fileName), imagesDownloadFolder);

            //FULL FLOWERS IMAGESET (3,600 files)
            //string fileName = "flower_photos.tgz";
            //string url = $"http://download.tensorflow.org/example_images/{fileName}";
            //Web.Download(url, imagesDownloadFolder, fileName);
            //Compress.ExtractTGZ(Path.Join(imagesDownloadFolder, fileName), imagesDownloadFolder);

            return Path.GetFileNameWithoutExtension(fileName);
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}
