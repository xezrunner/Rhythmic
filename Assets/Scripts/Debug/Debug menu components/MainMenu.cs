using UnityEngine;
using static DebugFunctionality;


namespace DebugMenus
{
    [DebugMenuComponent(false, updateMs: 250)]
    public class MainMenu : DebugMenuComponent
    {
        public static MainMenu Instance;

        DebugStats Stats { get { return (DebugStats)DebugStats.Instance.Component; } }

        public override void Init()
        {
            base.Init();
            Instance = this;

            AddEntry("Player Menu...", typeof(PlayerMenu));
            AddEntry("Give Max Health");
            AddEntry("Give random powerup");
            AddEntry("Keep Player Between Loads", () => false);
            AddEntry("Tunnel mode", new Ref<object>(() => RhythmicGame.IsTunnelMode, (v) => RhythmicGame.IsTunnelMode = (bool)v));
            AddEntry("Songs...", typeof(SongsMenu));
            AddEntry("Worlds...", typeof(WorldsMenu));
            //AddEntry("Scenes...");
            AddEntry("Short Short Stats", () => ToggleStats(StatsMode.ShortShort), () => Stats.StatsMode == StatsMode.ShortShort);
            AddEntry("Short Stats", () => ToggleStats(StatsMode.Short), () => Stats.StatsMode == StatsMode.Short, "This is some extra text.");
            AddEntry("Quit game", QuitGame);
        }

        // Functionality:

        void ToggleStats(StatsMode mode)
        {
            if (!Stats) return;
            Stats.StatsMode = (Stats.StatsMode == mode) ? StatsMode.None : mode;
        }

        void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
