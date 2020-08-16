using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

public class Track : MonoBehaviour
{
    AmplitudeSongController amp_ctrl { get { return AmplitudeSongController.Instance; } }
    public GameObject LaneContainer
    {
        get
        {
            return gameObject.transform.Find("LaneContainer").gameObject;
        }
    }
    public GameObject MeasureContainer { get { return gameObject.transform.Find("MeasureContainer").gameObject; } }
    public GameObject TriggerContainer { get { return gameObject.transform.Find("TriggerContainer").gameObject; } }
    public EdgeLightsController EdgeLights { get { return gameObject.transform.Find("EdgeLights").GetComponent<EdgeLightsController>(); } }

    bool _edgeLightsActive = false;
    public bool EdgeLightsActive { get { return _edgeLightsActive; } set { _edgeLightsActive = value; EdgeLights.IsActive = value; } }
    public Color EdgeLightsColor
    {
        get { return EdgeLights.Color; }
        set { EdgeLights.Color = value; }
    }

    // Props
    public int? ID; // ID of track - with track duplication, this stays the same as the original track
    public int? RealID; // the actual ID of this track, even if duplicated
    public string trackName;
    public TrackType? Instrument;
    public Note nearestNote;
    public int activeMeasureNum = -1;
    public float zRot;

    public bool TUT_IsTrackEnabled { get; set; } = true; // Tutorial track disabling
    bool _isTrackFocused = false; // TODO: also enable/disable the track coloring material
    public bool IsTrackFocused // Is the track focused by the player?
    {
        get { return _isTrackFocused; }
        set
        {
            _isTrackFocused = value;

            EdgeLights.IsActive = value;

            foreach (Measure measure in sequenceMeasures)
                measure.IsFocused = value;
        }
    }
    public bool IsTrackCaptured; // Is this track in its captured state right now?
    bool _isTrackBeingCaptured = false;
    public bool IsTrackBeingCaptured // Is this track being played right now? TODO: this being a prop and if changed, will update its own volume in SongController
    {
        get { return _isTrackBeingCaptured; }
        set
        {
            if (value != _isTrackBeingCaptured & value)
                CatcherController.Instance.FindNextMeasuresNotes(this, true);

            if (value)
                IsTrackCaptured = false;

            _isTrackBeingCaptured = value;
        }
    }
    public bool IsTrackEmpty { get { return trackNotes.Count == 0 ? true : false; } }

    public bool IsTrackConstant = false; // Should this track remain active even after capturing?
    public bool IsTrackConstantCaptureNotes = false; // Should the notes be captured when you capture a constant track?
    public bool DisableEmptyMeasures = true; // Whether empty measures should leave a hole instead

    public List<Note> trackNotes = new List<Note>();
    public List<GameObject> trackLanes
    {
        get
        {
            List<GameObject> finalList = new List<GameObject>();
            foreach (Transform child in LaneContainer.transform)
                finalList.Add(child.gameObject);
            return finalList;
        }
    }
    public List<Measure> trackMeasures = new List<Measure>();
    public List<Measure> trackActiveMeasures = new List<Measure>();

    // This list holds the measures that are considered as a sequence (2)
    public List<Measure> sequenceMeasures = new List<Measure>();

    private void Awake()
    {
        measurePrefab = Resources.Load("Prefabs/Measure");
    }

    void Start()
    {
        // Set track material
        //SetTrackMaterialByInstrument(Instrument);

        EdgeLightsActive = false;

        if (Instrument == TrackType.FREESTYLE)
            DisableEmptyMeasures = false;
    }

    public void TUT_SetTrackEnabledState(bool state = false)
    {
        transform.GetChild(0).gameObject.SetActive(state);
        trackNotes.ForEach(n => n.gameObject.SetActive(state));

        TUT_IsTrackEnabled = state;
    }

    // Materials
    public static string TrackMaterialPath = "Materials/Tracks/";
    public static Material GetTrackMaterial(TrackType type)
    {
        Material finalMaterial;
        string finalPath = TrackMaterialPath + "TrackMaterial";

        try
        {
            finalMaterial = new Material(Shader.Find("Standard"));
            finalMaterial.color = Colors.ConvertColor(Colors.ColorFromTrackType(type));

            return finalMaterial;
        }
        catch
        {
            Debug.LogError(string.Format("TRACK: Cannot find material {0}!", finalPath));
            return null;
        }
    }
    public static Material GetTrackMaterialFromString(string typestring)
    {
        Material finalMaterial;
        string finalPath = TrackMaterialPath + "TrackMaterial";

        try
        {
            finalMaterial = new Material(Shader.Find("Standard"));
            finalMaterial.color = Colors.ConvertColor(Colors.ColorFromTrackType((TrackTypeFromString(typestring))));

            return finalMaterial;
        }
        catch
        {
            Debug.LogError(string.Format("TRACK: Cannot find material {0}!", finalPath));
            return null;
        }
    }

    // Track population

    /// <summary>
    /// Populate lanes with Note (CATCH) objects from the list of MIDI events
    /// </summary>
    public void PopulateNotes()
    {
        // TODO: Rhythmic note population!

        //if (RhythmicGame.DebugNoteCreationEvents)
        //    Debug.LogFormat(string.Format("TRACK [{0}]: Created new note: {1}", TrackName, noteName));
    }

    // Measures
    UnityEngine.Object measurePrefab;
    public async void CreateMeasures(List<AmplitudeSongController.MeasureInfo> MeasureInfoList)
    {
        if (measurePrefab == null)
            measurePrefab = Resources.Load("Prefabs/Measure");

        int counter = 0;
        foreach (AmplitudeSongController.MeasureInfo MeasureInfo in MeasureInfoList)
        {
            // create GameObject for measure
            Vector3 measurePosition = new Vector3(MeasureContainer.transform.position.x, MeasureContainer.transform.position.y, MeasureInfo.startTimeInzPos);

            GameObject obj = (GameObject)GameObject.Instantiate(measurePrefab);

            obj.name = string.Format("MEASURE_{0}", MeasureInfo.measureNum);
            obj.transform.localPosition = measurePosition;
            obj.transform.localEulerAngles = gameObject.transform.eulerAngles;
            obj.transform.localScale = new Vector3(1, 1, amp_ctrl.measureLengthInzPos);
            obj.transform.SetParent(MeasureContainer.transform, true);

            // get Measure script and add component
            Measure measure = obj.GetComponent<Measure>();

            measure.measureNum = counter;
            measure.measureTrack = this;
            measure.trackInstrument = Instrument;
            measure.startTimeInZPos = MeasureInfo.startTimeInzPos;
            measure.endTimeInZPos = MeasureInfo.endTimeInzPos;
            measure.FullLength = amp_ctrl.subbeatLengthInzPos * 8;
            measure.MeasureColor = Colors.ConvertColor(Colors.ColorFromTrackType(Instrument.Value));
            measure.OnCaptureFinished += Measure_OnCaptureFinished;

            foreach (Note note in trackNotes) // add notes to measure note list
            {
                if (note.measureNum == counter)
                    measure.noteList.Add(note);
            }

            // deactivate measure if doesn't contain notes
            if (measure.noteList.Count == 0 & DisableEmptyMeasures)
                measure.IsMeasureEmpty = true;

            if (!measure.IsMeasureEmpty)
                trackActiveMeasures.Add(measure);

            trackMeasures.Add(measure);
            counter++;

            //if (!RhythmicGame.IsTunnelMode) // tunnel mode can't do async as rotation needs to happen right away
            await Task.Delay(6); // fake async
        }
    }

    public void AddSequenceMeasure(Measure measure)
    {
        if (sequenceMeasures.Count == 2)
            sequenceMeasures.Clear();
        if (measure.IsMeasureEmptyOrCaptured)
            return;

        measure.SetMeasureNotesToBeCaptured();

        sequenceMeasures.Add(measure);
        measure.IsMeasureQueued = true;
    }

    // This is called when 1 measure is cleared.
    public void OnMeasureClear(Measure e)
    {
        if (sequenceMeasures.Contains(e))
        {
            e.IsMeasureToBeCaptured = false;
            sequenceMeasures.Remove(e);
        }

        if (sequenceMeasures.Count != 0)
        {

        }
        //CatcherController.Instance.FindNextMeasuresNotes(this, true);
        else// if all sequence measures have been cleared, capture the track
        {
            sequenceMeasures.Clear();

            // TODO: wtf? double
            for (int i = CatcherController.Instance.CurrentMeasureID; i < CatcherController.Instance.CurrentMeasureID + RhythmicGame.TrackCaptureLength; i++)
                trackMeasures[i].IsMeasureCaptured = true;

            IsTrackBeingCaptured = false;
            IsTrackCaptured = true;

            CatcherController.Instance.FindNextMeasuresNotes();
            if (AmplitudeSongController.Instance.songName != "tut0" || GameObject.Find("TUT_SCRIPT") == null)
                CaptureMeasuresRange(CatcherController.Instance.CurrentMeasureID, RhythmicGame.TrackCaptureLength);
            else
                CaptureMeasures(CatcherController.Instance.CurrentMeasureID, trackMeasures.Count - 1 - CatcherController.Instance.CurrentMeasureID);
        }
    }

    public event EventHandler<int[]> OnTrackCaptureStart;
    public event EventHandler<int[]> OnTrackCaptured;

    public void CaptureMeasures(int start, int end)
    {
        OnTrackCaptureStart?.Invoke(this, new int[] { ID.Value, start, end });
        StartCoroutine(_CaptureMeasures(start, end));
    }

    public void CaptureMeasuresRange(int start, int count)
    {
        OnTrackCaptureStart?.Invoke(this, new int[] { ID.Value, start, start + count });
        StartCoroutine(_CaptureMeasuresRange(start, count));
    }

    IEnumerator _CaptureMeasures(int start, int end)
    {
        for (int i = start; i < end; i++)
            trackMeasures[i].IsMeasureCaptured = true;

        for (int i = start; i < end; i++)
            yield return trackMeasures[i].CaptureMeasure();

        OnTrackCaptured?.Invoke(this, new int[] { ID.Value, start, end });
    }

    IEnumerator _CaptureMeasuresRange(int start, int count)
    {
        for (int i = start; i < start + count; i++)
            trackMeasures[i].IsMeasureCaptured = true;

        for (int i = start; i < start + count; i++)
            yield return trackMeasures[i].CaptureMeasure();

        OnTrackCaptured?.Invoke(this, new int[] { ID.Value, start, start + count });
    }

    public event EventHandler<int> MeasureCaptureFinished;
    public int capturecounter = 0;
    private void Measure_OnCaptureFinished(object sender, int e)
    {
        MeasureCaptureFinished?.Invoke(this, e);
    }

    public bool GetIsMeasureEmptyForZPos(float zPos)
    {
        try
        {
            return GetMeasureForZPos(zPos).IsMeasureEmpty;
        }
        catch
        {
            return false;
        }

    }
    public Measure GetMeasureForZPos(float zPos)
    {
        return trackMeasures[amp_ctrl.GetMeasureNumForZPos(zPos)];
    }
    public Measure GetMeasureForID(int id)
    {
        try
        {
            return trackMeasures[id];
        }
        catch
        {
            Debug.Break();
            Debug.DebugBreak();

            return null;
        }
    }

    // Lanes
    public enum LaneType { Left = 0, Center = 1, Right = 2, UNKNOWN = 3 } // lane side

    /// <summary>
    /// Returns the X position for the specified lane inside the local GameObject
    /// </summary>
    /// <param name="laneType">The lane</param>
    public static float GetLocalXPosFromLaneType(LaneType laneType)
    {
        switch (laneType)
        {
            default:
                return 0f;

            case LaneType.Left:
                return -0.7466667f;
            case LaneType.Center:
                return 0f;
            case LaneType.Right:
                return 0.7466667f;
        }
    }
    public GameObject GetLaneObjectForLaneType(LaneType type)
    {
        return trackLanes[(int)type];
    }

    // Track types
    public static TrackType TrackTypeFromString(string s)
    {
        foreach (string type in Enum.GetNames(typeof(TrackType)))
        {
            if (s.Contains(type.ToString().ToLower()))
                return (TrackType)Enum.Parse((typeof(TrackType)), type, true);
        }
        throw new Exception("MoggSong: Invalid track string! " + s);
    }

    public enum TrackType { Drums = 0, Bass = 1, Synth = 2, Guitar = 3, gtr = 3, Vocals = 4, vox = 4, FREESTYLE = 5, bg_click = 6 }

    // Colors
    public static class Colors
    {
        static float Opacity = 180f;

        public static Color Invalid = new Color(0, 0, 0);

        public static Color Drums = new Color(212, 93, 180, Opacity);
        public static Color Bass = new Color(87, 159, 221, Opacity);
        public static Color Synth = new Color(221, 219, 89, Opacity);
        public static Color Guitar = new Color(255, 0, 0, Opacity);
        public static Color Vocals = new Color(0, 255, 0, Opacity);
        public static Color Freestyle = new Color(255, 255, 255, Opacity);

        public static Color ColorFromTrackType(TrackType type)
        {
            switch ((int)type)
            {
                default:
                    return Invalid;

                case (int)TrackType.Drums:
                    return Drums;
                case (int)TrackType.Bass:
                    return Bass;
                case (int)TrackType.Synth:
                    return Synth;
                case (int)TrackType.Guitar:
                    return Guitar;
                case (int)TrackType.Vocals:
                    return Vocals;
                case (int)TrackType.FREESTYLE:
                    return Freestyle;
            }
        }
        public static Color ConvertColor(Color color)
        {
            return new Color(color.r / 255, color.g / 255, color.b / 255, color.a / 255);
        }
    }
}
