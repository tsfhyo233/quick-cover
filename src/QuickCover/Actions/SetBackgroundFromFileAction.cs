using System;
using System.Collections.Generic;
using System.Linq;
using Playnite.SDK;
using Playnite.SDK.Models;
using QuickCover.Services;

namespace QuickCover.Actions
{
    public class SetBackgroundFromFileAction
    {
        private const string SupportedImageFilter = "Image Files|*.jpg;*.jpeg;*.png;*.webp|All Files|*.*";

        private readonly IPlayniteAPI playniteApi;
        private readonly ImageImportService imageImportService;

        public SetBackgroundFromFileAction(IPlayniteAPI playniteApi, ImageImportService imageImportService)
        {
            this.playniteApi = playniteApi;
            this.imageImportService = imageImportService;
        }

        public void Execute(IEnumerable<Game> games)
        {
            var selectedGames = games?.ToList() ?? new List<Game>();
            if (selectedGames.Count != 1)
            {
                ShowNotification("Please select exactly one game.", NotificationType.Error);
                return;
            }

            var imagePath = playniteApi.Dialogs.SelectFile(SupportedImageFilter);
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }

            var game = selectedGames[0];

            try
            {
                imageImportService.SetBackgroundImage(game, imagePath);
                ShowNotification($"Background updated for {game.Name}.", NotificationType.Info);
            }
            catch (Exception exception)
            {
                ShowNotification($"Failed to update background: {exception.Message}", NotificationType.Error);
            }
        }

        private void ShowNotification(string message, NotificationType notificationType)
        {
            playniteApi.Notifications.Add(new NotificationMessage(
                $"QuickCover_{Guid.NewGuid():N}",
                message,
                notificationType));
        }
    }
}
