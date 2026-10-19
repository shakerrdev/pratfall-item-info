using System;
using System.IO;
using Godot;
using ItemInfo;

public static class ModEntry
{
    private static Overlay _overlay;

    public static void ModInit()
    {
        try
        {
            var modDir = Path.GetDirectoryName(typeof(ModEntry).Assembly.Location);
            var config = Config.Load(Path.Combine(modDir ?? "", "config.json"));
            _overlay = new Overlay(config);
            _overlay.Attach();
            GD.Print("[ItemInfo] loaded");
        }
        catch (Exception e)
        {
            GD.PrintErr($"[ItemInfo] failed to load: {e}");
        }
    }

    public static void ModDestroy()
    {
        _overlay?.Detach();
        _overlay = null;
    }
}
