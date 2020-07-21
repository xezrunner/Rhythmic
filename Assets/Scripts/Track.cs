using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NAudio.Midi;
using System;
using System.Linq.Expressions;
using UnityEditor;

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
    public EdgeLightsController EdgeLights { get { return gameObject.transform.Find("EdgeLights").GetComponent<EdgeLightsController>(); } }

    public bool EdgeLightsActive { get { return EdgeLights.IsActive; } set { EdgeLights.IsActive = value; } }
    public Color EdgeLightsColor
    {
        get { return EdgeLights.Color; }
        set { EdgeLights.Color = value; }
    }

    // Props
    public int? ID;
    string _trackName;
    public string trackName
    {
        get { return _trackName; }
        set { _trackName = value; }
    }
    public TrackType? Instrument;
    public Note nearestNote;
    public int upcomingActiveMeasure = 0;

    bool _isTrackFocused = false; //  TODO: also enable/disable the track coloring material when it's in the game!
    public bool IsTrackFocused
    {
        get { return _isTrackFocused; }
        set
        {
            _isTrackFocused = value; EdgeLightsActive = value;
        }
    } // Is the track focused by the player?
    public bool IsTrackCaptured; // Is this track in its captured state right now?
    public bool IsTrackBeingCaptured; // Is this track being played right now? TODO: this being a prop and if changed, will update its own volume in SongController
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

    void Start()
    {
        // Set track material
        //SetTrackMaterialByInstrument(Instrument);
    }

    // Materials
    public string TrackMaterialPath = "Materials/Tracks/";
    Material GetTrackMaterial(TrackType type)
    {
        Material finalMaterial;
        string finalPath = TrackMaterialPath + "TrackMaterial";

        try
        {
            finalMaterial = new Material(Shader.Find("Standard"));
            finalMaterial.color = Colors.ConvertColor(Colors.ColorFromTrackType(type));

            if (RhythmicGame.DebugTrackMaterialEvents)
                Debug.LogFormat(string.Format("TRACK [{0}]: Using material {1}", trackName, finalPath));

            return finalMaterial;
        }
        catch
        {
            Debug.LogError(string.Format("TRACK [{0}]: Cannot find material {1}!", trackName, finalPath));
            return null;
        }
    }
    Material GetTrackMaterialFromString(string typestring)
    {
        Material finalMaterial;
        string finalPath = TrackMaterialPath + "TrackMaterial";

        try
        {
            finalMaterial = new Material(Shader.Find("Standard"));
            finalMaterial.color = Colors.ConvertColor(Colors.ColorFromTrackType((TrackTypeFromString(typestring))));

            if (RhythmicGame.DebugTrackMaterialEvents)
                Debug.LogFormat(string.Format("TRACK [{0}]: Using material {1}", trackName, finalPath));

            return finalMaterial;
        }
        catch
        {
            Debug.LogError(string.Format("TRACK [{0}]: Cannot find material {1}!", trackName, finalPath));
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
    UnityEngine.Object subBeatPrefab;
    public void CreateMeasures(List<AmplitudeSongController.MeasureInfo> MeasureInfoList)
    {
        if (subBeatPrefab == null)
            subBeatPrefab = Resources.Load("Prefabs/MeasureSubBeat");

        Vector3 subbeatScale = new Vector3(1, 1, amp_ctrl.subbeatLengthInzPos);

        int counter = 0;
        foreach (AmplitudeSongController.MeasureInfo MeasureInfo in MeasureInfoList)
        {
            // create GameObject for measure
            Vector3 measurePosition = new Vector3(MeasureContainer.transform.position.x, 0, MeasureInfo.startTimeInzPos);

            GameObject measureObj = new GameObject() { name = string.Format("MEASURE_{0}", MeasureInfo.measureNum) };
            measureObj.transform.localPosition = measurePosition;
            measureObj.transform.parent = MeasureContainer.transform;

            // create Measure script and add component
            Measure measure = measureObj.AddComponent<Measure>();
            measure.measureNum = counter;

            measure.startTimeInZPos = MeasureInfo.startTimeInzPos;
            measure.endTimeInZPos = MeasureInfo.endTimeInzPos;

            foreach (Note note in trackNotes) // add notes to measure note list
            {
                if (note.measureNum == counter)
                    measure.noteList.Add(note);
            }

            if (!measure.IsMeasureEmpty)
                trackActiveMeasures.Add(measure);

            // create subbeats
            float lastSubbeatPosition = MeasureInfo.startTimeInzPos;
            for (int i = 0; i < 8; i++)
            {
                GameObject obj = (GameObject)GameObject.Instantiate(subBeatPrefab);
                MeasureSubBeat script = obj.GetComponent<MeasureSubBeat>();

                obj.name = string.Format("MEASURE{0}_SUBBEAT{1}", counter, i);
                obj.transform.localScale = subbeatScale;
                obj.transform.localPosition = new Vector3(MeasureContainer.transform.position.x, 0, lastSubbeatPosition);
                obj.transform.parent = measureObj.transform;
                obj.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = GetTrackMaterial(Instrument.Value); // TODO: code cleanup!

                script.subbeatNum = i;

                script.EdgeLightsGlowIntensity = 0.5f;
                script.EdgeLightsColor = Colors.ColorFromTrackType(Instrument.Value);

                if (i == 0) // measure trigger
                    script.IsMeasureSubbeat = true;

                script.StartZPos = lastSubbeatPosition;

                measure.subbeatObjectList.Add(obj);
                measure.subbeatList.Add(script);

                lastSubbeatPosition += amp_ctrl.subbeatLengthInzPos;
                script.EndZPos = lastSubbeatPosition;
            }

            // deactivate measure if doesn't contain notes
            if (measure.IsMeasureEmpty & DisableEmptyMeasures)
                measure.IsMeasureActive = false;

            trackMeasures.Add(measure);

            counter++;
        }

        foreach (Note note in trackNotes)
        {
            // TODO: temp, move this to a better place / optimize!!! ?
            note.subbeatNum = trackMeasures[note.measureNum].GetSubbeatForZpos(note.zPos).subbeatNum;
        }
    }

    public bool GetIsMeasureActiveForZPos(float zPos)
    {
        try
        {
            return GetMeasureForZPos(zPos).IsMeasureActive;
        }
        catch
        {
            return false;
        }

    }
    public Measure GetMeasureForZPos(float zPos)
    {
        return trackMeasures[amp_ctrl.GetMeasureNumForzPos(zPos)];
    }
    public Measure GetMeasureForID(int id)
    {
        return trackMeasures[id];
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
                return (TrackType)Enum.Parse((typeof(TrackType)), type);
        }
        throw new Exception("MoggSong: Invalid track string! " + s);
    }

    public enum TrackType { Drums = 0, Bass = 1, Synth = 2, Guitar = 3, gtr = 3, Vocals = 4, vox = 4, FREESTYLE = 5, bg_click = 6 }

    // Colors
    public static class Colors
    {
        public static Color Invalid = new Color(0, 0, 0);

        public static Color Drums = new Color(212, 93, 180);
        public static Color Bass = new Color(87, 159, 221);
        public static Color Synth = new Color(221, 219, 89);
        public static Color Guitar = new Color(255, 0, 0);
        public static Color Vocals = new Color(0, 255, 0);

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
            }
        }
        public static Color ConvertColor(Color color)
        {
            return new Color(color.r / 255, color.g / 255, color.b / 255, color.a / 255);
        }
    }
}
