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
        material_horizon = Object.Instantiate(material_resource);

        // TODO: Global shader var for glow (emission)!

        // material.SetColor("_Color", info.instrument.color);
        material.SetColor("_Emission", info.instrument.color * 5f);
        material_horizon.SetColor("_Emission", info.instrument.color * 5f);
        material_horizon.SetInt("_PlaneEnabled", 1);
    }

    public Song song;
    public Song_Track info;
    public Transform parent_transform;

    public TrackSection[] sections;

    public static Material material_resource = (Material)Resources.Load("Materials/Track/track_bottom");
    public Material material;
    public Material material_horizon;
}