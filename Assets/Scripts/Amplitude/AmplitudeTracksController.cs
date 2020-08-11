using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AmplitudeTracksController : TracksController
{
    AmplitudeSongController amp_ctrl { get { return AmplitudeSongController.Instance; } }

    public List<string> songTracks { get { return amp_ctrl.songTracks; } } // list of tracks in string form - for initialization use only!

    // AMP TRACKS CREATION

    UnityEngine.Object trackPrefab;
    public async void CreateTracks()
    {
        if (trackPrefab == null) // Load prefab
            trackPrefab = Resources.Load("Prefabs/Track");

        float xPos = 2.24f + (0.06f * 2); // TODO: get track width in some other way?
        float lastX = 0f;
        int counter = 0;

        var watch = System.Diagnostics.Stopwatch.StartNew();
        foreach (string track in songTracks)
        {
            if ((track == "freestyle" & !RhythmicGame.PlayableFreestyleTracks) || track == "bg_click")
                continue;

            if (counter != 0) lastX += xPos; // if this is the 0th track, leave position at 0
            var position = counter == 0 ? new Vector3(0, 0, 0) : new Vector3(lastX, 0, 0); // if this isn't 0th track, add the width of a track + edge lights!
            var scale = new Vector3(1, 1, amp_ctrl.songLengthInMeasures * amp_ctrl.measureLengthInzPos); // scale the track to the song duration by measures

            // Create and position the track GameObject
            GameObject trackObject = (GameObject)GameObject.Instantiate(trackPrefab, position, new Quaternion(0, 0, 0, 0));
            trackObject.name = track;
            trackObject.transform.localScale = scale;
            trackObject.transform.parent = gameObject.transform;

            // create and assign AmplitudeTrack script
            var ampTrack = trackObject.AddComponent<AmplitudeTrack>();
            ampTrack.ID = counter;
            ampTrack.trackName = track;
            ampTrack.Instrument = AmplitudeTrack.TrackTypeFromString(track);
            ampTrack.EdgeLightsColor = Track.Colors.ColorFromTrackType(ampTrack.Instrument.Value);

            ampTrack.MeasureCaptureFinished += AmpTrack_MeasureCaptureFinished;

            // add this track to the list of tracks
            Tracks.Add(ampTrack);

            // Populate the notes on the track
            ampTrack.AMP_PopulateNotes();
            await Task.Delay(1); // Fake async for track note population

            counter++;

            // Update loading text
            // TODO: optimize
            if (loadingText != null)
                loadingText.GetComponent<TextMeshProUGUI>().text = string.Format("Charting song - {0}% done...", (((float)counter / (float)Tracks.Count) * 100f).ToString("0"));
        }

        if (loadingText != null)
            loadingText.GetComponent<TextMeshProUGUI>().text = "Charting song...";

        foreach (Track track in Tracks)
        {
            // Create the measures in the track
            track.CreateMeasures(amp_ctrl.songMeasures);
            /*
            if (!Application.isEditor)
                await Task.Delay(1); // Fake async for measure creation
            */
        }

        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Debug.LogFormat("AMP_TRACKS: Note chart creation took {0}ms", elapsedMs);

        // Unload loading scene
        // TODO: move to a better place / optimize!
        if (SceneManager.GetSceneByName("Loading").isLoaded)
            SceneManager.UnloadSceneAsync("Loading");

        while (Tracks[0].trackMeasures.Count < 3)
            await Task.Delay(500);

        // Get closest notes
        // TODO: do this somewhere else during init!
        CatcherController.Instance.FindNextMeasuresNotes();

        RhythmicGame.IsLoading = false;
    }
}