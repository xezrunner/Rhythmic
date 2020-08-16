using NAudio.Midi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class AmplitudeSongController : MonoBehaviour
{
    /// <summary>
    /// TODO: Implement better functionality
    /// </summary>

    public static AmplitudeSongController Instance;
    public PlayerController PlayerController { get { return PlayerController.Instance; } }
    AmplitudeTracksController TracksController = (AmplitudeTracksController)AmplitudeTracksController.Instance;

    MidiReader reader;
    public MoggSong moggSong;

    public string songName;
    public string songFolder = RhythmicGame.AMP_songFolder;
    public List<string> songTracks { get { return moggSong.songTracks; } }
    public List<MeasureInfo> songMeasures;

    // If this is set to true, no song will load when this entity is spawned.
    // TODO: swap for Enabled?
    public bool Enabled = true;

    // Song properties
    public float songBpm { get { return moggSong.songBpm; } }
    public float secPerBeat { get { return songBpm / 60f; } }
    public int songCountIn { get { return moggSong.songCountInTime; } } // TODO: figure out what countin is supposed to be
    public int songLengthInMeasures { get { return moggSong.songLengthInMeasures; } }
    public float fudgeFactor { get { return moggSong.songFudgeFactor; } } // Tunnel traversal scale

    // MIDI properties
    public float tickInMs { get { return 60000f / ((float)reader.bpm * (float)reader.midi.DeltaTicksPerQuarterNote); } } // 1 MIDI tick in milliseconds
    public float DeltaTicksPerQuarterNote { get { return Enabled ? reader.midi.DeltaTicksPerQuarterNote : 480; } } // 1 subbeat's length in MIDI ticks
    public float TunnelSpeedAccountation { get { return (1f + fudgeFactor); } } // tunnel scaling multiplication value

    // Music playback properties
    public float songPosition; //Current song position, in seconds
    public float songPositionInBeats; //Current song position, in beats
    public int songPositionInMeasures { get { return (int)songPositionInBeats / 4; } } //Current song position, in measures
    public float firstBeatOffset; //The offset to the first beat of the song in seconds
    public float dspSongTime; //How many seconds have passed since the song started

    // Z position calculations (zPos - Rhythmic unit)
    public float GetTickTimeInzPos(float absoluteTime) // Convert MIDI ticks into zPos unit
    {
        //     |       tick time in seconds      |   |     offset by 1 beat length in seconds    ||unit||     fudge factor     |
        return ((tickInMs * absoluteTime) / 1000f) / (tickInMs * DeltaTicksPerQuarterNote / 1000f) * 4 * TunnelSpeedAccountation;
    }
    public float GetzPosForNote(float absoluteTime) { return GetTickTimeInzPos(absoluteTime); } // Get note's zPos from its tick time | TODO: redundant?
    public float measureLengthInzPos { get { return GetTickTimeInzPos(DeltaTicksPerQuarterNote) * 4; } } // One measure's length in zPos unit (4 beats)
    public float subbeatLengthInzPos { get { return GetTickTimeInzPos(DeltaTicksPerQuarterNote) / 2; } } // One subbeat's length in zPos unit (1/2[half] of a beat)

    //An AudioSource attached to this GameObject that will play the music.
    public List<AudioSource> audiosrcList = new List<AudioSource>();

    public float SongPositionOffset = 0f;

    void Awake()
    {
        Instance = this;
    }
    public void Start()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("DevScene"));

        // Create Tracks controller!
        Debug.LogFormat("AMP_TRACKS: Using Amplitude track controller!");

        GameObject AmpTracksGameObject = new GameObject() { name = "TRACKS" };
        TracksController = AmpTracksGameObject.AddComponent<AmplitudeTracksController>();
        TracksController.OnTrackSwitched += AMPTracksCtrl_OnTrackSwitched;

        if (Enabled || songName == "")
        {
            // Check if song exists
            string songPath = RhythmicGame.AMP_GetSongFilePath(songName, RhythmicGame.AMP_FileExtension.mogg);
            if (!File.Exists(songPath))
            {
                Debug.LogErrorFormat("Song {0} does not exist at path: {1}", songName, songPath);
                Enabled = false;
                return;
            }

            // Load MoggSong!
            moggSong = gameObject.AddComponent<MoggSong>();
            moggSong.LoadMoggSong(songName);

            // Tunnel track duplication
            /*
            if (RhythmicGame.IsTunnelMode & RhythmicGame.TunnelTrackDuplication)
            {
                List<string> tempTracksList = songTracks.ToList();

                for (int i = 1; i < RhythmicGame.TunnelTrackDuplicationCount; i++)
                    foreach (string track in tempTracksList)
                        songTracks.Add(track);
            }
            */

            // Load MIDI!
            Debug.LogFormat("AMP_TRACKS: Starting MidiReader [{0}]...", songName);

            reader = gameObject.AddComponent<MidiReader>();
            reader.OnNoteEvent += Reader_OnNoteEvent;

            reader.bpm = moggSong.songBpm;
            reader.LoadMIDI(songName);

            // Create measure list!
            songMeasures = CreateMeasureList();

            //  Create tracks!
            TracksController.CreateTracks();
            Debug.LogFormat("AMP_TRACKS: Using tunnel scale fudge factor {0}", fudgeFactor);

            // Load song!
            // Assign clips to AudioSources
            // TODO: read from mogg!!!
            StartCoroutine(LoadSongClips());
        }
        else
            Debug.LogWarningFormat("AMP_TRACKS: DISABLED");
    }
    void Update()
    {
        if (!IsSongPlaying)
            return;

        //determine how many seconds since the song started
        //songPosition = (float)(AudioSettings.dspTime - dspSongTime - firstBeatOffset + SongPositionOffset);
        songPosition = audiosrcList[0].time;

        //determine how many beats since the song started
        songPositionInBeats = songPosition / secPerBeat;
    }

    // This loads in the audio clips for the songs.
    // This is temporary, while we do not have MOGG loading.
    IEnumerator LoadSongClips()
    {
        int counter = 0;
        foreach (string track in songTracks)
        {
            string path = string.Format("Songs/{0}_{1}", songName, track);

            ResourceRequest resourceRequest = Resources.LoadAsync<AudioClip>(path);
            //Debug.LogFormat("AMP_CTRL: Loading track {0}...", track);

            while (!resourceRequest.isDone)
                yield return 0;

            if (resourceRequest.asset == null)
            {
                Debug.LogWarningFormat("AMP_CTRL: Track {0} doesn't have audio clip - ignoring", track);
                continue;
            }

            AudioSource src = gameObject.AddComponent<AudioSource>();

            if (track == "bg_click")
                src.volume = 0.8f;
            else
                src.volume = counter == 0 ? 1f : 0f;

            src.clip = resourceRequest.asset as AudioClip;
            audiosrcList.Add(src);

            counter++;
        }
    }

    public bool IsSongPlaying = false;
    public void PlayMusic()
    {
        IsSongPlaying = true;

        //Record the time when the music starts
        dspSongTime = (float)AudioSettings.dspTime;

        //Start the music
        foreach (AudioSource src in audiosrcList)
            src.PlayScheduled(dspSongTime);
    }

    int currentAudioSourceID = 0; // current track ID that's playing
    private void AMPTracksCtrl_OnTrackSwitched(object sender, int e)
    {
        if (e >= audiosrcList.Count)
        {
            Debug.LogWarningFormat("AMP_CTRL: Track ID {0} does not have an audio source! - ignoring track switch volume change", e);
            return;
        }

        if (currentAudioSourceID != e & TracksController.Tracks[currentAudioSourceID].IsTrackCaptured)
            audiosrcList[currentAudioSourceID].volume = 0.45f;

        if (TracksController.Tracks[e].IsTrackCaptured)
            audiosrcList[e].volume = 1.1f;

        currentAudioSourceID = e;
    }

    public void AdjustTrackVolume(int track, float volume)
    {
        if (track == -1)
            return;
        if (track >= audiosrcList.Count)
        {
            Debug.LogWarningFormat("AMP_CTRL: Track ID {0} does not have an audio source! - ignoring track switch volume change", track);
            return;
        }

        audiosrcList[track].volume = volume;
    }

    public List<NoteOnEvent> GetNoteOnEventsForTrack(int trackid)
    {
        return reader.GetNoteOnEventsForTrack(trackid);
    }
    List<MeasureInfo> CreateMeasureList()
    {
        List<MeasureInfo> finalList = new List<MeasureInfo>();
        float prevTime = 0f;
        for (int i = 0; i < songLengthInMeasures + 3; i++)
        {
            MeasureInfo measure = new MeasureInfo() { measureNum = i, startTimeInzPos = prevTime, endTimeInzPos = prevTime + measureLengthInzPos };
            prevTime = prevTime + measureLengthInzPos;

            finalList.Add(measure);
        }

        return finalList;
    }
    private void Reader_OnNoteEvent(object sender, EventArgs e)
    {

    }

    // Gets the measure number for a z position (Rhythmic Game unit)
    public int GetMeasureNumForZPos(float zPos)
    {
        zPos = float.Parse(zPos.ToString("0.0"));
        foreach (MeasureInfo measure in songMeasures)
        {
            float endTimeInZPos = float.Parse(measure.endTimeInzPos.ToString("0.0"));
            if (zPos < endTimeInZPos)
                return measure.measureNum;
            else
                continue;
        }
        return -1;
    }
    public int GetSubbeatNumForZPos(int measureNum, float zPos)
    {
        zPos = float.Parse(zPos.ToString("0.0"));

        MeasureInfo info = songMeasures[measureNum];
        float measureTime = float.Parse((info.startTimeInzPos + subbeatLengthInzPos).ToString("0.0"));

        int finalValue = -1;

        for (int i = 0; i < 8; i++)
        {
            if (zPos < measureTime)
                return i;
            else
                measureTime += subbeatLengthInzPos;
        }

        Debug.LogErrorFormat("AMP_CTRL: couldn't find subbeat for note at measure {0}, zPos {1}", measureNum, zPos);
        return finalValue;
    }

    public class MeasureInfo
    {
        public int measureNum;
        public float startTimeInzPos;
        public float endTimeInzPos;
    }
}