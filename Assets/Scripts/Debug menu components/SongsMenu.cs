using System.IO;
using System.Linq;

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
            
            // Add entries for all directories in the song folder:
            {
                string path = AmplitudeGame.song_ogg_path; // TODO: AMP hard-coded!
                string[] dirs = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
                
                // Get folder name token from string
                for (int i = 0; i < dirs.Length; i++)
                {
                    string[] tokens = dirs[i].Split(new string[] { "\\" }, System.StringSplitOptions.RemoveEmptyEntries);
                    dirs[i] = tokens[tokens.Length - 1];
                }

                // Add entries:
                foreach (string s in dirs)
                    AddEntry(s);
            }
        }

        // Functionality:

        public static void LoadSong(string song)
        {
            DebugMenu.SetActive(false);
            SongController.songName = song;
            RhythmicGame.Restart();
        }
    }
}