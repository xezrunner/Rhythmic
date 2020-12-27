using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CatcherSide { Left = 0, Center = 1, Right = 2 }
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
    public AmpNote? note;
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

    // This is an array of upcoming notes that the player is supposed to catch
    public AmpNote[] targetNotes;

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
        targetNotes = new AmpNote[TracksController.Tracks.Count];
    }

    bool lastRefreshUpcomingState; // This is set to true when we've already found the upcoming notes for the other tracks.

    /// <summary>
    /// Finds the next notes that the player is supposed to catch. <br/>
    /// In case we're already playing a track, this will only update the current track's upcoming notes.
    /// </summary>
    /// <param name="track">Giving a track will skip forward a sequence amount of measures for the other tracks.</param>
    public void RefreshTargetNotes(AmpTrack track = null)
    {
        //Debug.Break();
        Debug.DebugBreak();

        // Find the current track's next note
        if (track)
        {
            AmpNote note = null;
            foreach (AmpTrackSection m in track.Sequences)
            {
                if (note) break;
                foreach (AmpNote n in m.Notes)
                    if (!n.IsCaptured) { note = n; break; }
            }

            if (!note)
                Debug.LogError($"Catching/RefreshTargetNotes({track.ID}): couldn't find any upcoming notes!");

            targetNotes[track.ID] = note;
            note.NoteMeshRenderer.material.color = Color.green;

            if (lastRefreshUpcomingState) return;
        }

        // Find the next notes in the other tracks (or in all tracks if a track is unspecified)
        foreach (AmpTrack t in TracksController.Tracks)
        {
            if (track && t.ID == track.ID) continue; // If a track was specified, ignore it.

            for (int i = Clock.Fbar; i < SongController.songLengthInMeasures; i++)
            {
                if (track) i += RhythmicGame.SequenceAmount; // If a track was specified, we want to skip ahead a sequence.

                AmpTrackSection m = t.Measures[i];
                if (m.IsEmpty || m.IsCaptured) continue;

                // Grab first note of measure
                AmpNote note = m.Notes[0];
                note.NoteMeshRenderer.material.color = Color.green;

                // Set target note
                // TODO: obstacle/error notes?
                targetNotes[t.ID] = note;
                break;
            }
        }

        // Do not refresh again after we've found these notes once.
        if (track) lastRefreshUpcomingState = true;
        else lastRefreshUpcomingState = false;
    }

    public void TriggerCatcher(int id) => TriggerCatcher((CatcherSide)id);
    public void TriggerCatcher(CatcherSide side)
    {
        if (RhythmicGame.DebugCatchResultEvents)
            Debug.Log($"Catching: Calling catcher: {side} ({(int)side})...");

        CatchResult result = Catchers[(int)side].Catch();

        switch (result.resultType)
        {
            default: { Debug.Log($"Catching/TriggerCatcher() [handling]: Catch result type was {result.resultType} for catcher {result.catcher.Name}"); break; }

            case CatchResultType.Success: { result.note.CaptureNote(); RefreshTargetNotes(TracksController.CurrentTrack); break; }
            case CatchResultType.Empty:
            case CatchResultType.Ignore:
            case CatchResultType.Miss: { RefreshTargetNotes(); break; }
        }

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