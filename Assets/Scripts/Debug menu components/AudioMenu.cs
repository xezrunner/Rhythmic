namespace DebugMenus
{
    class AudioMenu : DebugMenuComponent
    {
        public static AudioMenu Instance;

        public override void Init()
        {
            base.Init();
            Instance = this;

            AddEntry("Allow audio streaming", new Ref(() => RhythmicGame.AllowSongStreaming, (v) => RhythmicGame.AllowSongStreaming = (bool)v),
                                              "Streaming may cause slowdown in the Unity Editor during gameplay. Turning it off causes longer song load times.");
            AddEntry("Enable custom LibVorbis implementation", () => false, "Not yet implemented.", isEnabled: false);
            AddEntry("Vorbis...", false);
        }
    }
}
