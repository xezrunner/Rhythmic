using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Assets.Scripts.Amplitude
{
    public class AmplitudeSongController : MonoBehaviour
    {
        /// <summary>
        /// TODO: Implement better functionality
        /// </summary>

        AmplitudeTracksController AMPTracksCtrl;

        MidiReader reader;
        public MoggSong moggSong;

        public string songName = "dalatecht";
        public string songFolder = RhythmicGame.AMP_songFolder;
        public List<string> songTracks { get { return moggSong.songTracks; } }
        public List<MeasureInfo> songMeasures;

        // Song properties

        // Song length in measures (1 measure = 4 beats)
        public int songLengthInMeasures { get { return moggSong.songLengthInMeasures; } }
        // Tunnel traversal scale
        public float fudgeFactor { get { return moggSong.songFudgeFactor; } }
        // Song beats per minute - this is determined by the song you're trying to sync up to
        public float songBpm { get { return moggSong.songBpm; } }
        // The number of seconds for each song beat
        public float secPerBeat { get { return songBpm / 60; } }
        // Convert a MIDI tick into milliseconds
        public float tickInMs { get { return 60000f / ((float)reader.bpm * (float)reader.midi.DeltaTicksPerQuarterNote); } }
        // One line in the 8-chopped up measure
        public float DeltaTicksPerQuarterNote { get { return reader.midi.DeltaTicksPerQuarterNote; } }
        // Account for the song tunnel scaling
        public float SongSpeedAccountation { get { return (1f + fudgeFactor); } }
        // Convert a MIDI tick into game zPos
        public float GetTickTimeInzPos(float absoluteTime)
        {
            return ((tickInMs * absoluteTime / 1000f) * 4f) * SongSpeedAccountation;
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
        //How many seconds have passed since the song started
        public float dspSongTime;
        //The offset to the first beat of the song in seconds
        public float firstBeatOffset;

        #region AudioSources and AudioClips
        //An AudioSource attached to this GameObject that will play the music.
        public AudioSource src_drums;
        public AudioSource src_bass;
        public AudioSource src_synth;
        public AudioSource src_bgclick;

        public AudioClip drums;
        public AudioClip bass;
        public AudioClip synth;
        public AudioClip bgclick;
        #endregion

        void Start()
        {
            src_drums = gameObject.AddComponent<AudioSource>();
            src_bass = gameObject.AddComponent<AudioSource>();
            src_synth = gameObject.AddComponent<AudioSource>();
            src_bgclick = gameObject.AddComponent<AudioSource>();

            // Create Tracks controller!
            GameObject AmpTracksGameObject = new GameObject() { name = "AMP_TRACKS" };
            AMPTracksCtrl = AmpTracksGameObject.AddComponent<AmplitudeTracksController>();

            // Load MoggSong!
            moggSong = gameObject.AddComponent<MoggSong>();
            moggSong.LoadMoggSong(songName);

            Debug.LogFormat("AMP_TRACKS: Using Amplitude track controller!");
            Debug.LogFormat("AMP_TRACKS: Starting MidiReader...");

            reader = gameObject.AddComponent<MidiReader>();
            reader.OnNoteEvent += Reader_OnNoteEvent;
            Debug.LogFormat("MidiReader: created");

            reader.LoadMIDI(songName);
            songMeasures = CreateMeasureList();

            //  Create tracks!
            AMPTracksCtrl.CreateTracks();

            Debug.LogFormat("AMP_TRACKS: Using tunnel scale fudge factor {0}", fudgeFactor);
        }
        void Update()
        {
            //determine how many seconds since the song started
            songPosition = (float)(AudioSettings.dspTime - dspSongTime - firstBeatOffset);

            //determine how many beats since the song started
            songPositionInBeats = songPosition / secPerBeat;
        }

        public void PlayMusic()
        {
            // Assign clips to AudioSources
            // TODO: read from mogg or something (FMOD?!)
            bgclick = (AudioClip)Resources.Load(string.Format("Songs/{0}_bgclick", songName));
            drums = (AudioClip)Resources.Load(string.Format("Songs/{0}_drums", songName));
            bass = (AudioClip)Resources.Load(string.Format("Songs/{0}_bass", songName));
            synth = (AudioClip)Resources.Load(string.Format("Songs/{0}_synth", songName));

            src_drums.clip = drums;
            src_bass.clip = bass;
            src_synth.clip = synth;
            src_bgclick.clip = bgclick;

            //Record the time when the music starts
            dspSongTime = (float)AudioSettings.dspTime;

            //Start the music
            src_bgclick.PlayScheduled(AudioSettings.dspTime);
            src_drums.PlayScheduled(AudioSettings.dspTime);
            src_bass.PlayScheduled(AudioSettings.dspTime);
            src_synth.PlayScheduled(AudioSettings.dspTime);
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
        public int GetMeasureNumForzPos(float zPos)
        {
            foreach (MeasureInfo measure in songMeasures)
            {
                if (zPos < measure.endTimeInzPos & zPos > measure.startTimeInzPos)
                    return measure.measureNum;
            }
            return -1;
        }

        public class MeasureInfo
        {
            public int measureNum;
            public float startTimeInzPos;
            public float endTimeInzPos;
        }
    }
}
