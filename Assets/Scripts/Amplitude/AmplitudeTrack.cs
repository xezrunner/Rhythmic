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

    public void AMP_PopulateNotes()
    {
        // get midi note on event for track
        if (ID.HasValue)
            AMP_NoteOnEvents = amp_ctrl.GetNoteOnEventsForTrack(ID.Value);

        if (AMP_NoteOnEvents == null)
            throw new Exception("AMP_TRACK: Note on events are null!");

        int counter = 0;
        foreach (NoteOnEvent note in AMP_NoteOnEvents)
        {
            // get lane object
            LaneType laneType = GetLaneTypeFromNoteNumber(note.NoteNumber);
            if (laneType == LaneType.UNKNOWN)
                continue;

            GameObject lane = GetLaneObjectForLaneType(laneType);

            // assign name and type
            string noteName = string.Format("CATCH_{0}_{1}_{2}", laneType, Instrument, counter);
            Note.NoteType noteType = Note.NoteType.Generic; // TODO: AMP note types for powerups?!

            // get zPosition and measure number
            float zPos = amp_ctrl.GetzPosForNote(note.AbsoluteTime);
            int measureNum = amp_ctrl.GetMeasureNumForZPos(zPos);

            // create note!
            CreateNote(lane, zPos, noteName, noteType, laneType, this, measureNum);

            counter++;
        }
    }

    UnityEngine.Object notePrefab;
    void CreateNote(GameObject lane, float zPosition, string noteName = "", Note.NoteType noteType = Note.NoteType.Generic, LaneType noteLane = LaneType.Center, Track track = null, int measureNum = 0)
    {
        if (notePrefab == null)
            notePrefab = Resources.Load("Prefabs/Note");

        Vector3 position = new Vector3(lane.transform.position.x, 0.01f, zPosition);
        Vector3 scale = new Vector3(0.45f, 0.1f, 0.45f);

        // create GameObject for Note
        GameObject obj = (GameObject)GameObject.Instantiate(notePrefab, position, new Quaternion());
        obj.transform.parent = lane.transform;

        // create and assign Note to GameObject
        AmplitudeNote note = obj.AddComponent<AmplitudeNote>();
        note.name = noteName;
        note.noteType = noteType;
        note.noteLane = noteLane;
        note.noteTrack = track;
        note.measureNum = measureNum;
        note.subbeatNum = amp_ctrl.GetSubbeatNumForZPos(measureNum, zPosition);
        note.zPos = zPosition;
        note.DotLightColor = Track.Colors.ColorFromTrackType(track.Instrument.Value);

        // Add note to Notes list
        trackNotes.Add(note);
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

    /*
    // Track types
    public enum AMP_TrackType
    {
        Drums = 0,
        Bass = 1,
        Synth = 2,
        Guitar = 3,
        gtr = 3,
        Vocals = 4,
        vox = 4,
        FREESTYLE = 5,
        bg_click = 6
    }

    public static AMP_TrackType AMPTrackTypeFromString(string s)
    {
        foreach (string type in Enum.GetNames(typeof(AMP_TrackType)))
        {
            if (s.Contains(type.ToString().ToLower()))
                return (AMP_TrackType)Enum.Parse((typeof(AMP_TrackType)), type);
        }
        throw new Exception("MoggSong: Invalid track string! " + s);
    }
    */
}