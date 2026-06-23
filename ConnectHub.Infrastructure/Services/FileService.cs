using ConnectHub.Application.Interfaces;
using ConnectHub.Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Infrastructure.Services
{
    public class FileService : IFileService
    {

        private readonly string _baseUploadPath;
        private readonly string _baseUrl;

        // Allowed image types
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
        private const int AvatarSize = 256; // resize to 256x256

        public FileService(IOptions<FileSettings> fileSettings)
        {
            var settings = fileSettings.Value;
            _baseUploadPath = settings.UploadPath;
            _baseUrl = settings.BaseUrl;
        }
        public void deleteFile(string fileUrl)
        {
            var relativePath= fileUrl.Replace(_baseUrl, "").TrimStart('/');
            var filePath = Path.Combine(_baseUploadPath, relativePath.Replace("Uploads/","")); // here we changing from url to real physical path in our project in order to delete the file

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public async Task<string> SaveFileAsync(Stream filestream/*file uploaded from user*/, string fileName, string folder) // this method is responsible for saving the uploaded file to the server and returning the URL of the saved file.
        {
            ValidateFile(fileName, filestream);

            var uploadFolder = Path.Combine(_baseUploadPath, folder); // wwwroot/Uploads/FolderName
            Directory.CreateDirectory(uploadFolder);  // creates a physical directory in my project and stores images in it

            var extension = Path.GetExtension(fileName).ToLowerInvariant(); // .png
            var uniqueFileName = $"{Guid.NewGuid()}{extension}"; // f47ac10b-58cc-4372-a567-0e02b2c3d479.png
            var filePath = Path.Combine(uploadFolder, uniqueFileName); // wwwroot/Uploads/FolderName/f47ac10b-58cc-4372-a567-0e02b2c3d479.png

            using var memoryStream = new MemoryStream();
            await filestream.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            //using var skBitmap = new SKBitmap();

            using var sKBitmap = SKBitmap.Decode(imageBytes);
            using var resized = sKBitmap.Resize(new SKImageInfo(AvatarSize, AvatarSize), SKFilterQuality.High);
            using var skImage = SKImage.FromBitmap(resized);

            var format = extension==".png" ? SKEncodedImageFormat.Png : SKEncodedImageFormat.Jpeg;
            using var data = skImage.Encode(format, 85); // 85 is the quality for jpeg
            await File.WriteAllBytesAsync(filePath, data.ToArray());

            return $"{_baseUrl}/Uploads/{folder}/{uniqueFileName}"; // http://localhost:5190/Uploads/FolderName/f47ac10b-58cc-4372-a567-0e02b2c3d479.png
        }

        private static void ValidateFile(string fileName, Stream filestream)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new Exception("Unsupported file type. Allowed types are: " + string.Join(", ", AllowedExtensions));
            if (filestream.Length > MaxFileSizeBytes)
                throw new Exception("File size exceeds the maximum allowed size of 5MB.");
        }   
    }
}
