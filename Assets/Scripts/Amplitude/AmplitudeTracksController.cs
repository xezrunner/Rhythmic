using Assets.Scripts.Amplitude;
using NAudio.Midi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AmplitudeTracksController : TracksController
{
    /// <summary>
    /// TODO: Possibly play and read the notes in here instead of MidiReader
    /// </summary>
    /// 

    MidiReader reader;
    AmplitudeSongController amp_ctrl { get { return GameObject.Find("AMPController").GetComponent<AmplitudeSongController>(); } }

    public string songName = "tut0";
    public int bpm { get; set; }
    public float fudgeFactor
    {
        get
        {
            switch (songName)
            {
                default:
                case "tut0":
                case "perfectbrain":
                    return 1f;
                case "dalatecht":
                    return 0.85f;
            }
        }
    }

    public float FocusedTrackVolume = 1.1f;
    public float UnfocusedTrackVolume = 0.45f;

    void Start()
    {
        Debug.LogFormat("AMP_TRACKS: Using Amplitude track controller!");
        Debug.LogFormat("AMP_TRACKS: Starting MidiReader...");

        // TODO: init MidiReader properly, with arguments
        // for now, it inits with default props inside of itself
        reader = gameObject.AddComponent<MidiReader>();
        reader.OnNoteEvent += Midireader_OnNoteEvent;
        Debug.LogFormat("MidiReader: created");

        reader.LoadMIDI(songName);
        bpm = reader.bpm;
        // TODO: Get fudge factor fron moggsong!
        Debug.LogFormat("AMP_TRACKS: Using tunnel scale fudge factor {0}", fudgeFactor);

        // set AMPController values
        amp_ctrl.songBpm = bpm;
        amp_ctrl.secPerBeat = ((float)bpm / 60f);

        // TODO: TEMP - Give AMPController the musical instruments we want to play
        // TODO: Dynamically create AudioSource and AudioClip objects for the tracks we have
        amp_ctrl.bgclick = (AudioClip)Resources.Load(string.Format("Songs/{0}_bgclick", songName));
        amp_ctrl.drums = (AudioClip)Resources.Load(string.Format("Songs/{0}_drums", songName));
        amp_ctrl.bass = (AudioClip)Resources.Load(string.Format("Songs/{0}_bass", songName));
        amp_ctrl.synth = (AudioClip)Resources.Load(string.Format("Songs/{0}_synth", songName));

        PopulateTracks();
        AMP_PopulateTracks();

        //gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z + fudgeFactor);
    }

    /// <summary>
    /// Create lanes and populate them with CATCH notes.
    /// </summary>
    public void AMP_PopulateTracks()
    {
        Debug.LogFormat(string.Format("AMP_TRACKS [{0}]: Populating tracks...", reader.songName)); // TODO: songName should be in TracksController!

        int counter = 1; // TODO: CATCH tracks are offset by +1 in the mid
        foreach (AmplitudeTrack track in trackList)
        {
            if (counter > 6) // TODO: early test
                break;

            Debug.LogFormat(string.Format("AMP_TRACKS/PopulateTracks(): working on {0} [{1}]...",
                track.name, track.Instrument));

            int trackID;
            if (track.ID.HasValue) // TODO: Tracks should have IDs when generated on the fly!
                trackID = track.ID.Value;
            else // for now, we'll just count the ID
                trackID = counter;

            // init midi properties inside Track from MidiReader
            track.reader = reader;
            track.ID = trackID - 1;
            track.ticks = reader.ticks;
            track.bpm = reader.bpm;
            track.fudgefactor = fudgeFactor;
            //track.TrackMidiEvents = reader.GetNoteOnEventsFromTrack(trackID);

            // get the note list
            List<NoteOnEvent> noteEvents = reader.GetNoteOnEventsFromTrack(trackID);

            // Create the notes!
            track.AMP_PopulateLanes(noteEvents);

            Debug.LogFormat("AMP_TRACKS/PopulateTracks(): Finished populating track! ({0} notes)", track.TrackMidiEvents.Count);
            counter++;
        }
    }

    public void ChangeVoumeByID(int id, float volume)
    {
        switch (id)
        {
            case 0:
                amp_ctrl.src_drums.volume = volume; break;
            case 1:
                amp_ctrl.src_bass.volume = volume; break;
            case 2:
                amp_ctrl.src_synth.volume = volume; break;
        }
    }
    public void UpdateTracksVolume(Track targetTrack)
    {
        foreach (Track track in trackList)
        {
            if (track == targetTrack)
                ChangeVoumeByID(track.ID.Value, FocusedTrackVolume);
            else
                ChangeVoumeByID(track.ID.Value, UnfocusedTrackVolume);
        }
    }

    private void Midireader_OnNoteEvent(object sender, EventArgs e)
    {
        //UnityEditor.EditorApplication.Beep();
    }
}
