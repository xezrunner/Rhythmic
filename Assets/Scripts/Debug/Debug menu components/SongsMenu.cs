namespace DebugMenus
{
    public class SongsMenu : DebugMenuComponent
    {
        public static SongsMenu Instance;

        public override void Init()
        {
            base.Init();
            Instance = this;
            _GlobalEntryAction = (entry) => { LoadSong(entry.Text); };

            AddEntry("tut0");
            AddEntry("perfectbrain");
            AddEntry("dreamer");
            AddEntry("dalatecht");
        }

        // Functionality:

        void LoadSong(string song)
        {
            DebugMenu_Close();
            SongController.songName = song;
            RhythmicGame.Restart();
        }
    }
}