using UnityEngine;

public class Track
{
    public TrackSystem track_system;

    public Track(TrackSystem track_system, Song song, int id, Transform parent)
    {
        this.track_system = track_system;
        info = song.tracks[id];
        sections = new TrackSection[song.length_bars];
        this.parent_transform = parent;
    }

    public Song song;
    public Song_Track info;
    public Transform parent_transform;

    public TrackSection[] sections;
}