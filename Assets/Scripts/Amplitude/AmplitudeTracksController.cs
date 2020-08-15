using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AmplitudeTracksController : TracksController
{
    AmplitudeSongController amp_ctrl { get { return AmplitudeSongController.Instance; } }

    public List<string> songTracks { get { return amp_ctrl.songTracks; } } // list of tracks in string form - for initialization use only!

    public float rotZ = 0f;

    // AMP TRACKS CREATION
    UnityEngine.Object trackPrefab;
    public async void CreateTracks()
    {
        if (trackPrefab == null) // Load prefab
            trackPrefab = Resources.Load("Prefabs/Track");

        // preapre playable tracks list
        List<string> finalSongTracks = songTracks.ToList();

        if (RhythmicGame.IsTunnelMode & RhythmicGame.TunnelTrackDuplication)
        {
            for (int i = 1; i < RhythmicGame.TunnelTrackDuplicationCount; i++)
                foreach (string dupT in songTracks)
                    finalSongTracks.Add(dupT);
        }

        List<string> tempList = finalSongTracks.ToList();
        foreach (string track in tempList)
        {
            if ((track == "freestyle" & !RhythmicGame.PlayableFreestyleTracks) || track == "bg_click")
                finalSongTracks.Remove(track);
        }

        //float xPos = 2.24f + (0.06f * 2); // TODO: get track width in some other way?
        float lastX = 0f;
        float lastY = 0f;

        bool isTunnel = RhythmicGame.IsTunnelMode;
        int tunnelDuplicationCount = RhythmicGame.TunnelTrackDuplication ? RhythmicGame.TunnelTrackDuplicationCount : 1;
        rotZ = (360 * tunnelDuplicationCount) / finalSongTracks.Count;
        float lastRotZ = 0f;
        bool lastRotZReached180 = false;

        var watch = System.Diagnostics.Stopwatch.StartNew();
        int counter = 0;
        foreach (string track in finalSongTracks)
        {
            var position = new Vector3(lastX, lastY, 0); // add the width of a track + edge lights!
            var rotation = new Vector3(0, 0, lastRotZ);
            var scale = new Vector3(1, 1, amp_ctrl.songLengthInMeasures * amp_ctrl.measureLengthInzPos); // scale the track to the song duration by measures

            // Create and position the track GameObject
            GameObject trackObject = (GameObject)GameObject.Instantiate(trackPrefab);
            trackObject.name = track;
            trackObject.transform.localEulerAngles = rotation;
            trackObject.transform.position = position;
            trackObject.transform.localScale = scale;
            trackObject.transform.parent = gameObject.transform;

            // create and assign AmplitudeTrack script
            var ampTrack = trackObject.AddComponent<AmplitudeTrack>();
            ampTrack.ID = counter;
            ampTrack.trackName = track;
            ampTrack.Instrument = AmplitudeTrack.TrackTypeFromString(track);
            ampTrack.EdgeLightsColor = Track.Colors.ColorFromTrackType(ampTrack.Instrument.Value);
            ampTrack.zRot = lastRotZ;

            ampTrack.OnTrackCaptureStart += TracksController_OnTrackCaptureStart;
            ampTrack.OnTrackCaptured += TracksController_OnTrackCaptured;
            //ampTrack.MeasureCaptureFinished += AmpTrack_MeasureCaptureFinished;

            // add this track to the list of tracks
            Tracks.Add(ampTrack);

            if (RhythmicGame.IsTunnelMode & RhythmicGame.TunnelTrackDuplication)
            {
                if (finalSongTracks[counter + 1] == finalSongTracks[0])
                    counter = 0;
                else
                    counter++;
            }
            else
                counter++;
            // increase pos / rot props
            if (isTunnel)
            {
                if (lastRotZ + rotZ == (360 * tunnelDuplicationCount) / 2)
                {
                    lastRotZ += rotZ;
                    lastY += 1 + ((RhythmicGame.TrackWidth / 2f) / rotZ);
                    lastX = 0f;
                    lastRotZReached180 = true;
                }
                else if (!lastRotZReached180)
                {
                    lastRotZ += rotZ;
                    lastY += (lastRotZ / rotZ) + ((RhythmicGame.TrackWidth / 2f) / rotZ);
                    lastX = RhythmicGame.TrackWidth - (rotZ / 100f) + (1f / rotZ);
                }
                else if (lastRotZReached180)
                {
                    lastRotZ += rotZ;
                    lastY -= (lastRotZ - 180) / rotZ + ((RhythmicGame.TrackWidth / 2f) / rotZ);
                    lastX = -RhythmicGame.TrackWidth + (rotZ / 100f) - (1f / rotZ);
                }
            }
            else
                lastX += RhythmicGame.TrackWidth;

            // Populate the notes on the track
            ampTrack.AMP_PopulateNotes();
            await Task.Delay(1); // Fake async for track note population

            // Update loading text
            // TODO: optimize
            if (loadingText != null)
                loadingText.GetComponent<TextMeshProUGUI>().text = string.Format("Charting song - {0}% done...", (((float)counter / (float)Tracks.Count) * 100f).ToString("0"));
        }

        if (loadingText != null)
            loadingText.GetComponent<TextMeshProUGUI>().text = "Loading...";

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

        while (Tracks[Tracks.Count - 1].trackMeasures.Count < AmplitudeSongController.Instance.songMeasures.Count - 4)
            await Task.Delay(500);

        // Get closest notes
        // TODO: do this somewhere else during init!
        CatcherController.Instance.FindNextMeasuresNotes();

        // Unload loading scene
        // TODO: move to a better place / optimize!
        if (SceneManager.GetSceneByName("Loading").isLoaded)
            SceneManager.UnloadSceneAsync("Loading");

        RhythmicGame.IsLoading = false;
    }
}