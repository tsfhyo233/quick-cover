using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using QuickCover.Actions;
using QuickCover.Services;

namespace QuickCover
{
    public class QuickCoverPlugin : GenericPlugin
    {
        private static readonly Guid PluginId = Guid.Parse("ef18df4f-b6a1-4ab3-9a6f-67b8bbd4f8f8");

        private readonly QuickCoverSettings settings;
        private readonly SetCoverFromFileAction setCoverFromFileAction;
        private readonly SetBackgroundFromFileAction setBackgroundFromFileAction;
        private readonly ApplyDefaultImagesAction applyDefaultImagesAction;

        public QuickCoverPlugin(IPlayniteAPI api) : base(api)
        {
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            settings = new QuickCoverSettings(this);

            var imageImportService = new ImageImportService(api);
            setCoverFromFileAction = new SetCoverFromFileAction(api, imageImportService);
            setBackgroundFromFileAction = new SetBackgroundFromFileAction(api, imageImportService);
            applyDefaultImagesAction = new ApplyDefaultImagesAction(api, imageImportService, settings);
        }

        public override Guid Id => PluginId;

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new QuickCoverSettingsView(PlayniteApi, settings);
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
        }
    }
}
