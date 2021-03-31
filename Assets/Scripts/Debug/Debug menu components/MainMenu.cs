using UnityEngine;

namespace DebugMenus
{
    [DebugMenuComponent(false, updateMs: 250)]
    public class MainMenu : DebugMenuComponent
    {
        public static MainMenu Instance;

        DebugStats Stats { get { return (DebugStats)DebugStats.Instance.Component; } }

        float f_numberTest = 0.12344f;
        int i_numberTest = 4;

        public override void Init()
        {
            base.Init();
            Instance = this;

            AddEntry("Player Menu...", typeof(PlayerMenu));
            //AddEntry("Give Max Health");
            //AddEntry("Give random powerup");
            AddEntry("Keep Player Between Loads", () => false);
            AddEntry("Tunnel mode", new Ref(() => RhythmicGame.IsTunnelMode, (v) => RhythmicGame.IsTunnelMode = (bool)v));
            AddEntry("Enable seeker frames", new Ref(() => AmpTrackSection.SeekerEnabled, (v) => AmpTrackSection.SeekerEnabled = (bool)v));
            AddEntry("Songs...", typeof(SongsMenu));
            AddEntry("Worlds...", typeof(WorldsMenu));
            //AddEntry("Scenes...");
            AddEntry("Short Short Stats", () => ToggleStats(StatsMode.ShortShort), () => Stats.StatsMode == StatsMode.ShortShort);
            AddEntry("Short Stats", () => ToggleStats(StatsMode.Short), () => Stats.StatsMode == StatsMode.Short);

            //AddEntry();
            //AddEntry("(float) Number variable test", new Ref(() => f_numberTest, (v) => f_numberTest = (float)v));
            //AddEntry("(int) Number variable test", new Ref(() => i_numberTest, (v) => i_numberTest = (int)v));
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
