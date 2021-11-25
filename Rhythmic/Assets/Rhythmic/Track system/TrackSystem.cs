using UnityEngine;

public class TrackSystem : MonoBehaviour {
    Game Game = Game.Instance;
    SongSystem SongSystem = SongSystem.Instance;

    Song song;
    public TrackStreamer streamer;
    public Track[] tracks;
    public int track_count;

    public void SetupTrackSystem(Song song) {
        this.song = song;

        // Create tracks:
        tracks = new Track[song.track_count];
        track_count = song.track_count;
        for (int i = 0; i < song.track_count; ++i) {
            GameObject obj = new GameObject(song.tracks[i].name);
            obj.transform.SetParent(transform);
            tracks[i] = new Track(this, song, i, obj.transform);
        }

        // Create streamer:
        streamer = gameObject.AddComponent<TrackStreamer>();
        streamer.SetupTrackStreamer(this);
    }
}