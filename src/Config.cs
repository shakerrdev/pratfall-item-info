using System;
using System.IO;
using System.Text.Json;
using Godot;

namespace ItemInfo;

public sealed class Config
{
    public bool ShowHeldItem { get; private set; } = true;
    public bool ShowLookAtItem { get; private set; } = true;
    public bool RevealSillyBeans { get; private set; } = true;
    public Key ToggleKey { get; private set; } = Key.F4;
    public string PanelPosition { get; private set; } = "bottom-right";
    public Vector2 PanelOffset { get; private set; } = new(24, 160);
    public float PanelWidth { get; private set; } = 360;
    public float FontScale { get; private set; } = 1f;

    public static Config Load(string path)
    {
        var config = new Config();
        if (!File.Exists(path))
            return config;

        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path), new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });
            var root = doc.RootElement;
            config.ShowHeldItem = Bool(root, "ShowHeldItem", config.ShowHeldItem);
            config.ShowLookAtItem = Bool(root, "ShowLookAtItem", config.ShowLookAtItem);
            config.RevealSillyBeans = Bool(root, "RevealSillyBeans", config.RevealSillyBeans);
            config.PanelPosition = Str(root, "PanelPosition", config.PanelPosition).ToLowerInvariant();
            config.PanelOffset = new Vector2(
                Num(root, "PanelOffsetX", config.PanelOffset.X),
                Num(root, "PanelOffsetY", config.PanelOffset.Y));
            config.PanelWidth = Num(root, "PanelWidth", config.PanelWidth);
            config.FontScale = Math.Clamp(Num(root, "FontScale", config.FontScale), 0.5f, 3f);

            var keyName = Str(root, "ToggleKey", "");
            if (keyName.Length > 0)
            {
                var key = OS.FindKeycodeFromString(keyName);
                if (key != Key.None)
                    config.ToggleKey = key;
                else
                    GD.PrintErr($"[ItemInfo] unknown ToggleKey '{keyName}', keeping {config.ToggleKey}");
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"[ItemInfo] could not read config.json, using defaults: {e.Message}");
        }

        return config;
    }

    private static bool Bool(JsonElement root, string name, bool fallback) =>
        root.TryGetProperty(name, out var v) && v.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? v.GetBoolean()
            : fallback;

    private static float Num(JsonElement root, string name, float fallback) =>
        root.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetSingle() : fallback;

    private static string Str(JsonElement root, string name, string fallback) =>
        root.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : fallback;
}
