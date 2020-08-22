using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using UnityEngine;

public class CatcherController : MonoBehaviour
{
    public static CatcherController Instance;

    public AmplitudeSongController amp_ctrl { get { return AmplitudeSongController.Instance; } }
    public PlayerController PlayerController { get { return PlayerController.Instance; } }
    public TracksController TracksController { get { return TracksController.Instance; } }
    public Track CurrentTrack { get { return TracksController.CurrentTrack; } }
    public Measure CurrentMeasure { get { return CurrentTrack.GetMeasureForZPos(transform.position.z); } }
    //public int CurrentMeasureID { get { return CurrentMeasure.measureNum; } }
    public int CurrentMeasureID = 0;
    public int CurrentBeatID = 0;
    public List<Catcher> Catchers = new List<Catcher>();
    public KeyCode[] keycodes = new KeyCode[] { KeyCode.LeftArrow, KeyCode.UpArrow, KeyCode.RightArrow };

    public BoxCollider BoxCollider;

    public GameObject noteDestructPrefab;

    public float CatcherRadiusExtra
    {
        get { return Catchers[0].catchRadiusExtra; }
        set
        {
            foreach (Catcher catcher in Catchers)
                catcher.catchRadiusExtra = value;
        }
    }

    public Note LastHitNote;
    Note nearestNote;
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
        OnSubbeatTriggerLate += CatcherController_OnSubbeatTriggerLate;
        OnMeasureTrigger += CatcherController_OnMeasureTrigger;
        OnMeasureTriggerLate += CatcherController_OnMeasureTriggerLate;
        OnNoteExitTrigger += CatcherController_OnNoteTrigger;
    }

    public event EventHandler<int[]> OnSubbeatTrigger;
    public event EventHandler<int[]> OnSubbeatTriggerLate;
    public event EventHandler<int> OnMeasureTrigger;
    public event EventHandler<int> OnMeasureTriggerLate;
    public event EventHandler<Note> OnNoteExitTrigger;

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
    void OnTriggerExit(Collider coll)
    {
        if (coll.name == "SubbeatTrigger")
        {
            string[] subbeat_objName = coll.gameObject.transform.parent.name.Split('_');
            string[] measureName = subbeat_objName[0].Split(new string[] { "MEASURE" }, StringSplitOptions.None);
            string[] subbeatName = subbeat_objName[1].Split(new string[] { "SUBBEAT" }, StringSplitOptions.None);

            int measureNum = int.Parse(measureName[1].ToString());
            int subbeatNum = int.Parse(subbeatName[1].ToString());
            OnSubbeatTriggerLate?.Invoke(null, new int[] { measureNum, subbeatNum });
        }
        if (coll.name == "MeasureTrigger")
        {
            string[] subbeat_objName = coll.gameObject.transform.parent.name.Split('_');
            string[] measureName = subbeat_objName[0].Split(new string[] { "MEASURE" }, StringSplitOptions.None);

            int measureNum = int.Parse(measureName[1].ToString());
            OnMeasureTriggerLate?.Invoke(null, measureNum);
        }
        if (coll.tag == "Note")
        {
            string[] noteInfo = coll.name.Split('_'); // 0: CATCH | 1: lane | 2: instrument | 3: counter
            int noteNum = int.Parse(noteInfo[3]);
            Note note = coll.GetComponent<Note>();

            OnNoteExitTrigger?.Invoke(null, note);
        }
    }

    // When we trigger a measure | e: the measure num
    void CatcherController_OnMeasureTrigger(object sender, int e)
    {
        // DEBUG
        UpdateMeasureDebug(e);

        CurrentMeasureID = e;

        if (RhythmicGame.IsLoading)
            return;

        // increase current ACTIVE measure counter
        foreach (Track track in TracksController.Tracks)
        {
            // The active measure list in a track contains only those measures that are not empty.
            // This is used to find upcoming measures and notes.
            Measure m = track.trackMeasures[e];
            if (!m.IsMeasureEmptyOrCaptured)
                track.activeMeasureNum++;
        }
    }
    // (LATE) When we trigger a measure | e: the measure num
    private void CatcherController_OnMeasureTriggerLate(object sender, int e)
    {
        // if the current measure we're on is not empty or captured, set all notes 'to be captured'
        if (!CurrentMeasure.IsMeasureEmptyOrCaptured)
            TracksController.DisableOtherMeasures();

        foreach (Track track in TracksController.CurrentTrackSet)
        {
            if (track.IsTrackCaptured & !track.trackMeasures[CurrentMeasureID].IsMeasureCaptured)
                track.IsTrackCaptured = false;

            if (track.IsTrackBeingCaptured)
                AmplitudeSongController.Instance.AdjustTrackVolume(track.ID, 1f);
            else if (track.IsTrackCaptured)
                AmplitudeSongController.Instance.AdjustTrackVolume(track.ID, 0.7f);
            else
                AmplitudeSongController.Instance.AdjustTrackVolume(track.ID, 0f);
        }
    }

    // When we trigger a subbeat | e[0]: measure num | e[1]: subbeat num
    private void CatcherController_OnSubbeatTrigger(object sender, int[] e)
    {
        UpdateSubbeatDebug(e[1]);

        if (e[1] == 0 || e[1] % 2 == 0)
            CurrentBeatID = e[1];


    }
    // (LATE) When we trigger a subbeat | e[0]: measure num | e[1]: subbeat num
    void CatcherController_OnSubbeatTriggerLate(object sender, int[] e)
    {
        // IGNORE EMPTY TRACK NOTE DETECTION IN CONDITIONS:
        if (ShouldHit.Count == 0) // if ShouldHit is empty
            return;
        if (!CurrentMeasure.IsMeasureEnabled & !CurrentMeasure.IsMeasureCaptured & CurrentMeasure.IsMeasureActive) // if measure is disabled
        { Subbeat_DetectNoteMisses(e); return; }
        else if (!CurrentMeasure.IsMeasureEmptyOrCapturedFull & e[0] < ShouldHit[0].measureNum) // if we are on an active measure
        { return; }
        else if (IsSuccessfullyCatching || e[0] < ShouldHit[0].measureNum) // if we are not successfully catching, only miss beyond closest ShouldHit note
        { return; }

        Subbeat_DetectNoteMisses(e);
    }
    // Empty track note detection - miss notes as 'ignore' | e[0]: measure num | e[1]: subbeat num
    void Subbeat_DetectNoteMisses(int[] e)
    {
        Note note = null;

        foreach (Track track in TracksController.CurrentTrackSet)
        {
            foreach (Note n in track.trackMeasures[e[0]].noteList)
                if (n.IsNoteEnabled & n.subbeatNum < e[1] & n.zPos < transform.position.z)
                { note = n; break; }
        }

        if (note != null)
            PlayerController.DeclareMiss(note, Catcher.NoteMissType.Ignore);
    }

    // When we exit a note's collision trigger
    private void CatcherController_OnNoteTrigger(object sender, Note e)
    {
        if (e.IsNoteEnabled & !e.IsNoteCaptured)
        {
            amp_ctrl.AdjustTrackVolume(e.noteTrack.ID, 0f);
            PlayerController.DeclareMiss(e, Catcher.NoteMissType.Ignore);
        }
    }

    public bool FindNextMeasureEnabled { get; set; } = true;

    // Finds the upcoming measures and its notes
    public void FindNextMeasuresNotes(Track trackToKeep = null, bool doubleMeasure = false)
    {
        if (!FindNextMeasureEnabled)
            return;
        if (nearestNote != null)
            nearestNote.Color = new Color(255, 255, 255); // reset nearest note debug color

        foreach (Note note in ShouldHit)
            note.noteTrack.identicalTracks.ForEach(t => t.trackNotes.Find(n => n.noteName == note.noteName).Color = new Color(255, 255, 255)); // reset should hit notes' debug color

        nearestNote = null;

        //if (trackToKeep == null)
        ShouldHit.Clear();
        /*
        else // if we have a track to keep, remove every other shouldhit note other than the track we have to ignore
        {
            List<Note> shList = ShouldHit.ToList(); // create temporary list from ShouldHit
            foreach (Note note in shList) // go through temp list to avoid a changed collection issue
                if (note.noteTrack == trackToKeep) { ShouldHit.Remove(note); } // remove those ShouldHit notes that aren't from this track

            shList = null; // delete temporary list
        }
        */

        foreach (Track track in TracksController.Instance.CurrentTrackSet)
        {
            if (!track.TUT_IsTrackEnabled)
                continue;
            if (track == trackToKeep)
                continue;
            if (track.trackNotes.Count == 0)
                continue;

            // clear sequence measures
            track.sequenceMeasures.Clear();

            int? measureNum = null;

            int counter = 0;
            foreach (Measure m in track.trackMeasures) // find next enabled measure
            {
                if (m.IsMeasureEnabled & !m.IsMeasureEmptyOrCaptured & m.measureNum > CurrentMeasureID)
                {
                    if (doubleMeasure & m.IsMeasureToBeCaptured)
                        continue;
                    else
                        measureNum = counter;
                }
                else
                    counter++;

                if (measureNum.HasValue)
                    break;
            }

            if (!measureNum.HasValue)
            {
                Debug.LogErrorFormat("CATCHER: could not find upcoming measure for track {0}!", track.trackName);
                continue;
            }

            // add two measures as sequences
            for (int i = 0; i < 2; i++)
                track.AddSequenceMeasure(track.trackMeasures[measureNum.Value + i]);

            if (doubleMeasure)
                measureNum++; // ADD 1 TO MEASURENUM FOR GETTING SECOND MEASURE AS NOTE TO HIT
            Measure measure = track.trackMeasures[measureNum.Value];

            if (measure.noteList.Count == 0)
                return;

            if (!measure.IsMeasureEmptyOrCaptured)
            {
                Note note = null;
                foreach (Note n in measure.noteList)
                    if (track.IsTrackBeingCaptured & n.IsNoteEnabled & !n.IsNoteCaptured & !n.IsNoteToBeCaptured)
                    {
                        note = n;
                        break;
                    }
                    else if (!track.IsTrackBeingCaptured & !n.IsNoteCaptured)
                    {
                        note = n;
                        break;
                    }
                    else
                        continue;
                //Note note = measure.noteList[0]; // add first note of found measure to ShouldHit
                //if (note.IsNoteEnabled & !note.IsNoteCaptured & !note.IsNoteToBeCaptured)
                if (note != null)
                {
                    ShouldHit.Add(note);
                    track.nearestNote = note;
                }
                else
                    Debug.LogErrorFormat("CATCHER: The first note of measure {0} was not suitable for ShouldHit!", measure.name);
            }
        }

        if (ShouldHit.Count > 1)
        {
            List<Note> newList = ShouldHit.OrderBy(n => n.zPos).ToList();
            ShouldHit = newList;
        }

        // DEBUG
        if (!RhythmicGame.DebugNextNoteCheckEvents)
            return;

        PlayerController.NextNoteText.text = "Next notes: \n";

        foreach (Note note in ShouldHit)
        {
            if (note == null)
                break;

            PlayerController.NextNoteText.text += string.Format("Measure: {0} Subbeat: {1} Track: {2} Lane: {3}\n",
                note.measureNum, note.subbeatNum, note.noteTrack.trackName, note.noteLane.ToString());
            note.noteTrack.identicalTracks.ForEach(t => t.trackNotes.Find(n => n.noteName == note.noteName).Color = new Color(0, 255, 0));
        }
    }
    public bool FindNextNote()
    {
        Note note = null;

        foreach (Note n in CurrentMeasure.noteList)
        {
            if (!n.IsNoteCaptured & n.zPos >= transform.position.z)
            {
                note = n;
                break;
            }
        }

        if (note.measureNum != CurrentMeasureID) // if it's the last note, find all the next measures
            return false;

        //ShouldHit[prev_ShouldHitIndex] = note;
        //ShouldHit.Clear();
        CurrentTrack.nearestNote = note;
        nearestNote = note;

        note.Color = new Color(255, 0, 0);

        // DEBUG
        PlayerController.NextNoteText.text = string.Format("Measure: {0} Subbeat: {1} Track: {2} Lane: {3}\n",
                note.measureNum, note.subbeatNum, note.noteTrack.trackName, note.noteLane.ToString());

        return true;
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

    // CATCHING & INPUT

    public event EventHandler<CatchEventArgs> OnCatch;

    public bool IsSuccessfullyCatching { get; set; } = true;
    void Catcher_OnCatch(object sender, CatchEventArgs e)
    {
        LastHitNote = e.note;

        if (e.catchresult == Catcher.CatchResult.Powerup || e.catchresult == Catcher.CatchResult.Success)
        {
            IsSuccessfullyCatching = true;
            e.note.noteTrack.IsTrackBeingCaptured = true;

            TracksController.SetCurrentMeasuresNotesToBeCaptured();
            TracksController.DisableOtherMeasures();

            if (e.note.IsLastNote) // if it's the last note in the measure, consider measure as cleared
                e.note.noteTrack.OnMeasureClear(e.note.noteMeasure);

            amp_ctrl.AdjustTrackVolume(e.note.noteTrack.ID, 1f);
            CreateNoteDestruct(e.note.transform.position, e.note.transform.eulerAngles);
        }
        else
        {
            IsSuccessfullyCatching = false;
            TracksController.SetAllTracksCapturingState(false);

            //amp_ctrl.AdjustTrackVolume(CurrentTrack.ID, 0f);
        }

        if (e.catchresult == Catcher.CatchResult.Inactive)
        {
            amp_ctrl.AdjustTrackVolume(e.note.noteTrack.ID, 1f);
            CreateNoteDestruct(e.note.transform.position, e.note.transform.eulerAngles);
        }

        OnCatch?.Invoke(null, e);
    }

    void CreateNoteDestruct(Vector3 pos, Vector3 rot)
    {
        var go = Instantiate(noteDestructPrefab);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(0.21f, 0.05f, 0.21f);
        go.transform.localEulerAngles = rot;
    }

    void Update()
    {
        if (RhythmicGame.IsLoading)
            return;

        // INPUT

        /*
        foreach (string input in InputManager.Catcher.Inputs)
            if (InputManager.GetButtonDown(input))
                GetCatcherFromInput(input).PerformCatch();

        foreach (string key in InputManager.Catcher.Catching)
        {
            if (InputManager.GetCatcherButtonDown(key)) // If the key is down
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

        if (!m_pressingButton & Input.GetButton(pressedKey)) // if we're not holding the key and the input that's being held is the same as the previous key
        {
            GetCatcherFromInput(pressedKey).PerformCatch();
            m_pressingButton = true; // we're holding the button now - might not be needed because of the above checking? TODO: test!
        }
        if (!Input.GetButton(pressedKey)) // if the prev key is not being pressed, set the prev key to none
            pressedKey = "";
        */
    }

    public Catcher GetCatcherFromInput(string input)
    {
        return Catchers[(int)InputManager.Catcher.InputToTrackLane(input)];
    }
    public Catcher GetCatcherFromLane(Track.LaneType lane)
    {
        return Catchers[(int)(lane)];
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
