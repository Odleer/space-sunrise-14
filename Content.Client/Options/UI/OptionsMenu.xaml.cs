using Content.Client.Options.UI.Tabs;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.Options.UI
{
    [GenerateTypedNameReferences]
    public sealed partial class OptionsMenu : DefaultWindow
    {
        public OptionsMenu()
        {
            RobustXamlLoader.Load(this);
            IoCManager.InjectDependencies(this);

            // Sunrise-Start
            Tabs.SetTabTitle(0, Loc.GetString("ui-options-tab-extra"));
            Tabs.SetTabTitle(1, Loc.GetString("ui-options-tab-misc"));
            Tabs.SetTabTitle(2, Loc.GetString("ui-options-tab-graphics"));
            Tabs.SetTabTitle(3, Loc.GetString("ui-options-tab-controls"));
            Tabs.SetTabTitle(4, Loc.GetString("ui-options-tab-audio"));
            Tabs.SetTabTitle(5, Loc.GetString("ui-options-tab-accessibility"));
            // Sunrise-End

            UpdateTabs();
        }

        public void UpdateTabs()
        {
            GraphicsTab.Control.ReloadValues();
            MiscTab.Control.ReloadValues();
            AccessibilityTab.Control.ReloadValues();
            AudioTab.Control.ReloadValues();
        }
    }
}
