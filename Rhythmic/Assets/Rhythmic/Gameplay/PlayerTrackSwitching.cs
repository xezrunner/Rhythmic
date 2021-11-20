using UnityEngine;
using UnityEngine.InputSystem;

public enum SwitchTrackDir { Left = -1, Right = 1 }

public class PlayerTrackSwitching : MonoBehaviour
{
    SongSystem song_system;
    TrackSystem track_system;

    public PlayerLocomotion locomotion;

    float track_width;

    void Start()
    {
        song_system = SongSystem.Instance;
        track_system = song_system.track_system;

        if (Variables.TRACK_Width == -1)  // HACK! Provide a better way to do this, or provide value in Variables!
            track_width = track_system.tracks[0].sections[0].path_transform.desired_size.x;
        else track_width = Variables.TRACK_Width;
    }

    public int current_track = -1;

    int slam_count = 0;
    float slam_elapsed_ms = 0;
    SwitchTrackDir slam_last_dir;
    public bool SwitchTrack(SwitchTrackDir dir, bool force = false)
    {
        if (current_track == -1) current_track = track_system.track_count / 2;

        int new_target = current_track + (int)dir;
        if (new_target < 0 || new_target > track_system.track_count - 1)
        {
            if (!Variables.TRACKSWITCH_SlamEnabled) return false;

            if (slam_count == 0) slam_last_dir = dir;
            else if (dir != slam_last_dir)
            {
                slam_count = 0; slam_elapsed_ms = 0;
                return false;
            }

            ++slam_count;
            if (slam_count >= Variables.TRACKSWITCH_SlamsTarget)
            {
                new_target = (dir == SwitchTrackDir.Left) ? track_system.track_count - 1 : 0;
                slam_count = 0; slam_elapsed_ms = 0;
                return SwitchToTrack_FindClosest(new_target);
            }
            return false;
        }

        return SwitchToTrack(new_target, force);
    }

    public float target_x;
    public bool SwitchToTrack(int id, bool force = false)
    {
        if (id < 0 || id > track_system.track_count - 1) return false;

        // TODO: Checks ...

        smooth_time = ((track_system.track_count / 2f) + Mathf.Abs(current_track - id)) / smooth_time_factor;
        current_track = id;

        Track t = track_system.tracks[current_track];
        target_x = t.pos.x;

        return true;
    }

    // Finds the closest playable track to the given ID.
    public bool SwitchToTrack_FindClosest(int id)
    {
        // TODO
        return SwitchToTrack(id, true);
    }

    public float smooth_time_factor = 40f;
    public float smooth_time;

    public bool is_freestyle;

    Vector3 loco_lookat_temp;
    float loco_offset_x_temp;
    void Update()
    {
        if (slam_count > 0)
        {
            slam_elapsed_ms += Time.deltaTime * 1000f;

            if (slam_elapsed_ms > Variables.TRACKSWITCH_SlamTimeoutMs)
            {
                slam_elapsed_ms = 0;
                slam_count = 0;
            }
        }

        locomotion.lookat_target.localPosition = Vector3.SmoothDamp(locomotion.lookat_target.localPosition, !is_freestyle ? new Vector3(0, 0, 0) : new Vector3(0, 8, 0), ref loco_lookat_temp, smooth_time);
        locomotion.offset_pos.x = Mathf.SmoothDamp(locomotion.offset_pos.x, !is_freestyle ? target_x : default, ref loco_offset_x_temp, smooth_time);

        if (Keyboard.current.fKey.wasPressedThisFrame) is_freestyle = !is_freestyle;
    }
}