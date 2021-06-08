public partial class SongLoader
{
    public static string[] song_folders = { "Songs" };
    public static string[] song_exts = { ".r_song" };
    
    public static SongInfo LoadSongMetaFile(string file_name, GameLogic game_logic = GameLogic.RHYTHMIC)
    {
        SongInfo info = null;
        
        if (game_logic == GameLogic.AMPLITUDE)
            info = AMPLITUDE_LoadSongData(file_name);
        else if (game_logic == GameLogic.RHYTHMIC)
            info = RHYTHMIC_LoadSongData(file_name);
        
        return info;
    }
    
    static SongInfo RHYTHMIC_LoadSongData(string file_name)
    {
        Configuration conf = ConfigurationManager.LoadConfiguration(file_name, song_folders, song_exts);
        if (conf == null) return null;
        
        SongInfo info = new SongInfo()
        {
            song_name = conf.GetVariable(nameof(info.song_name)),
            song_world = conf.GetVariable(nameof(info.song_world)),
            song_bpm = conf.GetVariable(nameof(info.song_bpm)).ParseFloat(),
            
            slowmo_rate = conf.GetVariable(nameof(info.slowmo_rate)).ParseFloat(),
            tunnel_scale = conf.GetVariable(nameof(info.tunnel_scale)).ParseFloat(),
            // TODO: Additional props...
            checkpoint_bars = conf.GetVariable(nameof(info.checkpoint_bars)).ParseIntArray(),
            synesth_rate = conf.GetVariable(nameof(info.synesth_rate)).ParseFloatArray()
        };
        
        // TODO: Grab data
        
        return info;
    }
    
    public static void DEBUG_TestSongLoader(string file_name, string mode = null)
    {
        LoadSongMetaFile("song_test", mode == "amp" ? GameLogic.AMPLITUDE : GameLogic.RHYTHMIC);
    }
}