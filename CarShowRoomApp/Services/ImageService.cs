using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
namespace CarShowRoomApp.Services
{
    public class ImageService
    {
        private readonly IWebHostEnvironment _environment;

        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveImageAsync(IFormFile imageFile, string folderName)
        {
            if (imageFile == null || imageFile.Length == 0)
                return string.Empty;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", folderName);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return $"/images/{folderName}/{uniqueFileName}";
        }


        public void DeleteImage(string imagePath)
            {
                if (string.IsNullOrEmpty(imagePath)) return;

                var normalizedPath = imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);

                var physicalPath = Path.Combine(_environment.WebRootPath, normalizedPath);

                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }
        }
    }
}