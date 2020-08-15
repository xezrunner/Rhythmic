using System;
using System.Collections.Generic;
using UnityEngine;

public class TracksController : MonoBehaviour
{
    public static TracksController Instance;
    public GameObject loadingText;

    public CatcherController CatcherController { get { return CatcherController.Instance; } }
    public PlayerController Player { get { return PlayerController.Instance; } }

    public int StartTrackID = 0; // start track ID
    public int CurrentTrackID = 0; // the track that the player is currently on
    public Track CurrentTrack { get { return GetTrackByID(CurrentTrackID); } }

    public List<Track> Tracks = new List<Track>();

    public void Awake()
    {
        Instance = this;
        loadingText = GameObject.Find("loadingText");

        Player.OnTrackSwitched += Player_OnTrackSwitched;

        gameObject.layer = 11; // put TracksController onto Tracks layer
    }

    // Tracks
    /// <summary>
    /// This returns a Track object based on its ID.
    /// </summary>
    /// <param name="id">The ID of the wanted track</param>
    public Track GetTrackByID(int id)
    {
        if (Tracks.Count == 0) // if the track list is empty, don't continue
            throw new Exception("TracksController/GetTrackByID(): track list is empty!");

        /*
        Track finalTrack = null;
        // find Track by ID
        foreach (Track track in trackList)
        {
            if (track.ID.Value == id)
                return track;
        }
        */

        /*
        if (finalTrack == null) // if we didn't find the track of this ID
            throw new Exception(string.Format("TracksController/GetTrackByID(): could not find track ID {0}", id));
        return null;
        */

        try
        {
            return Tracks[id];
        }
        catch
        {
            throw new Exception(string.Format("TRACKSCONTROLLER: Cannot find track {0}", id));
        }
    }

    public void DisableOtherMeasures()
    {
        foreach (Track track in Tracks)
            if (track != CurrentTrack)
            {
                track.GetMeasureForID(CatcherController.CurrentMeasureID).IsMeasureEnabled = false;
                track.IsTrackBeingCaptured = false;
            }
    }
    public void DisableCurrentMeasures()
    {
        foreach (Track track in Tracks)
            track.GetMeasureForID(CatcherController.CurrentMeasureID).IsMeasureEnabled = false;

        SetAllTracksCapturingState(false);
    }
    public void SetCurrentMeasuresNotesToBeCaptured()
    {
        foreach (Track track in Tracks)
            if (track == CurrentTrack)
                track.GetMeasureForID(CatcherController.CurrentMeasureID).SetMeasureNotesToBeCaptured();
            else
                track.GetMeasureForID(CatcherController.CurrentMeasureID).IsMeasureEnabled = false;
    }
    public void SetAllTracksCapturingState(bool value)
    {
        foreach (Track track in Tracks)
            track.IsTrackBeingCaptured = value;
    }

    public event EventHandler<int> OnTrackSwitched;
    private void Player_OnTrackSwitched(object sender, int e)
    {
        // change props of tracks
        foreach (Track track in Tracks)
            track.IsTrackFocused = Tracks.IndexOf(track) == e;

        CurrentTrackID = e;

        CatcherController.FindNextMeasuresNotes();

        OnTrackSwitched?.Invoke(null, e);
    }

    // Measures
    /// <summary>
    /// Gets the measure that's directly under the player
    /// </summary>
    //public Measure CurrentMeasure { get { return CurrentTrack.GetMeasureForZPos(Player.transform.position.z); } }
    public Measure CurrentMeasure { get { return CurrentTrack.GetMeasureForID(CatcherController.CurrentMeasureID); } }
    public bool GetIsCurrentMeasureEmpty { get { return CurrentTrack.trackMeasures[CatcherController.CurrentMeasureID].IsMeasureEmpty; } }

    public event EventHandler<int[]> OnTrackCaptureStart;
    public event EventHandler<int[]> OnTrackCaptured;

    public void TracksController_OnTrackCaptureStart(object sender, int[] e)
    {
        OnTrackCaptureStart?.Invoke(sender, e);
    }

    public void TracksController_OnTrackCaptured(object sender, int[] e)
    {
        OnTrackCaptured?.Invoke(sender, e);
    }

    public event EventHandler<int> MeasureCaptureFinished;
    public void AmpTrack_MeasureCaptureFinished(object sender, int e)
    {
        MeasureCaptureFinished?.Invoke(sender, e);
    }
}