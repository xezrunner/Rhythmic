using System;
using System.Collections.Generic;
using UnityEngine;

public class TracksController : MonoBehaviour
{
    public static TracksController Instance;
    public CatcherController CatcherController { get { return CatcherController.Instance; } }
    public PlayerController Player { get { return PlayerController.Instance; } }

    public int StartTrackID = 0; // start track ID
    public int CurrentTrackID = 0; // the track that the player is currently on
    public Track CurrentTrack { get { return GetTrackByID(CurrentTrackID); } }

    public List<Track> Tracks = new List<Track>();

    public void Awake()
    {
        Instance = this;
        Player.OnTrackSwitched += Player_OnTrackSwitched;
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

    public void DisableCurrentMeasures()
    {
        foreach (Track track in Tracks)
            track.GetMeasureForID(CatcherController.CurrentMeasureID).IsMeasureCapturable = false;
    }

    public void SetCurrentMeasuresNotesToBeCaptured()
    {
        foreach (Track track in Tracks)
            if (track == CurrentTrack)
                track.GetMeasureForID(CatcherController.CurrentMeasureID).SetMeasureNotesToBeCaptured();
            else
                track.GetMeasureForID(CatcherController.CurrentMeasureID).SetMeasureNotesActive(false);
    }

    public event EventHandler<int> OnTrackSwitched;

    private void Player_OnTrackSwitched(object sender, int e)
    {
        // change props of tracks
        Track prevTrack = GetTrackByID(CurrentTrackID);
        Track nextTrack = GetTrackByID(e);

        prevTrack.IsTrackFocused = false;
        nextTrack.IsTrackFocused = true;

        CurrentTrackID = e;

        OnTrackSwitched?.Invoke(null, e);
    }

    // Measures
    /// <summary>
    /// Gets the measure that's directly under the player
    /// </summary>
    public Measure CurrentMeasure { get { return CurrentTrack.GetMeasureForZPos(Player.transform.position.z); } }
    public bool GetIsCurrentMeasureActive { get { return CurrentTrack.GetIsMeasureActiveForZPos(Player.transform.position.z); } }

}