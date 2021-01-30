using UnityEngine;
using PathCreation;
using System.Collections.Generic;

public enum MeasureCaptureState { None = 0, Capturing = 1, Captured = 2 }

/// AmpTrackSection (previously Measure)
// This is a single measure of a track.

public class AmpTrackSection : MonoBehaviour
{
    AmpPlayerLocomotion Locomotion;
    public VertexPath Path;

    [Header("Common")]
    public ClipManager ClipManager;
    public GameObject LengthPlane; // This plane trims the model to the desired length
    public GameObject ClipPlane;

    [Header("Content references")]
    public MeshRenderer MeshRenderer;
    public MeshFilter MeshFilter; // Mesh of the model

    public GameObject ActiveSurface;
    public MeshRenderer ActiveSurfaceMeshRenderer;
    public MeshFilter ActiveSurfaceMeshFilter;

    public EdgeLights EdgeLights_Local; // This is visually part of the track model itself - it gets clipped with capturing
    public EdgeLights EdgeLights_Global; // This is the 'focus indicator' edge lights, which does not get clipped.

    public Transform NotesContainer;

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

    bool _isEnabled = true;
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

    bool _isSequence;
    public bool IsSequence
    {
        get { return _isSequence; }
        set
        {
            _isSequence = value;
            if (value) IsFocused = Track.IsTrackFocused;
            else IsFocused = false;
        }
    }
    public bool IsFocused { get { return ActiveSurface.activeSelf; } set { ActiveSurface.SetActive(value); } }

    private Color _edgeLightsColor;
    public Color EdgeLightsColor
    {
        get { return _edgeLightsColor; }
        set { _edgeLightsColor = value; EdgeLights_Local.Color = /*EdgeLights_Global.Color =*/ value; }
    }

    private Color _measureColor;
    public Color MeasureColor
    {
        get { return _measureColor; }
        set { _measureColor = value; }
    }

    // Deformation
    public bool StartAutoDeformToPath = true;

    /// Temporary variables
    // Deformation
    Mesh originalMesh; // Live update - original mesh copy

    /// Functionality

    void Awake()
    {
        Path = PathTools.Path;
        Locomotion = AmpPlayerLocomotion.Instance;
    }

    void Start()
    {
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

        // Create base mesh for deformation
        MeshFilter.mesh = TrackMeshCreator.CreateMesh(RhythmicGame.TrackWidth, Length);

        if (MeshFilter)
            originalMesh = MeshFilter.sharedMesh;

        // Automatically deform to path
        if (StartAutoDeformToPath) DeformMeshToPath();
    }

    /// Edge lights
    public void SetGlobalEdgeLights(bool value = true) => EdgeLights_Global.IsActive = value;

    /// Model deformation and functionality

    // Deforms the mesh to the path
    // TODO: Deformation live updating!
    public void DeformMeshToPath() => DeformMeshToPath(Path, Length, Position, Rotation);
    public void DeformMeshToPath(VertexPath path, float length, Vector3 position, float angle) // Deforms the mesh at the given position and length
    {
        if (!MeshFilter)
        { Debug.LogError("TrackMeasure/DeformMeshToPath(): MeshFilter component not found!"); return; }

        Mesh mesh = MeshFilter.mesh;

        // Mesh deformation
        MeshDeformer.DeformMesh(path, mesh, position, angle, originalMesh.vertices);

        // Set active indicator mesh
        ActiveSurfaceMeshFilter.mesh = mesh;
        // Set Edge lights mesh to the same mesh as top mesh!
        EdgeLights_Local.Mesh = /*EdgeLights_Global.Mesh =*/ mesh;

        // Deform edge lights
        Vector3 edgelights_offset = new Vector3(0, 0.01f, 0); // Offset to the top of the track

        if (!IsEmpty || !IsCaptured)
            MeshDeformer.DeformMesh(path, EdgeLights_Local.Mesh, position, angle, originalMesh.vertices, edgelights_offset);

        //MeshDeformer.DeformMesh(path, EdgeLights_Global.Mesh, position, angle, originalMesh.vertices, edgelights_offset);
    }
}