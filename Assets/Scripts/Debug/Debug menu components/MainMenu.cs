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
            AddEntry("Debug events...", typeof(DebugEventsMenu));
            //AddEntry("Give Max Health");
            //AddEntry("Give random powerup");
            AddEntry("Keep Player Between Loads", () => false);
            AddEntry("Tunnel mode", new Ref(() => RhythmicGame.IsTunnelMode, (v) => RhythmicGame.IsTunnelMode = (bool)v));
            AddEntry("Enable seeker frames", new Ref(() => AmpTrackSection.SeekerEnabled, (v) => AmpTrackSection.SeekerEnabled = (bool)v), "T_EXTRA: Move to own subsection!");
            AddEntry("Enable note target info", new Ref(() => AmpNote.DEBUG_ShowTargetNoteIndicators, (v) => AmpNote.DEBUG_ShowTargetNoteIndicators = (bool)v), "T_EXTRA: Move to own subsection!");
            AddEntry("Songs...", typeof(SongsMenu));
            AddEntry("Worlds...", typeof(WorldsMenu));
            AddEntry("Short Short Stats", () => ToggleStats(StatsMode.ShortShort), () => Stats.StatsMode == StatsMode.ShortShort);
            AddEntry("Short Stats", () => ToggleStats(StatsMode.Short), () => Stats.StatsMode == StatsMode.Short);
            AddEntry("Long Stats", () => ToggleStats(StatsMode.Long), () => Stats.StatsMode == StatsMode.Long);

            AddEntry("Sound Menu...", false);
            AddEntry("Visuals Menu...", false);

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
            UnityEngine.Application.Quit();
#endif
        }
    }
}
