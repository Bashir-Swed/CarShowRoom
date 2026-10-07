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

        public async Task<string> SaveImageAsync(IFormFile imageFile,string folderName)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return string.Empty;
            }

            string uploadsFolder = Path.Combine(
                _environment.WebRootPath,
                "images",
                folderName
            );

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string safeOriginalFileName =
                Path.GetFileName(imageFile.FileName);

            string uniqueFileName =
                $"{Guid.NewGuid():N}_{safeOriginalFileName}";

            string filePath = Path.Combine(
                uploadsFolder,
                uniqueFileName
            );

            await using var fileStream =
                new FileStream(
                    filePath,
                    FileMode.CreateNew
                );

            await imageFile.CopyToAsync(fileStream);

            return $"/images/{folderName}/{uniqueFileName}";
        }


        public void DeleteImage(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }

            string webRoot =
                Path.GetFullPath(
                    _environment.WebRootPath
                );

            string normalizedPath =
                imagePath
                    .TrimStart('/')
                    .Replace(
                        '/',
                        Path.DirectorySeparatorChar
                    );

            string physicalPath =
                Path.GetFullPath(
                    Path.Combine(
                        webRoot,
                        normalizedPath
                    )
                );

            string allowedPrefix =
                webRoot.TrimEnd(
                    Path.DirectorySeparatorChar
                ) + Path.DirectorySeparatorChar;

            if (!physicalPath.StartsWith(
                allowedPrefix,
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
        }
    }
}