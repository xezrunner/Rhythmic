namespace DebugMenus
{
    class PlayerMenu : DebugMenuComponent
    {
        public static PlayerMenu Instance;

        public override void Init()
        {
            base.Init();
            Instance = this;

            AddEntry("Enable player input (global)", new Ref(() => AmpPlayerInputHandler.IsActive, (v) => AmpPlayerInputHandler.IsActive = (bool)v));
            AddEntry("Tunnel mode", new Ref(() => RhythmicGame.IsTunnelMode, (v) => RhythmicGame.IsTunnelMode = (bool)v));

            AddEntry("Enable seeker frames", new Ref(() => AmpTrackSection.SeekerEnabled, (v) => AmpTrackSection.SeekerEnabled = (bool)v));
            AddEntry("Enable track seeking", new Ref(() => RhythmicGame.TrackSeekingEnabled, (v) => RhythmicGame.TrackSeekingEnabled = (bool)v));
            AddEntry("Difficulty", new Ref(() => RhythmicGame.Difficulty, (v) => RhythmicGame.Difficulty = (RhythmicGame.GameDifficulty)v));
        }

        // Functionality:
    }
}
