namespace DebugMenus
{
    class PlayerMenu : DebugMenuComponent
    {
        public static PlayerMenu Instance;
        TrackStreamer TrackStreamer { get { return TrackStreamer.Instance; } }

        public override void Init()
        {
            base.Init();
            Instance = this;

            AddEntry("Enable player input (global)", new Ref(() => PlayerInputHandler.IsActive, (v) => PlayerInputHandler.IsActive = (bool)v));
            AddEntry("Tunnel mode", new Ref(() => RhythmicGame.IsTunnelMode, (v) => RhythmicGame.IsTunnelMode = (bool)v));

            AddEntry("Enable seeker frames", new Ref(() => TracksController.SeekerEnabled, (v) => TracksController.SeekerEnabled = (bool)v));
            AddEntry("Enable track seeking", new Ref(() => RhythmicGame.TrackSeekingEnabled, (v) => RhythmicGame.TrackSeekingEnabled = (bool)v));
            AddEntry("Difficulty", new Ref(() => RhythmicGame.Difficulty, (v) => RhythmicGame.Difficulty = (RhythmicGame.GameDifficulty)v));
            AddEntry();

            AddEntry("Fudge song measure length", new Ref(() => RhythmicGame.HACK_FudgeSongMeasureLength, (v) => RhythmicGame.HACK_FudgeSongMeasureLength = (int)v), "HACK", "This is a temporary hack for song length (in measures) not lining up to the actual song length.");
            AddEntry("Destroy behind offset", new Ref(() => TrackStreamer.DestroyBehind_Offset, (v) => TrackStreamer.DestroyBehind_Offset = (int)v), "Default: 1", "How many measures should the destroy-behind system lag behind the player.");
        }

        // Functionality:
    }
}
