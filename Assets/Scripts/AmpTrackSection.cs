#undef PATH_LIVE_UPDATE

using UnityEngine;
using PathCreation;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor.Experimental.SceneManagement;
#endif

public enum MeasureCaptureState { None = 0, Capturing = 1, Captured = 2 }

/// AmpTrackSection (previously Measure)
// This is a single measure of a track.

[ExecuteInEditMode]
public class AmpTrackSection : MonoBehaviour
{
    AmpPlayerLocomotion Locomotion;
    public VertexPath Path;

    /// References to the contents
    [Header("Common")]
    //public GameObject Model; // Main track section model
    public MeshRenderer MeshRenderer;
    public MeshFilter MeshFilter; // Mesh of the model
    public EdgeLights EdgeLights_Local; // This is visually part of the track model itself - it gets clipped with capturing
    public EdgeLights EdgeLights_Global; // This is the 'focus indicator' edge lights, which does not get clipped.
    public Transform NotesContainer;

    public ClippingPlane ClipManager;
    public GameObject LengthPlane; // This plane trims the model to the desired length
    public GameObject ClipPlane;

    [Header("Variables")]
    public AmpTrack Track; // The track this measure belongs to
    public List<AmpNote> Notes = new List<AmpNote>();

    /// Path-related stuff
    // TODO: Global Path variable? !!!

    [Header("Properties")]
    public Vector3 Position;
    public float Rotation; // Note: in Euler angles!

    /// Global variables, properties and events
    public int ID;
    public float Length = 32f; // in zPos!
    public AmpTrack.InstrumentType Instrument;
    public bool IsEmpty;

    public bool IsCapturing;
    public MeasureCaptureState CaptureState;
    public bool IsCaptured
    {
        get { return CaptureState > 0; }
        set { CaptureState = !value ? MeasureCaptureState.None : MeasureCaptureState.Captured; }
    }

    private bool _isEnabled = true;
    public bool IsEnabled
    {
        get { return _isEnabled; }
        set
        {
            _isEnabled = value;

            foreach (AmpNote n in Notes)
                n.IsEnabled = false;

            MeasureColor = Color.black;
        }
    }

    private Color _edgeLightsColor;
    public Color EdgeLightsColor
    {
        get { return _edgeLightsColor; }
        set { _edgeLightsColor = value; EdgeLights_Local.Color = EdgeLights_Global.Color = value; }
    }

    private Color _measureColor;
    public Color MeasureColor
    {
        get { return _measureColor; }
        set { _measureColor = value; /*if (Track.IsTrackFocused)*/ MeshRenderer.material.color = Colors.ConvertColor(value); }
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

        Path = PathTools.Path;
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

        // TEMP: visualize measures (add separator to the end)
        //{
        //    var a = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //    a.transform.parent = transform;
        //    a.transform.localScale = new Vector3(transform.localScale.x, 0.1f, 0.1f);
        //    a.transform.position = PathTools.GetPositionOnPath(Path, Position.z + Length, -Tunnel.Instance.center);
        //    a.transform.rotation = PathTools.GetRotationOnPath(Path, Position.z + Length);
        //}

        // Set up global edge light based on track focus state
        SetGlobalEdgeLights(Track.IsTrackFocused);

        if (IsEmpty || IsCaptured)
        {
            // Disable measure visuals when empty or captured
            MeshRenderer.gameObject.SetActive(false);
            EdgeLights_Local.gameObject.SetActive(false);
        }

        MeshFilter.mesh = TrackMeshCreator.CreateMesh(RhythmicGame.TrackWidth, Length);

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

        // Set Edge lights mesh to the same mesh as top mesh!
        EdgeLights_Local.Mesh = EdgeLights_Global.Mesh = mesh;

        // Deform edge lights
        Vector3 edgelights_offset = new Vector3(0, 0.01f, 0); // Offset to the top of the track

        if (!IsEmpty || !IsCaptured)
            MeshDeformer.DeformMesh(path, EdgeLights_Local.Mesh, position, angle, originalMesh.vertices, edgelights_offset);
        MeshDeformer.DeformMesh(path, EdgeLights_Global.Mesh, position, angle, originalMesh.vertices, edgelights_offset);
    }

    public void LengthClip()
    {
        // Calculate clip plane offset based on measure draw distance
        float dist = Locomotion.HorizonLength;

        Vector3 planePos = PathTools.GetPositionOnPath(Path, dist);
        Quaternion planeRot = PathTools.GetRotationOnPath(Path, dist, new Vector3(90, 0, 0));

        LengthPlane.transform.position = planePos;
        LengthPlane.transform.rotation = planeRot;

        ClipManager.Clip();
    }
}