using PathCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class AmpTrack : MonoBehaviour
{
    // TODO: performance accessing these might be a bit concerning?
    Clock Clock { get { return Clock.Instance; } }
    Tunnel Tunnel { get { return Tunnel.Instance; } }
    SongController SongController { get { return SongController.Instance; } }
    TracksController TracksController { get { return TracksController.Instance; } }

    /// References to the contents

    public ClipManager ClipManager;

    [Header("Containers")]
    public Transform MeasureContainer;

    [Header("Capture animation content refs")]
    public AmpTrackDestructFX DestructFX;

    [Header("Global Edge Light")]
    public EdgeLights GlobalEdgeLights;

    [Header("Track materials")]
    public Material Track_Bottom_Mat;
    public Material Track_Bottom_Global_Mat;

    /// Declarations, global variables, properties, events ...

    public VertexPath Path;

    /// Global variables and properties
    [Header("Properties & variables")]
    public int ID = -1; // Song track ID (in case of TUNNEL DUPLICATION, it's still the original song track ID!)
    public int RealID = -1; // The individual ID of each track. (in case of TUNNEL DUPLICATION, it's its own particular ID!)
    public int SetID = 0; // TUNNEL DUPLICATION: which track set does this track belong to?
    public string TrackName = ""; // Instrument name of the track. Object name *should* be the same.

    public bool IsCloneTrack; // Whether this track is cloned in Tunnel mode.
    public int TrackSetID; // The ID of the track set this track belongs to.
    public AmpTrack[] TrackTwins;

    public Vector3[] TunnelTransform;
    public Vector3 TunnelPos;
    public Vector3 TunnelRot;

    // TODO: Emission!
    Color _color = Colors.Bass;
    public Color Color
    {
        get { return _color; }
        set
        {
            _color = value;
            Track_Bottom_Mat.SetColor("_Color", value);
            Track_Bottom_Mat.SetColor("_Emission", value * TracksController.LocalEmission);
            Track_Bottom_Global_Mat.SetColor("_Color", value);
            Track_Bottom_Global_Mat.SetColor("_Emission", value * TracksController.GlobalEmission);
        }
    }

    // to be set when sequence!
    //Color = Colors.ConvertColor(Colors.ColorFromInstrument(value));

    InstrumentType _instrument;
    public InstrumentType Instrument
    {
        get { return _instrument; }
        set { _instrument = value; Color = Colors.ColorFromInstrument(value); }
    }

    public float zRot; // The Z rotation (looks like X-axis rotation from front) of this particular track.

    void Awake()
    {
        // Materials setup:
        // Instance the measure materials so they are shared for this particular track only.
        // This has to be done in Awake(), as the Color property (which changes a TrackMaterial_Active) is being set sooner than Start().
        Track_Bottom_Mat = Instantiate(Track_Bottom_Mat);
        Track_Bottom_Global_Mat = Instantiate(Track_Bottom_Global_Mat);
        Track_Bottom_Global_Mat.SetInteger("_Enabled", 0); // Global should start off
    }
    void Start()
    {
        TunnelTransform = Tunnel.GetTransformForTrackID(RealID);
        TunnelPos = TunnelTransform[0];
        TunnelRot = TunnelTransform[1];

        // Color
        Color = Colors.ColorFromInstrument(Instrument);
        DestructFX.TrackColor = Color;

        // Set up ClipManager for capture clipping
        ClipManager.AddMaterial(Track_Bottom_Mat);
        if (RhythmicGame.GlobalEdgeLightsCaptureClipping)
            ClipManager.AddMaterial(Track_Bottom_Global_Mat);

        // Clone tracks - set Sequences List references
        if (IsCloneTrack)
            foreach (AmpTrack t in TrackTwins)
                t.Sequences = TracksController.Tracks[ID].Sequences;

        StartCoroutine(WarmupDestructFX());
    }

    IEnumerator WarmupDestructFX()
    {
        DestructFX.Play();
        yield return new WaitForSeconds(3);
        DestructFX.Stop();
        DestructFX.gameObject.SetActive(true);
    }

    public List<AmpTrackSection> Measures = new List<AmpTrackSection>();
    public AmpTrackSection CurrentMeasure { get { return Measures[Clock.Fbar]; } }

    public int[] Sequence_Start_IDs = new int[RhythmicGame.SequenceAmount];
    public List<AmpTrackSection> Sequences = new List<AmpTrackSection>();
    public int AddSequence(AmpTrackSection measure)
    {
        measure.IsSequence = true;
        foreach (AmpTrack t in TrackTwins) t.Sequences.Add(measure);
        Sequences.Add(measure);
        return Sequences.Count;
    }
    public void RemoveSequence(AmpTrackSection measure, bool deactivate = false)
    {
        if (deactivate) measure.IsSequence = false;
        foreach (AmpTrack t in TrackTwins) t.Sequences.Remove(measure);
        Sequences.Remove(measure);
    }
    public void ClearSequences()
    {
        foreach (AmpTrack t in TrackTwins)
        {
            t.Sequences.ForEach(s => s.IsSequence = false);
            t.Sequences.Clear();
        }
        Sequences.ForEach(s => s.IsSequence = false);
        Sequences.Clear();
    }

    /// Compatibility backport from OG Track
    // TODO: change functionality in TUT?
    bool _TUT_IsTrackEnabled;
    public bool TUT_IsTrackEnabled { get; set; /* TBA */ }

    bool _isEnabled;
    // Disable all measures within track, grey out everything
    // TODO: May want to have this be what TUT_IsTrackEnabled is?
    public bool IsEnabled
    {
        get { return _isEnabled; }
        set { IsEnabled = true; /* additional logic here... */ }
    }

    public bool IsTrackBeingPlayed;
    public void SetIsTrackBeingPlayed(bool value)
    {
        foreach (AmpTrack t in TrackTwins) t.IsTrackBeingPlayed = value;
        IsTrackBeingPlayed = value;
    }

    bool _isTrackFocused;
    public bool IsTrackFocused
    {
        get { return _isTrackFocused; }
        set
        {
            _isTrackFocused = value;
            foreach (AmpTrackSection m in Measures)
                if (m) m.IsFocused = value;

            UpdateSequenceStates();
        }
    }
    public void SetIsTrackFocused(int realID)
    {
        IsTrackFocused = (RealID == realID);
        foreach (AmpTrack t in TrackTwins) t.IsTrackFocused = (t.RealID == realID);
    }

    int _capturedOnBar;
    bool _isTrackCaptured;
    public bool IsTrackCaptured
    {
        get
        {
            if (_isTrackCaptured) return Clock.Fbar < _capturedOnBar + RhythmicGame.TrackCaptureLength; // Is current bar less than the track capture bar + capture length?
            else return false;
        }
        set
        {
            _isTrackCaptured = value;
            _capturedOnBar = Clock.Fbar;
        }
    }

    bool _isTrackCapturing;
    public bool IsTrackCapturing { get; set; /* TBA */ }

    public bool IsTrackEmpty { get; /* TBA */ }
    public bool HideEmptyMeasures = true; // Whether empty measures should leave a hole

    /// Events
    // Track capturing
    public event EventHandler<int[]> OnTrackCaptureStart;
    public event EventHandler<int[]> OnTrackCaptured;
    // Measure capturing
    public event EventHandler<int[]> MeasureCaptureStarted; // start - end
    public event EventHandler<int[]> MeasureCaptureFinished; // start - end

    /// Functionality

    float smoothStep;

    [NonSerialized]
    public float captureAnimStep = 0.85f;
    private void Update()
    {
        if (IsTrackCapturing)
        {
            if (RhythmicGame.DebugTrackCapturingEase) Debug.Log("CAPTURE: step: " + captureAnimStep);
            captureAnimStep = Mathf.SmoothDamp(captureAnimStep, 3f, ref smoothStep, 1f, 10f, 1f * Time.deltaTime);
        }
    }

    // Sequences
    public void UpdateSequenceStates()
    {
        // Set the color for sequence measures
        //foreach (AmpTrackSection m in Measures)
        //    if (m) m.IsSequence = false;

        int range = Clock.Fbar + RhythmicGame.HorizonMeasures;
        if (range > SongController.songLengthInMeasures) return;

        for (int i = Clock.Fbar; i < Clock.Fbar + RhythmicGame.HorizonMeasures; i++)
        {
            if (i >= Measures.Count) continue;
            AmpTrackSection m = Measures[i];
            if (m) m.IsSequence = false;
        }

        for (int i = 0; i < Sequences.Count; i++)
        {
            AmpTrackSection m = Sequences[i];
            if (m) m.IsSequence = true;

            foreach (AmpNote n in m.Notes)
                n.IsLastNote = false;
            if (i == Sequences.Count - 1)
                m.Notes.Last().IsLastNote = true;
        }
    }

    // Measure capturing

    public IEnumerator CaptureMeasure(int id) { Debug.LogFormat("AmpTrack: Capturing measure: requested: {0} measure: {1} - Clock(bar): {2}", id, Measures[id].ID, Clock.Instance.bar); yield return CaptureMeasure(Measures[id]); }
    public IEnumerator CaptureMeasure(AmpTrackSection measure)
    {
        AmpTrackSectionDestruct destruct = measure.gameObject.AddComponent<AmpTrackSectionDestruct>();
        destruct.Init(measure);

        while (measure.IsCapturing && measure.gameObject != null) // TODO: When capturing all tracks (thus from 0), it can get stuck waiting on already destroyed measures.
            yield return null;

        // TODO: Edge lights should be visible somehow after destruction!
        // TODO: Remove destruct script after finished destructing?
        //Destroy(destruct);
    }

    public void CaptureMeasureRange(int start, int end) => TracksController.CaptureMeasureRange(start, end, ID);
    public void CaptureMeasureAmount(int start, int amount) => TracksController.CaptureMeasureAmount(start, amount, ID);

    /// Common
    // Lanes

    /// <summary>
    /// Returns the X position for the specified lane inside the local GameObject
    /// </summary>
    /// <param name="laneSide">The lane</param>
    /// TODO: multiple lanes are an idea (???)
    public static float GetLocalXPosFromLaneType(LaneSide laneSide)
    {
        switch (laneSide)
        {
            default:
                return 0f;

            case LaneSide.Left:
                //return -0.7466667f;
                return -(RhythmicGame.TrackWidth / 3);
            case LaneSide.Center:
                return 0f;
            case LaneSide.Right:
                return RhythmicGame.TrackWidth / 3;
                //return 0.7466667f;
        }
    }

    // Track types
    public enum InstrumentType
    { Drums = 0, DMS = 0, Bass = 1, Synth = 2, Guitar = 3, gtr = 3, Vocals = 4, vox = 4, FREESTYLE = 5, bg_click = 6 }

    public static InstrumentType InstrumentFromString(string s)
    {
        foreach (string type in Enum.GetNames(typeof(InstrumentType)))
            if (s.ToLower().Contains(type.ToString().ToLower())) // lowercase everything to ignore case
                return (InstrumentType)Enum.Parse((typeof(InstrumentType)), type, true);

        Debug.LogError("TRACK/InstrumentFromString(): Invalid track string! " + s);
        return InstrumentType.Synth;
    }

    // Colors
    public static class Colors
    {
        static float Opacity = 255f;

        public static Color Invalid = new Color(255, 255, 255, Opacity);
        public static Color Empty = new Color(118, 118, 118, Opacity);

        public static Color Drums = new Color(255, 61, 246, Opacity);
        public static Color Bass = new Color(20, 99, 252, Opacity);
        public static Color Synth = new Color(221, 255, 0, Opacity);
        public static Color Guitar = new Color(213, 21, 11, Opacity);
        public static Color Vocals = new Color(32, 202, 45, Opacity);
        public static Color Freestyle = new Color(110, 110, 110, Opacity);

        public static Material[] materialCache = new Material[6];
        public static Material GetMaterialForInstrument(InstrumentType inst)
        {
            // Try getting material from cache
            Material mat = materialCache[(int)inst];
            if (mat) return mat;
            else
            {
                // If not cached, cache material for later use
                mat = (Material)Resources.Load("Materials/Tracks/TrackMaterial");
                materialCache[(int)inst] = mat;

                return mat;
            }

        }

        public static Color ColorFromInstrument(InstrumentType type) { return ConvertColor(_ColorFromInstrument(type)); }
        public static Color _ColorFromInstrument(InstrumentType type)
        {
            switch ((int)type)
            {
                default:
                    return Invalid;

                case (int)InstrumentType.Drums:
                    return Drums;
                case (int)InstrumentType.Bass:
                    return Bass;
                case (int)InstrumentType.Synth:
                    return Synth;
                case (int)InstrumentType.Guitar:
                    return Guitar;
                case (int)InstrumentType.Vocals:
                    return Vocals;
                case (int)InstrumentType.FREESTYLE:
                    return Freestyle;
            }
        }
        public static Color ConvertColor(Color color)
        {
            return new Color(color.r / 255, color.g / 255, color.b / 255, color.a / 255);
        }
    }
}

public enum LaneSide { Left = 0, Center = 1, Right = 2, UNKNOWN = 3 }