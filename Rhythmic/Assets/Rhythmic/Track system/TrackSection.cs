using System.Collections.Generic;
using UnityEngine;
using static Logger;

public class TrackSection : MonoBehaviour {
    public PathTransform path_transform;
    public SongSystem song_system;
    public Song song;
    public Transform trans;

    void Awake() {
        if (!path_transform && LogW("A TrackSection needs to have a PathTransform component to exist. Deleting.".T(this)))
            Destroy(gameObject);
    }
    void Start() {
        if (track != null && id != -1) { /*Log("Created TrackSection for track %  id: %", track.info.name, id);}*/ }
    }

    GameObject a;
    public TrackSection Setup(Track track, int id /* ... */) {
        gameObject.SetActive(true);

        this.id = id;
        this.track = track;
        song_notes = track.info.notes[id];

        song_system = SongSystem.Instance;
        song = song_system.song;

        trans.parent = track.parent_transform;
        gameObject.name = "%::%".Parse(track.info.name, id);

        mesh_renderer.enabled = song_notes != null;
        notes = (song_notes != null) ? new Note[song_notes.Count] : null;

        path_transform.desired_size.z = song.time_units.pos_in_tick * Variables.bar_ticks;
        path_transform.pos.z = song.time_units.pos_in_tick * (id * Variables.bar_ticks);
        path_transform.pos.x = (-(track.track_system.track_count / 2f) + (track.info.id + 0.5f)) * path_transform.desired_size.x;

        // ...
        ChangeMaterial(track.material_horizon);

        path_transform.Deform();

        if (!a) a = GameObject.CreatePrimitive(PrimitiveType.Cube);
        a.transform.SetParent(trans);
        a.transform.localScale = new Vector3(Variables.TRACK_Width, 0.1f, 0.1f);
        a.transform.position = PathTransform.pathcreator_global.path.XZ_GetPointAtPosition(path_transform.pos + new Vector3(0, 0, path_transform.desired_size.z));
        a.transform.rotation = PathTransform.pathcreator_global.path.XZ_GetRotationAtDistance(path_transform.pos.z + path_transform.desired_size.z, path_transform.pos.x);

        return this;
    }
    public TrackSection Recycle() {
        gameObject.SetActive(false);
        // Log("Recycling %... - state: %, %", gameObject.name, gameObject.activeSelf, gameObject.activeInHierarchy);
        // ...

        return this;
    }

    public int id = -1;
    public Track track;
    public Note[] notes;
    public List<Song_Note> song_notes; // This is null if there are no notes in this measure.

    public MeshRenderer mesh_renderer;

    public void ChangeMaterial(Material mat) => mesh_renderer.material = mat;

    // ----- //

    public const string PREFAB_PATH = "Prefabs/Track/TrackSection";
    public static GameObject PREFAB_Cache = null;
    public static TrackSection CreateTrackSection(Track track, int id) {
        if (!PREFAB_Cache) PREFAB_Cache = (GameObject)Resources.Load(PREFAB_PATH);
        if (!PREFAB_Cache && LogE("Failed to load prefab (path: '%')".TM(nameof(TrackSection), PREFAB_PATH))) return null;

        GameObject obj = Instantiate(PREFAB_Cache, track.parent_transform);

        TrackSection ts = obj.GetComponent<TrackSection>(); // PERFORMANCE!!!
        ts.Setup(track, id);

        return ts;
    }
}