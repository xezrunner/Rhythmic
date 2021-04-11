using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NAudio.Midi;
using System.IO;
using System;
using System.Linq;
using UnityEditor;

// Source: https://gist.github.com/CustomPhase/033829c5c30f872d250a79e3d35b7048
public class MidiReader : MonoBehaviour
{
    public MidiFile midi { get; set; }

    // The bpm(tempo) of the track
    // TODO: read BPM from moggsong!
    public int bpm;

    public int ticks;
    public int offset;

    public List<string> midi_songTracks = new List<string>();
    public List<string> songTracks = new List<string>();

    public string songName = "tut0";
    public string songFolder { get { return AmplitudeGame.AMP_songFolder; } }

    /// <summary>
    /// This returns the path based on the songName and songFolder variables.
    /// </summary>
    /// <returns></returns>
    string GetMIDIPath()
    {
        return AmplitudeGame.AMP_GetSongFilePath(songName, AmplitudeGame.AMP_FileExtension.mid);
        //return string.Format("{0}//{1}//{1}.mid", songFolder, songName);
    }

    // Use this for initialization
    void Start()
    {
        if (midi != null)
            return;

        // Load MIDI if it hasn't been pre-assigned
        //LoadMIDI(GetMIDIPath());
        //Debug.LogWarning("MidiReader: Couldn't find manual initialization - loading default MIDI");
        Debug.LogWarning("MidiReader: Couldn't find manual initialization");
    }

    public void LoadMIDI(string song)
    {
        // Set song name
        songName = song;
        // Get path for MIDI
        string midiPath = GetMIDIPath();

        // Load midi file from resources folder
        Debug.LogFormat(string.Format("MidiReader: Loading MIDI from {0}", midiPath));

        byte[] asset = File.ReadAllBytes(midiPath);
        Stream s = new MemoryStream(asset);

        // Read the file
        midi = new MidiFile(s, true);

        // Ticks needed for timing calculations
        ticks = midi.DeltaTicksPerQuarterNote;

        // BPM
        bpm = GetBPMfromMidi();

        GetSongTracksFromMIDI();

        Debug.LogFormat(string.Format("MidiReader: MIDI loaded: \n" +
            "BPM: {0} | Tracks: {1} | Ticks: {2} | PPQ: {3}",
            bpm, midi.Tracks, ticks, midi.DeltaTicksPerQuarterNote));
    }

    /// <summary>
    /// Returns the amount of tracks in the MIDI file.
    /// </summary>
    public int GetMidiTrackCount()
    {
        return midi.Tracks;
    }

    // REDUNDANT - we get the BPM from moggsong (amp_ctrl) instead
    public int GetBPMfromMidi()
    {
        foreach (MidiEvent midevent in midi.Events[0])
        {
            if (midevent is TempoEvent) // tempo was found!
            {
                var tEvent = (TempoEvent)midevent;
                int finalBPM = 60000 / (tEvent.MicrosecondsPerQuarterNote / 1000);
                return finalBPM;
            }
        }

        throw new Exception("MidiReader: Tempo not found or BPM is 0!");
    }

    /// <summary>
    /// Get the Note ON events from a given track ID.
    /// It also gets and assigns the tempo from the MIDI file when found.
    /// </summary>
    /// <param name="track">The track ID to get the note events from</param>
    public List<NoteOnEvent> GetNoteOnEventsForTrack(int track)
    {
        List<NoteOnEvent> list = new List<NoteOnEvent>();

        foreach (MidiEvent midevent in midi.Events[track + 1])
        {
            if (midevent.CommandCode == MidiCommandCode.NoteOn) // note ON event was found!
            {
                NoteOnEvent noteEvent = (NoteOnEvent)midevent;
                if (AmplitudeGame.CurrentNoteNumberSet.Any(n => n == noteEvent.NoteNumber)) // is it any of the current difficulty note numbers?
                    list.Add(noteEvent); // add the note to list
            }
        }

        return list;
    }

    public class IdenticalTrack
    {
        public IdenticalTrack(string name, int counter)
        { Name = name; Counter = counter; }
        public string Name;
        public int Counter;
    }

    public void GetSongTracksFromMIDI()
    {
        songTracks.Clear();

        List<IdenticalTrack> identicalList = new List<IdenticalTrack>();

        // go through each track in MIDI
        for (int i = 0; i < midi.Tracks; i++)
        {
            string code = midi.Events[i][0].ToString().Trim(); // The first command on a track is meta info about the track
            code = code.Substring("0 SequenceTrackName ".Length + (code.Contains("CATCH") ? 3 : 0));
            string[] tokens = code.Split(':');
            if (tokens[0] == "?") continue;

            // "0 SequenceTrackName T1 CATCH:D:Drums"
            string midi_Tcode = (code.Contains("CATCH") ? $"T{i} " : "") + string.Join("_", tokens);
            midi_songTracks.Add(midi_Tcode);

            if (code.Contains("CATCH"))
            {
                tokens = code.Substring(code.IndexOf("CATCH")).Split(':'); // CATCH:T:NAME

                int identicalCounter = 0;
                foreach (string track in songTracks)
                {
                    if (track == tokens.Last().ToLower())
                    {
                        // find identical
                        var identical = identicalList.Find(x => x.Name == track);
                        if (identical == null)
                        {
                            identical = new IdenticalTrack(track, 2);
                            identicalList.Add(identical);
                        }
                        else
                            identical.Counter++;
                        tokens[2] = tokens[2] + identical.Counter;
                        break;
                    }
                    identicalCounter++;
                }

                songTracks.Add(tokens[2].ToLower());
            }
            else if (code.Contains("FREESTYLE"))
                songTracks.Add("freestyle");
            else if (code.Contains("BG_CLICK"))
                songTracks.Add("bg_click");
            else
                continue;
        }
        /*
        List<string> identical = new List<string>();
        int counter = 0;
        foreach (string track in songTracks)
        {

        }*/
    }

    /// <summary>
    /// Starts playing back a track from the MIDI.
    /// </summary>
    /// <param name="track">The track to play</param>
    public void StartTrackPlayback(int track)
    {
        foreach (MidiEvent note in midi.Events[track]) // go through MIDI events in track
        {
            if (note.CommandCode == MidiCommandCode.NoteOn) // If it's a note on event
            {
                NoteOnEvent noe = (NoteOnEvent)note; // Cast to note ON event and process it
                NoteEvent(noe); // fire event
            }
        }
    }

    public event EventHandler OnNoteEvent;

    /// <summary>
    /// This fires off a Note ON event
    /// </summary>
    /// <param name="noe"></param>
    public void NoteEvent(NoteOnEvent noe)
    {
        //Time until the start of the note in seconds
        float time = (60 * noe.AbsoluteTime) / (bpm * ticks);

        //The number (key) of the note. Heres a useful chart of number-to-note translation:
        //http://www.electronics.dit.ie/staff/tscarff/Music_technology/midi/midi_note_numbers_for_octaves.htm
        // TODO: translate note events of Amplitude
        int noteNumber = noe.NoteNumber;

        //Start coroutine for each note at the start of the playback
        //Really awful way to do stuff, but its simple
        // TODO: revise this! (?)
        StartCoroutine(CreateAction(time, noteNumber));
    }

    public bool DebugOnEvent = false;
    IEnumerator CreateAction(float t, int noteNumber)
    {
        //Wait for the start of the note
        yield return new WaitForSeconds(t);

        //The note is about to play, do your stuff here
        if (DebugOnEvent)
            Debug.LogFormat("MidiReader: Playing note: " + noteNumber);

        OnNoteEvent?.Invoke(this, null); // call event on MIDI event
    }
}