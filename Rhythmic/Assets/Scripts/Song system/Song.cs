public struct song_info {
    public string name;
    public string lookup_path;

    public float  bpm;
    public int    duration_ticks;

    public int[]  section_start_bars;
    public float  tunnel_scale;

    public song_track[] tracks;
    public int track_count;

    public song_metadata metadata;
}

public enum TrackInstrument {
    Drums = 0, 
    Bass = 1,
    Synth = 2,
    Guitar = 3,
    Vocals = 4,
    Freestyle = 5,
    UNKNOWN = -1
}
public struct song_track {
    public string name;
    public int    id;
    public TrackInstrument instrument;
    public string audio_path;
    
    public song_note[] notes;
}

public struct song_note {
    public long at_ticks;
    public int  duration_ticks;

    public GameDifficulty for_difficulty;
    public int lane; // @Notelanes
    public int type; // @Powerups
}

public struct song_metadata {
    // ...
}

