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

    bool drawDebugGizmos = false;
    bool isDebugGizmosDone = false;
    private void OnDrawGizmos()
    {
        if (!drawDebugGizmos)
            return;

        float outline = 6 * RhythmicGame.TrackWidth;
        float radius = -outline / (2f * Mathf.PI) + 0.25f; // negative outline for upwards circle
        float diameter = radius * 2f;

        Vector3 circle_center = new Vector3(0, -radius, 64);

        Gizmos.DrawWireSphere(circle_center, radius);

        if (isDebugGizmosDone)
            return;

        for (int i = 0; i < 6; i++)
        {
            bool isTunnel = RhythmicGame.IsTunnelMode;
            int tunnelDuplicationCount = RhythmicGame.TunnelTrackDuplication ? RhythmicGame.TunnelTrackDuplicationNum : 1;
            rotZ = 360 / 6;

            Vector3 lastTrackPos = Vector3.zero;
            Vector3 lastPos = !isTunnel ? Vector3.zero : new Vector3(-RhythmicGame.TrackWidth / 2, 0, 0);
            Vector3 lastRot = new Vector3(0, 0, i * -60);

            float posX = radius * Mathf.Sin(lastRot.z * Mathf.Deg2Rad) + circle_center.x;
            float posY = radius * Mathf.Cos(lastRot.z * Mathf.Deg2Rad) + circle_center.y;

            /*
            Gizmos.DrawSphere(circle_center, radius);
            Gizmos.DrawCube(new Vector3(posX, posY, 64), new Vector3(1, 1, 1));
            */

            var go_pos = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go_pos.transform.position = new Vector3(posX, posY, 64);
            go_pos.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        }

        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = new Vector3(circle_center.x, circle_center.y, 64);
        go.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

        isDebugGizmosDone = true;
    }

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
            for (int i = 1; i < RhythmicGame.TunnelTrackDuplicationNum; i++)
                foreach (string dupT in songTracks)
                    finalSongTracks.Add(dupT);
        }

        List<string> tempList = finalSongTracks.ToList();
        foreach (string track in tempList)
        {
            if ((track == "freestyle" & !RhythmicGame.PlayableFreestyleTracks) || track == "bg_click")
                finalSongTracks.Remove(track);
        }

        bool isTunnel = RhythmicGame.IsTunnelMode;
        int tunnelDuplicationCount = RhythmicGame.TunnelTrackDuplication ? RhythmicGame.TunnelTrackDuplicationNum : 1;
        rotZ = 360 / finalSongTracks.Count;

        Vector3 lastTrackPos = Vector3.zero;
        Vector3 lastPos = !isTunnel ? Vector3.zero : new Vector3(-RhythmicGame.TrackWidth / 2, 0, 0);
        Vector3 lastRot = new Vector3(0, 0, 0);

        float outline = finalSongTracks.Count * RhythmicGame.TrackWidth;
        float radius = -outline / (2f * Mathf.PI) + 0.205f; // negative outline for upwards circle
        float diameter = radius * 2f;

        Vector2 circle_center = new Vector2(0, -radius);

        var watch = System.Diagnostics.Stopwatch.StartNew();
        int duplicationCounter = 0;
        int realcounter = 0;
        foreach (string track in finalSongTracks)
        {
            //var position = new Vector3(lastX, lastY, 0); // add the width of a track + edge lights!
            var scale = new Vector3(1, 1, amp_ctrl.songLengthInMeasures * amp_ctrl.measureLengthInzPos); // scale the track to the song duration by measures

            // Create and position the track GameObject
            GameObject trackObject = (GameObject)GameObject.Instantiate(trackPrefab);
            trackObject.name = track;
            trackObject.transform.localScale = scale;
            if (!isTunnel)
            {
                trackObject.transform.localPosition = lastTrackPos;
                trackObject.transform.parent = gameObject.transform;
            }
            else
            {
                // Get the pos coords for the rotation angle of the track
                float posX = radius * Mathf.Sin(lastRot.z * Mathf.Deg2Rad) + circle_center.x;
                float posY = radius * Mathf.Cos(lastRot.z * Mathf.Deg2Rad) + circle_center.y;

                trackObject.transform.localPosition = new Vector3(posX, posY, 0); // pos from angle calc
                trackObject.transform.localEulerAngles = -lastRot; // counter the counter-clockwise effects here
                trackObject.transform.parent = gameObject.transform;
            }

            // create and assign AmplitudeTrack script
            var ampTrack = trackObject.AddComponent<AmplitudeTrack>();
            ampTrack.ID = duplicationCounter;
            ampTrack.RealID = realcounter;
            ampTrack.trackName = track;
            ampTrack.Instrument = AmplitudeTrack.TrackTypeFromString(track);
            ampTrack.EdgeLightsColor = Track.Colors.ColorFromTrackType(ampTrack.Instrument.Value);
            ampTrack.zRot = -lastRot.z;

            ampTrack.OnTrackCaptureStart += TracksController_OnTrackCaptureStart;
            ampTrack.OnTrackCaptured += TracksController_OnTrackCaptured;

            // add this track to the list of tracks
            Tracks.Add(ampTrack);

            // TRACK DUPLICATION
            if (RhythmicGame.IsTunnelMode & RhythmicGame.TunnelTrackDuplication)
            {
                if (finalSongTracks[duplicationCounter + 1] == finalSongTracks[0])
                    duplicationCounter = 0;
                else
                    duplicationCounter++;
            }
            else { duplicationCounter++; }
            realcounter++;

            // increase pos / rot props
            if (isTunnel)
            {
                lastPos = new Vector3(lastPos.x + RhythmicGame.TrackWidth, 0, 0);
                lastRot.z += -rotZ; // go counter-clockwise for tunnel creation (pos/angle only)
            }
            lastTrackPos = new Vector3(lastTrackPos.x + RhythmicGame.TrackWidth, 0, 0);

            // Populate the notes on the track
            ampTrack.AMP_PopulateNotes();
            await Task.Delay(1); // Fake async for track note population

            // Update loading text
            // TODO: optimize
            if (loadingText != null)
                loadingText.GetComponent<TextMeshProUGUI>().text = string.Format("Charting song - {0}% done...", (((float)realcounter / (float)Tracks.Count) * 100f).ToString("0"));
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