using UnityEngine;
using UnityEngine.InputSystem;

public enum SwitchTrackDir { Left = -1, Right = 1 }

public class PlayerTrackSwitching : MonoBehaviour {
    public static PlayerTrackSwitching Instance;

    SongSystem song_system;
    TrackSystem track_system;

    public PlayerLocomotion locomotion;

    void Awake() => Instance = this;

    void Start() {
        song_system = SongSystem.Instance;
        track_system = song_system.track_system;

        smooth_time = 3 / smooth_time_factor;
    }

    public int current_track_id = -1;

    int slam_count = 0;
    float slam_elapsed_ms = 0;
    SwitchTrackDir slam_last_dir;
    public bool SwitchTrack(SwitchTrackDir dir, bool force = false) {
        if (current_track_id == -1) current_track_id = track_system.track_count / 2;

        int new_target = current_track_id + (int)dir;
        if (new_target < 0 || new_target > track_system.track_count - 1) {
            if (!Variables.TRACKSWITCH_SlamEnabled) return false;

            if (slam_count == 0) slam_last_dir = dir;
            else if (dir != slam_last_dir) {
                slam_count = 0; slam_elapsed_ms = 0;
                return false;
            }

            ++slam_count;
            if (slam_count >= Variables.TRACKSWITCH_SlamsTarget) {
                new_target = (dir == SwitchTrackDir.Left) ? track_system.track_count - 1 : 0;
                slam_count = 0; slam_elapsed_ms = 0;
                return SwitchToTrack_FindClosest(new_target);
            }
            return false;
        }

        return SwitchToTrack(new_target, force);
    }

    public float target_x;
    public bool SwitchToTrack(int id, bool force = false) {
        if (id < 0 || id > track_system.track_count - 1) return false;

        // TODO: Checks ...

        smooth_time = ((track_system.track_count / 2f) + Mathf.Abs(current_track_id - id)) / smooth_time_factor;
        current_track_id = id;

        Track t = track_system.tracks[current_track_id];
        target_x = t.pos.x;

        return true;
    }

    // Finds the closest playable track to the given ID.
    public bool SwitchToTrack_FindClosest(int id) {
        // TODO
        return SwitchToTrack(id, true);
    }

    public bool is_freestyle;
    public float freestyle_trans_duration_s = 0.8f;
    float freestyle_trans_elapsed;
    float freestyle_trans_frac; // 0-1


    public float smooth_time_factor = 40f;
    public float smooth_time;
    Vector3 loco_lookat_temp;
    Vector3 freestyle_ori_offset_temp;
    float loco_offset_x_temp;
    float loco_offset_z_temp;
    void Update() {
        if (Keyboard.current != null) {
            if (Keyboard.current.qKey.wasPressedThisFrame) SwitchToTrack(0, true);
            if (Keyboard.current.pKey.wasPressedThisFrame) SwitchToTrack(track_system.track_count - 1, true);
            if (Keyboard.current.fKey.wasPressedThisFrame) is_freestyle = !is_freestyle;
        }

        float calculated_smooth_time;
        float pos_offset_x;

        if (is_freestyle) {
            float freestyle_smooth_time = (3 / smooth_time_factor) * 4f;
            calculated_smooth_time = Mathf.Lerp(smooth_time, freestyle_smooth_time, freestyle_trans_frac);
            pos_offset_x = 0;

            freestyle_trans_frac = freestyle_trans_elapsed / freestyle_trans_duration_s;
        } else {
            if (freestyle_trans_frac > 0) {

            }

            calculated_smooth_time = (3 / smooth_time_factor);

            if (slam_count > 0) {
                slam_elapsed_ms += Time.deltaTime * 1000f;

                if (slam_elapsed_ms > Variables.TRACKSWITCH_SlamTimeoutMs) {
                    slam_elapsed_ms = 0;
                    slam_count = 0;
                }
            }

            pos_offset_x = target_x;
        }

        locomotion.offset_pos.x = Mathf.SmoothDamp(locomotion.offset_pos.x, pos_offset_x, ref loco_offset_x_temp, calculated_smooth_time);

        /*

        locomotion.lookat_target.localPosition = Vector3.SmoothDamp(locomotion.lookat_target.localPosition, (!is_freestyle ? new Vector3(0, 0, 0) : new Vector3(0, 7, 0)) + locomotion.lookat_pos_offset, ref loco_lookat_temp, smooth_time * 4f);

        locomotion.offset_pos.x = Mathf.SmoothDamp(locomotion.offset_pos.x, !is_freestyle ? target_x : default, ref loco_offset_x_temp, smooth_time * (is_freestyle ? 4f : 1f));

        locomotion._camera_pos_offset.z = Mathf.SmoothDamp(locomotion._camera_pos_offset.z, !is_freestyle ? locomotion.camera_pos_offset.z : locomotion.camera_pos_offset.z - 10f, ref loco_offset_z_temp, smooth_time * 4f);
        locomotion._camera_ori_offset = Vector3.SmoothDamp(locomotion._camera_ori_offset, (is_freestyle ? default : locomotion.camera_ori_offset), ref freestyle_ori_offset_temp, smooth_time * 4f);
        */
    }
}