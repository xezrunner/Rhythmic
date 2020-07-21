using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CatcherController : MonoBehaviour
{
    public static CatcherController Instance;

    public AmplitudeSongController amp_ctrl { get { return AmplitudeSongController.Instance; } }
    public PlayerController PlayerController { get { return PlayerController.Instance; } }
    public TracksController TracksController { get { return TracksController.Instance; } }
    public Track CurrentTrack { get { return TracksController.CurrentTrack; } }
    public int CurrentMeasureID = 0;
    public Measure CurrentMeasure { get { return CurrentTrack.GetMeasureForID(CurrentMeasureID); } }
    public List<Catcher> Catchers = new List<Catcher>();
    public KeyCode[] keycodes = new KeyCode[] { KeyCode.LeftArrow, KeyCode.UpArrow, KeyCode.RightArrow };

    float _catcherCollisionRadius;
    public float CatcherCatchRadius
    {
        get { return _catcherCollisionRadius; }
        set
        {
            _catcherCollisionRadius = value;
            foreach (Catcher catcher in Catchers)
                catcher.catchRadius = value;
        }
    }

    public Note LastHitNote;
    public List<Note> ShouldHit = new List<Note>();

    void Awake()
    {
        Instance = this;

        // find catchers
        foreach (Transform obj in transform)
        {
            Catcher catcher = obj.GetComponent<Catcher>();
            catcher.OnCatch += Catcher_OnCatch;
            if (catcher != null)
                Catchers.Add(catcher);
        }

        // create a Collision detector
        //BoxCollider collider = gameObject.AddComponent<BoxCollider>();

        OnSubbeatTrigger += CatcherController_OnSubbeatTrigger;
        OnMeasureTrigger += CatcherController_OnMeasureTrigger;
    }

    #region MEASURE & SUBBEAT TRIGGERS

    public event EventHandler<int[]> OnSubbeatTrigger;
    public event EventHandler<int> OnMeasureTrigger;

    // TODO: Cleanup!
    void OnTriggerEnter(Collider coll)
    {
        if (coll.name == "MeasureTrigger")
        {
            string[] subbeat_objName = coll.gameObject.transform.parent.name.Split('_');
            string[] measureName = subbeat_objName[0].Split(new string[] { "MEASURE" }, StringSplitOptions.None);

            int measureNum = int.Parse(measureName[1].ToString());
            OnMeasureTrigger?.Invoke(null, measureNum);
        }
    }
    void OnTriggerExit(Collider coll)
    {
        if (coll.name == "SubbeatTrigger")
        {
            string[] subbeat_objName = coll.gameObject.transform.parent.name.Split('_');
            string[] measureName = subbeat_objName[0].Split(new string[] { "MEASURE" }, StringSplitOptions.None);
            string[] subbeatName = subbeat_objName[1].Split(new string[] { "SUBBEAT" }, StringSplitOptions.None);

            int measureNum = int.Parse(measureName[1].ToString());
            int subbeatNum = int.Parse(subbeatName[1].ToString());
            OnSubbeatTrigger?.Invoke(null, new int[] { measureNum, subbeatNum });
        }
    }

    // When we trigger a subbeat
    // e[0]: measure num | e[1]: subbeat num
    void CatcherController_OnSubbeatTrigger(object sender, int[] e)
    {
        Note note = nearestNote != null ? nearestNote : ShouldHit[0];
        if (note != null & LastHitNote != note & e[0] == note.measureNum & e[1] == note.subbeatNum)
            PlayerController.DeclareMiss(note, Catcher.NoteMissType.Ignore);

        UpdateSubbeatDebug(e[1]);
    }

    // When we trigger a measure
    // e: the measure num
    void CatcherController_OnMeasureTrigger(object sender, int e)
    {
        Measure measure = CurrentTrack.GetMeasureForID(e);
        // if the current measure we're on is not empty
        if (!measure.IsMeasureEmpty) // set the notes to 'to be captured' state
            TracksController.SetCurrentMeasuresNotesToBeCaptured();

        foreach (Track track in TracksController.Tracks)
        {
            if (!track.trackMeasures[e].IsMeasureEmpty)
                track.upcomingActiveMeasure++;
        }

        // find ShouldHit notes if there aren't any
        if (ShouldHit.Count == 0)
            FindNextMeasureNotes(); // find the next measure for the [measure that we are on + 1]

        // DEBUG
        UpdateMeasureDebug(e);

        if (CurrentMeasureID != e)
            CurrentMeasureID++;
    }

    Note nearestNote;
    public void FindNextMeasureNotes()
    {
        // Go through each track and try to find 0th note of the upcoming measure
        foreach (Track track in TracksController.Instance.Tracks)
        {
            Measure measure = null;

            if (!track.IsTrackEmpty)
                measure = track.trackActiveMeasures[track.upcomingActiveMeasure];
            else
                continue;

            if (measure.IsMeasureEmpty)
            {
                Debug.LogWarningFormat("CATCHERCONTROLLER: The upcoming measure in track {0} doesn't have any notes!", track.trackName);
                continue;
            }

            Note note = measure.noteList[0];
            if (note.IsNoteActive & !note.IsNoteCaptured & !note.IsNoteToBeCaptured & !track.IsTrackBeingCaptured)
            {
                ShouldHit.Add(note);
                track.nearestNote = note;
            }
        }

        if (ShouldHit.Count == 0)
            Debug.LogError("CATCHERCONTROLLER: Couldn't find any upcoming notes! This is bad!");

        nearestNote = null;
        ShouldHit = ShouldHit.OrderBy(o => o.zPos).ToList();

        // DEBUG
        if (!RhythmicGame.DebugNextNoteCheckEvents)
            return;

        PlayerController.NextNoteText.text = "Next notes: \n";

        foreach (Note note in ShouldHit)
        {
            PlayerController.NextNoteText.text += string.Format("Measure: {0} Subbeat: {1} Track: {2} Lane: {3}\n",
                note.measureNum, note.subbeatNum, note.noteTrack.trackName, note.noteLane.ToString());
            note.Color = new Color(0, 255, 0);
        }
    }
    public void FindNextNote()
    {
        Note note;
        int prev_ShouldHitIndex = CurrentTrack.trackNotes.IndexOf(LastHitNote);

        note = CurrentTrack.trackNotes[prev_ShouldHitIndex + 1];

        /*
        if (CurrentTrack.nearestNote == null & ShouldHit.Count != 0)
        {
            note = ShouldHit[0];
            CurrentTrack.nearestNote = note;
        }
        else if (CurrentTrack.nearestNote != null)
            note = CurrentTrack.nearestNote;
        else
            return;

        int prev_ShouldHitIndex = ShouldHit.IndexOf(note);

        if (note.noteMeasure.noteList.IndexOf(note) == note.noteMeasure.noteList.Count - 1)
            note = CurrentTrack.trackActiveMeasures[CurrentTrack.upcomingActiveMeasure].noteList[0];
        else
            note = note.noteMeasure.noteList[note.noteMeasure.noteList.IndexOf(note) + 1];
        */

        //ShouldHit[prev_ShouldHitIndex] = note;
        CurrentTrack.nearestNote = note;
        nearestNote = note;

        // DEBUG
        PlayerController.NextNoteText.text = string.Format("Measure: {0} Subbeat: {1} Track: {2} Lane: {3}\n",
                note.measureNum, note.subbeatNum, note.noteTrack.trackName, note.noteLane.ToString());
    }

    async void UpdateMeasureDebug(int measureNum)
    {
        if (!RhythmicGame.DebugNextNoteCheckEvents)
            return;

        PlayerController.MeasureCounterText.text = string.Format("Measure: {0}", measureNum);
        PlayerController.MeasureCounterText.color = Color.red;
        await System.Threading.Tasks.Task.Delay(1000);
        PlayerController.MeasureCounterText.color = Color.white;
    }
    async void UpdateSubbeatDebug(int subbeatNum)
    {
        if (!RhythmicGame.DebugNextNoteCheckEvents)
            return;

        PlayerController.SubbeatCounterText.text = string.Format("Subbeat: {0}", subbeatNum);
        PlayerController.SubbeatCounterText.color = Color.red;
        await System.Threading.Tasks.Task.Delay(50);
        PlayerController.SubbeatCounterText.color = Color.white;
    }

    #endregion

    public event EventHandler<CatchEventArgs> OnCatch;

    void Catcher_OnCatch(object sender, CatchEventArgs e)
    {
        LastHitNote = e.note;

        OnCatch?.Invoke(null, e);

        if (e.catchresult == Catcher.CatchResult.Powerup || e.catchresult == Catcher.CatchResult.Success)
            FindNextNote();

    }

    public void PerformCatch(Track.LaneType lane)
    {
        GetCatcherFromLane(lane).PerformCatch();
    }

    KeyCode pressedKey = KeyCode.None;
    bool m_pressingButton = false;
    void Update()
    {
        // INPUT
        foreach (KeyCode key in keycodes)
        {
            if (Input.GetKeyDown(key)) // If the key is down
            {
                if (pressedKey == key) // If it's the same key that has been down before
                {
                    m_pressingButton = true; // We're holding the key
                    continue;
                }
                else // If it's a new key
                {
                    pressedKey = key; // replace the pressed key with the new key
                    m_pressingButton = false; // register as if we only pressed it, but not holding it
                    break;
                }
            }
        }

        if (!m_pressingButton & Input.GetKey(pressedKey)) // if we're not holding the key and the input that's being held is the same as the previous key
        {
            GetCatcherFromKeyCode(pressedKey).PerformCatch();
            m_pressingButton = true; // we're holding the button now - might not be needed because of the above checking? TODO: test!
        }
        if (!Input.GetKey(pressedKey)) // if the prev key is not being pressed, set the prev key to none
            pressedKey = KeyCode.None;
    }

    public Catcher GetCatcherFromKeyCode(KeyCode key)
    {
        return Catchers[(int)KeyCodeToLane(key)];
    }
    public Catcher GetCatcherFromLane(Track.LaneType lane)
    {
        return Catchers[(int)LaneToKeyCode(lane)];
    }
    /// <summary>
    /// Gives back the appropriate key for the lane type.
    /// </summary>
    /// <param name="lane">The lane in question</param>
    public static KeyCode LaneToKeyCode(Track.LaneType lane)
    {
        switch (lane)
        {
            case Track.LaneType.Left:
                return KeyCode.LeftArrow;
            case Track.LaneType.Center:
                return KeyCode.UpArrow;
            case Track.LaneType.Right:
                return KeyCode.RightArrow;

            default:
                return KeyCode.None;
        }
    }
    /// <summary>
    /// Gives back the appropriate track for the input key
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    public static Track.LaneType KeyCodeToLane(KeyCode key)
    {
        switch (key)
        {
            default:
                return Track.LaneType.Center;

            case KeyCode.LeftArrow:
                return Track.LaneType.Left;
            case KeyCode.UpArrow:
                return Track.LaneType.Center;
            case KeyCode.RightArrow:
                return Track.LaneType.Right;
        }
    }

    // TODO: move into its own class file?
    public class CatchEventArgs
    {
        public Catcher.CatchResult? catchresult;
        public Track.LaneType lane;
        public Note note;
        public Note.NoteType? notetype;
        public Catcher.NoteMissType? noteMissType;
    }
}
