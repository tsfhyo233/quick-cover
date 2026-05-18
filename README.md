# Quick Cover

Quick Cover is a Playnite Generic Plugin for quickly setting a game's cover and background images from local files.

## Features

- Add `Quick Cover` items to the game context menu in Playnite
- Set a game's cover image from a local file
- Set a game's background image from a local file
- Configure default cover and background images in plugin settings
- Apply configured default images to selected games
- Preview configured default images directly in the settings page

## Current behavior

`Apply Default Images` overwrites the current cover and background when a default image is configured.

## Requirements

- Playnite
- .NET SDK installed for local development
- Windows environment for building and testing the plugin

## Project structure

- `extension.yaml` - Playnite extension manifest
- `src/QuickCover/` - plugin source code
- `src/QuickCover/QuickCover.csproj` - project file

## Build

Debug build:

```powershell
dotnet build "src/QuickCover/QuickCover.csproj" -c Debug
```

Release build:

```powershell
dotnet build "src/QuickCover/QuickCover.csproj" -c Release
```

The plugin output is written to:

- `build/playnite-dev/`

## Test in Playnite

1. Build the project.
2. In Playnite, open `Settings -> For developers -> External extensions`.
3. Add the folder `build/playnite-dev/`.
4. Fully restart Playnite.
5. Right-click a game and open the `Quick Cover` menu.

## Usage

### Set cover manually

- Right-click a game
- Open `Quick Cover`
- Click `Set Cover From File...`

### Set background manually

- Right-click a game
- Open `Quick Cover`
- Click `Set Background From File...`

### Configure default images

- Open the Quick Cover plugin settings in Playnite
- Select a default cover image
- Select a default background image
- Review the preview thumbnails

### Apply default images

- Select one or more games
- Right-click and open `Quick Cover`
- Click `Apply Default Images`

## Notes

- The plugin currently uses local image files as the source for both manual and default image application.
- If a configured default image file is moved or deleted, applying defaults will fail until the path is updated in settings.
- Because Playnite loads the plugin DLL, you may need to fully close Playnite before rebuilding if the output DLL is locked.

## Manifest

Current extension metadata:

- Name: `Quick Cover`
- Version: `0.1`
- Type: `GenericPlugin`

## Roadmap ideas

- Apply images from URL
- Separate overwrite/only-fill-missing modes
- Harmony theme integration
- Packaging for easier release distribution
