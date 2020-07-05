using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NAudio.Midi;
using System.Linq.Expressions;
using System;
using System.Threading.Tasks;

public class AmplitudeTrack : Track
{
    public float ticks { get; set; }
    public int bpm { get; set; }
    public List<NoteOnEvent> TrackMidiEvents { get; set; }
    public MidiReader reader;

    private void Update()
    {
    }

    /// <summary>
    /// Populate lanes with Note (CATCH) objects from the list of MIDI events
    /// TODO: possibly rename to AMP_PopulateNotes()?
    /// </summary>
    float zPos = 0f;
    public void AMP_PopulateLanes()
    {
        int counter = 0;
        foreach (NoteOnEvent note in TrackMidiEvents) // go through the midi events in the list and create notes
        {
            TrackLane lane;

            switch (note.NoteNumber)
            {
                case 114: // left
                    lane = Lanes[0]; break;
                case 116: // center
                    lane = Lanes[1]; break;
                case 118: // right
                    lane = Lanes[2]; break;

                default: // if it isn't either of these notes, go to next note
                    continue;
            }

            Note.NoteType noteType = Note.NoteType.Generic; // TODO: AMP note types for powerups?!

            //float time = (60 * note.AbsoluteTime) / (bpm * ticks);

            //zPos = (long)((note.AbsoluteTime / (float)reader.midi.DeltaTicksPerQuarterNote) * reader.tempo);
            /*
            if (counter == 0)
            {
                firstZOffset = (float)note.AbsoluteTime / (float)bpm;
                zPos = (float)note.AbsoluteTime / (float)bpm - (float)firstZOffset;
                firstZOffsetMinusFirstNote = zPos;
            }
            else
            zPos = (float)note.AbsoluteTime / (float)bpm - (float)firstZOffset;
            */
            //zPos = (float)note.AbsoluteTime / (float)bpm;
            //zPos = (float)note.AbsoluteTime / bpm;
            float tick = (60000f / ((float)bpm * (float)reader.midi.DeltaTicksPerQuarterNote) * 200);
            zPos = ((float)note.AbsoluteTime / tick) + ((float)note.NoteLength * 0.001f);
            Debug.Log(note.NoteLength);


            //Debug.Log(zPos);

            //float time = note.NoteLength / 10;
            //Debug.Log(note.NoteLength);

            //zPos += time;
            string noteName = string.Format("AMP_CATCH_{0}_{1}_{2}", lane.laneType, Instrument.ToString(), counter);

            lane.CreateNoteObject(zPos, noteName, noteType, lane.laneType);

            if (RhythmicGame.DebugNoteCreationEvents)
                Debug.LogFormat(string.Format("TRACK [{0}]: Created new note: {1}", TrackName, noteName));

            counter++;
        }
    }
}
