using NAudio.Midi;
using System;
using System.Collections.Generic;
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

    public string songName = "dalatecht";
    public string songFolder = RhythmicGame.AMP_songFolder;
    public List<string> songTracks { get { return moggSong.songTracks; } }
    public List<MeasureInfo> songMeasures;

    // Song properties

    // Song length in measures (1 measure = 4 beats)
    public int songCountIn { get { return moggSong.songCountInTime; } }
    public int songLengthInMeasures { get { return moggSong.songLengthInMeasures + moggSong.songCountInTime; } }
    // Tunnel traversal scale
    public float fudgeFactor { get { return moggSong.songFudgeFactor; } }
    // Song beats per minute - this is determined by the song you're trying to sync up to
    public float songBpm { get { return moggSong.songBpm; } }
    // The number of seconds for each song beat
    public float secPerBeat { get { return songBpm / 60f; } }
    // Convert a MIDI tick into milliseconds
    public float tickInMs { get { return 60000f / ((float)reader.bpm * (float)reader.midi.DeltaTicksPerQuarterNote); } }
    // One line in the 8-chopped up measure
    public float DeltaTicksPerQuarterNote { get { return reader.midi.DeltaTicksPerQuarterNote; } }
    // Account for the song tunnel scaling
    public float TunnelSpeedAccountation { get { return (1f + fudgeFactor); } }
    // Account for the song BPM and tunnel scaling
    public float SongSpeedAccountation { get { return secPerBeat * TunnelSpeedAccountation; } }
    // Convert a MIDI tick into game zPos
    public float GetTickTimeInzPos(float absoluteTime)
    {
        return ((tickInMs * absoluteTime / 1000f * 4f + secPerBeat - 1)) * TunnelSpeedAccountation;
    }
    // Get back the Z position for a note from a MIDI tick
    public float GetzPosForNote(float absoluteTime) { return GetTickTimeInzPos(absoluteTime); }

    // One measure's length (4 * two subbeats) // ???
    public float measureLengthInzPos { get { return GetTickTimeInzPos(DeltaTicksPerQuarterNote) * 4; } }
    // One subbeat's length (2 * the time between a beat) // ???
    public float subbeatLengthInzPos { get { return GetTickTimeInzPos(DeltaTicksPerQuarterNote) / 2; } }

    //Current song position, in seconds
    public float songPosition;
    //Current song position, in beats
    public float songPositionInBeats;
    //Current song position, in beats
    public int songPositionInMeasures { get { return (int)songPositionInBeats / 4; } }
    //How many seconds have passed since the song started
    public float dspSongTime;
    //The offset to the first beat of the song in seconds
    public float firstBeatOffset;

    #region AudioSources and AudioClips
    //An AudioSource attached to this GameObject that will play the music.
    List<AudioSource> audiosrcList = new List<AudioSource>();

    public AudioSource src_drums;
    public AudioSource src_bass;
    public AudioSource src_synth;
    public AudioSource src_bgclick;

    public AudioClip drums;
    public AudioClip bass;
    public AudioClip synth;
    public AudioClip bgclick;
    #endregion

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        // Create Tracks controller!
        GameObject AmpTracksGameObject = new GameObject() { name = "TRACKS" };
        TracksController = AmpTracksGameObject.AddComponent<AmplitudeTracksController>();
        TracksController.OnTrackSwitched += AMPTracksCtrl_OnTrackSwitched;

        // Load MoggSong!
        moggSong = gameObject.AddComponent<MoggSong>();
        moggSong.LoadMoggSong(songName);

        Debug.LogFormat("AMP_TRACKS: Using Amplitude track controller!");
        Debug.LogFormat("AMP_TRACKS: Starting MidiReader...");

        reader = gameObject.AddComponent<MidiReader>();
        reader.OnNoteEvent += Reader_OnNoteEvent;

        reader.LoadMIDI(songName);

        // Create measure list!
        songMeasures = CreateMeasureList();

        //  Create tracks!
        TracksController.CreateTracks();
        Debug.LogFormat("AMP_TRACKS: Using tunnel scale fudge factor {0}", fudgeFactor);

        // Load song!
        // Assign clips to AudioSources
        // TODO: read from mogg!!!
        int counter = 0;
        foreach (string track in songTracks)
        {
            AudioClip clip = (AudioClip)Resources.Load(string.Format("Songs/{0}_{1}", songName, track));

            if (clip == null)
            {
                Debug.LogErrorFormat("AMP_CTRL: Can't find clip for track {0} - ignoring AudioSource creation", track);
                continue;
            }

            AudioSource src = gameObject.AddComponent<AudioSource>();
            if (track == "bg_click")
                src.volume = 0.8f;
            else
                src.volume = counter == 0 ? 1.1f : 0f;
            src.clip = clip;

            audiosrcList.Add(src);

            counter++;
        }

        // Get closest notes
        // TODO: do this somewhere else during init!
        CatcherController.Instance.FindNextMeasureNotes();

        // TODO: cleanup! (?)
        //PlayerController.StartZOffset += songCountIn;
        PlayerController.PlayerSpeed = 4 * TunnelSpeedAccountation;
    }
    void Update()
    {
        if (!IsSongPlaying)
            return;

        //determine how many seconds since the song started
        songPosition = (float)(AudioSettings.dspTime - dspSongTime - firstBeatOffset);

        //determine how many beats since the song started
        songPositionInBeats = songPosition / secPerBeat;
    }

    public bool IsSongPlaying = false;
    public void PlayMusic()
    {
        IsSongPlaying = true;

        //Record the time when the music starts
        dspSongTime = (float)AudioSettings.dspTime;

        //Start the music
        /*
        src_bgclick.PlayScheduled(AudioSettings.dspTime);
        src_drums.PlayScheduled(AudioSettings.dspTime);
        src_bass.PlayScheduled(AudioSettings.dspTime);
        src_synth.PlayScheduled(AudioSettings.dspTime);
        */
        foreach (AudioSource src in audiosrcList)
            src.PlayScheduled(AudioSettings.dspTime);
    }

    int currentAudioSourceID = 0;
    private void AMPTracksCtrl_OnTrackSwitched(object sender, int e)
    {
        if (e >= audiosrcList.Count)
        {
            Debug.LogFormat("AMP_CTRL: Track ID {0} does not have an audio source! - ignoring track switch volume change", e);
            return;
        }

        if (currentAudioSourceID != e)
            audiosrcList[currentAudioSourceID].volume = 0.45f;

        audiosrcList[e].volume = 1.1f;
    }

    public List<NoteOnEvent> GetNoteOnEventsForTrack(int trackid)
    {
        return reader.GetNoteOnEventsForTrack(trackid);
    }
    List<MeasureInfo> CreateMeasureList()
    {
        List<MeasureInfo> finalList = new List<MeasureInfo>();
        float prevTime = 0f;
        for (int i = 0; i <= songLengthInMeasures; i++)
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
        string zPosFormat = zPos.ToString("0.0");
        zPos = float.Parse(zPosFormat);
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
        float zPosTime = float.Parse(zPos.ToString("0.0"));

        MeasureInfo info = songMeasures[measureNum];
        float measureTime = float.Parse((info.startTimeInzPos + subbeatLengthInzPos).ToString("0.0"));

        int finalValue = -1;

        for (int i = 0; i < 8; i++)
        {
            if (zPosTime < measureTime)
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