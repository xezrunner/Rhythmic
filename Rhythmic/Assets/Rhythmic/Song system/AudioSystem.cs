// Static:  Only full audio tracks play.
// Dynamic: Individual instruments make up the song.
// Mixed:   Both full audio tracks and instruments are mixed.
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using static Logger;

public enum AudioSystemMode { Static = 0, Dynamic = 1, Mixed = 2, UNKNOWN = -1 }

public class AudioSystem : MonoBehaviour
{
    Game Game = Game.Instance;
    SongSystem SongSystem = SongSystem.Instance;
    Clock Clock;
    public AudioSystemMode audio_system_mode;

    // TODO: Dynamic mode might have its own component!

    public AudioClip[] static_clips;
    public AudioSource[] static_sources;

    public void SetupAudioSystem(Song song)
    {
        // Set Audio system mode:
        if (Game.game_type == GameType.Amplitude2016)
            audio_system_mode = AudioSystemMode.Static; // AMP2016 only supports static songs!
        else { /* RHX ...*/ }

        Clock = SongSystem.clock;

        static_clips = new AudioClip[song.track_count];
        static_sources = new AudioSource[song.track_count];

        foreach (Song_Track t in song.tracks)
        {
            if (!File.Exists(t.audio_path)) continue;
            StartCoroutine(COROUTINE_LoadAudioForTrack(t));
        }
    }

    // TODO: Variables should move to Variables class!
    public static bool AUDIO_AllowStreaming = true;

    // Using a coroutine to load / stream songs in the background.
    public IEnumerator COROUTINE_LoadAudioForTrack(Song_Track track)
    {
        AudioClip clip = null;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(track.audio_path, AudioType.OGGVORBIS))
        {
            DownloadHandlerAudioClip download_handler = (DownloadHandlerAudioClip)www.downloadHandler;
            download_handler.streamAudio = AUDIO_AllowStreaming;

            www.SendWebRequest();
            while (!www.isDone) yield return null;

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                LogE("Failed to load audioclip: %", track.audio_path);
                yield break;
            }
            else
                clip = DownloadHandlerAudioClip.GetContent(www);
        }

        clip.name = track.name;
        static_clips[track.id] = clip;

        // Create AudioSource: 
        {
            GameObject obj = new GameObject("AudioSource: %".Parse(track.name));
            obj.transform.parent = transform;

            AudioSource src = obj.AddComponent<AudioSource>();
            src.clip = clip;

            static_sources[track.id] = src;
        }
    }

    // Playback:

    public float audio_progress;
    public float audio_deltatime;
    public float audio_timescale;

    public void AUDIO_Play() { }
    public void AUDIO_Pause() { }
    public void AUDIO_Resume() { }
    public void AUDIO_TogglePause() { }
    public void AUDIO_Restart() { }
}