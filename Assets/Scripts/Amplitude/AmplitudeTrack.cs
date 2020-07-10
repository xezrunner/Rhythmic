using System;
using System.Collections.Generic;
using UnityEngine;
using NAudio.Midi;

public class AmplitudeTrack : Track
{
    public List<NoteOnEvent> TrackMidiEvents { get; set; }
    public MidiReader reader;
    PlayerController player { get { return GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>(); } }

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
            //     |      note tick time in ms         | -> |to s| |unit| |tempo correction|  |     scale     |
            zPos = ((tickInMs * (float)note.AbsoluteTime) / 1000f * 4f + (bpm / 60f - 1)) * (1f + fudgefactor);

            // create the note!
            lane.CreateNoteObject(zPos, noteName, noteType, lane.laneType, gameObject.GetComponent<Track>());

            if (RhythmicGame.DebugNoteCreationEvents)
                Debug.LogFormat(string.Format("TRACK [{0}]: Created new note: {1}", TrackName, noteName));
            counter++;
        }
    }

    public void DisableForMeasures(int measures)
    {
        DisablingMeasure = true;

        IsTrackActive = false;

        gameObject.GetComponent<MeshRenderer>().enabled = false;
        Lanes[0].gameObject.SetActive(false);
        Lanes[1].gameObject.SetActive(false);
        Lanes[2].gameObject.SetActive(false);
        measuretarget = measures;
        beatcounter = 0;

        gameObject.GetComponent<TrackEdgeLightController>().IsTrackActive = false;
    }

    bool DisablingMeasure = false;
    bool reachedtarget = false;
    int prevBeatPos = 4;
    int measuretarget = 0;
    int beatcounter = 0;
    private void Update()
    {
        if (Mathf.RoundToInt(player.amp_ctrl.songPositionInBeats) - prevBeatPos == 0)
        {
            prevBeatPos = prevBeatPos + 4;
            beatcounter++;
        }

        if (beatcounter == measuretarget)
            reachedtarget = true;
        else
            reachedtarget = false;

        //Debug.LogFormat("beatcounter: {0} | measuretarget: {1} | prevBeatPos: {2} | DisablingMeasure: {3} | songPosInBeats: {4}", beatcounter, measuretarget, prevBeatPos, DisablingMeasure, Mathf.RoundToInt(player.amp_ctrl.songPositionInBeats));

        if (reachedtarget & DisablingMeasure)
        {
            IsTrackActive = true;
            DisablingMeasure = false;
            gameObject.GetComponent<MeshRenderer>().enabled = true;
            Lanes[0].gameObject.SetActive(true);
            Lanes[1].gameObject.SetActive(true);
            Lanes[2].gameObject.SetActive(true);
            beatcounter = 0;

            gameObject.GetComponent<TrackEdgeLightController>().IsTrackActive = true;
        }
    }
}
