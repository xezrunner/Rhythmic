using UnityEngine;

public class Track {
    public TrackSystem track_system;

    public Track(TrackSystem track_system, Song song, int id, Transform parent) {
        this.track_system = track_system;
        info = song.tracks[id];
        sections = new TrackSection[song.length_bars];
        parent_transform = parent;

        pos.x = (-(track_system.track_count / 2f) + (info.id + 0.5f)) * Variables.TRACK_Width;

        material = Object.Instantiate(material_resource);
        material_horizon = Object.Instantiate(material_resource);

        // TODO: Global shader var for glow (emission)!

        // material.SetColor("_Color", info.instrument.color);
        material.SetColor("_Emission", info.instrument.color * 5f);
        material_horizon.SetColor("_Emission", info.instrument.color * 5f);
        material_horizon.SetInt("_PlaneEnabled", 1);

        /*
        Log("Printing notes for track %: ", info.name);
        for (int m = 0; m < 50; ++m)
        {
            if (info.notes[m] == null) continue;
            for (int i = 0; i < info.notes[m].Count; ++i)
            {
                Log("    - [measure % / %]: lane id: % code: %", m, i, info.notes[m][i].lane, (AMP_NoteLane)info.notes[m][i].code);
            }

        }
        */
    }

    public Song song;
    public Song_Track info;
    public Transform parent_transform;

    public TrackSection[] sections;

    public Vector3 pos;
    public Vector3 ori;

    public static Material material_resource = (Material)Resources.Load("Materials/Track/track_bottom");
    public Material material;
    public Material material_horizon;
}