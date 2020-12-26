#undef PATH_LIVE_UPDATE

using UnityEngine;
using PathCreation;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor.Experimental.SceneManagement;
#endif

/// AmpTrackSection (previously Measure)
// This is a single measure of a track.

[ExecuteInEditMode]
public class AmpTrackSection : MonoBehaviour
{
    SongController SongController { get { return SongController.Instance; } }
    AmpPlayerLocomotion Locomotion;
    public PathCreator PathCreator { get { return GameObject.Find("Path").GetComponent<PathCreator>(); } } // TODO: change these to variables
    public VertexPath Path { get { if (PathCreator) return PathCreator.path; else { Debug.LogError("AmpTrack: Path not available - PathCreator is null!"); return null; } } }

    /// References to the contents
    [Header("Common")]
    //public GameObject Model; // Main track section model
    public MeshRenderer MeshRenderer;
    public MeshFilter MeshFilter; // Mesh of the model
    public EdgeLights EdgeLights_Local; // This is visually part of the track model itself - it gets clipped with capturing
    public EdgeLights EdgeLights_Global; // This is the 'focus indicator' edge lights, which does not get clipped.
    public Transform NoteContainer;

    public ClippingPlane ClipManager;
    public GameObject LengthPlane; // This plane trims the model to the desired length
    public GameObject ClipPlane;

    [Header("Variables")]
    public AmpTrack Track; // The track this measure belongs to
    public List<AmpNote> Notes = new List<AmpNote>();

    /// Path-related stuff
    /// TODO: Global Path variable? !!!

    [Header("Properties")]
    public Vector3 Position;
    public float Rotation; // Note: in Euler angles!

    /// Global variables, properties and events
    public int ID;
    public float Length = 32f; // in zPos!
    public AmpTrack.InstrumentType Instrument;
    public bool IsEmpty;

    public bool IsCapturing;
    public bool IsCaptured;

    // MeshRenderer.material.SetColor("_Color", Colors.ConvertColor(value));

    private Color _color;
    public Color Color
    {
        get { return _color; }
        set { _color = value; EdgeLights_Local.Color = EdgeLights_Global.Color = value; }
    }

    // Deformation
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
    // Deformation
    Mesh originalMesh; // Live update - original mesh copy
    #region Live path update
#if PATH_LIVE_UPDATE
    Vector3 _prevPositionOnPath;
    float _prevRotationOnPath;
    float _prevLength;
#endif
    #endregion

    /// Functionality

    [ExecuteInEditMode]
    void Awake()
    {
#if UNITY_EDITOR
        if (PrefabStageUtility.GetCurrentPrefabStage() != null) // Warning: do not change mesh in prefab isolation!
            return;
#endif

        Locomotion = AmpPlayerLocomotion.Instance;
    }
    //[ExecuteInEditMode]
    void Start()
    {
#if PATH_LIVE_UPDATE
        // Set up live mesh update
        _prevLength = Length;
        _prevPositionOnPath = PositionOnPath;
        _prevRotationOnPath = RotationOnPath;
#endif
#if UNITY_EDITOR
        if (PrefabStageUtility.GetCurrentPrefabStage() != null) // Warning: do not change mesh in prefab isolation!
            return;
#endif

        // Set up global edge light based on track focus state
        SetGlobalEdgeLights(Track.IsTrackFocused);

        MeshFilter.mesh = AmpMeshTestScript.CreateMesh(RhythmicGame.TrackWidth, Length);

        if (MeshFilter)
            originalMesh = MeshFilter.sharedMesh;

        // Automatically deform to path
        if (StartAutoDeformToPath) DeformMeshToPath();

        LengthClip();
    }

    /// Edge lights
    public void SetGlobalEdgeLights(bool value = true) => EdgeLights_Global.IsActive = value;

    /// Model deformation and functionality

    #region Live path update
#if PATH_LIVE_UPDATE
    void FixedUpdate()
    {
        if (DeformLiveUpdate)
        {
            if (_prevLength != Length)
            { _prevLength = Length; UpdateModelLength(); }
            if (_prevPositionOnPath != PositionOnPath)
            { _prevPositionOnPath = PositionOnPath; DeformMeshToPath(); } // Deform mesh when position changes
            if (_prevRotationOnPath != RotationOnPath)
            { _prevRotationOnPath = RotationOnPath; DeformMeshToPath(); }
        }
}
#endif
    #endregion

    // Deforms the mesh to the path
    // TODO: Deformation live updating!
    public void DeformMeshToPath() => DeformMeshToPath(Path, Length, Position, Rotation);
    public void DeformMeshToPath(VertexPath path, float length, Vector3 position, float angle) // Deforms the mesh at the given position and length
    {
        if (!Application.isPlaying & BlockDeformsInEditMode) // Do not deform mesh when edit deformation is blocked!
            return;
#if UNITY_EDITOR
        if (PrefabStageUtility.GetCurrentPrefabStage() != null) // Warning: do not change mesh in prefab isolation!
            return;
#endif

        if (!MeshFilter)
        { Debug.LogError("TrackMeasure/DeformMeshToPath(): MeshFilter component not found!"); return; }

        Mesh mesh = MeshFilter.mesh;

        // Mesh deformation
        MeshDeformer.DeformMesh(path, mesh, position, angle, originalMesh.vertices);
        EdgeLights_Local.Mesh = EdgeLights_Global.Mesh = mesh;
        // Set Edge lights mesh to the same mesh as top mesh!
        Vector3 edgelights_offset = new Vector3(0, 0.01f, 0); // Offset to the top of the track
        MeshDeformer.DeformMesh(path, EdgeLights_Local.Mesh, position, angle, originalMesh.vertices, edgelights_offset);
        MeshDeformer.DeformMesh(path, EdgeLights_Global.Mesh, position, angle, originalMesh.vertices, edgelights_offset);
    }

    public void LengthClip()
    {
        // Calculate clip plane offset based on measure draw distance
        float offset = Locomotion.HorizonLength;

        Vector3 localUp = Vector3.Cross(Path.GetTangentAtDistance(offset), Path.GetNormalAtDistance(offset));
        Vector3 localRight = Path.GetNormalAtDistance(offset);

        Vector3 planePos = Path.GetPointAtDistance(offset);

        Quaternion planeRot = Path.GetRotationAtDistance(offset) * Quaternion.Euler(90, 0, 0);

        LengthPlane.transform.position = planePos;
        LengthPlane.transform.rotation = planeRot;

        ClipManager.Clip();
    }
}