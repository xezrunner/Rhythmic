using System;
using System.Collections.Generic;
using UnityEngine;

public class AmpTrack : MonoBehaviour
{
    // TODO: performance accessing these might be a bit concerning?
    SongController SongController { get { return SongController.Instance; } }

    /// Declarations, global variables, properties, events ...
    #region 
    /// References to the contents
    public Transform MeasureContainer;

    /// Path-related stuff
    public PathCreation.PathCreator PathCreator; // TODO: Global property? !!!
    public PathCreation.VertexPath Path
    {
        get
        {
            if (PathCreator) return PathCreator.path;
            else { Debug.LogError("AmpTrack: Path not available - PathCreator is null!"); return null; }
        }
    }

    /// Global variables and properties
    public GameObject TrackSectionPrefab;

    public int ID = -1; // Song track ID (in case of TUNNEL DUPLICATION, it's still the original song track ID!)
    public int RealID = -1; // The individual ID of each track. (in case of TUNNEL DUPLICATION, it's its own particular ID!)
    public string TrackName = ""; // Instrument name of the track. Object name *should* be the same.
    public int SetID = 0; // TUNNEL DUPLICATION: which track set does this track belong to?

    public List<AmpTrackSection> Measures = new List<AmpTrackSection>();

    InstrumentType _instrument;
    public InstrumentType Instrument
    {
        get { return _instrument; }
        set
        {
            _instrument = value;
            /* Set edgelights color and other stuff! */
        }
    }

    public float zRot; // The Z rotation (looks like X-axis rotation from front) of this particular track.

    bool _isEnabled;
    public bool IsEnabled // Disable notes within track, grey out everything
    {
        get { return _isEnabled; }
        set { IsEnabled = true; /* additional logic here... */ }
    }

    // Compatibility backport from OG Track
    // TODO: change functionality in TUT?
    bool _TUT_IsTrackEnabled;
    public bool TUT_IsTrackEnabled { get; set; /* TBA */ }

    bool _isTrackFocused;
    public bool IsTrackFocused { get; set; /* TBA */ }

    bool _isTrackCaptured;
    public bool IsTrackCaptured { get; set; /* TBA */ }

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
    #endregion

    /// Functionality

    /// <summary>
    /// Creates a measure and adds it to the measure list.
    /// </summary>
    public AmpTrackSection CreateMeasure(MetaMeasure meta)
    {
        /// Create object
        var obj = Instantiate(TrackSectionPrefab);
        obj.transform.parent = MeasureContainer;

        /// Get and configure script
        AmpTrackSection measure = obj.GetComponent<AmpTrackSection>();
        measure.ID = meta.ID;
        measure.Instrument = Instrument;
        measure.Length = SongController.measureLengthInzPos;

        // TODO: possibly simplify position &/ rotation properties?
        Vector3 measurePos = new Vector3(
            RhythmicGame.TrackWidth * ID, // X is the track's horizontal position
            0, meta.StartDistance // Z is the distance at which the measure needs to be placed
            );

        measure.PositionOnPath = measurePos;

        /// Add measure to measure list
        Measures.Add(measure);
        return measure;
    }

    // Measure capturing
    /// <summary>
    /// Captures measures from a given start point until an end point
    /// </summary>
    /// <param name="start">Measure ID to start capturing from</param>
    /// <param name="end">Last Measure ID to capture</param>
    public void CaptureMeasureRange(int start, int end)
    {

    }

    /// <summary>
    /// Captures measures from a given start point and onward
    /// </summary>
    /// <param name="start">Measure ID to start capturing from</param>
    /// <param name="amount">Amount of measures to capture from starting point onward</param>
    public void CaptureMeasureAmount(int start, int amount)
    {

    }

    // 

    /// Common
    #region
    // Lanes
    public enum LaneSide { Left = 0, Center = 1, Right = 2, UNKNOWN = 3 }

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

        public static Color Invalid = new Color(0, 0, 0);

        public static Color Drums = new Color(212, 93, 180, Opacity);
        public static Color Bass = new Color(87, 159, 221, Opacity);
        public static Color Synth = new Color(221, 219, 89, Opacity);
        public static Color Guitar = new Color(255, 15, 20, Opacity);
        public static Color Vocals = new Color(0, 255, 0, Opacity);
        public static Color Freestyle = new Color(255, 255, 255, Opacity);

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

        public static Color ColorFromTrackType(InstrumentType type)
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
    #endregion
}