using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracksController : MonoBehaviour
{
    public List<Track> trackList = new List<Track>();

    /// <summary>
    /// The current active track.
    /// </summary>
    public Track ActiveTrack;

    void Start()
    {
        Debug.LogFormat("TRACKS: Using RHYTMHIC track controller!");

        if (trackList.Count == 0) // if tracks aren't populated yet
            PopulateTracks();
    }

    public Track.TrackType DefaultTrackType;
    /// <summary>
    /// Returns the Track component of the default track.
    /// </summary>
    /// <returns>GameObject</returns>
    public Track GetDefaultTrack()
    {
        Track finalTrackComponent = null;

        foreach (var track in trackList)
        {
            if (track.Instrument == DefaultTrackType)
            {
                finalTrackComponent = track;
                break;
            }
        }

        if (finalTrackComponent != null)
            return finalTrackComponent;
        else
            throw new Exception("TRACKS: Can't find the default track's Track component!");
    }
    /// <summary>
    /// Returns the entire GameObject for the default track.
    /// </summary>
    /// <returns>GameObject</returns>
    public GameObject GetDefaultTrackGameObject()
    {
        return GetDefaultTrack().gameObject;
    }

    /// <summary>
    /// Create lanes and populate them with CATCH notes.
    /// </summary>
    public void PopulateTracks()
    {
        if (transform.childCount == 0) // if there are no pre-determined tracks in the Scene
        {
            // TODO: Create tracks!
        }
        else // if we have pre-determined tracks in the Scene
        {
            foreach (Transform child in transform)
            {
                if (child.tag == "Track")
                {
                    Track track = child.GetComponent<Track>();
                    trackList.Add(track);
                }
                else
                    Debug.LogWarning(string.Format("TRACKS: Object {0} is not of tag Track.", child.gameObject.name));
            }

            Debug.LogFormat("TRACKS: Found tracks: {0} (predetermined from Scene!)", trackList.Count);
        }
    }
}
