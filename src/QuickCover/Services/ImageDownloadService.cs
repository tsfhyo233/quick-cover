using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace QuickCover.Services
{
    public class ImageDownloadService
    {
        private static readonly HttpClient HttpClient = CreateHttpClient();

        private readonly string cacheDirectory;

        public ImageDownloadService(string cacheDirectory)
        {
            if (string.IsNullOrWhiteSpace(cacheDirectory))
            {
                throw new ArgumentException("Cache directory is required.", nameof(cacheDirectory));
            }

            this.cacheDirectory = cacheDirectory;
            Directory.CreateDirectory(cacheDirectory);
        }

        public string GetOrDownloadCachedFile(string imageUrl)
        {
            if (!QuickCoverSettings.IsValidHttpUrl(imageUrl))
            {
                throw new ArgumentException("Image URL must be a valid http or https URL.", nameof(imageUrl));
            }

            var cacheKey = GetCacheKey(imageUrl);
            var existingCachePath = GetExistingCachePath(cacheKey);
            if (!string.IsNullOrWhiteSpace(existingCachePath))
            {
                return existingCachePath;
            }

            using (var response = HttpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult())
            {
                response.EnsureSuccessStatusCode();

                var mediaType = response.Content.Headers.ContentType?.MediaType;
                if (!string.IsNullOrWhiteSpace(mediaType) && !mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("The URL did not return an image.");
                }

                var targetFilePath = Path.Combine(
                    cacheDirectory,
                    $"quickcover_{cacheKey}{GetFileExtension(mediaType, imageUrl)}");

                using (var inputStream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                using (var outputStream = File.Create(targetFilePath))
                {
                    inputStream.CopyTo(outputStream);
                }

                return targetFilePath;
            }
        }

        private static HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            return httpClient;
        }

        private string GetExistingCachePath(string cacheKey)
        {
            foreach (var filePath in Directory.GetFiles(cacheDirectory, $"quickcover_{cacheKey}.*"))
            {
                if (File.Exists(filePath))
                {
                    return filePath;
                }
            }

            return null;
        }

        private static string GetCacheKey(string imageUrl)
        {
            using (var sha256 = SHA256.Create())
            {
                var urlBytes = Encoding.UTF8.GetBytes(imageUrl.Trim());
                var hashBytes = sha256.ComputeHash(urlBytes);
                var hashBuilder = new StringBuilder(hashBytes.Length * 2);
                foreach (var hashByte in hashBytes)
                {
                    hashBuilder.Append(hashByte.ToString("x2"));
                }

                return hashBuilder.ToString();
            }
        }

        private static string GetFileExtension(string mediaType, string imageUrl)
        {
            switch (mediaType)
            {
                case "image/jpeg":
                case "image/jpg":
                    return ".jpg";
                case "image/png":
                    return ".png";
                case "image/webp":
                    return ".webp";
                case "image/gif":
                    return ".gif";
                case "image/bmp":
                    return ".bmp";
            }

            var urlPath = Path.GetExtension(new Uri(imageUrl, UriKind.Absolute).AbsolutePath);
            if (!string.IsNullOrWhiteSpace(urlPath) && urlPath.Length <= 6)
            {
                return urlPath;
            }

            return ".img";
        }
    }
}
