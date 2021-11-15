using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackSystem : MonoBehaviour
{
    Game Game = Game.Instance;
    SongSystem SongSystem = SongSystem.Instance;

    Song song;
    public Track[] tracks;
    public TrackStreamer streamer;

    public void SetupTrackSystem(Song song)
    {
        this.song = song;

        // Create tracks:
        tracks = new Track[song.track_count];
        for (int i = 0; i < song.track_count; ++i)
        {
            GameObject obj = new GameObject(song.tracks[i].name);
            obj.transform.SetParent(transform);
            tracks[i] = new Track(song, i, obj.transform);
        }

        // Create streamer:
        streamer = gameObject.AddComponent<TrackStreamer>();
        streamer.SetupTrackStreamer(this);
    }
}