using System;
using System.Collections.Generic;
using UnityEngine;
using NAudio.Midi;
using System.Collections;
using System.Threading.Tasks;

public class AmplitudeTrack : Track
{
    AmplitudeSongController amp_ctrl { get { return AmplitudeSongController.Instance; } }

    public List<NoteOnEvent> AMP_NoteOnEvents;
    public async override void PopulateNotes()
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
            LaneType laneType = GetLaneTypeFromNoteNumber(note.NoteNumber);
            if (laneType == LaneType.UNKNOWN)
                continue;

            string noteName = string.Format("CATCH_{0}_{1}_{2}", laneType, Instrument, counter);
            Note.NoteType noteType = Note.NoteType.Generic; // TODO: AMP note types for powerups?!

            float zPos = amp_ctrl.GetTickTimeInzPos(note.AbsoluteTime);

            CreateNote(noteName, zPos, noteType, laneType);

            counter++;
        }
        await Task.Delay(1);
    }

    public static LaneType GetLaneTypeFromNoteNumber(int num)
    {
        switch (num)
        {
            case 114: // left
                return LaneType.Left;
            case 116: // center
                return LaneType.Center;
            case 118: // right
                return LaneType.Right;

            default: // if it isn't either of these notes, go to next note
                return LaneType.UNKNOWN;
        }
    }
}