using PathCreation;
using System;
using UnityEngine;
using static Logger;

public class PlayerLocomotion : MonoBehaviour {
    // TODO: We need to figure out how things get set up - accessing 'Instance's of each
    // class is a bit tedious...
    SongSystem song_system;
    AudioSystem audio_system;
    Clock clock;
    TrackSystem track_system;

    public PlayerTrackSwitching track_switching;

    VertexPath path;

    [NonSerialized] public Transform trans;

    Vector3 orig_pos;
    public Camera main_camera;
    Transform main_camera_trans;
    public Vector3 camera_pos_offset;
    public Vector3 camera_ori_offset;
    [NonSerialized] public Vector3 _camera_pos_offset;
    [NonSerialized] public Vector3 _camera_ori_offset;

    public bool is_following_path = true;
    public Transform lookat_target;
    public Vector3 lookat_pos_offset;

    public Transform interp;
    public Transform non_interp;

    void Start() {
        song_system = SongSystem.Instance;
        audio_system = song_system.audio_system;
        clock = song_system.clock;
        track_system = song_system.track_system;

        path = PathTransform.PATH_FindPath();
        if (path == null) LogE("No path found!".T(this));

        trans = transform;
        orig_pos = transform.position;

        // Camera: | TODO: read these from config!
        main_camera_trans = main_camera.transform;
        main_camera.transform.localPosition = camera_pos_offset;
        //main_camera.transform.localEulerAngles = camera_ori_offset;
        _camera_pos_offset = camera_pos_offset;
    }

    public Vector3 offset_pos = default;
    public Vector3 offset_ori = default;

    public float smooth_time = 0.2f; // TODO: Config!
    public float interp_peekahead = 20f;

    public Vector3 pos_interp;
    public Quaternion rot_interp;
    Vector3 pos_interp_temp;
    Quaternion rot_interp_temp;
    void Update() {
        if (!is_following_path) {
            if (trans.position != orig_pos) trans.position = orig_pos;
            return;
        }
        if (!clock) return;

        float dist = clock.pos; // TODO: base this on audio_system? ...

        Vector3 pos_target = path.XZ_GetPointAtDistance(dist, offset_pos); // TODO: Get the 'x' out of position automatically?
        pos_interp = Vector3.SmoothDamp(pos_interp, pos_target, ref pos_interp_temp, smooth_time);

        Quaternion rot_target = path.XZ_GetRotationAtDistance(dist, offset_pos.x) * Quaternion.Euler(offset_ori);
        Quaternion rot_target_interp = path.XZ_GetRotationAtDistance(dist + interp_peekahead, offset_pos.x) * Quaternion.Euler(offset_ori);
        rot_interp = QuaternionUtil.SmoothDamp(rot_interp, rot_target_interp, ref rot_interp_temp, smooth_time);

        trans.position = pos_target;

        interp.rotation = rot_interp;
        non_interp.rotation = rot_target;

        Vector3 normal = non_interp.up;
        Vector3 normal2 = path.XZ_NormTest(dist, offset_pos.x);

        //Log("normal: %  normal2: %", normal, normal2);

        Debug.DrawLine(pos_target, pos_target + (normal * 1f), Color.red);
        Debug.DrawLine(pos_target, pos_target + (normal2 * 1f), Color.green);

        //main_camera_trans.LookAt(lookat_target, interp.up);
        main_camera_trans.localEulerAngles = _camera_ori_offset;
        main_camera_trans.localPosition = _camera_pos_offset;
    }
}