#define DYNAMIC

using PathCreation;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Logger;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public partial class PathTransform : MonoBehaviour
{
    public enum OriginMode
    {
        Default = 0, Center = 0, // The origin point of the model is not altered.
        Front = 1, // The origin point moves to the front of the model - changing size Z alters length.
        Back = 2, // The origin point moves to the back of the model - changin size Z alters inverse length (!).
        Custom = 3
    }

    public static bool PATHTRANSFORM_DynamicUpdate = true;

    //Transform trans;

    public static PathCreator pathcreator_global;
    public PathCreator pathcreator;
#if DYNAMIC || UNITY_EDITOR
    public VertexPath path { get { return pathcreator.path; } }
#else
    public VertexPath path;
#endif

    public MeshFilter mesh_filter;
    [NonSerialized] public Mesh mesh;

    public int vertex_count;
    List<Vector3> OG_vertices;
    List<Vector3> vertices;
    [NonSerialized] public Vector3 max_values;
    [NonSerialized] public Vector3 max_values_double; // cache

    public bool AutoDeform = true;

    Vector3 prev_pos;
    [HideInInspector] public Vector3 pos;

    Vector3 prev_rot;
    [HideInInspector] public Vector3 euler_rot;

    Vector3 prev_desired_size;
    [HideInInspector] public Vector3 desired_size = new Vector3(-1, -1, -1);

    OriginMode prev_origin_mode;
    public OriginMode origin_mode = OriginMode.Default;
    public float origin_custom = 0f;

    public float prev_min_clip_frac = 0f;
    public float prev_max_clip_frac = 1f;
    [Range(0f, 1f)] public float min_clip_frac = 0f;
    [Range(0f, 1f)] public float max_clip_frac = 1f;

    public static PathCreator PATH_FindPathCreator()
    {
        PathCreator a = FindObjectOfType<PathCreator>();
        if (!a) LogE("Could not find a PathCreator!".M());

        return a;
    }

    public static VertexPath PATH_FindPath()
    {
        PathCreator creator = PATH_FindPathCreator();
        if (creator) return creator.path;

        return null;
    }

    void Awake()
    {
        //trans = transform;
        desired_size = new Vector3(-1, -1, -1);

        // PathCreator and path: | TODO: Improve this!
        if (!pathcreator)
        {
            if (!pathcreator_global) pathcreator_global = FindObjectOfType<PathCreator>();
            if (!pathcreator_global && LogE("% - Could not find a global PathCreator candidate!".T(this), gameObject.name.AddColor(Colors.Unimportant))) return;

            pathcreator = pathcreator_global;
        }

        if (!pathcreator && LogW("No PathCreators were found! - %".T(this), gameObject.name)) return;

        // Assign path once in release builds for performance:
#if !UNITY_EDITOR && !DYNAMIC
        path = pathcreator.path;
#endif

        // Subscribe to path update events to reflect path changes dynamically:
#if DYNAMIC && UNITY_EDITOR
        if (PATHTRANSFORM_DynamicUpdate)
            pathcreator.pathUpdated += Pathcreator_pathUpdated;
#endif

        // Mesh:
        if (!mesh_filter) mesh_filter = GetComponent<MeshFilter>();
        InitMesh();

        // TEST:
        //if (desired_size == new Vector3(-1, -1, -1))
        //    desired_size = max_values;

        if (desired_size.x == -1) desired_size.x = max_values.x * 2;
        if (desired_size.y == -1) desired_size.y = max_values.y * 2;
        if (desired_size.z == -1) desired_size.z = max_values.z * 2;

        Deform();
    }

    void InitMesh()
    {
        if (mesh_filter.sharedMesh == null)
        {
            LogW("There is no mesh for the PathTransform object %.", gameObject.name);
            return;
        }

        mesh = mesh_filter.mesh;
        vertex_count = mesh.vertexCount;

        if (OG_vertices != null) return; // Already initialized.

        OG_vertices = new List<Vector3>(vertex_count);
        vertices = new List<Vector3>(vertex_count);
        for (int i = 0; i < vertex_count; ++i) vertices.Add(default);

        mesh_filter.sharedMesh.GetVertices(OG_vertices);

        // Get max X/Y:
        max_values = GetMaxValuesFromVertices(OG_vertices);
        max_values_double = (max_values * 2);
    }
    public void Restore_OG() => mesh.SetVertices(OG_vertices);

    public static Vector3 GetMaxValuesFromVertices(List<Vector3> vertices)
    {
        return new Vector3(
            Mathf.Abs(vertices.Max(v => v.x)),
            Mathf.Abs(vertices.Max(v => v.y)),
            Mathf.Abs(vertices.Max(v => v.z)));
    }

    /// <summary>Specify -1 to not change a given clip value!</summary>
    public void ChangeClipValues(float min, float max)
    {
        if (min < 0 && max < 0) return;

        if (min >= 0) min_clip_frac = min;
        if (max >= 0) max_clip_frac = max;

        if (max < min || min > max) return;

        Deform();
    }

    void Update()
    {
        if (!AutoDeform) return;

        // Check previous value and update deformation if any has changed.
        if (prev_pos == pos && prev_rot == euler_rot && prev_desired_size == desired_size 
            && prev_origin_mode == origin_mode 
            && prev_min_clip_frac == min_clip_frac && prev_max_clip_frac == max_clip_frac
            ) return;

        prev_pos = pos;
        prev_rot = euler_rot;
        prev_desired_size = desired_size;

        prev_origin_mode = origin_mode;
        prev_min_clip_frac = min_clip_frac;
        prev_max_clip_frac = max_clip_frac;

        Deform();
    }

    private void Pathcreator_pathUpdated() => Deform();

#if UNITY_EDITOR && DYNAMIC
    void OnDestroy()
    {
        if (pathcreator)
            pathcreator.pathUpdated -= Pathcreator_pathUpdated;
    }
#endif
}