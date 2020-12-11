using NAudio.Midi;
using PathCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class AmplitudeSongController : SongController
{
    MidiReader reader;
    public MoggSong moggSong { get; set; }

    public List<MeasureInfo> songMeasures;

    // MIDI properties
    public float DeltaTicksPerQuarterNote { get { return Enabled ? reader.midi.DeltaTicksPerQuarterNote : 480; } } // 1 subbeat's length in MIDI ticks
    public float TunnelSpeedAccountation { get { return (songFudgeFactor == 0 ? 1f : songFudgeFactor); } } // tunnel scaling multiplication value

    // Z position calculations (zPos - Rhythmic unit)
    public float GetTickTimeInzPos(float absoluteTime) // Convert MIDI ticks into zPos unit
    {
        //     |       tick time in seconds      |   |     offset by 1 beat length in seconds    ||unit||     fudge factor     |
        return (((tickInMs * absoluteTime) / 1000f) / (tickInMs * DeltaTicksPerQuarterNote / 1000f) * 4) / TunnelSpeedAccountation * (1f + 0.8f);
    }
    public float GetzPosForNote(float absoluteTime) { return GetTickTimeInzPos(absoluteTime); } // Get note's zPos from its tick time | TODO: redundant?

    public override void LoadSong(string song)
    {
        base.LoadSong(song);

        // Check if song exists
        string songPath = AmplitudeGame.AMP_GetSongFilePath(songName, AmplitudeGame.AMP_FileExtension.moggsong);
        if (!File.Exists(songPath))
        {
            Debug.LogErrorFormat("AMP_SONGCTRL: Song {0} does not exist at path: {1}", songName, songPath);
            Enabled = false; return;
        }

        // Load MIDI and set props!
        Debug.LogFormat("AMP_SONGCTRL: Starting MidiReader [{0}]...", songName);
        {
            // start reader
            reader = gameObject.AddComponent<MidiReader>();
            reader.OnNoteEvent += Reader_OnNoteEvent;
            reader.LoadMIDI(songName);

            // set props gathered from reader
            songBpm = reader.bpm;
            songTracks = reader.songTracks;
        }

        // Load MoggSong!
        {
            moggSong = gameObject.AddComponent<MoggSong>();
            moggSong.LoadMoggSong(songName);

            // set SongController props
            songFudgeFactor = moggSong.songFudgeFactor;
            songCountIn = moggSong.songCountInTime;
            songLengthInMeasures = moggSong.songLengthInMeasures;
            songLength = TickTimeToSec(measureTicks * songLengthInMeasures);
            songLengthInzPos = TickTimeTozPos(measureTicks * songLengthInMeasures);
        }

        // Create measure list!
        // TODO: eliminate!!!!!
        songMeasures = CreateMeasureList();
        songNotes = CreateNoteList();

        // Create Tracks controller!
        //CreateTracksController_OLD();
        CreateAmpTrackController();

        // TODO: move elsewhere
        // Scale the catchers and CatcherController
        CatcherController.Instance.BoxCollider.size = new Vector3(CatcherController.Instance.BoxCollider.size.x, CatcherController.Instance.BoxCollider.size.y, CatcherController.Instance.BoxCollider.size.z / TunnelSpeedAccountation * 1.3f);
        CatcherController.Instance.CatcherRadiusExtra = CatcherController.Instance.CatcherRadiusExtra / TunnelSpeedAccountation;
        CatcherController.Instance.gameObject.SetActive(false);

        Time.timeScale = 1f;

        // Load song!
        // Assign clips to AudioSources
        // TODO: read from mogg!!!
        StartCoroutine(LoadSongClips());
    }

    // This loads in the audio clips for the songs.
    // This is temporary, while we do not have MOGG loading.
    // TODO: FMOD implementation
    IEnumerator LoadSongClips()
    {
        int counter = 0;
        foreach (string track in songTracks)
        {
            string path = string.Format("Songs/{0}/{1}", songName, track);

            ResourceRequest resourceRequest = Resources.LoadAsync<AudioClip>(path);
            //Debug.LogFormat("AMP_CTRL: Loading track {0}...", track);

            while (!resourceRequest.isDone)
                yield return 0;

            if (resourceRequest.asset == null)
            {
                Debug.LogWarningFormat("AMP_SONGCTRL: Track {0} doesn't have audio clip - ignoring", track);
                audioSrcList.Add(gameObject.AddComponent<AudioSource>());
                continue;
            }

            AudioSource src = gameObject.AddComponent<AudioSource>(); // create AudioSource
            src.clip = resourceRequest.asset as AudioClip;

            if (track == "bg_click")
            {
                src.volume = 0.8f;
                songLength = src.clip.length; // set song length to BG_CLICK clip length
                mainAudioSource = src; // set as main BG_CLICK AudioSource
            }
            else
                src.volume = counter == 0 ? 1f : 0f;
            // add to AudioSource list
            audioSrcList.Add(src);

            counter++;
        }
    }

    public List<NoteOnEvent> GetNoteOnEventsForTrack(int trackid) { return reader.GetNoteOnEventsForTrack(trackid); }
    private void Reader_OnNoteEvent(object sender, EventArgs e) { }

    // Create notes!
    public override List<List<KeyValuePair<int, MetaNote>>> CreateNoteList()
    {
        List<List<KeyValuePair<int, MetaNote>>> list = new List<List<KeyValuePair<int, MetaNote>>>();
        for (int i = 0; i < songTracks.Count; i++)
        {
            var AMP_NoteOnEvents = GetNoteOnEventsForTrack(i);

            if (AMP_NoteOnEvents == null)
                throw new Exception("AMP_TRACK: Note on events are null for track " + songTracks[i]);

            int counter = 0;
            List<KeyValuePair<int, MetaNote>> kvList = new List<KeyValuePair<int, MetaNote>>();
            foreach (NoteOnEvent note in AMP_NoteOnEvents)
            {
                // get lane type for note lane
                AmpTrack.LaneSide laneType = AmplitudeGame.GetLaneTypeFromNoteNumber(note.NoteNumber);
                if (laneType == AmpTrack.LaneSide.UNKNOWN)
                    continue;

                string noteName = string.Format("CATCH_{0}_{1}_{2}", laneType, i, counter);
                Note.NoteType noteType = Note.NoteType.Generic; // TODO: AMP note types for powerups?!

                float zPos = GetTickTimeInzPos(note.AbsoluteTime);
                int measureID = (int)note.AbsoluteTime / measureTicks;

                MetaNote metaNote = new MetaNote()
                {
                    Name = noteName,
                    Type = noteType,
                    Lane = laneType,
                    MeasureID = measureID,
                    Distance = zPos
                };

                kvList.Add(new KeyValuePair<int, MetaNote>(measureID, metaNote));

                counter++;
            }

            list.Add(kvList);
        }
        return list;
    }

    List<MeasureInfo> CreateMeasureList()
    {
        List<MeasureInfo> finalList = new List<MeasureInfo>();
        float prevTime = 0f;

        if (songLengthInMeasures * measureLengthInzPos > GameObject.Find("Path").GetComponent<PathCreator>().path.length)
        {
            prevTime = GameObject.Find("Path").GetComponent<PathCreator>().path.length - songLengthInMeasures * measureLengthInzPos;
            Debug.LogFormat("Offsetting tracks: {0}", prevTime);
        }

        for (int i = 0; i < songLengthInMeasures + 3; i++)
        {
            MeasureInfo measure = new MeasureInfo() { measureNum = i, startTimeInzPos = prevTime, endTimeInzPos = prevTime + measureLengthInzPos };
            prevTime = prevTime + measureLengthInzPos;

            finalList.Add(measure);
        }

        return finalList;
    }

    public class MeasureInfo
    {
        public int measureNum;
        public float startTimeInzPos;
        public float endTimeInzPos;
    }
}