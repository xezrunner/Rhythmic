using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;

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
    public GameObject MeasurePlane_Active;

    public EdgeLightsController EdgeLightsController;
    public List<GameObject> EdgeLightsFrontBack = new List<GameObject>();

    public GameObject MeasureDestructionNoteDetector;

    #region Properties
    private Color _measureColor = Track.Colors.Drums;
    public Color MeasureColor
    {
        get { return _measureColor; }
        set
        {
            _measureColor = value;
            MeasurePlane_Active.GetComponent<MeshRenderer>().material.color = value;
        }
    }

    Color _edgeLightsColor = Track.Colors.Drums;
    public Color EdgeLightsColor
    {
        get { return _edgeLightsColor; }
        set
        {
            _edgeLightsColor = value;
            EdgeLightsController.Color = value;
        }
    }

    float _edgeLightsGlowIntensity;
    public float EdgeLightsGlowIntensity
    {
        get { return EdgeLightsController.GlowIntensity; }
        set
        {
            EdgeLightsController.GlowIntensity = value;
        }
    }

    bool _edgeLightsGlow;
    public bool EdgeLightsGlow
    {
        get { return _edgeLightsGlow; }
        set
        {
            _edgeLightsGlow = value;
            EdgeLightsController.EnableGlow = value;
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
                //MeasurePlane_Active.GetComponent<MeshRenderer>().material.color = Color.black;
                SetMeasureNotesActive(value);
        }
    }

    // TODO: temporarily set to true | measure queueing TBD
    private bool isMeasureQueued = true;
    public bool IsMeasureQueued // Is the measure queued for playing | i.e. is this an upcoming measure, even while being played
    {
        get { return isMeasureQueued; }
        set
        {
            isMeasureQueued = value;
            if (IsMeasureFocused)
                MeasurePlane_Active.SetActive(value);
        }
    }

    private bool isMeasureFocused = false;
    public bool IsMeasureFocused // Is the measure focused
    {
        get { return isMeasureFocused; }
        set
        {
            isMeasureFocused = value;
            if (value)
                MeasurePlane_Active.SetActive(IsMeasureQueued);
            else
                MeasurePlane_Active.SetActive(false);
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

    #endregion

    UnityEngine.Object subBeatPrefab;
    UnityEngine.Object detectorPrefab;
    void Awake()
    {
        subBeatPrefab = Resources.Load("Prefabs/MeasureSubBeat");
        detectorPrefab = Resources.Load("Prefabs/MeasureDestructDetector");

        // get front & back edge lights
        for (int i = 2; i <= 5; i++)
            EdgeLightsFrontBack.Add(EdgeLightsController.gameObject.transform.GetChild(i).gameObject);

        // unparent front & back edge lights
        // TODO: This is very hacky!
        foreach (GameObject obj in EdgeLightsFrontBack)
            obj.transform.parent = null;
    }
    [ExecuteInEditMode]
    void Start()
    {
        if (amp_ctrl.Enabled)
            CreateSubbeats();

        // Move front & back edge lights to their correct positions
        // TODO: This is very hacky!
        PositionFrontBackEdgeLights();

        ogParent = transform.parent; // for destruction and length changes

        EdgeLightsGlow = false;
    }

    // Edge lights

    // This function positions the 6 edge lights on the measure after scaling the measure.
    // This is needed because when scaling a measure, it ends up scaling the edge lights as well.
    // TODO: make the edge lights be a texture on the measure model instead
    void PositionFrontBackEdgeLights()
    {
        foreach (GameObject obj in EdgeLightsFrontBack)
        {
            obj.transform.parent = null;
            Vector3 finalScale = obj.transform.lossyScale;
            Vector3 finalPos = obj.transform.position;
            finalPos.z = obj.transform.name.Contains("Front") ? transform.position.z : (transform.position.z + transform.lossyScale.z);
            finalScale.z = 0.01f;

            obj.transform.position = finalPos;
            obj.transform.localScale = finalScale;

            obj.transform.Translate(transform.right * transform.position.x);

            obj.transform.parent = EdgeLightsController.transform;
            EdgeLightsController.Color = EdgeLightsColor;
        }
    }

    // Subbeat creation

    public void CreateSubbeats()
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

            script.IsMeasureSubbeat = i == 0; // enable measure trigger if first subbeat

            script.StartZPos = lastSubbeatPosition;

            subbeatObjectList.Add(obj);
            subbeatList.Add(script);

            lastSubbeatPosition += amp_ctrl.subbeatLengthInzPos;
            script.EndZPos = lastSubbeatPosition;
        }
    }

    // Capturing

    public float FullLength = 1f;
    [Range(0f, 1f)]
    public float Length = 0f;
    [Range(0f, 1f)]
    public float CaptureLength = 0f;

    // This function scales the measure according to the capture length / inverselength properties
    // It needs to run in Update()
    // TODO: revise
    Transform ogParent;
    void ScaleAndPos()
    {
        transform.parent = null; // unparent from track
        Vector3 ogPosition = transform.position; // store original position

        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, FullLength * (Length - CaptureLength)); // scale measure based on capture length
        transform.position = new Vector3(ogPosition.x, ogPosition.y, startTimeInZPos + (FullLength * CaptureLength)); // with the un-parenting, we need to use the stored position instead

        /*
        if (Application.isPlaying)
            PositionFrontBackEdgeLights();
        */
    }

    public event EventHandler<int> OnCaptureFinished;
    public void CaptureMeasure()
    {
        gameObject.GetComponent<Animation>().Play(); // play capture anim!

        GameObject detector = (GameObject)Instantiate(detectorPrefab, gameObject.transform); // create measure destruct detector

        // setup particles
        detector.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material.color = Colors.ConvertColor(EdgeLightsColor);
        detector.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material.SetColor("_EmissionColor", Colors.ConvertColor(EdgeLightsColor * 1.3f));
    }

    // This function is called by the capture animation when it finishes.
    public void OnCaptureAnimFinished()
    {
        IsMeasureActive = false; // disable measure
        transform.parent = ogParent; // re-parent measure from the previous ScaleAndPos() unparent
        OnCaptureFinished?.Invoke(null, measureNum); // invoke event to let things know we finished capturing

        // Capture all notes in case we missed any. This might remain as a hack anyway.
        // TODO: revise why the detector is inconsistent at catching notes. Perhaps limited by FPS?
        foreach (Note note in noteList)
            note.CaptureNote();
    }

    [ExecuteInEditMode]
    private void OnValidate()
    {
        ScaleAndPos();
    }

    public bool capturing = false;
    void Update()
    {
        if (capturing)
            ScaleAndPos();
    }

    // Z position

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