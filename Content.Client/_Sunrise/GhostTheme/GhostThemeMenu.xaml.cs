// © SUNRISE, An EULA/CLA with a hosting restriction, full text: https://github.com/space-sunrise/space-station-14/blob/master/CLA.txt
using System.Numerics;
using Content.Client.Stylesheets;
using Content.Shared._Sunrise.GhostTheme;
using Content.Shared._Sunrise.SunriseCCVars;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Client.Utility;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Client._Sunrise.GhostTheme;

[GenerateTypedNameReferences]
public sealed partial class GhostThemeMenu : DefaultWindow
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public event Action<string>? OnIdSelected;

    private List<string> _availableGhostThemes = [];

    public GhostThemeMenu()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);
    }

    public void UpdateState(List<string> ghostThemes)
    {
        _availableGhostThemes = ghostThemes;
        UpdateGrid();
    }

    private void UpdateGrid()
    {
        ClearGrid();

        foreach (var ghostTheme in _availableGhostThemes)
        {
            if (!_prototypeManager.TryIndex(ghostTheme, out GhostThemePrototype? ghostThemePrototype))
                continue;

            var button = new Button
            {
                SetSize = new Vector2(128, 128),
                HorizontalExpand = true,
                ToggleMode = false,
                StyleClasses = {StyleBase.ButtonSquare},
            };
            button.OnPressed += _ =>
            {
                OnIdSelected?.Invoke(ghostTheme);
                _cfg.SetCVar(SunriseCCVars.SponsorGhostTheme, ghostTheme);
                _cfg.SaveToFile();
            };
            Grid.AddChild(button);

            var ghost = new TextureRect()
            {
                Texture = ghostThemePrototype.Sprite.Frame0(),
                Stretch = TextureRect.StretchMode.KeepAspectCentered,
            };

            button.AddChild(ghost);
        }
    }

    private void ClearGrid()
    {
        Grid.RemoveAllChildren();
    }
}
