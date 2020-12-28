using NUnit.Framework;
using System;
using System.Collections;
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

    public void HandleResult(CatchResult result)
    {
        switch (result.resultType)
        {
            default: { Debug.Log($"Catching/TriggerCatcher() [handling]: Catch result type was {result.resultType} for catcher {result.catcher.Name}"); break; }

            case CatchResultType.Success:
                {
                    AmpNote note = result.note;

                    note.CaptureNote();

                    if (note.IsLastNote & note.MeasureID == note.Track.Sequences.Last().ID)
                    {
                        note.Track.CaptureMeasureAmount(Clock.Fbar, RhythmicGame.TrackCaptureLength);
                        TracksController.lastRefreshUpcomingState = false; // TODO: do this in a better place?
                    }
                    else
                    {
                        // Disable other measures
                        foreach (AmpTrack t in TracksController.Tracks)
                        {
                            if (t == TracksController.CurrentTrack) continue;

                            t.Measures[Clock.Fbar].IsEnabled = false;
                        }

                        TracksController.RefreshTargetNotes(TracksController.CurrentTrack);
                    }

                    break;
                }
            case CatchResultType.Empty:
            case CatchResultType.Ignore:
            case CatchResultType.Miss:
                {
                    TracksController.CurrentTrack.Measures[Clock.Fbar].IsEnabled = false;

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
        if (result.catcher == null) { Debug.LogError("AmpPlayerCatching/DebugPrintResult(): Catcher was null!"); return; }

        string noteName = (result.note) ? result.note.name : "null";

        Debug.Log($"Catcher: {result.catcher.Name} [{result.catcher.ID}] | " +
            $"Result: {result.resultType}, Note: {noteName}");
    }
}