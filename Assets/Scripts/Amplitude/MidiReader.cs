using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NAudio.Midi;
using System.IO;
using System;

// Source: https://gist.github.com/CustomPhase/033829c5c30f872d250a79e3d35b7048
public class MidiReader : MonoBehaviour
{
    public MidiFile midi;

    //The bpm(tempo) of the track
    // TODO: read BPM from moggsong!
    public int bpm = 110;

    public int tempo = 110;

    public int ticks;
    public int offset;

    public string songName = "perfectbrain";
    public string songFolder = "H://HMXAMPLITUDE//Extractions//amplitude_ps4_extraction//ps4//songs";

    string GetSongPath()
    {
        return string.Format("{0}//{1}//{1}.mid", songFolder, songName);
    }

    // Use this for initialization
    void Start()
    {
        if (midi == null)
        {
            LoadMIDI();
            Debug.LogWarning("MidiReader: Couldn't find manual initialization - loading default MIDI");
        }
    }

    public void LoadMIDI()
    {
        //Loading midi file from resources folder
        Debug.LogFormat(string.Format("MidiReader: Loading MIDI from {0}", GetSongPath()));
        byte[] asset = File.ReadAllBytes(GetSongPath());
        Stream s = new MemoryStream(asset);

        //Read the file
        midi = new MidiFile(s, true);

        //Ticks needed for timing calculations
        ticks = midi.DeltaTicksPerQuarterNote;

        Debug.LogFormat(string.Format("MidiReader: MIDI loaded: \n" +
            "BPM: {0} | Tracks: {1} | Ticks: {2}",
            bpm, midi.Tracks, ticks));
    }

    public int GetMidiTrackCount()
    {
        return midi.Tracks;
    }

    public List<NoteOnEvent> GetNoteOnEventsFromTrack(int track)
    {
        List<NoteOnEvent> list = new List<NoteOnEvent>();
        foreach (MidiEvent midevent in midi.Events[track])
        {
            if (midevent is TempoEvent)
            {
                var tEvent = (TempoEvent)midevent;
                tempo = tEvent.MicrosecondsPerQuarterNote / 1000;
            }
            else if (midevent.CommandCode == MidiCommandCode.NoteOn)
                list.Add((NoteOnEvent)midevent);
        }
        return list;
    }

    public void StartPlayback(int track)
    {
        foreach (MidiEvent note in midi.Events[track])
        {
            //If its the start of the note event
            if (note.CommandCode == MidiCommandCode.NoteOn)
            {
                //Cast to note event and process it
                NoteOnEvent noe = (NoteOnEvent)note;
                NoteEvent(noe);
            }
        }
    }

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
        StartCoroutine(CreateAction(time, noteNumber, noe.NoteLength));
        //Debug.LogWarning("MIDI: hit!");
    }

    public event EventHandler OnNoteEvent;

    public bool DebugOnEvent = false;
    IEnumerator CreateAction(float t, int noteNumber, float length)
    {
        //Wait for the start of the note
        yield return new WaitForSeconds(t);

        //The note is about to play, do your stuff here
        if (DebugOnEvent)
            Debug.LogFormat("MidiReader: Playing note: " + noteNumber);

        OnNoteEvent?.Invoke(this, null); // call event on MIDI event
    }
}
