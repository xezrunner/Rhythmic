using UnityEngine;
using static Logger;

public enum GameDifficulty { Easy = 0, Medium = 1, Hard = 2, Very_Hard = 3, UNKNOWN = -1 } // TODO
public enum GameType { Rhythmic = 0, Amplitude2016 = 1, UNKNOWN = -1 }
public enum GameState { Startup = 0, Ingame = 1, Editor = 2, UNKNOWN = -1 }

public class RhythmicGame : Game {
    public static new RhythmicGame Instance;

    public override void Awake() { base.Awake(); Instance = this; }
    public override void Start() {
        base.Start();
        INIT_SongSystem("allthetime");

        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
    }

    public GameType game_type;

    public SongSystem song_system;
    void INIT_SongSystem(string song_name, GameType game_type = GameType.Amplitude2016) {
        song_system = SongSystem.CreateSongSystem();
        bool success = song_system.LoadSong(song_name, game_type);

        if (!success) LogE("Failed to load song % (game_type: %).", song_name, game_type);
    }

    // NOTE: This reflects the difficulty index for the given game_type.
    public int game_difficulty = 3;

    public override void INIT_Variables(bool hot_reload = false) {
        base.INIT_Variables(hot_reload);
        if (song_system && song_system.track_system && song_system.track_system.streamer) // TEMP
        {
            song_system.song.time_units = Clock.Instance.time_units = new Song_TimeUnits(song_system.song.bpm, song_system.song.tunnel_scale);
            song_system.track_system.streamer.STREAMER_Reload(true);
        }
    }
}