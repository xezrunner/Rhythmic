using System.IO;
using UnityEngine;
using static Logging;

namespace AMP_2016 {
    public class AMP2016_SongLoader : ISongLoader {
        public const string MOGGSONG_FILE_EXT = ".moggsong";
        public const string MIDI_FILE_EXT     = ".mid";

        // This here should be used for development testing purposes only:
        public static bool  ALLOW_JSON_FORMAT   = true;
        public const string JSON_FILE_NAME      = "song.json";
        public const string JSON_MIDI_FILE_NAME = "song.mid";
        public const string JSON_AUDIO_EXT      = ".ogg";

        // TODO: should this be static?
        public song_info load_song(string song_name, string lookup_path) {
            song_info song_info = new() { name = song_name, lookup_path = lookup_path };

            bool success = false;
            if (ALLOW_JSON_FORMAT) {
                log_warn("JSON support allowed. DO NOT SHIP!!!");

                string path_to_json_file = Path.Combine(lookup_path, JSON_FILE_NAME);
                log(LogLevel.IO, "path_to_json_file: %".interp(path_to_json_file));

                if (!File.Exists(path_to_json_file)) throw new ("JSON file doesn't exist!");

                json_info json_info = load_info_from_json(path_to_json_file);
                song_info.bpm = json_info.bpm;
                song_info.section_start_bars = json_info.section_start_bars;
                song_info.tunnel_scale = json_info.tunnel_scale;
                // ..

                success = true;
            }

            // TODO: We're going to have to load pan information from the .moggsong, as the JSON file
            // doesn't contain that information.
            // This could just be a hack, since the game isn't meant to ship with JSON support for AMP_2016 mode.

            if (!success) {
                string path_to_moggsong_file = Path.Combine(lookup_path, song_name, MOGGSONG_FILE_EXT);
                log("path_to_moggsong_file: %".interp(path_to_moggsong_file));

                if (!File.Exists(path_to_moggsong_file)) throw new("moggsong file doesn't exist!");

                song_info info = MoggsongLoader.load_as_song_info(path_to_moggsong_file);
                success = true;
            }

            // Load MIDI:
            string path_to_midi_file;
            if (ALLOW_JSON_FORMAT && success) path_to_midi_file = Path.Combine(lookup_path, JSON_MIDI_FILE_NAME);
            else                              path_to_midi_file = Path.Combine(lookup_path, song_name, MIDI_FILE_EXT);
            if (!File.Exists(path_to_midi_file)) throw new("MIDI file doesn't exist!");

            midi_info midi_info = MidiLoader.load_midi(path_to_midi_file);

            // Load audio info:
            string[] audio_paths = new string[midi_info.track_count];
            if (ALLOW_JSON_FORMAT) {
                for (int i = 0; i < midi_info.track_count; ++i) {
                    string track_name = midi_info.tracks[i].name;
                    // Replace ':' with '_', as per JSON song standard:
                    track_name = track_name.Replace(':', '_');
                    // Add audio file extension:
                    track_name += JSON_AUDIO_EXT;
                    // Build full path to audio:
                    string path = Path.Combine(lookup_path, "audio", track_name);

                    bool path_exists = File.Exists(path);
                    log(path_exists ? LogLevel.IO : LogLevel.IO | LogLevel.Error, 
                        "[%]: exists: %  path: %".interp(i, path_exists ? '1' : '0', path));

                    audio_paths[i] = path;
                }
            } else log_error("We don't load .mogg files yet.");

            // Build tracks:
            song_info.track_count = midi_info.track_count;
            song_info.tracks = new song_track[song_info.track_count];
            for (int i = 0; i < song_info.track_count; ++i) {
                song_info.tracks[i] = new song_track() {
                    name = midi_info.tracks[i].name,
                    id = i,
                    is_playable = midi_info.tracks[i].is_playable_track,

                    audio_path = audio_paths[i],
                    audio_exists = File.Exists(audio_paths[i])
                };
            }

            // Notes:
            for (int i = 0; i < song_info.track_count; ++i) {
                midi_track track = midi_info.tracks[i];
                song_info.tracks[i].notes = new song_note[track.notes.Count];
                for (int x = 0; x < track.notes.Count; ++x) {
                    midi_note midi_note = track.notes[i];
                    song_note note = new() {
                        at_ticks = midi_note.on_ticks,
                        duration_ticks = midi_note.duration_ticks,
                        lane = AMP2016_Constants.get_lane_index_from_note_number(midi_note.note_number)
                    };
                    song_info.tracks[i].notes[x] = note;
                }
            }

            return song_info;
        }

        public struct json_info {
            public float tunnel_scale;
            public int[] section_start_bars;
            public float bpm;
            public float preview_start_ms;
            public float preview_length_ms;

            public string title;
            public string artist;
            public string description;
        }
        public static json_info load_info_from_json(string file_path) {
            string json_text = File.ReadAllText(file_path);
            return JsonUtility.FromJson<json_info>(json_text);
        }
    }
}