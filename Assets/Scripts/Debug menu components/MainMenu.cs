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

            AddEntry("Restart game (debug)", () => RhythmicGame.Restart());
            AddEntry("Songs...", typeof(SongsMenu));

            AddEntry("Player Menu...", typeof(PlayerMenu));
            AddEntry("Debug events...", typeof(DebugEventsMenu));
            AddEntry("Audio Menu...", typeof(AudioMenu));

            //AddEntry("Give Max Health");
            //AddEntry("Give random powerup");

            AddEntry("Keep Player Between Loads", () => false);
            AddEntry("Worlds...", typeof(WorldsMenu));
            AddEntry("Tunnel mode", new Ref(() => RhythmicGame.IsTunnelMode, (v) => RhythmicGame.IsTunnelMode = (bool)v));
            AddEntry("Short Short Stats", () => ToggleStats(StatsMode.ShortShort), () => Stats.StatsMode == StatsMode.ShortShort);
            AddEntry("Short Stats", () => ToggleStats(StatsMode.Short), () => Stats.StatsMode == StatsMode.Short);
            AddEntry("Long Stats", () => ToggleStats(StatsMode.Long), () => Stats.StatsMode == StatsMode.Long);

            AddEntry("Visuals Menu...", false);

            AddEntry("Quit game", QuitGame);
        }

        // Functionality:

        void ToggleStats(StatsMode mode)
        {
            if (!Stats) return;
            Stats.StatsMode = (Stats.StatsMode == mode) ? StatsMode.None : mode;
        }

        public static void QuitGame()
        {
            Logger.LogE("Shutting down...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}
