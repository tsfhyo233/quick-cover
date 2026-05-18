using System;
using System.IO;
using System.Net.Http;

namespace QuickCover.Services
{
    public class ImageDownloadService
    {
        private static readonly HttpClient HttpClient = CreateHttpClient();

        public string DownloadToTempFile(string imageUrl)
        {
            if (!QuickCoverSettings.IsValidHttpUrl(imageUrl))
            {
                throw new ArgumentException("Image URL must be a valid http or https URL.", nameof(imageUrl));
            }

            using (var response = HttpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult())
            {
                response.EnsureSuccessStatusCode();

                var mediaType = response.Content.Headers.ContentType?.MediaType;
                if (!string.IsNullOrWhiteSpace(mediaType) && !mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("The URL did not return an image.");
                }

                var tempFilePath = Path.Combine(
                    Path.GetTempPath(),
                    $"quickcover_{Guid.NewGuid():N}{GetFileExtension(mediaType, imageUrl)}");

                using (var inputStream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                using (var outputStream = File.Create(tempFilePath))
                {
                    inputStream.CopyTo(outputStream);
                }

                return tempFilePath;
            }
        }

        private static HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            return httpClient;
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
