using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;

public class Measure : MonoBehaviour
{
    AmplitudeSongController amp_ctrl { get { return AmplitudeSongController.Instance; } }

    public int measureNum; // measure ID
    public List<Note> noteList = new List<Note>(); // notes in this measure
    public Track measureTrack;
    public List<MeasureSubBeat> subbeatList = new List<MeasureSubBeat>();
    public List<GameObject> subbeatObjectList = new List<GameObject>(); // TODO: this is bad
    public Track.InstrumentType? trackInstrument;

    public float startTime;
    public float endTime;

    public GameObject Visuals;
    public GameObject MeasurePlane;
    public GameObject MeasurePlane_Active;
    public GameObject MeasureSeparator;
    public Material SubbeatSeparatorMaterial;

    public EdgeLightsController EdgeLightsController;
    public List<GameObject> EdgeLightsFrontBack = new List<GameObject>();

    public Animation anim;
    public MeasureDestructDetector detector;

    #region Properties
    private Color _measureColor = Track.Colors.Drums;
    public Color MeasureColor
    {
        get { return _measureColor; }
        set
        {
            _measureColor = value;
            EdgeLightsColor = value;
            MeasurePlane_Active.GetComponent<MeshRenderer>().material.color = value;
            MeasurePlane_Active.GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
            MeasurePlane_Active.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", value / 1.5f);
        }
    }

    public Color EdgeLightsColor
    {
        get { return EdgeLightsController.Color; }
        set { EdgeLightsController.Color = value; }
    }

    public float EdgeLightsGlowIntensity
    {
        get { return EdgeLightsController.GlowIntensity; }
        set { EdgeLightsController.GlowIntensity = value; }
    }

    public bool EdgeLightsGlow
    {
        get { return EdgeLightsController.EnableGlow; }
        set { EdgeLightsController.EnableGlow = value; }
    }

    // This gets/controls whether the measure is visible.
    // TODO: make this disable/enable the gameObject completely?
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

    // This controls the enabled state of this measure and its notes.
    // It enables/disables the focused visuals and the notes inside.
    bool _isMeasureEnabled = true;
    public bool IsMeasureEnabled
    {
        get { return _isMeasureEnabled; }
        set
        {
            _isMeasureEnabled = value;

            SetMeasureNotesActive(value);
            MeasurePlane_Active.SetActive(false);
        }
    }

    private bool? _isFocused;
    public bool IsFocused
    {
        get { return _isFocused.HasValue ? _isFocused.Value : measureTrack.IsTrackFocused; }
        set
        {
            _isFocused = value;

            if (IsMeasureQueued & IsMeasureEnabled)
                MeasurePlane_Active.SetActive(value);
            else
                MeasurePlane_Active.SetActive(false);
        }
    }

    private bool isMeasureQueued = false;
    public bool IsMeasureQueued // Is the measure queued for playing | i.e. is this an upcoming measure, even while being played
    {
        get { return isMeasureQueued; }
        set { isMeasureQueued = value; if (IsMeasureEnabled) MeasurePlane_Active.SetActive(IsFocused); }
    }

    // If a measure doesn't contain any notes, this should be true.
    bool _isMeasureEmpty = false;
    public bool IsMeasureEmpty
    {
        get { return _isMeasureEmpty; }
        set
        {
            _isMeasureEmpty = value;
            IsMeasureActive = !value;
            //Visuals.SetActive(IsMeasureActive & !value);
        }
    }

    bool _isMeasureCaptured;
    public bool IsMeasureCaptured
    {
        get { return _isMeasureCaptured; }
        set
        {
            _isMeasureCaptured = value;
            IsMeasureEnabled = !value;
            //SetMeasureNotesActive(false);
        }
    }
    public bool IsMeasureCapturedFull { get { return (IsMeasureCaptured & !IsMeasureActive); } }

    public bool IsMeasureToBeCaptured = false;

    // The difference between regular and 'full' is the latter only returns true for capture state when the animation has finished
    public bool IsMeasureEmptyOrCaptured { get { return (IsMeasureEmpty || IsMeasureCaptured); } }
    public bool IsMeasureEmptyOrCapturedFull { get { return (IsMeasureEmpty || IsMeasureCapturedFull); } }

    /* DOESN'T SEEM LIKE THIS IS USEFUL
    public bool IsMeasureEmptyAndCaptured { get { return IsMeasureEmpty & IsMeasureCaptured; } }
    public bool IsMeasureEmptyAndCapturedFull { get { return IsMeasureEmpty & IsMeasureCapturedFull; } }
    */

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

    public bool IsMeasureScorable = true; // Should this measure score points?
    public bool IsMeasureStreakable = true; // Should this measure count towards increasing the streak counter?
    #endregion

    UnityEngine.Object subBeatPrefab;
    void Awake()
    {
        subBeatPrefab = TracksController.Instance.subbeatPrefab;

        // get front & back edge lights
        for (int i = 2; i <= 5; i++)
            EdgeLightsFrontBack.Add(EdgeLightsController.gameObject.transform.GetChild(i).gameObject);

        // unparent front & back edge lights
        // TODO: This is very hacky!
        /*
        foreach (GameObject obj in EdgeLightsFrontBack)
            obj.transform.parent = null;
        */
    }
    [ExecuteInEditMode]
    void Start()
    {
        if (amp_ctrl.Enabled)
            CreateSubbeats();

        // Create subbeat separators
        for (int i = 1; i < 8; i++)
        {
            var subbeatSeparator = Instantiate(MeasureSeparator);
            subbeatSeparator.name = "SubbeatSeparator_" + i;
            subbeatSeparator.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + AmplitudeSongController.Instance.subbeatLengthInzPos * i);
            subbeatSeparator.transform.eulerAngles = MeasureSeparator.transform.eulerAngles;
            subbeatSeparator.transform.localScale = new Vector3(MeasureSeparator.transform.lossyScale.x, MeasureSeparator.transform.lossyScale.y, 0.03f);
            subbeatSeparator.GetComponent<MeshRenderer>().material = SubbeatSeparatorMaterial;
            subbeatSeparator.transform.parent = MeasurePlane.transform;
        }

        // Move front & back edge lights to their correct positions
        // TODO: This is very hacky!
        PositionFrontBackEdgeLights();

        ogParent = transform.parent; // for destruction and length changes

        EdgeLightsGlowIntensity = 1f;
        EdgeLightsColor = Track.Colors.ColorFromTrackType(trackInstrument.Value);

        // setup particles
        detector.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material.color = Colors.ConvertColor(EdgeLightsColor);
        detector.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material.SetColor("_EmissionColor", Colors.ConvertColor(EdgeLightsColor * 1.3f));
    }

    // Edge lights

    // This function positions the 6 edge lights on the measure after scaling the measure.
    // This is needed because when scaling a measure, it ends up scaling the edge lights as well.
    // TODO: make the edge lights be a texture on the measure model instead
    public void PositionFrontBackEdgeLights()
    {
        MeasureSeparator.transform.parent = null;
        /*
        Vector3 pos = MeasureSeparator.transform.position;
        Vector3 rot = MeasureSeparator.transform.eulerAngles;
        Vector3 scale = MeasureSeparator.transform.lossyScale;
        pos.z = transform.position.z;
        scale.z = 0.01f; 

        MeasureSeparator.transform.position = pos;
        MeasureSeparator.transform.localEulerAngles = rot;*/
        MeasureSeparator.transform.localScale = new Vector3(MeasureSeparator.transform.localScale.x, MeasureSeparator.transform.localScale.y, 0.1f);
        MeasureSeparator.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + (0.1f / 2f));
        MeasureSeparator.transform.parent = MeasurePlane.transform;

        foreach (GameObject obj in EdgeLightsFrontBack)
        {
            
            obj.transform.parent = null;
            obj.transform.localScale = new Vector3(obj.transform.localScale.x, obj.transform.localScale.y, 0.01f);
            obj.transform.parent = EdgeLightsController.transform;
            EdgeLightsController.Color = EdgeLightsColor;
        }
    }

    public void SetMeasureNotesActive(bool state)
    {
        foreach (Note note in noteList)
            note.IsNoteEnabled = state;
    }
    public void SetMeasureNotesToBeCaptured(bool state = true)
    {
        if (!IsMeasureCapturable & IsMeasureEmptyOrCaptured & !IsMeasureEnabled)
            return;

        foreach (Note note in noteList)
            note.IsNoteToBeCaptured = state;

        IsMeasureToBeCaptured = state;
    }

    // Subbeat creation

    public void CreateSubbeats()
    {
        // create subbeats
        if (subBeatPrefab == null)
            subBeatPrefab = Resources.Load("Prefabs/MeasureSubBeat");

        Vector3 subbeatScale = new Vector3(1, 1, amp_ctrl.subbeatLengthInzPos);
        float lastSubbeatPosition = startTime;

        for (int i = 0; i < 8; i++)
        {
            GameObject obj = (GameObject)GameObject.Instantiate(subBeatPrefab);
            MeasureSubBeat script = obj.GetComponent<MeasureSubBeat>();

            obj.name = string.Format("MEASURE{0}_SUBBEAT{1}", measureNum, i);
            obj.transform.localScale = subbeatScale;
            obj.transform.localEulerAngles = gameObject.transform.eulerAngles;
            //obj.transform.parent = measureTrack.TriggerContainer.transform;
            obj.transform.position = new Vector3(transform.position.x, transform.position.y, lastSubbeatPosition);
            obj.transform.SetParent(measureTrack.TriggerContainer.transform, true);

            script.subbeatNum = i;

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
    public void MeasureCaptureUpdate(object sender, EventArgs e)
    {
        if (IsMeasureCapturing)
        {
            transform.parent = null; // unparent from track
            Vector3 ogPosition = transform.position; // store original position

            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, FullLength * (Length - CaptureLength)); // scale measure based on capture length
            transform.position = new Vector3(ogPosition.x, ogPosition.y, startTime + (FullLength * CaptureLength)); // with the un-parenting, we need to use the stored position instead

            transform.parent = ogParent;
            /*
            if (Application.isPlaying)
               PositionFrontBackEdgeLights();
            */
        }
    }

    public bool IsMeasureCapturing = false;
    public event EventHandler<int> OnCaptureFinished;
    public IEnumerator CaptureMeasure()
    {
        anim.Stop();
        anim.Play(); // play capture anim!

        UpdateEvent += MeasureCaptureUpdate;

        IsMeasureCapturing = true;

        detector.Begin();

        while (IsMeasureCapturing)
            yield return null;

        UpdateEvent -= MeasureCaptureUpdate;
    }

    // Captures this measure without an animation
    public void CaptureMeasureImmediate(bool removeNotes = false)
    {
        IsMeasureActive = false;
        IsMeasureCaptured = true;
        noteList.ForEach(n => { n.CaptureNote(false); if (removeNotes) n.gameObject.SetActive(false); });
    }

    // This function is called by the capture animation when it finishes.
    public void OnCaptureAnimFinished()
    {
        //IsMeasureCapturing = false; // anim sets this prop
        IsMeasureActive = false; // disable measure visuals completely

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
        //MeasureCaptureUpdate(null, null);
    }

    public event EventHandler UpdateEvent;
    void Update()
    {
        UpdateEvent?.Invoke(null, null);
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