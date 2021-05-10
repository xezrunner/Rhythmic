using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CatchResultType { UNKNOWN = 0, Success = 1, Empty = 2, Ignore = 3, Miss = 4, Error = 5 }
public struct CatchResult
{
    public CatchResult(Catcher c, CatchResultType t, Note n)
    {
        resultType = t;
        catcher = c;
        note = n;
    }

    public CatchResultType resultType;
    public Catcher catcher;
    public Note note;
}

public class PlayerCatching : MonoBehaviour
{
    public static PlayerCatching Instance;

    Clock Clock { get { return Clock.Instance; } }
    SongController SongController { get { return SongController.Instance; } }
    TracksController TracksController { get { return TracksController.Instance; } }
    PlayerPowerupManager PlayerPowerupManager { get { return PlayerPowerupManager.Instance; } } // TODO: Assign like the ones below?
    public Player Player;
    public PlayerLocomotion Locomotion;

    public Transform CatcherContainer; // This will automatically populate the Catchers list!
    public GameObject CatcherVisuals;
    public List<Catcher> Catchers = new List<Catcher>();

    void Awake()
    {
        Instance = this;

        // Populate catchers from the catcher comtainer if no catchers were assigned
        if (Catchers.Count == 0 & CatcherContainer != null)
        {
            foreach (Transform t in CatcherContainer)
            {
                Catcher c = t.GetComponent<Catcher>();
                if (c)
                {
                    Catchers.Add(c);

                    // Assign required player scripts
                    c.Catching = this;
                    c.Player = Player;
                    c.Locomotion = Locomotion;
                }
                else
                    Debug.LogError($"Invalid object in specific catcher container! | container: {CatcherContainer.name}, object: {t.name}");
            }
        }
    }
    private void Start()
    {
        if (!SongController.IsEnabled) return;

        // Assign ourselves in TracksController
        TracksController.Catching = this;

        // Set up notesToCatch array
        TracksController.targetNotes = new Note[TracksController.MainTracks.Length];

        // Slop visualization:
        IsSlopVisualization = _isSlopVisualization;
    }

    // TODO: TEMP: should remove/redo (if needed) at some point
    List<GameObject> _slopVisualizationObjects = new List<GameObject>();
    public static bool _isSlopVisualization;
    public static bool IsSlopVisualization
    {
        get { return _isSlopVisualization; }
        set
        {
            _isSlopVisualization = value;
            if (Instance) Instance.SetSlopVisualization(value);
        }
    }
    void SetSlopVisualization(bool value)
    {
        if (value)
        {
            for (int i = 0; i < 3; ++i)
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.position = Catchers[i].transform.position + Catchers[i].transform.right * Track.GetLocalXPosFromLaneType((LaneSide)i);
                obj.transform.rotation = Catchers[i].transform.rotation;
                obj.transform.localScale = new Vector3(0.5f, 0.5f, SongController.posInMs * SongController.SlopMs);
                obj.transform.parent = Catchers[i].transform;
                _slopVisualizationObjects.Add(obj);
            }
        }
        else
        {
            _slopVisualizationObjects.ForEach(o => Destroy(o));
            _slopVisualizationObjects.Clear();
        }
    }

    int lastIgnoreBar = -1;

    private void LateUpdate()
    {
        if (!SongController.IsPlaying) return;
        if (SongController.IsSongOver) return;

        // TEMP | TODO: Move elsewhere? (partially?)
        if (Clock.Fbar >= 4) // TODO: countin
        {
            foreach (Track t in TracksController.MainTracks)
            {
                if (t.IsTrackCaptured)
                {
                    SongController.audioSrcList[t.ID].volume = t.IsTrackFocused ? 1f : 0.4f;
                    continue;
                }
                else if (t.IsTrackBeingPlayed) SongController.audioSrcList[t.ID].volume = 1f;
                else SongController.audioSrcList[t.ID].volume = 0f;
            }
        }

        if (lastIgnoreBar == Clock.Fbar) return; // avoid spamming

        // *** SLOP ***
        float dist = Locomotion.DistanceTravelled;

        Measure currentMeasure = TracksController.CurrentMeasure;
        if (!currentMeasure.IsEmpty & !currentMeasure.IsCaptured)
        {
            Note note = TracksController.targetNotes[TracksController.CurrentTrackID];
            HandleSlop(dist, note);
        }
        else
            foreach (Note note in TracksController.targetNotes)
                if (note) HandleSlop(dist, note);
    }

    public void HandleSlop(float dist, Note note)
    {
        if (!note) { Debug.LogWarning($"Catching/HandleSlop(): No note was passed!"); HandleResult(new CatchResult()); lastIgnoreBar = Clock.Fbar; return; }

        // If the distance is bigger than the note distance + slop distance, we have 'ignored' the note.
        float max_dist = note.Distance + (SongController.SlopPos / 2); // Only one axis of SlopMs is required | NOTE: there was a bug here where missing the very last note in a sequence would result in the next measure being disabled.
        if (note.IsEnabled && dist > max_dist)
        {
            if (RhythmicGame.DebugCatcherSlopEvents)
                Logger.LogWarning($">>>>>>> SLOP! <<<<<<< | Track ID: {TracksController.CurrentRealTrackID}; targetNote timeMs: {note.TimeMs}, ms: {dist}, maxMs: {max_dist}");

            HandleResult(new CatchResult(Catchers[(int)note.Lane], CatchResultType.Ignore, note));
            lastIgnoreBar = Clock.Fbar; // avoid slop check spam
        }
    }

    public void HandleResult(CatchResult result)
    {
        if (!result.catcher) return;

        switch (result.resultType)
        {
            default: break;

            case CatchResultType.Success:
                {
                    Note note = result.note;

                    note.CaptureNote(NoteCaptureFX.CatcherCapture, true);

                    if (!note.Track.IsTrackBeingPlayed)
                    {
                        note.Track.SetIsTrackBeingPlayed(true);
                        TracksController.RefreshAll(note.Track);
                    }

                    // Give powerup for the given measure | TODO: This needs revising!! - What if there's a sequence worth of powerups? A simple check might be enough.
                    if (note == note.Track.Measures[note.MeasureID].Notes.Last() && note.PowerupType > 0)
                        PlayerPowerupManager.InventorizePowerup(note.PowerupType); // TODO: Revise - should this be from the note or the measure instead? Theoretically, the last picked up note would make the most sense.

                    // Capture track if last note was captured IN A SEQUENCE
                    if (note == note.Track.Sequences.Last().Notes.Last())
                        note.Track.CaptureMeasureAmount(Clock.Fbar, RhythmicGame.TrackCaptureLength);

                    else // Increment to next note, disable current measures
                    {
                        TracksController.IncrementTargetNote(note.Track);
                        TracksController.DisableCurrentMeasures(false, note.MeasureID);
                    }

                    break;
                }
            case CatchResultType.Empty:
            case CatchResultType.Ignore:
            case CatchResultType.Miss:
                {
                    if (result.note) result.note.NoteMeshRenderer.material.color = Color.red; // TODO: note fail state visuals

                    // TODO: This might be needed in the future, in case we have a long slop value.
                    //if (!(Clock.bar == result.note.MeasureID + 1 & (int)Clock.beat % 8 == 0))
                    TracksController.DisableCurrentMeasures(true);
                    TracksController.RefreshAll();

                    break;
                }
        }
    }

    public void TriggerCatcher(int id) => TriggerCatcher((CatcherSide)id);
    public void TriggerCatcher(CatcherSide side)
    {
        if (RhythmicGame.DebugCatchResultEvents)
            Debug.Log($"Catching: Calling catcher: {side} ({(int)side})...");

        CatchResult result = Catchers[(int)side].Catch();
        HandleResult(result);

        if (RhythmicGame.DebugCatchResultEvents)
            DebugPrintResult(result);
    }

    public static void DebugPrintResult(CatchResult result)
    {
        string noteName = (result.note) ? result.note.name : "null";
        string catcherString = result.catcher ?
            $"Catcher: {result.catcher.Name} [{result.catcher.ID}] | " :
            "Catcher: null | ";

        Debug.Log(catcherString + $"Result: {result.resultType}, Note: {noteName}");
    }
}