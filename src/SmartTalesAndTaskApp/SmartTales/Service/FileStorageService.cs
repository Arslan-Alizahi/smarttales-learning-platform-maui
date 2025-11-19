using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;

namespace SmartTales.Service
{
    public class FileStorageService
    {
        private readonly string _uploadPath;

        public FileStorageService()
        {
            // Create uploads directory in app's local data folder
            _uploadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SmartTales", "Uploads");
            Directory.CreateDirectory(_uploadPath);
            Console.WriteLine($"File storage path: {_uploadPath}");
        }

        /// <summary>
        /// Save an uploaded file to local storage
        /// </summary>
        /// <param name="file">The uploaded file</param>
        /// <param name="subfolder">Optional subfolder (e.g., "assignments", "submissions")</param>
        /// <returns>The relative file path for storage in database</returns>
        public async Task<string> SaveFileAsync(IBrowserFile file, string subfolder = "")
        {
            try
            {
                // Create subfolder if specified
                var targetDirectory = string.IsNullOrEmpty(subfolder) 
                    ? _uploadPath 
                    : Path.Combine(_uploadPath, subfolder);
                Directory.CreateDirectory(targetDirectory);

                // Generate unique filename to avoid conflicts
                var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}_{file.Name}";
                var filePath = Path.Combine(targetDirectory, fileName);

                // Save file to disk
                using var fileStream = new FileStream(filePath, FileMode.Create);
                await file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024).CopyToAsync(fileStream); // 50MB limit

                // Return relative path for database storage
                var relativePath = string.IsNullOrEmpty(subfolder) 
                    ? fileName 
                    : Path.Combine(subfolder, fileName);

                Console.WriteLine($"File saved: {file.Name} -> {relativePath}");
                return relativePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file {file.Name}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get the full file path from relative path stored in database
        /// </summary>
        /// <param name="relativePath">Relative path stored in database</param>
        /// <returns>Full file path</returns>
        public string GetFullPath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            return Path.Combine(_uploadPath, relativePath);
        }

        /// <summary>
        /// Check if a file exists
        /// </summary>
        /// <param name="relativePath">Relative path stored in database</param>
        /// <returns>True if file exists</returns>
        public bool FileExists(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return false;

            var fullPath = GetFullPath(relativePath);
            return File.Exists(fullPath);
        }

        /// <summary>
        /// Get file info for download
        /// </summary>
        /// <param name="relativePath">Relative path stored in database</param>
        /// <returns>File info or null if not found</returns>
        public FileInfo? GetFileInfo(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;

            var fullPath = GetFullPath(relativePath);
            return File.Exists(fullPath) ? new FileInfo(fullPath) : null;
        }

        /// <summary>
        /// Read file as byte array for download
        /// </summary>
        /// <param name="relativePath">Relative path stored in database</param>
        /// <returns>File content as byte array</returns>
        public async Task<byte[]?> ReadFileAsync(string relativePath)
        {
            try
            {
                if (string.IsNullOrEmpty(relativePath))
                    return null;

                var fullPath = GetFullPath(relativePath);
                if (!File.Exists(fullPath))
                    return null;

                return await File.ReadAllBytesAsync(fullPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file {relativePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Delete a file
        /// </summary>
        /// <param name="relativePath">Relative path stored in database</param>
        /// <returns>True if file was deleted successfully</returns>
        public bool DeleteFile(string relativePath)
        {
            try
            {
                if (string.IsNullOrEmpty(relativePath))
                    return false;

                var fullPath = GetFullPath(relativePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    Console.WriteLine($"File deleted: {relativePath}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file {relativePath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get original filename from stored path
        /// </summary>
        /// <param name="relativePath">Relative path stored in database</param>
        /// <returns>Original filename</returns>
        public string GetOriginalFileName(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            var fileName = Path.GetFileName(relativePath);
            
            // Extract original name from our naming convention: yyyyMMdd_HHmmss_guid_originalname
            var parts = fileName.Split('_');
            if (parts.Length >= 4)
            {
                // Join all parts after the timestamp and guid
                return string.Join("_", parts.Skip(3));
            }
            
            return fileName;
        }
    }
}
