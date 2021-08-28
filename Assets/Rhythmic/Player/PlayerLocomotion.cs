using PathCreation;
using UnityEngine;
using static Logger;

public class PlayerLocomotion : MonoBehaviour
{
    public static PlayerLocomotion Instance;

    Transform trans;
    public Camera loco_camera; // Follows the path.
    public PathCreator pathcreator;
    public WorldSystem worldsystem;

    public Vector3 pos_offset;
    public Vector3 rot_offset;
    public float distance;

#if UNITY_EDITOR
    public VertexPath path { get { return pathcreator.path; } }
#else
    public VertexPath path;
#endif

    public void Awake()
    {
        Instance = this;
        trans = transform;
        worldsystem = WorldSystem.Instance;

        if (!pathcreator) pathcreator = WorldSystem.GetAPathCreator();
        if (!pathcreator && LogE("No pathcreator!".T(this))) return;

        // Changes to the path are reflected in debug builds:
#if !UNITY_EDITOR
        path = pathcreator.path;
#endif
    }

    public void Start()
    {
        LOCOMOTION_Step();
    }

    public float rot_smoothness = 0.5f;
    Quaternion rot_ref;
    public void LOCOMOTION_Step()
    {
        if (!pathcreator) return;

        // TODO: separate interp-able vs non-interp-able
        trans.localPosition = path.XZ_GetPointAtDistance(distance, pos_offset, pos_offset.x);
        // trans.localRotation = path.XZ_GetRotationAtDistance(distance, pos_offset.x) * Quaternion.Euler(rot_offset); // TODO: simpify this! (?)
        trans.localRotation = QuaternionUtil.SmoothDamp(trans.localRotation, path.XZ_GetRotationAtDistance(distance, pos_offset.x) * Quaternion.Euler(rot_offset), ref rot_ref, rot_smoothness);
    }

    public float temp_speed = 5f;
    public bool temp_isplaying = false;
    public void Update()
    {
        if (!pathcreator) return;
        LOCOMOTION_Step();

        if (!temp_isplaying) return;
        distance += temp_speed * Time.deltaTime;
    }
}
