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

    private Color _measureColor;
    public Color MeasureColor
    {
        get { return _measureColor; }
        set
        {
            _measureColor = value;
            MeasurePlane_Active.GetComponent<MeshRenderer>().material.color = value;
        }
    }

    Color _edgeLightsColor;
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

        // unparent front & back edge lights
        // TODO: This is very hacky!
        foreach (GameObject obj in EdgeLightsFrontBack)
            obj.transform.parent = null;
    }
    void Start()
    {
        if (!amp_ctrl.Disable)
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

        // Move front & back edge lights to their correct positions
        // TODO: This is very hacky!
        PositionFrontBackEdgeLights();

        ogParent = transform.parent; // for destruction and length changes

        EdgeLightsGlow = false;
    }

    void PositionFrontBackEdgeLights()
    {
        foreach (GameObject obj in EdgeLightsFrontBack)
        {
            if (obj.transform.name.Contains("Front"))
                obj.transform.Translate(transform.forward * transform.position.z);
            else
                obj.transform.Translate(transform.forward * (transform.position.z + transform.lossyScale.z - 1));

            obj.transform.Translate(transform.right * transform.position.x);

            obj.transform.parent = EdgeLightsController.transform;
            EdgeLightsController.Color = EdgeLightsColor;
        }
    }

    // Subbeat creation

    public void CreateSubbeats()
    {

    }

    // Capturing

    public float FullLength = 1f;
    [Range(0f, 1f)]
    public float Length = 1f;
    [Range(0f, 1f)]
    public float CaptureLength = 0f;

    Transform ogParent;
    void ScaleAndPos()
    {
        transform.parent = null;
        Vector3 ogPosition = transform.position;

        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, FullLength * (Length - CaptureLength));
        transform.position = new Vector3(ogPosition.x, ogPosition.y, startTimeInZPos + (FullLength * CaptureLength));
    }

    public event EventHandler<int> OnCaptureFinished;

    public void CaptureMeasure()
    {
        gameObject.GetComponent<Animation>().Play();
        GameObject detector = (GameObject)Instantiate(detectorPrefab, gameObject.transform);
        detector.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material.color = Track.Colors.ConvertColor(EdgeLightsColor);
        detector.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material.SetColor("_EmissionColor", Track.Colors.ConvertColor(EdgeLightsColor * 1.3f));

        //MeasureDestructionNoteDetector.SetActive(true); // start capturing notes in measure

        /*
        await Task.Delay(TimeSpan.FromSeconds(.5));

        //Destroy(gameObject);
        //MeasureDestructionNoteDetector.SetActive(false); // stop capturing notes
        //Visuals.SetActive(false); // disable Measure object
        OnCaptureFinished?.Invoke(null, null);
        */
    }

    public void OnCaptureAnimFinished()
    {
        IsMeasureActive = false;
        transform.parent = ogParent;
        OnCaptureFinished?.Invoke(null, measureNum);

        foreach (Note note in noteList)
            note.CaptureNote();
    }

    private void OnValidate()
    {
        /*
#if UNITY_EDITOR
        ScaleAndPos();
#endif
        */
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
