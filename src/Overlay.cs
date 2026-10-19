using System;
using System.Linq;
using Godot;

namespace ItemInfo;

internal sealed class Overlay
{
    private static readonly Color TitleColor = new(1f, 0.85f, 0.45f);
    private static readonly Color DescriptionColor = new(0.92f, 0.92f, 0.92f);
    private static readonly Color StatColor = new(0.65f, 0.9f, 1f);
    private const float ReferenceHeight = 900f;

    private readonly Config _config;
    private readonly ItemInfoBuilder _builder;

    private SceneTree _tree;
    private CanvasLayer _layer;
    private PanelContainer _panel;
    private Label _title;
    private Label _description;
    private Label _stats;

    private bool _enabled = true;
    private bool _toggleWasDown;
    private bool _reportedError;
    private string _lookText;
    private float _scale = -1;
    private StyleBoxFlat _style;
    private InteractableComponent _lookTarget;
    private double _nextCleanup;

    public Overlay(Config config)
    {
        _config = config;
        _builder = new ItemInfoBuilder(config);
    }

    public void Attach()
    {
        _tree = Engine.GetMainLoop() as SceneTree;
        if (_tree == null)
            throw new InvalidOperationException("no SceneTree");

        BuildUi();
        _tree.Root.CallDeferred(Node.MethodName.AddChild, _layer);
        _tree.ProcessFrame += OnFrame;
    }

    public void Detach()
    {
        if (_tree != null)
            _tree.ProcessFrame -= OnFrame;
        ClearLookText();
        if (_layer.IsValid())
            _layer.QueueFree();
        _layer = null;
    }

    private void BuildUi()
    {
        _layer = new CanvasLayer { Name = "ItemInfoOverlay", Layer = 5 };

        _style = new StyleBoxFlat
        {
            BgColor = new Color(0.06f, 0.05f, 0.04f, 0.8f),
            BorderColor = new Color(1f, 1f, 1f, 0.12f)
        };

        _panel = new PanelContainer
        {
            Name = "HeldItemPanel",
            Visible = false,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _panel.AddThemeStyleboxOverride("panel", _style);

        var box = new VBoxContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        _panel.AddChild(box);

        _title = MakeLabel(TitleColor);
        _description = MakeLabel(DescriptionColor);
        _stats = MakeLabel(StatColor);
        box.AddChild(_title);
        box.AddChild(_description);
        box.AddChild(_stats);

        _layer.AddChild(_panel);
    }

    private static Label MakeLabel(Color color)
    {
        var label = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.85f));
        return label;
    }

    private void ApplyScale()
    {
        if (!_panel.IsInsideTree())
            return;

        var height = _panel.GetViewport().GetVisibleRect().Size.Y;
        var scale = Mathf.Max(height / ReferenceHeight, 0.5f);
        if (Mathf.IsEqualApprox(scale, _scale))
            return;
        _scale = scale;

        var text = scale * _config.FontScale;
        SizeLabel(_title, 26 * text, scale);
        SizeLabel(_description, 18 * text, scale);
        SizeLabel(_stats, 18 * text, scale);
        ((VBoxContainer)_title.GetParent()).AddThemeConstantOverride("separation", Mathf.RoundToInt(4 * scale));

        _style.ContentMarginLeft = _style.ContentMarginRight = 16 * scale;
        _style.ContentMarginTop = 10 * scale;
        _style.ContentMarginBottom = 12 * scale;
        _style.SetCornerRadiusAll(Mathf.RoundToInt(10 * scale));
        _style.SetBorderWidthAll(Mathf.Max(1, Mathf.RoundToInt(2 * scale)));

        PlacePanel(scale);
    }

    private void SizeLabel(Label label, float fontSize, float scale)
    {
        label.CustomMinimumSize = new Vector2(_config.PanelWidth * scale, 0);
        label.AddThemeFontSizeOverride("font_size", Mathf.RoundToInt(fontSize));
        label.AddThemeConstantOverride("outline_size", Mathf.RoundToInt(fontSize / 6));
    }

    private void PlacePanel(float scale)
    {
        var position = _config.PanelPosition;
        var right = !position.Contains("left");
        var top = position.Contains("top");
        var bottom = position.Contains("bottom");
        var offset = _config.PanelOffset * scale;

        _panel.AnchorLeft = _panel.AnchorRight = right ? 1 : 0;
        _panel.GrowHorizontal = right ? Control.GrowDirection.Begin : Control.GrowDirection.End;
        _panel.OffsetLeft = _panel.OffsetRight = right ? -offset.X : offset.X;

        if (top || bottom)
        {
            _panel.AnchorTop = _panel.AnchorBottom = bottom ? 1 : 0;
            _panel.GrowVertical = bottom ? Control.GrowDirection.Begin : Control.GrowDirection.End;
            _panel.OffsetTop = _panel.OffsetBottom = bottom ? -offset.Y : offset.Y;
        }
        else
        {
            _panel.AnchorTop = _panel.AnchorBottom = 0.5f;
            _panel.GrowVertical = Control.GrowDirection.Both;
            _panel.OffsetTop = _panel.OffsetBottom = offset.Y;
        }
    }

    private void OnFrame()
    {
        try
        {
            HandleToggle();
            var player = Player.LocalPlayer;
            var hud = PlayerHudController.Instance;
            var active = _enabled && player.IsValid() && hud.IsValid() && HudVisible(hud)
                         && !(player.DeathComponent.IsValid() && player.DeathComponent.IsDead());

            ApplyScale();
            UpdateHeld(active ? player : null);
            UpdateLookAt(player, hud, active);

            var now = Time.GetTicksMsec() / 1000.0;
            if (now >= _nextCleanup)
            {
                _nextCleanup = now + 10;
                _builder.ForgetStale();
            }
        }
        catch (Exception e)
        {
            if (!_reportedError)
            {
                _reportedError = true;
                GD.PrintErr($"[ItemInfo] {e}");
            }
        }
    }

    private static bool HudVisible(PlayerHudController hud)
    {
        if (hud.RootLayer.IsValid() && !hud.RootLayer.Visible)
            return false;
        return !hud.RootControl.IsValid() || hud.RootControl.IsVisibleInTree();
    }

    private void HandleToggle()
    {
        var down = Input.IsKeyPressed(_config.ToggleKey);
        if (down && !_toggleWasDown)
            _enabled = !_enabled;
        _toggleWasDown = down;
    }

    private void UpdateHeld(Player player)
    {
        ItemInfo info = null;
        if (player != null && _config.ShowHeldItem)
            info = _builder.Build(player.InteractableHolderComponent.CurrentlyHeldInteractable);

        if (info == null)
        {
            _panel.Visible = false;
            return;
        }

        SetText(_title, info.Name);
        SetText(_description, info.Description);
        SetText(_stats, string.Join("\n", info.Stats.Select(s => "• " + s)));
        _panel.Visible = true;
    }

    private void UpdateLookAt(Player player, PlayerHudController hud, bool active)
    {
        var target = player.IsValid() && player.InteractionComponent.IsValid()
            ? Reflect.Get<InteractableComponent>(player.InteractionComponent, "_lastInteractable")
            : null;

        if (target != _lookTarget)
        {
            _lookTarget = target;
            _lookText = null;
        }

        var ours = target.IsValid() && target.GetDetailInteractableText == null;
        var info = active && _config.ShowLookAtItem && ours ? _builder.Build(target) : null;
        if (info == null)
        {
            if (_lookText != null && ours && hud.IsValid())
                hud.InteractionDetailPanel.Visible = false;
            _lookText = null;
            return;
        }

        var text = info.Stats.Count > 0 ? string.Join("  ·  ", info.Stats) : info.Description;
        if (text == _lookText && hud.InteractionDetailPanel.Visible)
            return;

        hud.InteractionDetailLabel.Text = text;
        hud.InteractionDetailPanel.Visible = true;
        _lookText = text;
    }

    private void ClearLookText()
    {
        if (_lookText == null)
            return;
        _lookText = null;

        var hud = PlayerHudController.Instance;
        if (hud.IsValid() && _lookTarget.IsValid() && _lookTarget.GetDetailInteractableText == null)
            hud.InteractionDetailPanel.Visible = false;
    }

    private static void SetText(Label label, string text)
    {
        text ??= "";
        if (label.Text != text)
            label.Text = text;
        label.Visible = text.Length > 0;
    }
}
