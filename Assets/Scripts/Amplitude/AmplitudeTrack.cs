using System;
using System.Collections.Generic;
using UnityEngine;
using NAudio.Midi;

public class AmplitudeTrack : Track
{
    public List<NoteOnEvent> TrackMidiEvents { get; set; }
    public MidiReader reader;

    public float ticks { get; set; }
    public float bpm { get; set; }
    public float fudgefactor { get; set; }

    /// <summary>
    /// Populate lanes with Note (CATCH) objects from the list of MIDI events
    /// TODO: possibly rename to AMP_PopulateNotes()?
    /// </summary>
    float zPos = 0f;
    float tickInMs { get { return 60000f / ((float)reader.bpm * (float)reader.midi.DeltaTicksPerQuarterNote); } }
    public void AMP_PopulateLanes(List<NoteOnEvent> noteEvents = null)
    {
        if (TrackMidiEvents == null & noteEvents != null)
            TrackMidiEvents = noteEvents;
        else if (TrackMidiEvents == null & noteEvents == null)
            Debug.LogErrorFormat("TRACK/AMP_PopulateLanes() [{0}]: Note ON events are null!", name);

        int counter = 0;
        foreach (NoteOnEvent note in TrackMidiEvents) // go through the midi events in the list and create notes
        {
            // get the lane this note is for
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

            // assign name and type
            string noteName = string.Format("AMP_CATCH_{0}_{1}_{2}", lane.laneType, Instrument.ToString(), counter);
            Note.NoteType noteType = Note.NoteType.Generic; // TODO: AMP note types for powerups?!

            // get note Z position in track lane
            //zPos = ((tickInMs * (float)note.AbsoluteTime) / (1000f + bpm / 60f) * 4f - 1);
            //      1t in ms        note tick time = ms      to s      bps      qnote      scale
            // zPos = (tickInMs * (float)note.AbsoluteTime) / (1000f + (bpm/60f) - 1) * 4f;
            zPos = ((tickInMs * (float)note.AbsoluteTime) / 1000f * 4f + (bpm / 60f - 1f)) * (1f + fudgefactor);

            // create the note!
            lane.CreateNoteObject(zPos, noteName, noteType, lane.laneType);

            if (RhythmicGame.DebugNoteCreationEvents)
                Debug.LogFormat(string.Format("TRACK [{0}]: Created new note: {1}", TrackName, noteName));
            counter++;
        }
    }
}
