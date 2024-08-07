using NAudio.Midi;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Logging;

namespace AMP_2016 {
    public struct midi_note {
        public long on_ticks;
        public long off_ticks;
        public int  duration_ticks;
        public int  note_number;
    }

    public enum MidiTextEventType { GeneralText, Lyric, Comment, UNKNOWN = -1 }
    public struct midi_text_event {
        public MidiTextEventType text_type;
        public int    on_ticks;
        public string text;
    }

    public struct midi_track {
        public string name;
        public int    midi_id;
        public bool   is_playable_track;
        public List<midi_note>       notes;
        public List<midi_text_event> text_events;
    }

    public struct midi_info {
        public string midi_file_path;
        public int    bpm;
        public int    delta_ticks_pqn;

        public int          track_count;
        public midi_track[] tracks;

        public int         note_event_count;
        public midi_note[] note_events;
    }

    public static class MidiLoader {
        public static midi_info load_midi(string file_path) {
            log(LogLevel.IO, "loading midi file at '%'".interp(file_path));
            if (!File.Exists(file_path)) throw new("MIDI file doesn't exist!");

            byte[] bytes  = File.ReadAllBytes(file_path);
            MemoryStream stream = new(bytes);
            MidiFile midi_file = new(stream, false);

            // Tracks:
            List<midi_track> tracks = new();
            for (int i = 0; i < midi_file.Tracks; ++i) {
                // 0 SequenceTrackName Tx Catch:T:track name
                string track_name = midi_file.Events[i][0].ToString();
                string[] split = track_name.Split(' ');
                track_name = string.Join(' ', split, 2, split.Length - 2);

                // TODO: bg_click!
                if (!track_name.StartsWith("T") && !char.IsDigit(track_name[1])) continue;
                // log("[%]: %".interp(i, track_name));

                midi_track track = new() {
                    name = track_name,
                    midi_id = i,
                    notes = new(),
                    is_playable_track = !track_name.ToLower().Contains("bg_click") && !track_name.ToLower().Contains("freestyle")
                };

                tracks.Add(track);
            }

            // Notes:
            // For each track (i):
            for (int i = 0; i < tracks.Count; ++i) {
                foreach (MidiEvent midi_event in midi_file.Events[tracks[i].midi_id]) {
                    if (midi_event.CommandCode != MidiCommandCode.NoteOn) continue;

                    NoteOnEvent note_event = (NoteOnEvent)midi_event;
                    // TODO: tick correction?

                    foreach (int note_num in AMP2016_Constants.note_numbers_for_difficulties) {
                        if (note_event.NoteNumber != note_num) continue;
                        midi_note note = new() {
                            on_ticks = note_event.AbsoluteTime,
                            off_ticks = -1,
                            duration_ticks = note_event.NoteLength,
                            note_number = note_event.NoteNumber
                        };
                        tracks[i].notes.Add(note);
                        break;
                    }
                }
            }

            midi_info info = new() {
                midi_file_path = file_path,
                delta_ticks_pqn = midi_file.DeltaTicksPerQuarterNote,

                track_count = tracks.Count,
                tracks = tracks.ToArray(),

                bpm = -1 // Read from moggsong instead
            };

            log("listing T tracks from MIDI:");
            for (int i = 0; i < info.track_count; ++i)
                log("track [%]: \"%\"  note event count: %".interp(i, 
                    info.tracks[i].name.PadRight(info.tracks.Max(t => t.name.Length)),
                    info.tracks[i].notes.Count));

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