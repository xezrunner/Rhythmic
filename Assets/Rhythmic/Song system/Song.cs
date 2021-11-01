public class Song
{
    public Song_Type song_type;

    public string name;
    public string friendly_name;
    public string file_path; // In case of AMP, path to the moggsong

    public float bpm;
    public float duration_sec;
    public int duration_tick;
    public int duration_bars;

    public float tunnel_scale;

    public SongTimeInfo time_info;

    public SongData data;
}

public enum Note_Lane { Left = 1, Center = 2, Right = 3, UNKNOWN = 0 } // Move

public struct SongMetaNote
{
    public int id;
    public int lane_id;
    public Note_Lane lane;

    public float distance_ms;
    public int distance_tick;
    public int bar;

}