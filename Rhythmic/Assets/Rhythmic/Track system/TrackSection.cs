using System.Collections.Generic;
using UnityEngine;
using static Logger;

public class TrackSection : MonoBehaviour {
    SongSystem song_system;
    Song song;
    TrackSystem track_system;

    public PathTransform path_transform;
    public Transform trans;

    void Awake() {
        if (!path_transform && LogW("A TrackSection needs to have a PathTransform component to exist. Deleting.".T(this)))
            Destroy(gameObject);

        song_system = SongSystem.Instance;
        song = song_system.song;
        track_system = TrackSystem.Instance;
    }

    public int id = -1;
    public Track track;
    public Note[] notes;
    public List<Song_Note> song_notes; // This is null if there are no notes in this measure.

    public MeshRenderer mesh_renderer;

    public bool is_enabled; // NOTE: Not the same as the component/object being active!
    public bool is_empty;
    public bool is_captured;

    public void SetEnabled(bool enabled) {
        is_enabled = enabled;
        if (notes == null) return;
        foreach (Note n in notes)
            n?.ChangeMaterial(enabled ? Note.MAT_regular : Note.MAT_disabled);
    }

    GameObject measure_separator; // temp!
    public TrackSection Setup(Track track, int id /* ... */) {
        gameObject.SetActive(true);

        this.id = id;
        this.track = track;
        song_notes = track.info.notes[id];

        trans.parent = track.parent_transform;
        gameObject.name = "%::%".Parse(track.info.name, id);

        Song_Section info = track.info.sections[id];

        is_enabled = info.is_enabled;
        is_captured = info.is_captured;
        is_empty = (song_notes == null);

        notes = (!is_empty) ? new Note[song_notes.Count] : null;
        mesh_renderer.enabled = !is_empty;

        path_transform.desired_size = new Vector2(Variables.TRACK_Width, Variables.TRACK_Height);
        path_transform.desired_size.z = song.time_units.pos_in_tick * Variables.bar_ticks;
        path_transform.pos.z = song.time_units.pos_in_tick * (id * Variables.bar_ticks);
        path_transform.pos.x = (-(track_system.track_count / 2f) + (track.info.id + 0.5f)) * path_transform.desired_size.x;

        // ...
        ChangeMaterial(track.material_horizon);

        path_transform.Deform();

        // Measure separators (temp!):
        {
            if (!measure_separator) measure_separator = GameObject.CreatePrimitive(PrimitiveType.Cube);
            measure_separator.transform.SetParent(trans);
            measure_separator.transform.localScale = new Vector3(Variables.TRACK_Width, 0.1f, 0.1f);
            measure_separator.transform.position = PathTransform.pathcreator_global.path.XZ_GetPointAtPosition(path_transform.pos + new Vector3(0, 0, path_transform.desired_size.z));
            measure_separator.transform.rotation = PathTransform.pathcreator_global.path.XZ_GetRotationAtDistance(path_transform.pos.z + path_transform.desired_size.z, path_transform.pos.x);
        }

        return this;
    }
    public TrackSection Recycle() {
        gameObject.SetActive(false);
        // Log("Recycling %... - state: %, %", gameObject.name, gameObject.activeSelf, gameObject.activeInHierarchy);
        // ...

        return this;
    }

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