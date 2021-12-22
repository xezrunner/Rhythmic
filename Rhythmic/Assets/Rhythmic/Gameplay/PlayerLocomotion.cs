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

    [NonSerialized] public Transform trans;
    public PlayerTrackSwitching track_switching;
    VertexPath path;

    Vector3 orig_pos;

    // Camera:
    public Transform main_camera_trans;

    // Containers:
    public Transform container_interp;
    public Transform container_noninterp;

    public bool is_following_path = true;

    void Start() {
        song_system = SongSystem.Instance;
        audio_system = song_system.audio_system;
        clock = song_system.clock;
        track_system = song_system.track_system;

        path = PathTransform.PATH_FindPath();
        if (path == null) LogE("No path found!".T(this));

        trans = transform;
        orig_pos = transform.position;

    }

    float dist;
    public float interp_peekahead = 20f;
    public Vector3 offset_pos;

    Quaternion rot_interp_ref;

    void LOCOMOTION_Step(bool smooth = true) {
        // Basic locomotion:
        Vector3 pos = path.XZ_GetPointAtDistance(dist, offset_pos);
        trans.position = pos;

        Quaternion rot = path.XZ_GetRotationAtDistance(dist, offset_pos.x);
        container_noninterp.localRotation = rot;

        Quaternion rot_interp = path.XZ_GetRotationAtDistance(dist + interp_peekahead, offset_pos.x);
        if (!smooth) {
            container_interp.localRotation = rot_interp;
        } else {
            container_interp.localRotation = QuaternionUtil.SmoothDamp(container_interp.localRotation, rot_interp, ref rot_interp_ref, Variables.CAMERA_SmoothTime);
        }
    }

    void Update() {
        dist = clock.pos; // TODO: base this on audio_system? ...
        bool is_playing = (clock.is_scrubbing || audio_system.is_playing);
        LOCOMOTION_Step(is_playing);
    }
}