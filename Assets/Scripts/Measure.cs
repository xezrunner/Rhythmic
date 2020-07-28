using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Measure : MonoBehaviour
{
    AmplitudeSongController amp_ctrl { get { return AmplitudeSongController.Instance; } }

    public int measureNum; // measure ID
    public List<Note> noteList = new List<Note>(); // notes in this measure
    public List<MeasureSubBeat> subbeatList = new List<MeasureSubBeat>();
    public List<GameObject> subbeatObjectList = new List<GameObject>(); // TODO: this is bad
    public Track.TrackType? trackInstrument;

    public float startTimeInZPos;
    public float endTimeInZPos;

    public GameObject Visuals;
    public GameObject MeasurePlane;

    public EdgeLightsController EdgeLights;

    Color _edgeLightsColor;
    public Color EdgeLightsColor
    {
        get { return _edgeLightsColor; }
        set
        {
            _edgeLightsColor = value;
            EdgeLights.Color = value;
        }
    }

    float _edgeLightsGlowIntensity;
    public float EdgeLightsGlowIntensity
    {
        get { return EdgeLights.GlowIntensity; }
        set
        {
            EdgeLights.GlowIntensity = value;
        }
    }

    bool _edgeLightsGlow;
    public bool EdgeLightsGlow
    {
        get { return _edgeLightsGlow; }
        set
        {
            _edgeLightsGlow = value;
            EdgeLights.EnableGlow = value;
        }
    }

    /// <summary>
    /// If a measure doesn't contain any notes, this returns true.
    /// </summary>
    public bool IsMeasureEmpty { get { return noteList.Count == 0 ? true : false; } }

    /// <summary>
    /// If this is set to false, the measure GameObject will disable itself. This creates a hole in the tracks.
    /// </summary>
    bool _isMeasureActive = true;
    public bool IsMeasureActive
    {
        get { return _isMeasureActive; }
        set
        {
            _isMeasureActive = value;
            Visuals.SetActive(value);
        }
    }

    bool _isMeasureEnabled = true;
    public bool IsMeasureEnabled
    {
        get { return _isMeasureEnabled; }
        set
        {
            _isMeasureEnabled = value;

            if (!value)
                MeasurePlane.GetComponent<MeshRenderer>().material.color = Color.black;
            SetMeasureNotesActive(value);
        }
    }

    bool _isMeasureCapturable = true;
    public bool IsMeasureCapturable // Is this measure capable of being captured? TODO: revisit this. Perhaps some corrupt measures? Lose streak when not capturable.
    {
        get { return _isMeasureCapturable; }
        set
        {
            _isMeasureCapturable = value;
            if (value)
                SetMeasureNotesActive(true);
            else
                SetMeasureNotesActive(false);
        }
    }

    public void SetMeasureNotesActive(bool state)
    {
        foreach (Note note in noteList)
            note.IsNoteActive = state;
    }
    public void SetMeasureNotesToBeCaptured(bool state = true)
    {
        if (!IsMeasureCapturable & !IsMeasureEmpty & IsMeasureEnabled)
            return;

        foreach (Note note in noteList)
            note.IsNoteToBeCaptured = state;
    }

    public bool IsMeasureScorable = true; // Should this measure score points?
    public bool IsMeasureStreakable = true; // Should this measure count towards increasing the streak counter?

    void Awake()
    {
        subBeatPrefab = Resources.Load("Prefabs/MeasureSubBeat");

        EdgeLights = transform.GetChild(0).GetChild(1).gameObject.GetComponent<EdgeLightsController>();
    }

    UnityEngine.Object subBeatPrefab;
    private void Start()
    {
        // create subbeats
        if (subBeatPrefab == null)
            subBeatPrefab = Resources.Load("Prefabs/MeasureSubBeat");

        Vector3 subbeatScale = new Vector3(1, 1, amp_ctrl.subbeatLengthInzPos);
        float lastSubbeatPosition = startTimeInZPos;

        for (int i = 0; i < 8; i++)
        {
            GameObject obj = (GameObject)GameObject.Instantiate(subBeatPrefab);
            MeasureSubBeat script = obj.GetComponent<MeasureSubBeat>();

            obj.name = string.Format("MEASURE{0}_SUBBEAT{1}", measureNum, i);
            obj.transform.localScale = subbeatScale;
            obj.transform.localPosition = new Vector3(transform.position.x, 0, lastSubbeatPosition);
            obj.transform.parent = transform;

            script.subbeatNum = i;

            EdgeLightsGlowIntensity = 0.5f;
            EdgeLightsColor = Track.Colors.ColorFromTrackType(trackInstrument.Value);

            if (i == 0) // measure trigger
                script.IsMeasureSubbeat = true;

            script.StartZPos = lastSubbeatPosition;

            subbeatObjectList.Add(obj);
            subbeatList.Add(script);

            lastSubbeatPosition += amp_ctrl.subbeatLengthInzPos;
            script.EndZPos = lastSubbeatPosition;
        }
    }

    public void CreateSubbeats()
    {

    }

    public void CaptureMeasure()
    {

    }

    // TODO: optimize!
    public MeasureSubBeat GetSubbeatForZpos(float zPos)
    {
        int counter = 0;
        foreach (MeasureSubBeat subbeat in subbeatList)
        {
            if (zPos < subbeat.EndZPos)
                return subbeat;
            /*
            else if (zPos == subbeat.EndZPos)
                return subbeatList[counter + 1];
            */
            counter++;
        }
        throw new Exception("Cannot find subbeat for this zPos: " + zPos);
    }
}
