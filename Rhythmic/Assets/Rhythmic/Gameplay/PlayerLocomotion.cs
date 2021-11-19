using PathCreation;
using UnityEngine;
using static Logger;

public class PlayerLocomotion : MonoBehaviour
{
    // TODO: We need to figure out how things get set up - accessing 'Instance's of each
    // class is a bit tedious...
    SongSystem song_system;
    AudioSystem audio_system;
    Clock clock;
    TrackSystem track_system;

    VertexPath path;

    Transform trans;

    Vector3 orig_pos;
    public Camera main_camera;
    Transform main_camera_trans;
    public Vector3 camera_pos_offset;
    public Vector3 camera_ori_offset;

    public bool is_following_path = true;
    public Transform lookat_target;

    void Start()
    {
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
    }

    public Vector3 offset_pos = default;
    public Vector3 offset_ori = default;

    void Update()
    {
        if (!is_following_path)
        {
            if (trans.position != orig_pos) trans.position = orig_pos;
            return;
        }

        float dist = clock.pos; // TODO: base this on audio_system? ...
        Vector3 pos = path.XZ_GetPointAtDistance(dist, offset_pos, offset_pos.x); // TODO: Get the 'x' out of position automatically?
        Quaternion rot = path.XZ_GetRotationAtDistance(dist, offset_pos.x) * Quaternion.Euler(offset_ori);

        trans.position = pos;
        trans.rotation = rot;

        main_camera_trans.LookAt(lookat_target, trans.up);
        main_camera_trans.localEulerAngles += camera_ori_offset;
    }
}