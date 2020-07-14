using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NAudio.Midi;
using System;
using System.Linq.Expressions;
using Assets.Scripts.Amplitude;
using UnityEditor;

public class Track : MonoBehaviour
{
    AmplitudeSongController amp_ctrl { get { return GameObject.Find("AMPController").GetComponent<AmplitudeSongController>(); } }
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

    //public bool IsTrackActive = true; // Is the track enabled?
    bool _isTrackFocused = false;
    public bool IsTrackFocused
    {
        get { return _isTrackFocused; }
        set { _isTrackFocused = value; EdgeLightsActive = value; }
    } // Is the track focused by the player?
    public bool IsTrackCaptured; // Is this track in its captured state right now?
    public bool IsTrackBeingCaptured; // Is this track being played right now?
    public bool IsTrackConstant = false; // Should this track remain active even after capturing?
    public bool IsTrackConstantCaptureNotes = false; // Should the notes be captured when you capture a constant track?

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
    public List<TrackMeasure> trackMeasures = new List<TrackMeasure>();

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

    /// <summary>
    /// Populate lanes with Note (CATCH) objects from the list of MIDI events
    /// </summary>
    public void PopulateLanes()
    {
        // TODO: Rhythmic lane population!

        //if (RhythmicGame.DebugNoteCreationEvents)
        //    Debug.LogFormat(string.Format("TRACK [{0}]: Created new note: {1}", TrackName, noteName));
    }

    UnityEngine.Object subBeatPrefab;
    public void CreateMeasures(List<AmplitudeSongController.MeasureInfo> MeasureInfoList)
    {
        if (subBeatPrefab == null)
            subBeatPrefab = Resources.Load("Prefabs/MeasureSubBeat");

        int counter = 0;
        foreach (AmplitudeSongController.MeasureInfo MeasureInfo in MeasureInfoList)
        {
            Vector3 measurePosition = new Vector3(MeasureContainer.transform.position.x, 0, MeasureInfo.startTimeInzPos);
            Vector3 subbeatScale = new Vector3(1, 1, amp_ctrl.subbeatLengthInzPos);

            // create GameObject for measure
            GameObject measureObj = new GameObject() { name = string.Format("MEASURE_{0}", MeasureInfo.measureNum) };
            measureObj.transform.localPosition = measurePosition;
            measureObj.transform.parent = MeasureContainer.transform;

            // create Measure script and add component
            TrackMeasure measure = measureObj.AddComponent<TrackMeasure>();
            measure.measureNum = counter;
            foreach (Note note in trackNotes) // add notes to measure note list
            {
                if (note.measureNum == counter)
                    measure.noteList.Add(note);
            }

            // deactivate measure if doesn't contain notes
            if (measure.IsMeasureEmpty)
                measure.IsMeasureActive = false;

            trackMeasures.Add(measure);

            // create subbeats
            float lastSubbeatPosition = MeasureInfo.startTimeInzPos;
            for (int i = 0; i < 8; i++)
            {
                GameObject obj = (GameObject)GameObject.Instantiate(subBeatPrefab);
                MeasureSubBeat script = obj.AddComponent<MeasureSubBeat>();

                obj.name = string.Format("MEASURE{0}_SUBBEAT{1}", counter, i);
                obj.transform.localScale = subbeatScale;
                obj.transform.localPosition = new Vector3(MeasureContainer.transform.position.x, 0, lastSubbeatPosition);
                obj.transform.parent = measureObj.transform;
                obj.transform.Find("MeasurePlane").GetComponent<MeshRenderer>().material = GetTrackMaterial(Instrument.Value);
                lastSubbeatPosition += amp_ctrl.subbeatLengthInzPos;

                script.EdgeLightsGlowIntensity = 0.5f;
                script.EdgeLightsColor = Colors.ColorFromTrackType(Instrument.Value);
            }

            counter++;
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
