using UnityEngine;
using System.Collections.Generic;
using PathCreation;

public enum MeasureCaptureState { None = 0, Capturing = 1, Captured = 2 }

/// AmpTrackSection (previously Measure)
/// This is a single measure of a track.

public class AmpTrackSection : MonoBehaviour
{
    public VertexPath Path;

    /// ----- Properties -----

    [Header("Content references")]
    public MeshRenderer ModelRenderer;
    public MeshFilter ModelMesh; // Mesh of the model

    public Material GlobalEdgeLightMaterial; // test

    public Transform NotesContainer;

    public ClipManager ClipManager;
    public GameObject ClipPlane;

    [Header("Variables")]
    public AmpTrack Track; // The track this measure belongs to
    public List<AmpNote> Notes = new List<AmpNote>();

    [Header("Properties")]
    public int ID;
    public AmpTrack.InstrumentType Instrument;

    public float Length = 32f; // meters
    public Vector3 Position; // meters
    public float Rotation; // angles

    public bool IsEmpty;
    public bool IsCapturing;
    public MeasureCaptureState CaptureState = MeasureCaptureState.None;
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
            Notes.ForEach(n => n.IsEnabled = false);
            // TODO: Disable active surface! (IsFocused / IsSequence to false?)
        }
    }

    bool _isFocused;
    public bool IsFocused
    {
        get { return _isFocused; }
        set
        {
            _isFocused = value;

            // Toggle edge light material
            GlobalEdgeLightMaterial.SetInteger("_Enabled", value ? 1 : 0); // TODO: Optimization?
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
            //else IsFocused = false;
        }
    }

    public Color EdgeLightsColor { get; set; } // tba
    public Color Color { get; set; }

    /// ----- Functionality -----

    public static bool AllowDeformations = true; // TODO!

    void Awake() => Path = PathTools.Path;
    void Start()
    {
        if (!ModelMesh || !ModelMesh.sharedMesh)
        { Debug.LogWarning($"AmpTrackSection [init]: measure {ID} does not have a Model!"); return; }

        // Disable measure visuals when empty or captured
        if (IsEmpty || IsCaptured)
        {
            ModelRenderer.materials = new Material[0];
            ModelRenderer.material = GlobalEdgeLightMaterial;
        }
        // Deform the mesh!
        DeformMesh();
    }

    public void DeformMesh() => MeshDeformer.DeformMesh
                                  (Path, ModelMesh.mesh, Position, Rotation, ogVerts: null, offset: null, RhythmicGame.TrackWidth, -1, Length, movePivotToStart: true); // TODO: unneccessary parameters
}