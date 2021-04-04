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

    // MIDI properties
    public float DeltaTicksPerQuarterNote { get { return IsEnabled ? reader.midi.DeltaTicksPerQuarterNote : 480; } } // 1 subbeat's length in MIDI ticks
    public float TunnelSpeedAccountation { get { return (songFudgeFactor == 0 ? 1f : songFudgeFactor); } } // tunnel scaling multiplication value

    public float GetzPosForNote(float absoluteTime) { return TickToPos(absoluteTime); } // Get note's zPos from its tick time | TODO: redundant?

    public override void LoadSong(string song)
    {
        base.LoadSong(song);

        // Check if song exists
        string songPath = AmplitudeGame.AMP_GetSongFilePath(songName, AmplitudeGame.AMP_FileExtension.moggsong);
        if (!File.Exists(songPath))
        {
            Debug.LogErrorFormat("AMP_SONGCTRL: Song {0} does not exist at path: {1}", songName, songPath);
            IsEnabled = false; return;
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
            songLength = TickToSec(measureTicks * songLengthInMeasures);
            songLengthInzPos = TickToPos(measureTicks * songLengthInMeasures);
        }

        // *** PROPS LOADED *** //

        CalculateTimeUnits();

        // Create Tracks controller!
        //CreateTracksController_OLD();
        CreateAmpTrackController();

        // Create note list!
        CreateMetaNotes();

        // TODO: We want to scale time based on song BPM & fudge factor, possibly?
        //Time.timeScale = 1f;

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
                Logger.LogWarning($"AMP_SONGCTRL: Track {track} doesn't have audio clip - ignoring");
                audioSrcList.Add(gameObject.AddComponent<AudioSource>());
                continue;
            }

            AudioSource src = gameObject.AddComponent<AudioSource>(); // create AudioSource
            src.clip = resourceRequest.asset as AudioClip;

            if (track == "bg_click")
            {
                src.volume = 0.8f;
                songLength = src.clip.length; // set song length to BG_CLICK clip length
                BG_CLICKSrc = src; // set as main BG_CLICK AudioSource
            }
            else
                src.volume = 1f;
            // add to AudioSource list
            audioSrcList.Add(src);

            counter++;
        }
    }

    public List<NoteOnEvent> GetNoteOnEventsForTrack(int trackid) { return reader.GetNoteOnEventsForTrack(trackid); }
    private void Reader_OnNoteEvent(object sender, EventArgs e) { }

    // Create notes!
    public override void CreateMetaNotes()
    {
        songNotes = new MetaNote[songTracks.Count, songLengthInMeasures][];

        for (int t = 0; t < songTracks.Count; t++)
        {
            List<NoteOnEvent> AMP_NoteOnEvents = GetNoteOnEventsForTrack(t);
            int count = (AMP_NoteOnEvents != null) ? AMP_NoteOnEvents.Count : 0;

            if (count == 0)
                Logger.LogMethodW("Note events are null for track " + songTracks[t], this);

            //for (int i = 0; i < songLengthInMeasures; i++)
            //    songNotes[t, i] = new MetaNote[count];

            float prev_dist = 0.0f;
            int prev_m = -1;

            List<MetaNote> note_list = new List<MetaNote>();

            // PASS: add notes to a note list:
            for (int i = 0, m_note_id = 0; i < count; i++, m_note_id++)
            {
                NoteOnEvent note = AMP_NoteOnEvents[i];
                LaneSide lane_side = AmplitudeGame.GetLaneTypeFromNoteNumber(note.NoteNumber);
                if (lane_side == LaneSide.UNKNOWN) continue; // TODO: warn?
                NoteType note_type = NoteType.Generic;

                float dist = StartDistance + TickToPos(note.AbsoluteTime);
                if (dist == prev_dist) { Logger.LogMethodW($"2 (or more) notes at the same distance! Ignoring! dist: {dist}", this); continue; }

                int m = (int)note.AbsoluteTime / measureTicks; // TODO: note.AbsoluteTime is a long!
                if (m != prev_m) prev_m = 0; // rollover!
                prev_m = m;

                string note_name = $"CATCH/{songTracks[t]}:{m} [{i}] ({lane_side})";

                MetaNote meta_note = new MetaNote()
                {
                    Name = note_name,
                    TotalID = i,
                    TrackID = t,
                    MeasureID = m,
                    Distance = dist,
                    Type = note_type,
                    Lane = lane_side,
                };

                note_list.Add(meta_note);
            }

            // PASS: distribute notes to their own measure arrays:
            int note_list_count = note_list.Count;
            for (int i = 0; i < songLengthInMeasures; i++)
                songNotes[t, i] = note_list.Where(n => n.MeasureID == i).ToArray();
        }
    }
}