using System.Collections;
using UnityEngine;
using TMPro;

public class FontLoadingIndicator : MonoBehaviour
{
    public TMP_Text Text;
    public float Framerate = 60f;

    public char start_char = '\ue100';
    public char end_char = '\ue176';

    void Awake()
    {
        if (is_playing) Play();
        else Stop();
    }

    [Header("Default values: ")]
    public bool is_playing = true;
    public bool IsPlaying
    {
        get { return is_playing; }
        set { is_playing = value; if (!is_playing) Play(); else Stop(); }
    }

    public void Play()
    {
        StartCoroutine(Main_Coroutine());
        Text.text = start_char.ToString();
    }

    public void Stop()
    {
        stop_requested = true;
        Text.text = null;
        stop_requested = false; // TODO: what?
    }

    bool stop_requested = false;
    IEnumerator Main_Coroutine()
    {
        char c = start_char;
        float elapsed_ms = 0f;

        while (true)
        {
            Text.text = c.ToString();
            c++; if (c > end_char) c = start_char;

            if (stop_requested) yield break;

            float fps_ms = (1000f / Framerate); // This could be defined above the loop - we're keeping it here in case we want to adjust framerate live.
            while (elapsed_ms < fps_ms)
            {
                elapsed_ms += Time.deltaTime * 1000f;
                yield return null;
            }
            elapsed_ms = 0f;
        }
    }
}
