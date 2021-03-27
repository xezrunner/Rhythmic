

namespace DebugMenus
{
    [DebugMenuComponent(hasMainMenuShortcut: false)]
    public class MainMenu : DebugMenuComponent
    {
        public static DebugMenuComponent Instance;

        public override void Init()
        {
            base.Init();

            Instance = this;

            AddEntry("Player Menu...", null);
            AddEntry("Give Max Health", null);
            AddEntry("Give random powerup", null);
            AddEntry("Keep Player Between Loads", null, false);
            AddEntry("Worlds", null);
            AddEntry("Scenes", null);
            AddEntry("Short Short Stats", () => DebugUI.Instance.SwitchToComponent(typeof(DebugStats)));
            AddEntry("Short Stats", null);
            AddEntry("Quit game", null);
        }

        // Functionality:


    }
}
