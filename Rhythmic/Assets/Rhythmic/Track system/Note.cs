using UnityEngine;
using static Logger;

public class Note : MonoBehaviour {
    // TODO: I'm having second thoughts about just throwing in the info structs like this.
    // We might transform this so that the components actually include the information.

    public Transform trans;

    SongSystem song_system;
    Song song;
    TrackSystem track_system;

    public Song_Note info;
    public int id;
    public TrackSection section;

    public MeshRenderer mesh_renderer;
    public MeshFilter mesh_filter;

    public bool is_last_note;

    void Awake() {
        song_system = SongSystem.Instance;
        song = song_system.song;
        track_system = TrackSystem.Instance;
    }

    public Note Setup(int note_id, TrackSection section) {
        gameObject.SetActive(true);
        this.section = section;
        id = note_id;

        info = section.song_notes[note_id];

        gameObject.name = "%::%_NOTE-%".Parse(section.track.info.name, info.bar, info.code);
        trans.SetParent(section.trans);

        float lane_offset = GetPosOffsetForLane(info.lane);

        Vector3 pos = new Vector3(section.path_transform.pos.x + lane_offset, 0, song.time_units.pos_in_tick * (section.id * Variables.bar_ticks) + info.pos);
        trans.position = PathTransform.pathcreator_global.path.XZ_GetPointAtPosition(pos);
        trans.rotation = PathTransform.pathcreator_global.path.XZ_GetRotationAtDistance(pos.z, pos.x);
        trans.localScale = new Vector3(Variables.NOTE_Size, Variables.NOTE_Size, Variables.NOTE_Size);

        // ..
        is_last_note = (note_id == section.notes.Length - 1);
        ChangeMaterial((is_last_note) ? MAT_green : MAT_regular);

        return this;
    }
    public Note Recycle() {
        gameObject.SetActive(false);
        trans.SetParent(section.track.parent_transform);

        // ..

        return this;
    }

    public void ChangeMaterial(Material mat) {
        mesh_renderer.material = mat;
    }

    public void Capture(bool success = true) {
        if (success) {
            gameObject.SetActive(false);

            if (!is_last_note)
                ++track_system.tracks[info.track_id].info.sections[info.bar].next_note_index;
            else 
                track_system.tracks[info.track_id].info.sections[info.bar].next_note_index = -1;
        } else {
            ChangeMaterial(MAT_red);
        }
    }

    // --------------- //

    public static float GetPosOffsetForLane(int lane) {
        float lane_piece = (Variables.TRACK_Width / Variables.TRACK_Lanes) * Variables.NOTE_PaddingFrac;
        float offset = -(lane_piece * (Variables.TRACK_Lanes - 1f)) / 2f;

        float result = offset + (lane * lane_piece);
        return result;
    }

    // --------------- //

    public const string PREFAB_PATH = "Prefabs/Note";
    public static GameObject PREFAB_Cache = null;

    public static Material MAT_regular;
    public static Material MAT_disabled;
    public static Material MAT_green;
    public static Material MAT_red;

    public static Note CreateNote(int note_id, TrackSection section) {
        if (!PREFAB_Cache)
            PREFAB_Cache = (GameObject)Resources.Load(PREFAB_PATH);
        if (!PREFAB_Cache && LogE("PREFAB_Cache is null!".TM(nameof(Note)))) return null;

        // TEMP:
        MAT_regular = Instantiate((Material)Resources.Load("Materials/note"));
        MAT_disabled = Instantiate((Material)Resources.Load("Materials/note"));
        MAT_disabled.color = Color.gray;
        MAT_green = Instantiate((Material)Resources.Load("Materials/note"));
        MAT_green.color = Color.green;
        MAT_red = Instantiate((Material)Resources.Load("Materials/note"));
        MAT_red.color = Color.red;

        GameObject obj = Instantiate(PREFAB_Cache);

        return obj.GetComponent<Note>().Setup(note_id, section);
    }
}