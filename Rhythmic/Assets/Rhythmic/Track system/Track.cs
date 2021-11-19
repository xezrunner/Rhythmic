using UnityEngine;

public class Track
{
    public TrackSystem track_system;

    public Track(TrackSystem track_system, Song song, int id, Transform parent)
    {
        this.track_system = track_system;
        info = song.tracks[id];
        sections = new TrackSection[song.length_bars];
        parent_transform = parent;

        material = Object.Instantiate(material_resource);
        // material.SetColor("_Color", info.instrument.color);
        material.SetColor("_Emission", info.instrument.color * 5f);
    }

    public Song song;
    public Song_Track info;
    public Transform parent_transform;

    public TrackSection[] sections;

    public static Material material_resource = (Material)Resources.Load("Materials/Track/track_bottom");
    public Material material;
}