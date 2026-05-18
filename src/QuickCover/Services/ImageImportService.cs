using System;
using System.IO;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace QuickCover.Services
{
    public class ImageImportService
    {
        private readonly IPlayniteAPI playniteApi;

        public ImageImportService(IPlayniteAPI playniteApi)
        {
            this.playniteApi = playniteApi;
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

        public bool ApplyDefaultImages(Game game, string defaultCoverImagePath, string defaultBackgroundImagePath)
        {
            ValidateGame(game);

            var changed = false;

            if (!string.IsNullOrWhiteSpace(defaultCoverImagePath))
            {
                ValidateImagePath(defaultCoverImagePath);
                ReplaceCoverImage(game, defaultCoverImagePath);
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(defaultBackgroundImagePath))
            {
                ValidateImagePath(defaultBackgroundImagePath);
                ReplaceBackgroundImage(game, defaultBackgroundImagePath);
                changed = true;
            }

            if (changed)
            {
                playniteApi.Database.Games.Update(game);
            }

            return changed;
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
    }
}
