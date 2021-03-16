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

    public MeshRenderer SeekerRenderer;
    public MeshFilter SeekerMesh; // Mesh of the model
    public GameObject Seeker;

    public Material GlobalEdgeLightMaterial; // test

    public Transform NotesContainer;

    public ClipManager ClipManager;
    public GameObject ClipPlane;

    [Header("Capture animation content refs")]
    public AmpTrackDestructFX DestructFX;

    [Header("Variables")]
    public AmpTrack Track; // The track this measure belongs to
    public List<AmpNote> Notes = new List<AmpNote>();

    [Header("Properties")]
    public int ID;
    public AmpTrack.InstrumentType Instrument;

    public float Length = 32f; // meters
    public Vector3 Position; // meters
    public float Rotation; // angles
    public Quaternion RotationQuat; // angles

    public bool IsEmpty;
    public bool IsCapturing;
    public MeasureCaptureState CaptureState = MeasureCaptureState.None;
    public bool IsCaptured
    {
        get { return CaptureState > 0; }
        set
        {
            CaptureState = !value ? MeasureCaptureState.None : MeasureCaptureState.Captured;
            if (CaptureState > 0)
                IsSequence = false; // TODO: revise?
        }
    }

    bool _isEnabled = true;
    public bool IsEnabled
    {
        get { return _isEnabled; }
        set
        {
            _isEnabled = value;
            Notes.ForEach(n => n.IsEnabled = false);
            Seeker.SetActive(false);
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
            if (!IsEmpty || !IsCaptured)
                Seeker.SetActive(value ? (_isSequence ? true : false) : false);
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

    public static Vector3[] og_verts;
    public static Vector3[] og_vertsSeeker;
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

        if (og_verts == null)
        {
            og_verts = new Vector3[ModelMesh.mesh.vertices.Length];
            ModelMesh.mesh.vertices.CopyTo(og_verts, 0);
            og_vertsSeeker = new Vector3[SeekerMesh.mesh.vertices.Length];
            SeekerMesh.mesh.vertices.CopyTo(og_vertsSeeker, 0);
        }
        // Deform the mesh!
        DeformMesh();
        MeshDeformer.DeformMesh
                                  (Path, SeekerMesh.mesh, Position, Rotation, ogVerts: og_vertsSeeker, offset: null, RhythmicGame.TrackWidth - 0.2f, -1, Length, movePivotToStart: true); // TODO: unneccessary parameters
        SeekerRenderer.material.color = Track.Color;
    }


    public void DeformMesh() => MeshDeformer.DeformMesh
                                  (Path, ModelMesh.mesh, Position, Rotation, ogVerts: og_verts, offset: null, RhythmicGame.TrackWidth, -1, Length, movePivotToStart: true); // TODO: unneccessary parameters
}