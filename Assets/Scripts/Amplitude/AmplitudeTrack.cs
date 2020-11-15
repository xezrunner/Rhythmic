using System;
using System.Collections.Generic;
using UnityEngine;
using NAudio.Midi;
using System.Collections;
using System.Threading.Tasks;
using System.Linq;
using static AmpTrack;

public class AmplitudeTrack : Track
{
    AmplitudeSongController amp_ctrl { get { return (AmplitudeSongController)SongController.Instance; } }

    public List<NoteOnEvent> AMP_NoteOnEvents;
    public override void PopulateNotes()
    {
        // get midi note on events for track
        if (ID != -1)
            AMP_NoteOnEvents = amp_ctrl.GetNoteOnEventsForTrack(ID);

        if (AMP_NoteOnEvents == null)
            throw new Exception("AMP_TRACK: Note on events are null for track " + trackName);

        int counter = 0;
        foreach (NoteOnEvent note in AMP_NoteOnEvents)
        {
            // get lane type for note lane
            LaneSide laneType = AmplitudeGame.GetLaneTypeFromNoteNumber(note.NoteNumber);
            if (laneType == LaneSide.UNKNOWN)
                continue;

            string noteName = string.Format("CATCH_{0}_{1}_{2}", laneType, Instrument, counter);
            Note.NoteType noteType = Note.NoteType.Generic; // TODO: AMP note types for powerups?!

            float zPos = amp_ctrl.GetTickTimeInzPos(note.AbsoluteTime);

            CreateNote(noteName, zPos, noteType, (LaneType)laneType);

            counter++;
        }
        //await Task.Delay(1);
    }

    
}