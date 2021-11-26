using UnityEngine;
using static Logger;

public class Note : MonoBehaviour {
    // TODO: I'm having second thoughts about just throwing in the info structs like this.
    // We might transform this so that the components actually include the information.

    public Transform trans;

    public Song_Note info;
    public SongSystem song_system;
    public Song song;
    public TrackSection section;

    public MeshRenderer mesh_renderer;
    public MeshFilter mesh_filter;

    public Note Setup(int note_id, TrackSection section) {
        gameObject.SetActive(true);

        this.section = section;
        info = section.song_notes[note_id];

        song_system = SongSystem.Instance;
        song = song_system.song;

        gameObject.name = "%::%_NOTE-%".Parse(section.track.info.name, info.bar, info.code);
        trans.SetParent(section.trans);

        float lane_offset = GetPosOffsetForLane(info.lane);

        Vector3 pos = new Vector3(section.path_transform.pos.x + lane_offset, 0, song.time_units.pos_in_tick * (section.id * Variables.bar_ticks) + info.pos);
        trans.position = PathTransform.pathcreator_global.path.XZ_GetPointAtPosition(pos);
        trans.rotation = PathTransform.pathcreator_global.path.XZ_GetRotationAtDistance(pos.z, pos.x);
        trans.localScale = new Vector3(Variables.NOTE_Size, Variables.NOTE_Size, Variables.NOTE_Size);

        // ..

        return this;
    }
    public Note Recycle() {
        gameObject.SetActive(false);
        trans.SetParent(section.track.parent_transform);

        // ..

        return this;
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

    public static Note CreateNote(int note_id, TrackSection section) {
        if (!PREFAB_Cache) PREFAB_Cache = (GameObject)Resources.Load(PREFAB_PATH);
        if (!PREFAB_Cache && LogE("PREFAB_Cache is null!".TM(nameof(Note)))) return null;

        GameObject obj = Instantiate(PREFAB_Cache);

        return obj.GetComponent<Note>().Setup(note_id, section);
    }
}