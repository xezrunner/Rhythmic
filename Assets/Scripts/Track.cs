using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using PathCreation;

public class Track : MonoBehaviour
{
    AmplitudeSongController amp_ctrl { get { return (AmplitudeSongController)SongController.Instance; } }
    SongController SongController { get { return SongController.Instance; } }
    TracksController TracksController { get { return TracksController.Instance; } }
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

    // Properties
    public int ID = -1; // ID of track - with track duplication, this stays the same as the original track
    public int RealID = -1; // the actual ID of this track, even if duplicated
    public string trackName;
    public int SetID = 0;

    InstrumentType _Instrument;
    public InstrumentType Instrument
    {
        get { return _Instrument; }
        set { _Instrument = value; EdgeLightsColor = Colors.ColorFromTrackType(value); }
    }

    public Note nearestNote;
    public int activeMeasureNum = -1;
    public float zRot { get { return transform.eulerAngles.z; } }

    bool _TUT_IsTrackEnabled = true;
    public bool TUT_IsTrackEnabled // Tutorial track disabling
    {
        get { return _TUT_IsTrackEnabled; }
        set { identicalTracks.ForEach(t => t._TUT_IsTrackEnabled = value); }
    }

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

    bool _isTrackCaptured = false;
    public bool IsTrackCaptured // Is this track in its captured state right now?
    {
        get { return _isTrackCaptured; }
        set { identicalTracks.ForEach(t => t._isTrackCaptured = value); }
    }

    public bool _isTrackBeingCaptured { get; set; } = false;
    public bool IsTrackBeingCaptured // Is this track being played right now? TODO: this being a prop and if changed, will update its own volume in SongController
    {
        get { return _isTrackBeingCaptured; }
        set
        {
            if (value != _isTrackBeingCaptured & value)
                CatcherController.Instance.FindNextMeasuresNotes(this, true);

            if (value)
                identicalTracks.ForEach(t => t.IsTrackCaptured = false);

            _isTrackBeingCaptured = value;
        }
    }

    public bool IsTrackEmpty { get { return trackNotes.Count == 0 ? true : false; } }

    public bool IsTrackConstant = false; // Should this track remain active even after capturing?
    public bool IsTrackConstantCaptureNotes = false; // Should the notes be captured when you capture a constant track?
    public bool DisableEmptyMeasures = true; // Whether empty measures should leave a hole instead

    public List<Track> identicalTracks;
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

    GameObject measurePrefab;
    GameObject notePrefab;
    GameObject trackSectionPrefab;

    VertexPath path;

    private void Awake()
    {
        path = GameObject.Find("Path").GetComponent<PathCreator>().path;

        measurePrefab = TracksController.measurePrefab;
        notePrefab = TracksController.notePrefab;
        trackSectionPrefab = (GameObject)Resources.Load("Prefabs/AmpTrackSection");
    }
    void Start()
    {
        EdgeLightsActive = false;

        if (Instrument == InstrumentType.FREESTYLE)
            DisableEmptyMeasures = false;

        PopulateNotes();
        PopulateMeasures();
    }

    public void TUT_SetTrackEnabledState(bool state = false)
    {
        transform.GetChild(0).gameObject.SetActive(state);
        trackNotes.ForEach(n => n.gameObject.SetActive(state));

        TUT_IsTrackEnabled = state;
    }

    // Track population
    /// <summary>
    /// Populate lanes with Note (CATCH) objects from the list of MIDI events
    /// </summary>
    public virtual void PopulateNotes()
    {
        // TODO: Rhythmic note population!
    }

    // This creates a note (CATCH) GameObject and script.
    public void CreateNote(string noteName, float zPos, Note.NoteType noteType, LaneType laneType)
    {
        var lane = GetLaneObjectForLaneType(laneType).transform;
        Vector3 position = new Vector3(lane.position.x, lane.position.y + 0.01f, zPos);

        // create GameObject
        GameObject obj = Instantiate(notePrefab);

        //obj.transform.position = position;
        //obj.transform.eulerAngles = transform.eulerAngles; // angled on the track

        //obj.transform.SetParent(lane, false); // parent to the lane

        obj.transform.position = path.GetPointAtDistance(zPos);
        obj.transform.rotation = path.GetRotationAtDistance(zPos) * Quaternion.Euler(0, 0, 90);
        obj.transform.transform.Translate(Vector3.right * lane.position.x);

        //obj.transform.parent = lane;

        // set up
        var note = obj.AddComponent<Note>();
        note.noteName = noteName;
        note.noteType = noteType;
        note.noteLane = laneType;
        note.noteTrack = this;
        note.measureNum = SongController.GetMeasureNumForZPos(zPos);
        note.subbeatNum = SongController.GetSubbeatNumForZPos(note.measureNum, zPos);
        note.DotLightColor = Colors.ColorFromTrackType(Instrument);

        trackNotes.Add(note);
    }

    // Measures
    public async void PopulateMeasures1()
    {
        // TODO: DO NOT USE MEASUREINFO!!!
        if (RhythmicGame.GameType == RhythmicGame._GameType.RHYTHMIC)
        { Debug.LogErrorFormat("TRACK: We are still using MeasureInfo for populating measures - this only works in Amplitude gamemode!"); return; }

        int counter = 0;
        foreach (AmplitudeSongController.MeasureInfo MeasureInfo in amp_ctrl.songMeasures)
        {
            // create GameObject for measure
            Vector3 measurePosition = new Vector3(MeasureContainer.transform.position.x, MeasureContainer.transform.position.y, MeasureInfo.startTimeInzPos);

            GameObject obj = (GameObject)GameObject.Instantiate(measurePrefab);

            obj.name = string.Format("MEASURE_{0}", MeasureInfo.measureNum);

            obj.transform.localPosition = measurePosition;
            obj.transform.localEulerAngles = gameObject.transform.eulerAngles;
            obj.transform.localScale = new Vector3(1, 1, SongController.measureLengthInzPos);

            obj.transform.SetParent(MeasureContainer.transform, true);

            // get Measure script and add component
            Measure measure = obj.GetComponent<Measure>();

            measure.measureNum = counter;
            measure.measureTrack = this;
            measure.trackInstrument = Instrument;
            measure.startTime = MeasureInfo.startTimeInzPos;
            measure.endTime = MeasureInfo.endTimeInzPos;
            measure.FullLength = SongController.subbeatLengthInzPos * 8;
            measure.MeasureColor = Colors.ConvertColor(Colors.ColorFromTrackType(Instrument));
            measure.OnCaptureFinished += Measure_OnCaptureFinished;

            foreach (Note note in trackNotes) // add notes to measure note list
                if (note.measureNum == counter)
                    measure.noteList.Add(note);

            // deactivate measure if doesn't contain notes
            if (Instrument != InstrumentType.FREESTYLE & measure.noteList.Count == 0 & DisableEmptyMeasures)
                measure.IsMeasureEmpty = true;

            if (!measure.IsMeasureEmpty)
                trackActiveMeasures.Add(measure);

            trackMeasures.Add(measure);
            counter++;

            //if (!RhythmicGame.IsTunnelMode) // tunnel mode can't do async as rotation needs to happen right away
            await Task.Delay(1); // fake async
        }
    }
    public async void PopulateMeasures()
    {
        // TODO: DO NOT USE MEASUREINFO!!!
        if (RhythmicGame.GameType == RhythmicGame._GameType.RHYTHMIC)
        { Debug.LogErrorFormat("TRACK: We are still using MeasureInfo for populating measures - this only works in Amplitude gamemode!"); return; }

        int counter = 0;
        foreach (AmplitudeSongController.MeasureInfo MeasureInfo in amp_ctrl.songMeasures)
        {
            if (counter > 15) break;
            // create GameObject for measure
            Vector3 measurePosition = new Vector3(MeasureContainer.transform.position.x, MeasureContainer.transform.position.y, MeasureInfo.startTimeInzPos);

            GameObject obj = Instantiate(trackSectionPrefab);
            AmpTrackSection s = obj.GetComponent<AmpTrackSection>();
            s.PositionOnPath = measurePosition;
            s.RotationOnPath = gameObject.transform.eulerAngles.z;
            s.Length = SongController.measureLengthInzPos;

            //GameObject obj = (GameObject)GameObject.Instantiate(measurePrefab);

            obj.name = string.Format("MEASURE_{0}", MeasureInfo.measureNum);

            /*
            obj.transform.localPosition = measurePosition;
            obj.transform.localEulerAngles = gameObject.transform.eulerAngles;
            obj.transform.localScale = new Vector3(1, 1, SongController.measureLengthInzPos);
            */

            obj.transform.SetParent(MeasureContainer.transform, true);

            /*
            // get Measure script and add component
            Measure measure = obj.GetComponent<Measure>();

            measure.measureNum = counter;
            measure.measureTrack = this;
            measure.trackInstrument = Instrument;
            measure.startTime = MeasureInfo.startTimeInzPos;
            measure.endTime = MeasureInfo.endTimeInzPos;
            measure.FullLength = SongController.subbeatLengthInzPos * 8;
            measure.MeasureColor = Colors.ConvertColor(Colors.ColorFromTrackType(Instrument));
            measure.OnCaptureFinished += Measure_OnCaptureFinished;

            foreach (Note note in trackNotes) // add notes to measure note list
                if (note.measureNum == counter)
                    measure.noteList.Add(note);

            // deactivate measure if doesn't contain notes
            if (Instrument != InstrumentType.FREESTYLE & measure.noteList.Count == 0 & DisableEmptyMeasures)
                measure.IsMeasureEmpty = true;

            if (!measure.IsMeasureEmpty)
                trackActiveMeasures.Add(measure);

            trackMeasures.Add(measure);
            */
            counter++;

            //if (!RhythmicGame.IsTunnelMode) // tunnel mode can't do async as rotation needs to happen right away
            await Task.Delay(1); // fake async
        }
    }

    /* 
                GameObject go = Instantiate(trackSectionPrefab);
                AmpTrackSection s = go.GetComponent<AmpTrackSection>();
                s.PositionOnPath = script.TestTrackSectionPos;
                s.RotationOnPath = script.TestTrackSectionRot;
     */

    public void AddSequenceMeasure(Measure m)
    {
        identicalTracks.ForEach(t =>
        {
            Measure measure = t.trackMeasures[m.measureNum];

            if (t.sequenceMeasures.Count > 2)
                t.sequenceMeasures.Clear();
            if (measure.IsMeasureEmptyOrCaptured)
                return;

            measure.SetMeasureNotesToBeCaptured();

            t.sequenceMeasures.Add(measure);
            measure.IsMeasureQueued = true;
        });
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
            return;

        // if all sequence measures have been cleared, capture the track
        CaptureTrack();
    }

    public void CaptureTrack()
    {
        IsTrackBeingCaptured = false;
        IsTrackCaptured = true;
        Player.Instance.Multiplier++;

        identicalTracks.ForEach(t =>
        {
            t.sequenceMeasures.Clear();

            // TODO: These have to be set here as the FindNextMeasureNotes() needs to know this immediately.
            for (int i = CatcherController.Instance.CurrentMeasureID; i <= CatcherController.Instance.CurrentMeasureID + RhythmicGame.TrackCaptureLength; i++)
                t.trackMeasures[i].IsMeasureCaptured = true;
        });

        CatcherController.Instance.FindNextMeasuresNotes();
        if (SongController.songName != "tut0" || GameObject.Find("TUT_SCRIPT") == null)
            CaptureMeasuresRange(CatcherController.Instance.CurrentMeasureID, RhythmicGame.TrackCaptureLength);
        else
            CaptureMeasures(CatcherController.Instance.CurrentMeasureID, trackMeasures.Count - 1 - CatcherController.Instance.CurrentMeasureID);
    }

    public event EventHandler<int[]> OnTrackCaptureStart;
    public event EventHandler<int[]> OnTrackCaptured;

    public void CaptureMeasures(int start, int end)
    {
        OnTrackCaptureStart?.Invoke(this, new int[] { ID, start, end });
        identicalTracks.ForEach(t =>
        {
            for (int i = start; i <= end; i++)
                t.trackMeasures[i].IsMeasureCaptured = true;

            t.StartCoroutine(_CaptureMeasures(start, end, t));
        });

        OnTrackCaptured?.Invoke(this, new int[] { ID, start, end });
    }
    public void CaptureMeasuresRange(int start, int count)
    {
        OnTrackCaptureStart?.Invoke(this, new int[] { ID, start, start + count });
        identicalTracks.ForEach(t =>
        {
            for (int i = start; i <= start + count; i++)
                t.trackMeasures[i].IsMeasureCaptured = true;

            t.StartCoroutine(_CaptureMeasuresRange(start, count, t));
        });

        OnTrackCaptured?.Invoke(this, new int[] { ID, start, start + count });
    }

    static IEnumerator _CaptureMeasures(int start, int end, Track t)
    {
        for (int i = start; i <= end; i++)
            yield return t.trackMeasures[i].CaptureMeasure();
    }
    static IEnumerator _CaptureMeasuresRange(int start, int count, Track t)
    {
        for (int i = start; i <= start + count; i++)
            yield return t.trackMeasures[i].CaptureMeasure();
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
        return trackMeasures[SongController.GetMeasureNumForZPos(zPos)];
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
    public static InstrumentType InstrumentFromString(string s)
    {
        foreach (string type in Enum.GetNames(typeof(InstrumentType)))
            if (s.ToLower().Contains(type.ToString().ToLower())) // lowercase everything to ignore case
                return (InstrumentType)Enum.Parse((typeof(InstrumentType)), type, true);

        Debug.LogError("TRACK/InstrumentFromString(): Invalid track string! " + s);
        return InstrumentType.Synth;
    }

    public enum InstrumentType
    { Drums = 0, DMS = 0, Bass = 1, Synth = 2, Guitar = 3, gtr = 3, Vocals = 4, vox = 4, FREESTYLE = 5, bg_click = 6 }

    // Colors
    public static class Colors
    {
        static float Opacity = 180f;

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
}
