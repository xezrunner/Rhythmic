using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CatchResultType { UNKNOWN = 0, Success = 1, Empty = 2, Ignore = 3, Miss = 4, Error = 5 }
public struct CatchResult
{
    public CatchResult(Catcher c, CatchResultType t, AmpNote n)
    {
        resultType = t;
        catcher = c;
        note = n;
    }

    public CatchResultType resultType;
    public Catcher catcher;
    public AmpNote note;
}

public class AmpPlayerCatching : MonoBehaviour
{
    public static AmpPlayerCatching Instance;

    Clock Clock { get { return Clock.Instance; } }
    SongController SongController { get { return SongController.Instance; } }
    TracksController TracksController { get { return TracksController.Instance; } }
    public AmpPlayer Player;
    public AmpPlayerLocomotion Locomotion;

    public Transform CatcherContainer; // This will automatically populate the Catchers list!
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
        // Assign ourselves in TracksController
        TracksController.Catching = this;

        // Set up notesToCatch array
        TracksController.targetNotes = new AmpNote[TracksController.Tracks.Count];
    }

    int lastIgnoreBar = -1;

    private void LateUpdate()
    {
        if (!SongController.IsPlaying) return;

        // TEMP
        foreach (AmpTrack t in TracksController.Tracks)
        {
            if (t.IsTrackCaptured)
            {
                SongController.audioSrcList[t.ID].volume = t.IsTrackFocused ? 1f : 0.4f;
                continue;
            }
            else if (t.IsTrackBeingPlayed) SongController.audioSrcList[t.ID].volume = 1f;
            else SongController.audioSrcList[t.ID].volume = 0f;
        }

        if (lastIgnoreBar == Clock.Fbar) return; // avoid spamming

        // *** SLOP ***
        float dist = Locomotion.DistanceTravelled;

        var currentMeasure = TracksController.CurrentMeasure;
        if (!currentMeasure.IsEmpty & !currentMeasure.IsCaptured)
        {
            AmpNote note = TracksController.targetNotes[TracksController.CurrentTrackID];
            HandleSlop(dist, note);
        }
        else
            foreach (AmpNote note in TracksController.targetNotes)
                HandleSlop(dist, note);
    }

    public void HandleSlop(float dist, AmpNote note)
    {
        if (!note) { Debug.LogWarning($"Catching/HandleSlop(): No note was passed!"); HandleResult(new CatchResult()); lastIgnoreBar = Clock.Fbar; return; }

        // If the distance is bigger than the note distance + slop distance, we have 'ignored' the note.
        if (dist > note.Distance + SongController.SlopPos)
        {
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
                    AmpNote note = result.note;

                    note.CaptureNote();
                    note.Track.IsTrackBeingPlayed = true;

                    if (note.IsLastNote & note.MeasureID == note.Track.Sequences.Last().ID)
                    {
                        note.Track.CaptureMeasureAmount(Clock.Fbar, RhythmicGame.TrackCaptureLength);
                        TracksController.lastRefreshUpcomingState = false; // TODO: do this in a better place?
                    }
                    else
                    {
                        // Disable other measures
                        TracksController.DisableCurrentMeasures();
                        TracksController.RefreshTargetNotes(TracksController.CurrentTrack);
                    }

                    break;
                }
            case CatchResultType.Empty:
            case CatchResultType.Ignore:
            case CatchResultType.Miss:
                {
                    TracksController.CurrentMeasure.IsEnabled = false; // TODO: unify with below?
                    TracksController.DisableCurrentMeasures(true);

                    if (result.note) result.note.NoteMeshRenderer.material.color = Color.red;

                    TracksController.RefreshSequences();
                    TracksController.RefreshTargetNotes();

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