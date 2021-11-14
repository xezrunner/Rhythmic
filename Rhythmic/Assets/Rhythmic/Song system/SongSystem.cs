using UnityEngine;
using static Logger;

public class SongSystem : MonoBehaviour
{
    public static SongSystem Instance;
    GameState GameState = GameState.Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (song == null && LogW("No song!".T(this))) return;
    }

    public AudioSystem audio_system;
    public TrackSystem track_system;
    public Clock clock;

    public Song song;

    public bool LoadSong(string song_name, GameMode game_mode)
    {
        Song song = null;

        if (game_mode == GameMode.Amplitude2016)
            song = AMP_SongLoader.LoadSong(song_name);
        else
        {
            // song = RHX_SongLoader.LoadSong(...
        }

        if (song == null) return false;
        this.song = song;

        GameState.game_mode = game_mode;

        // Create Clock:
        clock = gameObject.AddComponent<Clock>();
        clock.SetupClock();

        // Create AudioSystem:
        GameObject audiosystem_obj = new GameObject("AudioSystem");
        audiosystem_obj.transform.SetParent(transform);
        audio_system = audiosystem_obj.AddComponent<AudioSystem>();
        audio_system.SetupAudioSystem(song);

        // Create TrackSystem:
        GameObject tracksystem_obj = new GameObject("TrackSystem");
        track_system = tracksystem_obj.AddComponent<TrackSystem>();
        track_system.SetupTrackSystem(song);

        return true;
    }

    public static SongSystem CreateSongSystem()
    {
        if (Instance && LogE("There already exists an instance of a SongSystem. Ignoring.")) return null;

        GameObject obj = new GameObject("SongSystem");
        return obj.AddComponent<SongSystem>();
    }
}