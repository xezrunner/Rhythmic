using UnityEngine;
using static Logger;

public class SongSystem : MonoBehaviour
{
    public static SongSystem Instance;

    public string song_name;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (song_name.IsEmpty() && LogW("No song!".T(this))) return;
    }

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

        this.song_name = song_name;
        return true;
    }

    public static SongSystem CreateSongSystem()
    {
        if (Instance && LogE("There already exists an instance of a SongSystem. Ignoring.")) return null;

        GameObject obj = new GameObject("SongSystem");
        return obj.AddComponent<SongSystem>();
    }
}