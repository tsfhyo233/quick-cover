using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using QuickCover.Actions;
using QuickCover.Models;
using QuickCover.Services;

namespace QuickCover
{
    public class QuickCoverPlugin : GenericPlugin
    {
        private static readonly Guid PluginId = Guid.Parse("ef18df4f-b6a1-4ab3-9a6f-67b8bbd4f8f8");

        private readonly QuickCoverSettings settings;
        private readonly ImageDownloadService imageDownloadService;
        private readonly SetCoverFromFileAction setCoverFromFileAction;
        private readonly SetBackgroundFromFileAction setBackgroundFromFileAction;
        private readonly ApplyDefaultImagesAction applyDefaultImagesAction;
        private readonly ApplyDefaultImagesAction applyDefaultUrlImagesAction;
        private readonly ApplyDefaultImagesAction applyDefaultCoverImageAction;
        private readonly ApplyDefaultImagesAction applyDefaultBackgroundImageAction;
        private readonly string topPanelIconPath;

        public QuickCoverPlugin(IPlayniteAPI api) : base(api)
        {
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            settings = new QuickCoverSettings(this);

            var cacheDirectory = System.IO.Path.Combine(GetPluginUserDataPath(), "cache");
            imageDownloadService = new ImageDownloadService(cacheDirectory);
            var imageImportService = new ImageImportService(api, imageDownloadService);
            setCoverFromFileAction = new SetCoverFromFileAction(api, imageImportService);
            setBackgroundFromFileAction = new SetBackgroundFromFileAction(api, imageImportService);
            applyDefaultImagesAction = new ApplyDefaultImagesAction(api, imageImportService, settings, DefaultImageSourceMode.PreferLocalThenUrl);
            applyDefaultUrlImagesAction = new ApplyDefaultImagesAction(api, imageImportService, settings, DefaultImageSourceMode.UrlOnly);
            applyDefaultCoverImageAction = new ApplyDefaultImagesAction(api, imageImportService, settings, DefaultImageSourceMode.PreferLocalThenUrl, DefaultImageTarget.CoverOnly);
            applyDefaultBackgroundImageAction = new ApplyDefaultImagesAction(api, imageImportService, settings, DefaultImageSourceMode.PreferLocalThenUrl, DefaultImageTarget.BackgroundOnly);
            topPanelIconPath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "source", "icon.png");
        }

        public override Guid Id => PluginId;

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new QuickCoverSettingsView(PlayniteApi, settings, imageDownloadService);
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            yield return new GameMenuItem
            {
                Description = "Set Cover From File...",
                MenuSection = "Quick Cover",
                Action = menuArgs => setCoverFromFileAction.Execute(menuArgs.Games)
            };

            yield return new GameMenuItem
            {
                Description = "Set Background From File...",
                MenuSection = "Quick Cover",
                Action = menuArgs => setBackgroundFromFileAction.Execute(menuArgs.Games)
            };

            yield return new GameMenuItem
            {
                Description = "Apply Default Images",
                MenuSection = "Quick Cover",
                Action = menuArgs => applyDefaultImagesAction.Execute(menuArgs.Games)
            };

            yield return new GameMenuItem
            {
                Description = "Apply Default URL Images",
                MenuSection = "Quick Cover",
                Action = menuArgs => applyDefaultUrlImagesAction.Execute(menuArgs.Games)
            };

            yield return new GameMenuItem
            {
                Description = "Apply Default Cover Image",
                MenuSection = "Quick Cover",
                Action = menuArgs => applyDefaultCoverImageAction.Execute(menuArgs.Games)
            };

            yield return new GameMenuItem
            {
                Description = "Apply Default Background Image",
                MenuSection = "Quick Cover",
                Action = menuArgs => applyDefaultBackgroundImageAction.Execute(menuArgs.Games)
            };
        }

        public override IEnumerable<TopPanelItem> GetTopPanelItems()
        {
            yield return new TopPanelItem
            {
                Title = "Quick Cover",
                Icon = CreateTopPanelIcon(),
                Activated = () => applyDefaultImagesAction.Execute(GetSelectedGames()),
                Visible = true
            };
        }

        private IEnumerable<Game> GetSelectedGames()
        {
            return PlayniteApi.MainView.SelectedGames?.ToList() ?? new List<Game>();
        }

        private object CreateTopPanelIcon()
        {
            if (!File.Exists(topPanelIconPath))
            {
                return null;
            }

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(topPanelIconPath, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();

            var iconSource = new CroppedBitmap(bitmap, CreateCenteredSquareCrop(bitmap.PixelWidth, bitmap.PixelHeight, 0.8));
            iconSource.Freeze();

            return new Image
            {
                Source = iconSource,
                Width = 22,
                Height = 22,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                RenderTransform = new TranslateTransform(2, 2)
            };
        }

        private static System.Windows.Int32Rect CreateCenteredSquareCrop(int pixelWidth, int pixelHeight, double cropRatio)
        {
            var squareSize = (int)Math.Round(Math.Min(pixelWidth, pixelHeight) * cropRatio);
            var x = Math.Max(0, (pixelWidth - squareSize) / 2);
            var y = Math.Max(0, (pixelHeight - squareSize) / 2);
            return new System.Windows.Int32Rect(x, y, squareSize, squareSize);
        }

    }
}
