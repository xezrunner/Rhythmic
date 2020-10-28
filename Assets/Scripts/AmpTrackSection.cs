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

    public Vector3 PositionOnPath;
    public float RotationOnPath; // Note: in Euler angles!

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
    Vector3 _prevPositionOnPath;
    float _prevRotationOnPath;
    float _prevLength;

    void Awake()
    {
        if (MeshFilter)
            originalMesh = MeshFilter.sharedMesh;

        _prevLength = Length;
        _prevPositionOnPath = PositionOnPath;
        _prevRotationOnPath = RotationOnPath;
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
        {
            if (_prevLength != Length)
            { _prevLength = Length; UpdateModelLength(); }
            if (_prevPositionOnPath != PositionOnPath)
            { _prevPositionOnPath = PositionOnPath; DeformMeshToPath(); } // Deform mesh when position changes
            if (_prevRotationOnPath != RotationOnPath)
            { _prevRotationOnPath = RotationOnPath; DeformMeshToPath(); }
        }
#endif
    }

    /// Model functions

    // Deforms the mesh to the path
    // TODO: Deformation live updating!
    public void DeformMeshToPath() => DeformMeshToPath(Path, Length, PositionOnPath, RotationOnPath);
    public void DeformMeshToPath(VertexPath path, float length, Vector3 position, float angle) // Deforms the mesh at the given position and length
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

    public void UpdateModelLength() => ChangeModelLength(Length, PositionOnPath);
    public void ChangeModelLength(float length, Vector3 pos)
    {
        Vector3 planePos = Path.GetPointAtDistance(pos.z + length);
        Quaternion planeRot = Path.GetRotationAtDistance(planePos.z + length) * Quaternion.Euler(90, 0, 0);

        LengthPlane.transform.position = planePos;
        LengthPlane.transform.rotation = planeRot;
    }
}