using UnityEngine;
using static Logger;

public class PlayerCatcher : MonoBehaviour {
    public Transform trans;

    PlayerTrackSwitching player_trackswitch;
    TrackSystem track_system;
    Clock clock;

    public int id;

    Vector3 catch_anim_idle;
    Vector3 catch_anim_start;
    Vector3 catch_anim_target;

    void Awake() {
        track_system = TrackSystem.Instance;
        clock = Clock.Instance;
        player_trackswitch = PlayerTrackSwitching.Instance;
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

        // ..
        int track_id = player_trackswitch.current_track_id;

        Note n = track_system.next_notes[track_id];

        n.Capture();
        track_system.FindNextNote(track_id);
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