# Item Info (Pratfall mod)

Shows what an item does and how much of it is left.

- **Held item:** a panel at the bottom left shows the item's name, a short description and live stats: shots, rockets or ziplines left, battery or flare time left, how much it heals, fuse and blast radius, the material it makes, and so on.
- **Looked-at item:** an item lying in the world shows its stats just above its name, in the game's own interaction label.
- **Silly Beans:** the panel shows which effect a bean gives and for how long. You can turn this off in the config.
- **F4** shows or hides everything.

## Install

1. Copy the `ItemInfo` folder into `<Pratfall install>\mods\`, so you end up with `mods\ItemInfo\ItemInfo.dll`.
2. Start the game, open the **Mods** menu and enable **Item Info**.

## Config

Edit `mods\ItemInfo\config.json` and restart the game:

| Key | Default | Meaning |
| --- | --- | --- |
| `ToggleKey` | `"F4"` | Key that shows or hides the info (any Godot key name) |
| `ShowHeldItem` | `true` | Panel for the item in your hand |
| `ShowLookAtItem` | `true` | Stats above the name of the item you look at |
| `RevealSillyBeans` | `true` | Show which effect a Silly Bean gives |
| `PanelPosition` | `"bottom-left"` | `bottom-right`, `bottom-left`, `top-right`, `top-left`, `right` or `left` |
| `PanelOffsetX` / `PanelOffsetY` | `60` / `150` | Distance from the screen edge, in pixels at 900p (scaled with the screen) |
| `PanelWidth` | `360` | Panel width, in pixels at 900p (scaled with the screen) |
| `FontScale` | `1.0` | Text size multiplier |

## Build

You need the .NET 8 SDK. The project references `Pratfall.dll` and `GodotSharp.dll` from your game install:

```
dotnet build -c Release -p:GameDir="D:\SteamLibrary\steamapps\common\Pratfall"
```

Copy `ItemInfo.dll`, `manifest.json` and `config.json` from `bin\Release` into `mods\ItemInfo`.

## How it works

The game loads `ItemInfo.dll` through its built-in mod loader, which calls `ModEntry.ModInit()`. The mod adds a HUD layer and refreshes it every frame. It reads the stats straight from the item's components, so the numbers always match the game. Descriptions are English only.
