using System.Collections.Generic;
using UnityEngine;
using static Logger;

public class TrackSection : MonoBehaviour
{
    public PathTransform path_transform;

    void Awake()
    {
        if (!path_transform && LogW("A TrackSection needs to have a PathTransform component to exist. Deleting.".T(this)))
            Destroy(gameObject);
    }
    void Start()
    {
        if (track != null && id != -1)
        {
            // Log("Created TrackSection for track %  id: %", track.info.name, id);
        }
    }

    public int id = -1;
    public Track track;
    public List<Song_Note> song_notes;

    public MeshRenderer mesh_renderer;    

    public TrackSection Setup(Track track, int id /* ... */)
    {
        gameObject.SetActive(true);

        this.id = id;
        this.track = track;
        path_transform.pos.z = id * path_transform.desired_size.z;
        path_transform.pos.x = (-(track.track_system.track_count / 2f) + (track.info.id + 0.5f)) * path_transform.desired_size.x;

        // ...
        mesh_renderer.material = track.material;

        path_transform.Deform();

        return this;
    }

    public TrackSection Recycle()
    {
        gameObject.SetActive(false);
        // Log("Recycling %... - state: %, %", gameObject.name, gameObject.activeSelf, gameObject.activeInHierarchy);
        // ...

        return this;
    }

    // ----- //

    public const string PREFAB_PATH = "Prefabs/Track/TrackSection";
    public static GameObject PREFAB_Instance = null;
    public static TrackSection CreateTrackSection(Track track, int id)
    {
        if (!PREFAB_Instance) PREFAB_Instance = (GameObject)Resources.Load(PREFAB_PATH);
        if (!PREFAB_Instance && LogE("Failed to load prefab (path: '%')".TM(nameof(TrackSection), PREFAB_PATH))) return null;

        GameObject obj = Instantiate(PREFAB_Instance, track.parent_transform);
        obj.name = "%::%".Parse(track.info.name, id);

        TrackSection ts = obj.GetComponent<TrackSection>(); // PERFORMANCE!!!
        ts.Setup(track, id);

        return ts;
    }
}