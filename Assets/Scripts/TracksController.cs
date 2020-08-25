using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TracksController : MonoBehaviour
{
    public static TracksController Instance;
    public TextMeshProUGUI loadingText;

    public AmplitudeSongController SongController { get { return AmplitudeSongController.Instance; } }
    public CatcherController CatcherController { get { return CatcherController.Instance; } }
    public PlayerController Player { get { return PlayerController.Instance; } }

    public int StartTrackID = 0; // start track ID
    public int CurrentTrackID = 0; // the track that the player is currently on
    public Track CurrentTrack { get { return GetTrackByID(CurrentTrackID); } }

    float songLength;

    public List<string> songTracks;
    public List<Track> Tracks = new List<Track>();

    public int CurrentTrackSetID = -1;
    public List<Track> CurrentTrackSet = new List<Track>();
    List<List<Track>> TrackSets = new List<List<Track>>();

    public Tunnel Tunnel;

    public GameObject trackPrefab;
    public GameObject notePrefab;
    public GameObject measurePrefab;
    public GameObject subbeatPrefab;
    public void Awake()
    {
        Instance = this;
        try { loadingText = GameObject.Find("loadingText").GetComponent<TextMeshProUGUI>(); }
        catch { Debug.LogWarning("TRACKS: loading text no found - won't report progress stats to GameStarter"); }

        trackPrefab = (GameObject)Resources.Load("Prefabs/Track");
        notePrefab = (GameObject)Resources.Load("Prefabs/Note");
        measurePrefab = (GameObject)Resources.Load("Prefabs/Measure");
        subbeatPrefab = (GameObject)Resources.Load("Prefabs/MeasureSubbeat");

        songLength = SongController.songLengthInMeasures * SongController.measureLengthInzPos; // songLengthInzPos?
        songTracks = SongController.songTracks.ToList(); // copy

        Player.OnTrackSwitched += Player_OnTrackSwitched;

        gameObject.layer = 11; // put TracksController onto Tracks layer
    }
    public void Start()
    {
        List<string> tracks = songTracks.ToList();
        songTracks.Clear();
        foreach (string tr in tracks)
        {
            Track.InstrumentType inst = Track.InstrumentFromString(tr);
            if (inst == Track.InstrumentType.FREESTYLE & !RhythmicGame.PlayableFreestyleTracks)
                continue;
            else if (inst == Track.InstrumentType.bg_click)
                continue;
            else
                songTracks.Add(tr);
        }

        Tunnel = gameObject.AddComponent<Tunnel>();
        Tunnel.Init(songTracks.Count * RhythmicGame.TunnelTrackDuplicationNum);

        CreateTracks();
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
            if (track.ID == id)
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

    public async void CreateTracks()
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        int realID = 0;
        for (int i = 0; i < RhythmicGame.TunnelTrackDuplicationNum; i++)
        {
            TrackSets.Add(new List<Track>());

            // Create trackcs
            int id = 0;
            foreach (string trackName in songTracks)
            {
                Track track = await CreateTrack(id, trackName, realID);

                // Update loading screen progress
                if (loadingText != null)
                {
                    float progress = (realID / (float)(songTracks.Count * (float)RhythmicGame.TunnelTrackDuplicationNum) * 100f);
                    loadingText.text = string.Format("Charting song - {0}%...", progress.ToString("0"));
                }

                // Add to track sets
                track.SetID = i;
                TrackSets[i].Add(track);

                id++; realID++;
            }
        }

        // Create identical tracks list
        List<List<Track>> identicalList = new List<List<Track>>();
        songTracks.ForEach(s => identicalList.Add(new List<Track>()));

        // Build & assign identical tracks list
        Tracks.ForEach(t => identicalList[t.ID].Add(t));
        Tracks.ForEach(t => t.identicalTracks = identicalList[t.ID]);

        watch.Stop(); Debug.LogFormat("TRACKS: Note chart creation took {0}ms", watch.ElapsedMilliseconds);
        while (Tracks.Last().trackMeasures.Count < AmplitudeSongController.Instance.songMeasures.Count)
        {
            if (loadingText != null)
            {
                float progress = ((float)Tracks.Last().trackMeasures.Count / (float)AmplitudeSongController.Instance.songMeasures.Count) * 100f;
                loadingText.text = string.Format("Genereating measures - {0}%...", progress.ToString("0"));
            }
            await Task.Delay(500);
        }

        // Get closest notes
        // TODO: do this somewhere else during init!
        CatcherController.Instance.FindNextMeasuresNotes();

        // Unload loading scene
        // TODO: move to a better place / optimize!
        if (SceneManager.GetSceneByName("Loading").isLoaded)
            SceneManager.UnloadSceneAsync("Loading");

        Player.StartCamera.SetActive(false);

        RhythmicGame.IsLoading = false;
    }
    public async Task<Track> CreateTrack(int id, string name, int? realID = null)
    {
        // Create GameObject for track
        var obj = Instantiate(trackPrefab);
        obj.name = name;

        Vector3[] transform = Tunnel.GetTransformForTrackID(realID.HasValue ? realID.Value : id);
        obj.transform.position = transform[0];
        obj.transform.eulerAngles = transform[1];
        obj.transform.localScale = new Vector3(1, 1, songLength);
        obj.transform.SetParent(gameObject.transform); // parent to TracksController

        // Create script for track
        Track track = null;
        if (RhythmicGame.GameType == RhythmicGame._GameType.AMPLITUDE)
            track = obj.AddComponent<AmplitudeTrack>();
        else if (RhythmicGame.GameType == RhythmicGame._GameType.RHYTHMIC)
            track = obj.AddComponent<Track>();

        track.ID = id;
        track.RealID = realID.HasValue ? realID.Value : id;
        track.trackName = name;
        track.Instrument = Track.InstrumentFromString(name);

        track.OnTrackCaptureStart += TracksController_OnTrackCaptureStart;
        track.OnTrackCaptured += TracksController_OnTrackCaptured;

        Tracks.Add(track);

        await Task.Delay(1);

        return track;
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
    private void Player_OnTrackSwitched(object sender, Track e)
    {
        // change props of tracks
        foreach (Track track in Tracks)
            track.IsTrackFocused = Tracks.IndexOf(track) == e.RealID;

        CurrentTrackID = e.RealID;

        //CatcherController.FindNextMeasuresNotes();

        // switch active set
        if (CurrentTrackSetID != e.RealID)
            SetCurrentTrackSet(e.SetID);

        OnTrackSwitched?.Invoke(null, e.RealID);
    }

    public void SetCurrentTrackSet(int setID)
    {
        CurrentTrackSetID = setID;
        CurrentTrackSet = TrackSets[setID];
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
}