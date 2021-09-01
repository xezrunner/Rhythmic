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
    public static bool PATHTRANSFORM_DynamicUpdate = false;

    //Transform trans;

    public static PathCreator pathcreator_global;
    public PathCreator pathcreator;
#if UNITY_EDITOR && DYNAMIC // || UNITY_EDITOR
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

    public bool AutoDeform = true;

    Vector3 prev_pos;
    [HideInInspector] public Vector3 pos;

    Vector3 prev_rot;
    [HideInInspector] public Vector3 euler_rot;

    void Awake()
    {
        //trans = transform;

        // PathCreator and path: | TODO: Improve this!
        if (!pathcreator)
        {
            if (!pathcreator_global) pathcreator_global = FindObjectOfType<PathCreator>();
            if (!pathcreator_global && LogE("Could not find a global PathCreator candidate!".T(this))) return;

            pathcreator = pathcreator_global;
        }

        if (!pathcreator && LogW("No PathCreators were found! - %".T(this), gameObject.name)) return;

        // Assign path once in release builds for performance:
#if !UNITY_EDITOR || !DYNAMIC //!UNITY_EDITOR
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
        max_values = new Vector3(
            Mathf.Abs(OG_vertices.Max(v => v.x)),
            Mathf.Abs(OG_vertices.Max(v => v.y)),
            Mathf.Abs(OG_vertices.Max(v => v.z)));
    }
    public void Restore_OG() => mesh.SetVertices(OG_vertices);

    void Update()
    {
        if (!AutoDeform) return;
        if (prev_pos == pos && prev_rot == euler_rot) return;

        prev_pos = pos;
        prev_rot = euler_rot;
        Deform();
    }

    private void Pathcreator_pathUpdated() => Deform();

#if UNITY_EDITOR && DYNAMIC
    void OnDestroy() => pathcreator.pathUpdated -= Pathcreator_pathUpdated;
#endif
}