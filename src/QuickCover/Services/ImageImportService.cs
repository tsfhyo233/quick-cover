using System;
using System.Collections.Generic;
using System.IO;
using Playnite.SDK;
using Playnite.SDK.Models;
using QuickCover.Models;

namespace QuickCover.Services
{
    public class ImageImportService
    {
        private readonly IPlayniteAPI playniteApi;
        private readonly ImageDownloadService imageDownloadService;

        public ImageImportService(IPlayniteAPI playniteApi, ImageDownloadService imageDownloadService)
        {
            this.playniteApi = playniteApi;
            this.imageDownloadService = imageDownloadService;
        }

        public void SetCoverImage(Game game, string imagePath)
        {
            ValidateGame(game);
            ValidateImagePath(imagePath);

            ReplaceCoverImage(game, imagePath);
            playniteApi.Database.Games.Update(game);
        }

        public void SetBackgroundImage(Game game, string imagePath)
        {
            ValidateGame(game);
            ValidateImagePath(imagePath);

            ReplaceBackgroundImage(game, imagePath);
            playniteApi.Database.Games.Update(game);
        }

        public bool ApplyDefaultImages(
            Game game,
            string defaultCoverImagePath,
            string defaultCoverImageUrl,
            string defaultBackgroundImagePath,
            string defaultBackgroundImageUrl,
            DefaultImageSourceMode sourceMode)
        {
            ValidateGame(game);

            var tempFilePaths = new List<string>();

            try
            {
                var resolvedCoverImagePath = ResolveImageSource(defaultCoverImagePath, defaultCoverImageUrl, sourceMode, tempFilePaths);
                var resolvedBackgroundImagePath = ResolveImageSource(defaultBackgroundImagePath, defaultBackgroundImageUrl, sourceMode, tempFilePaths);
                var changed = false;

                if (!string.IsNullOrWhiteSpace(resolvedCoverImagePath))
                {
                    ReplaceCoverImage(game, resolvedCoverImagePath);
                    changed = true;
                }

                if (!string.IsNullOrWhiteSpace(resolvedBackgroundImagePath))
                {
                    ReplaceBackgroundImage(game, resolvedBackgroundImagePath);
                    changed = true;
                }

                if (changed)
                {
                    playniteApi.Database.Games.Update(game);
                }

                return changed;
            }
            finally
            {
                CleanupTempFiles(tempFilePaths);
            }
        }

        private string ResolveImageSource(string imagePath, string imageUrl, DefaultImageSourceMode sourceMode, IList<string> tempFilePaths)
        {
            if (sourceMode == DefaultImageSourceMode.PreferLocalThenUrl && !string.IsNullOrWhiteSpace(imagePath))
            {
                ValidateImagePath(imagePath);
                return imagePath;
            }

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return null;
            }

            if (!QuickCoverSettings.IsValidHttpUrl(imageUrl))
            {
                throw new ArgumentException("Image URL must be a valid http or https URL.", nameof(imageUrl));
            }

            var tempFilePath = imageDownloadService.DownloadToTempFile(imageUrl);
            tempFilePaths.Add(tempFilePath);
            return tempFilePath;
        }

        private static void ValidateGame(Game game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }
        }

        private static void ValidateImagePath(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                throw new ArgumentException("Image path is required.", nameof(imagePath));
            }

            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException("Image file was not found.", imagePath);
            }
        }

        private void ReplaceCoverImage(Game game, string imagePath)
        {
            if (!string.IsNullOrWhiteSpace(game.CoverImage))
            {
                playniteApi.Database.RemoveFile(game.CoverImage);
            }

            game.CoverImage = playniteApi.Database.AddFile(imagePath, game.Id);
        }

        private void ReplaceBackgroundImage(Game game, string imagePath)
        {
            if (!string.IsNullOrWhiteSpace(game.BackgroundImage))
            {
                playniteApi.Database.RemoveFile(game.BackgroundImage);
            }

            game.BackgroundImage = playniteApi.Database.AddFile(imagePath, game.Id);
        }

        private static void CleanupTempFiles(IEnumerable<string> tempFilePaths)
        {
            foreach (var tempFilePath in tempFilePaths)
            {
                try
                {
                    if (File.Exists(tempFilePath))
                    {
                        File.Delete(tempFilePath);
                    }
                }
                catch
                {
                }
            }
        }
    }
}
