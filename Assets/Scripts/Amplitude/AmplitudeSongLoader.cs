using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using NAudio.Midi;
using UnityEngine;
using UnityEngine.Networking;

public partial class SongLoader
{
    static SongInfo AMPLITUDE_LoadSongData(string file_name)
    {
        MoggSong meta = MoggSong.LoadMoggSong(file_name);
        AMPMidi_Info midi_info = AmplitudeMidiReader.LoadMIDI(file_name);

        SongInfo info = new SongInfo()
        {
            song_name = file_name, // TODO: name should probably be gathered from MoggSong?
            song_bpm = meta.songBpm,
            tunnel_scale = meta.songFudgeFactor,
            song_length_bars = meta.songLengthInMeasures,
            song_countin = meta.songCountInTime,
            // TODO: more data...
        };

        // Time units:
        SongTimeUnit time_units = new SongTimeUnit(info.song_bpm);
        info.time_units = time_units;

        // Track names:
        info.song_tracks = midi_info.song_tracks;
        info.midi_tracks = midi_info.midi_tracks;

        // Meta notes:
        AMPLITUDE_LoadNoteData(midi_info, meta, ref info);
        
        return info;
    }
    
    // TODO: Should this really use a ref param?
    static void AMPLITUDE_LoadNoteData(AMPMidi_Info midi_info, MoggSong meta, ref SongInfo info)
    {
        info.data_notes = new MetaNote[midi_info.track_count, meta.songLengthInMeasures][];

        // TODO: Probably should move this elsewhere, to the /Amplitude folder.
        for (int t = 0; t < midi_info.track_count; ++t)
        {
            List<NoteOnEvent> note_events = midi_info.song_note_events[t];
            int note_count = note_events.Count;

            if (note_count == 0)
            {
                Logger.Log("No note events for track %", midi_info.song_tracks[t]);
                continue;
            }
            
            List<MetaNote> note_list = new List<MetaNote>();
            for (int i = 0; i < note_count; ++i)
            {
                NoteOnEvent note = note_events[i];
                LaneSide lane_side = AmplitudeGame.GetLaneSideFromNoteNumber(note.NoteNumber);
                if (lane_side == LaneSide.UNKNOWN) { Logger.LogConsoleW("Unknown LaneSide for note ID % at track %", i, midi_info.song_tracks[t]); continue; }
                // TODO: Amplitude Note types!!!
                NoteType note_type = NoteType.Generic;

                // TODO: Distance checking! Only 1 note can exist across all lanes for a specific distance!
                float distance = info.time_units.TickToPos(note.AbsoluteTime);
                int measure = (int)note.AbsoluteTime / 1920; // TODO: const measure ticks!!!

                // TODO: note name!
                string note_name = $"CATCH/T{t} :: {i}";

                /// Final meta note creation
                MetaNote meta_note = new MetaNote()
                {
                    Name = note_name,
                    TrackID = t,
                    TotalID = i,
                    MeasureID = measure,
                    Distance = distance,
                    TimeMs = info.time_units.TickToMs(note.AbsoluteTime),
                    Type = note_type,
                    Lane = lane_side
                };

                note_list.Add(meta_note);
            }
            
            info.total_note_count += note_list.Count;
            for (int i = 0; i < meta.songLengthInMeasures; ++i)
                info.data_notes[t, i] = note_list.Where(n => n.MeasureID == i).ToArray();
        }
    }
}

public partial class GenericSongController
{
    public IEnumerator AMPLITUDE_LoadAudioClips(SongInfo song_info)
    {
        List<AudioClip> list = new List<AudioClip>();
        
        for (int i = 0; i < song_info.song_tracks.Count; ++i)
        {
            string track = song_info.song_tracks[i];
            // TODO: improve:
            string path = Path.Combine(AmplitudeGame.song_ogg_path, song_info.song_name, "audio", song_info.midi_tracks[i] + ".ogg");
            
            if (!File.Exists(path))
            {
                Logger.LogW("File not found: '%'".TM("SongLoader"), path);
                list.Add(null);
                continue;
            }
            
            AudioClip clip = null;
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.OGGVORBIS))
            {
                if (RhythmicGame.AllowSongStreaming) ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;
                www.SendWebRequest();
                while (!www.isDone) yield return null;
                
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Logger.LogMethodW("Failed to load audioclip: " + path);
                    continue;
                }
                else
                    clip = DownloadHandlerAudioClip.GetContent(www);
            }
            clip.name = track;
            list.Add(clip);
        }

        audio_clips = list;
        audio_clips_loaded = true;
    }
}