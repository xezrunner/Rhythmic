using UnityEngine;
using static Logger;

public class PlayerCatcher : MonoBehaviour {
    public Transform trans;

    PlayerTrackSwitching player_trackswitch;
    SongSystem song_system;
    Song song;
    Clock clock;
    TrackSystem track_system;

    public int id;

    Vector3 catch_anim_idle;
    Vector3 catch_anim_start;
    Vector3 catch_anim_target;

    void Start() {
        clock = Clock.Instance;
        track_system = TrackSystem.Instance;
        player_trackswitch = PlayerTrackSwitching.Instance;

        song_system = SongSystem.Instance;
        song = song_system.song;
    }

    public PlayerCatcher Setup(int id, Transform parent) {
        this.id = id;
        trans.SetParent(parent);
        INIT_SetPositionAndScale();

        return this;
    }

    public void Catch() {
        catch_anim_start = trans.localScale;
        catch_anim_elapsed_ms = 0f;
        is_catching = true;

        int track_id = player_trackswitch.current_track_id;
        if (track_id == -1 && LogW("Invalid track! %".TM(this), track_id)) return;

        Track t = track_system.tracks[track_id];

        Song_Note n = track_system.next_notes[track_id];
        Song_Section s = t.info.sections[n.bar];

        // TODO: Perhaps we might want to check ticks instead, for accuracy?
        bool success;
        // TODO: Slop to take into account the tunnel scale (?)
        if (!Variables.CATCHER_UseTicksForSlop) {

            float current_ms = clock.ms;
            float note_ms = n.ms;
            float diff = Mathf.Abs(note_ms - current_ms);

            Log("SLOP (ms %): current: %  note_ms: %  diff: %", Variables.CATCHER_SlopMs, current_ms, note_ms, diff);

            success = (s.is_enabled && diff <= Variables.CATCHER_SlopMs);
        } else {
            long current_ticks = clock.ticks;
            long note_ticks = n.ticks;
            long diff = (long)Mathf.Abs(note_ticks - current_ticks);

            float slop_target = song.time_units.tick_in_ms * Variables.CATCHER_SlopMs;
            LogW("SLOP (ticks %): current: %  note_ticks: %  diff: %", slop_target, current_ticks, note_ticks, diff);

            success = (s.is_enabled && diff <= slop_target);
        }

        // Check if correct lane:
        if (n.lane == id) success = (success && true);
        else {
            LogW("CATCH: Mismatching lane for note (%/%::%): lane % but catcher %", n.track_id, n.bar, n.bar_id, n.lane, id);
            success = false;
        }

        Note note = null;
        if (t.sections[n.bar]) note = t.sections[n.bar].notes[n.bar_id];

        if (success) {
            if (note) note.Capture(success);
            else n.is_captured = success;

            track_system.track_being_played = track_id;

            track_system.SetSectionsEnabled(n.bar, track_id, false);

            if (n.is_last_note) {
                int next_sect_id = track_system.GetNextSectionID(track_id);
                int sect_id = track_system.next_sections[track_id][next_sect_id];

                // Disable current (note) section: 
                t.info.sections[sect_id].is_enabled = false;
                if (t.sections[sect_id]) t.sections[sect_id].is_enabled = false;
                // Remove next section assignment: 
                track_system.next_sections[track_id][next_sect_id] = -1; 

                // If we don't have any remaining next sections, we've captured all sections: 
                if (track_system.GetNextSectionID(track_id) == -1) {
                    Log("SUCCESS! Capture track! (for now, you should be able to continue catching)");
                    track_system.track_being_played = -1;
                    track_system.FindNextSections();
                    return;
                }
            }

            track_system.FindNextNote(track_id);
        } else {
            if (track_system.track_being_played != -1)
                if (note) note.Capture(success);

            track_system.SetSectionsEnabled((int)clock.bar, -1, false);

            track_system.track_being_played = -1;
            track_system.FindNextSections();
        }
    }

    // Transform and animation:
    void INIT_SetPositionAndScale() {
        if (is_catching) return;

        trans.localPosition = new Vector3(Note.GetPosOffsetForLane(id), 0, 0);
        trans.localScale = catch_anim_idle = new Vector3(Variables.NOTE_Size, 0.01f, Variables.NOTE_Size);

        float anim_target = Variables.CATCHER_CatchAnimTarget;
        catch_anim_target = new Vector3(anim_target, catch_anim_idle.y, anim_target);
    }

    bool is_catching;
    float catch_anim_elapsed_ms = 0;
    void UPDATE_CatchAnimation() {
        if (!is_catching) return;

        float t = catch_anim_elapsed_ms / Variables.CATCHER_CatchAnimMs;

        if (t < 1.0f)
            trans.localScale = Vector3.Lerp(catch_anim_start, catch_anim_target, t);
        if (t >= 1.0f)
            trans.localScale = Vector3.Lerp(catch_anim_target, catch_anim_idle, t - 1f);

        if (t > 2.0f)
            is_catching = false;

        catch_anim_elapsed_ms += Time.deltaTime * 1000f; // Audio deltatime! (with testing)
    }

    void Update() {
        INIT_SetPositionAndScale(); // TODO: Remove this or make it controllable - used for hotreload during design phase!
        UPDATE_CatchAnimation();
    }

    // ----- //

    public const string PREFAB_PATH = "Prefabs/" + nameof(PlayerCatcher);
    public static GameObject PREFAB_Cache = null;
    public static PlayerCatcher PREFAB_Create(int id, Transform parent) {
        if (!PREFAB_Cache) PREFAB_Cache = (GameObject)Resources.Load(PREFAB_PATH);
        if (!PREFAB_Cache && LogE("PREFAB_Cache is null!".TM(nameof(PlayerCatcher)))) return null;

        GameObject obj = Instantiate(PREFAB_Cache);

        return obj.GetComponent<PlayerCatcher>().Setup(id, parent);
    }
}