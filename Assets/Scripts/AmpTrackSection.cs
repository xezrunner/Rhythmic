#define PATH_LIVE_UPDATE

using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

/// AmpTrackSection (previously Measure)
/// This is a single measure of a track.

[ExecuteInEditMode]
public class AmpTrackSection : MonoBehaviour
{
    SongController SongController { get { return SongController.Instance; } }

    /// References to the contents
    public GameObject Model; // Main track section model
    public MeshFilter MeshFilter; // Mesh of the model

    public GameObject LengthPlane; // This plane trims the model to the desired length.

    /// Path-related stuff
    /// TODO: Global Path variable? !!!
    public PathCreation.PathCreator PathCreator { get { return GameObject.Find("Path").GetComponent<PathCreator>(); } }
    public PathCreation.VertexPath Path { get { if (PathCreator) return PathCreator.path; else { Debug.LogError("AmpTrack: Path not available - PathCreator is null!"); return null; } } }

    // TODO: make these properties that'll move the track along the path!
    Vector3 _prevPositionOnPath;
    public Vector3 PositionOnPath;
    public Vector3 RotationOnPath; // Note: in Euler angles!

    /// Global variables and properties
    public float Length = 32f; // in zPos!

    public bool StartAutoDeformToPath = true;
    public bool BlockDeformsInEditMode = true;

    public bool _deformLiveUpdate = false;
    public bool DeformLiveUpdate  // Whether we should support moving the section along the path in real-time (performance impact!)
    {
        get
        {
#if PATH_LIVE_UPDATE
            return _deformLiveUpdate;
#else
            return false;
#endif
        }
        set { _deformLiveUpdate = value; }
    }

    /// Temporary variables
    Mesh originalMesh; // Live update - original mesh copy

    void Awake()
    {
        if (MeshFilter)
            originalMesh = MeshFilter.sharedMesh;

        // Keep copy of original mesh if live update is supported
        //if (DeformLiveUpdate)
        //{
        //    if (MeshFilter)
        //        originalMesh = MeshFilter.sharedMesh;
        //    else
        //        Debug.LogError("TrackMeasure [live mesh updating]: No mesh specified! Live updating will not work.");
        //}
    }

    [ExecuteInEditMode]
    void Start()
    {
        // Automatically deform to path
        if (StartAutoDeformToPath) DeformMeshToPath();
    }

    void FixedUpdate()
    {
#if PATH_LIVE_UPDATE
        if (DeformLiveUpdate)
            if (_prevPositionOnPath != PositionOnPath)
            { _prevPositionOnPath = PositionOnPath; DeformMeshToPath(); } // Deform mesh when position changes
#endif
    }

    /// Model functions

    // Deforms the mesh to the path
    // TODO: Deformation live updating!
    public void DeformMeshToPath() => DeformMeshToPath(Path, Length, PositionOnPath, RotationOnPath);
    public void DeformMeshToPath(VertexPath path, float length, Vector3 position, Vector3 rotation) // Deforms the mesh at the given position and length
    {
        if (!Application.isPlaying & BlockDeformsInEditMode)
            return;
        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            return;

        if (!MeshFilter)
        { Debug.LogError("TrackMeasure/DeformMeshToPath(): MeshFilter component not found!"); return; }

        Mesh mesh = MeshFilter.mesh;

        // Mesh deformation
        MeshDeformer.DeformMesh(path, mesh, PositionOnPath, RotationOnPath, originalMesh.vertices);

        // Change perceived length of model
        UpdateModelLength();
    }

    public void UpdateModelLength() => ChangeModelLength(Length, PositionOnPath, RotationOnPath);
    public void ChangeModelLength(float length, Vector3 pos, Vector3 rot)
    {
        Vector3 planePos = Path.GetPointAtDistance(pos.z + length);
        Quaternion planeRot = Path.GetRotationAtDistance(planePos.z + length) * Quaternion.Euler(90, 0, 0);

        LengthPlane.transform.position = planePos;
        LengthPlane.transform.rotation = planeRot;
    }
}