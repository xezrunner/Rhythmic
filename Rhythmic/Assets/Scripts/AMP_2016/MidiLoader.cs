using NAudio.Midi;
using System.Collections.Generic;
using System.IO;
using static Logging;

namespace AMP_2016 {
    public struct midi_note {
        public int on_ticks;
        public int off_ticks;
        public int duration_ticks;
        public int note_type_id;
    }

    public enum MidiTextEventType { GeneralText, Lyric, Comment, UNKNOWN = -1 }
    public struct midi_text_event {
        public MidiTextEventType text_type;
        public int    on_ticks;
        public string text;
    }

    public struct midi_track {
        public string name;
        public midi_note[]       notes;
        public midi_text_event[] text_events;
    }

    public struct midi_info {
        public string midi_file_path;
        public int    bpm;
        public int    delta_ticks_pqn;

        public int          track_count;
        public midi_track[] tracks;
    }

    public static class MidiLoader {
        public static midi_info load_midi(string file_path) {
            log("loading midi file at '%'".interp(file_path), LogLevel.IO);
            if (!File.Exists(file_path)) throw new("MIDI file doesn't exist!");

            byte[] bytes  = File.ReadAllBytes(file_path);
            MemoryStream stream = new(bytes);
            MidiFile midi_file = new(stream, false);

            List<midi_track> tracks = new();
            for (int i = 0; i < midi_file.Tracks; ++i) {
                // 0 SequenceTrackName Tx Catch:T:track name
                string track_name = midi_file.Events[i][0].ToString();
                string[] split = track_name.Split(' ');
                track_name = string.Join(' ', split, 2, split.Length - 2);

                if (!track_name.StartsWith("T") && !char.IsDigit(track_name[1])) continue;
                // log("[%]: %".interp(i, track_name));

                midi_track track = new() { name = track_name };
                tracks.Add(track);
            }

            midi_info info = new() {
                midi_file_path = file_path,
                delta_ticks_pqn = midi_file.DeltaTicksPerQuarterNote,
                track_count = tracks.Count,
                bpm = -1, // Read from moggsong instead
                tracks = tracks.ToArray()
            };

            log("listing T tracks from MIDI:");
            for (int i = 0; i < info.track_count; ++i)
                log("track [%]: \"%\"".interp(i, info.tracks[i].name));

            return info;
        }

        public static int get_bpm(MidiFile midi_file) {
            foreach (MidiEvent midevent in midi_file.Events[0]) {
                if (midevent is TempoEvent) {
                    TempoEvent tempo_event = (TempoEvent)midevent;
                    return 60000 / (tempo_event.MicrosecondsPerQuarterNote / 1000);
                }
            }
            throw new("MidiReader: tempo event not found or BPM is 0!");
        }
    }
}