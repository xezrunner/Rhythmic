using UnityEngine;

public class Track
{
    public Track(Song song, int id, Transform parent)
    {
        info = song.tracks[id];
        sections = new TrackSection[song.length_bars];
        this.parent = parent;
    }

    public Song song;
    public Song_Track info;
    public Transform parent;

    public TrackSection[] sections;
}