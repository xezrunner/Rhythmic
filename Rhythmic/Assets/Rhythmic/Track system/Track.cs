using UnityEngine;

public class Track {
    Clock clock;
    Song song;
    TrackSystem track_system;

    public Track(TrackSystem track_system, Song song, int id, Transform parent) {
        clock = Clock.Instance;
        this.track_system = track_system;
        this.song = song;

        info = song.tracks[id];
        sections = new TrackSection[song.length_bars];
        parent_transform = parent;

        pos.x = (-(track_system.track_count / 2f) + (info.id + 0.5f)) * Variables.TRACK_Width;

        material = Object.Instantiate(material_resource);
        material_horizon = Object.Instantiate(material_resource);
        material_global  = Object.Instantiate((Material)Resources.Load("Materials/Track/track_bottom_global"));

        // TODO: Global shader var for glow (emission)!

        // material.SetColor("_Color", info.instrument.color);
        material.SetColor("_Emission", info.instrument.color * 5f);
        material_horizon.SetColor("_Emission", info.instrument.color * 5f);
        material_horizon.SetInt("_PlaneEnabled", 1);

        material_global.SetColor("_Emission", info.instrument.color * 5f);
        material_global.SetInt("_PlaneEnabled", 1);

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

    public Song_Track info;
    public Transform parent_transform;

    public TrackSection[] sections;

    public Vector3 pos;
    public Vector3 ori;

    public static Material material_resource = (Material)Resources.Load("Materials/Track/track_bottom");
    public Material material;
    public Material material_horizon;
    public Material material_global;

}