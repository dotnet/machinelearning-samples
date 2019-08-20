using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;


namespace OnnxObjectDetectionWeb.Infrastructure
{
    /// <summary>
    /// Interface to use in DI/IoC
    /// </summary>
    public interface IImageFileWriter
    {
        Task<string> UploadImageAsync(IFormFile file, string imagesTempFolder);
        void DeleteImageTempFile(string filePathName);
    }

    /// <summary>
    /// Implementation class to inject with DI/IoC
    /// </summary>
    public class ImageFileWriter : IImageFileWriter
    {
        public async Task<string> UploadImageAsync(IFormFile file, string imagesTempFolder)
        {
            if (CheckIfImageFile(file))
            {
                return await WriteFile(file, imagesTempFolder);
            }

            return "Invalid image file";
        }

        /// <summary>
        /// Method to check if file is image file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private bool CheckIfImageFile(IFormFile file)
        {
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                fileBytes = ms.ToArray();
            }

            return ImageValidationExtensions.GetImageFormat(fileBytes) != ImageValidationExtensions.ImageFormat.unknown;
        }

        /// <summary>
        /// Method to write file onto the disk
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<string> WriteFile(IFormFile file, string imagesTempFolder)
        {
            string fileName;
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                fileName = Guid.NewGuid().ToString() + extension; //Create a new name for the file 

                var filePathName = Path.Combine(Directory.GetCurrentDirectory(), imagesTempFolder, fileName);

                using (var fileStream = new FileStream(filePathName, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return fileName;
        }

        public void DeleteImageTempFile(string filePathName)
        {
            try
            {
                File.Delete(filePathName);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}
