using UnityEngine;
using System.Collections.Generic;
using PathCreation;

public enum MeasureCaptureState { None = 0, Capturing = 1, Captured = 2 }

/// AmpTrackSection (previously Measure)
/// This is a single measure of a track.

public class AmpTrackSection : MonoBehaviour
{
    public VertexPath Path;
    SongController SongController { get { return SongController.Instance; } }
    TrackStreamer TrackStreamer { get { return TrackStreamer.Instance; } }

    /// ----- Properties -----

    [Header("Content references")]
    public MeshRenderer ModelRenderer;
    public MeshFilter ModelMesh; // Mesh of the model

    public MeshRenderer SeekerRenderer;
    public MeshFilter SeekerMesh; // Mesh of the model
    public GameObject Seeker;

    public Transform NotesContainer;

    public ClipManager ClipManager;
    public GameObject ClipPlane;

    public static bool _seekerEnabled = true;
    public static bool SeekerEnabled
    {
        get { return _seekerEnabled; }
        set
        {
            _seekerEnabled = value;
            foreach (AmpTrack t in TracksController.Instance.Tracks)
            {
                foreach (AmpTrackSection m in t.Measures)
                    if (m && m.SeekerRenderer) m.SeekerRenderer.enabled = value;
            }
        }
    }

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
            {
                IsSequence = false; // TODO: revise?
                SetEmptyMaterials();
            }
        }
    }

    void SetEmptyMaterials()
    {
        ModelRenderer.materials = new Material[0];
        ModelRenderer.material = Track.Track_Bottom_Global_Mat;
    }

    public bool IsEnabled = true;
    public void SetIsEnabled(bool value)
    {
        if (IsEnabled == value) return;
        IsEnabled = value;
        Notes.ForEach(n => n.IsEnabled = false);
        Seeker.SetActive(false);
        // TODO: Disable active surface! (IsFocused / IsSequence to false?)

        foreach (AmpTrack t in Track.TrackTwins)
            t.Measures[ID].SetIsEnabled(value);
    }

    bool _isFocused;
    public bool IsFocused
    {
        get { return _isFocused; }
        set
        {
            _isFocused = value;

            // Toggle edge light material
            Track.Track_Bottom_Global_Mat.SetInteger("_Enabled", value ? 1 : 0); // TODO: Optimization?
            if (!IsEmpty || !IsCaptured)
                Seeker.SetActive(value ? (_isSequence && !IsCaptured ? true : false) : false);
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
            else if (Seeker) Seeker.SetActive(false);
        }
    }

    public Color EdgeLightsColor { get; set; } // tba
    public Color Color { get; set; }

    /// ----- Functionality -----

    public static Vector3[] og_verts;
    public static Vector3[] og_verts_seeker;
    public static bool AllowDeformations = true; // TODO!

    void PreloadMeshes()
    {
        if (og_verts == null)
        {
            og_verts = new Vector3[ModelMesh.mesh.vertices.Length]; ModelMesh.mesh.vertices.CopyTo(og_verts, 0);
            og_verts_seeker = new Vector3[SeekerMesh.mesh.vertices.Length]; SeekerMesh.mesh.vertices.CopyTo(og_verts_seeker, 0);
        }
    }

    public List<GameObject> Cap_Objects;
    void SetupEndcaps()
    {
        if (IsEmpty || IsCaptured) return;

        // Check whether we need front caps:
        if (ID - 1 > 0)
        {
            if (Track.Measures[ID - 1].IsEmpty || Track.Measures[ID - 1].IsCaptured)
            {
                // Instatiate caps for FRONT:
                var bottom_wrap = Track.Cap_Instantiate(this, Track_Cap_Type.Bottom_Wrap);
                var top_cap = Track.Cap_Instantiate(this, Track_Cap_Type.Top_Bevel);

                DeformEndcap(Position, bottom_wrap, new Vector3(0, -0.032f, 0.015f)); // TODO: OFFSET HERE IS A HACK!
                DeformEndcap(Position + new Vector3(0,0, -0.7498201f * 2f), top_cap, new Vector3(0, 0, 0)); // TODO: OFFSET HERE IS A HACK!
            }
        }

        // Check whether we need back caps:
        if (ID + 1 < SongController.songLengthInMeasures && ID + 1 < TrackStreamer.metaMeasures.Length)
        {
            //if (Track.Measures[ID + 1].IsEmpty || Track.Measures[ID + 1].IsCaptured)
            if (TrackStreamer.metaMeasures[Track.ID, ID + 1].IsEmpty || TrackStreamer.metaMeasures[Track.ID, ID + 1].IsCaptured)
            {
                // Instatiate caps for FRONT:
                var bottom_wrap = Track.Cap_Instantiate(this, Track_Cap_Type.Bottom_Wrap, true);
                var top_cap = Track.Cap_Instantiate(this, Track_Cap_Type.Top_Bevel, true);

                DeformEndcap(Position + new Vector3(0, 0, Length - 0.8608165f), bottom_wrap, new Vector3(0, -0.032f, 0)); // TODO: OFFSETS HERE ARE HACKS!
                DeformEndcap(Position + new Vector3(0, 0, Length - (-1.23151f - (-1.23151f / 2))), top_cap); // TODO: OFFSETS HERE ARE HACKS!
            }
        }
    }

    void Awake() => Path = PathTools.Path;
    void Start()
    {
        if (!ModelMesh || !ModelMesh.sharedMesh)
        { Debug.LogWarning($"AmpTrackSection [init]: measure {ID} does not have a Model!"); return; }

        SeekerRenderer.enabled = SeekerEnabled;

        // Disable measure visuals when empty or captured
        if (IsEmpty || IsCaptured)
            SetEmptyMaterials();

        // TODO: optimize and revise
        if (og_verts == null)
            PreloadMeshes();

        // End caps:
        SetupEndcaps();

        // Seeker:
        SeekerRenderer.material.color = Track.Color;
        DeformSeeker(); // TODO: perhaps we won't have seekers in this way?

        // Deform the mesh!
        DeformMesh();

    }

    public void DeformMesh() => MeshDeformer.DeformMesh
                                  (Path, ModelMesh.mesh, Position, Rotation, ogVerts: og_verts, offset: null, RhythmicGame.TrackWidth, -1, Length, movePivotToStart: true); // TODO: unneccessary parameters
    public void DeformEndcap(Vector3 pos, TrackModelCap cap, Vector3? offset = null)
    {
        MeshDeformer.DeformMesh(Path, cap.MeshFilter.mesh, pos, Rotation, ogVerts: null, offset: offset, RhythmicGame.TrackWidth, -1, -1, movePivotToStart: true); // TODO: unneccessary parameters
    }
    public void DeformSeeker()
    {
        MeshDeformer.DeformMesh(Path, SeekerMesh.mesh, Position, Rotation, ogVerts: og_verts_seeker, offset: new Vector3(0, 0.018f, 0), RhythmicGame.TrackWidth + 0.05f, -1, Length, movePivotToStart: false); // TODO: unneccessary parameters
    }
}