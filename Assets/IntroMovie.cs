using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static Logger;

public class IntroMovie : MonoBehaviour
{
    public AudioSource audio_src;
    public List<AudioClip> audio_clips = new List<AudioClip>();

    public TMP_Text xesign_text;
    public TMP_Text rhythmic_text;

    public test2 progress_indicator;
    
    [Header("Properties")]
    public float fade_factor = 0.05f;
    public float audio_target_vol = 0.1f;
    
    void Start()
    {
        StartCoroutine(Main_Coroutine());
    }
    
    IEnumerator Audio_Fade(float target)
    {
        float dir = (audio_src.volume > target) ? -fade_factor : fade_factor;
        while (audio_src.volume < target)
        {
            audio_src.volume += dir * Time.deltaTime;
            yield return null;
        }
    }
    
    IEnumerator Main_Coroutine()
    {
        xesign_text.color = new Color(xesign_text.color.r, xesign_text.color.g, xesign_text.color.b, 0);
        rhythmic_text.color = new Color(rhythmic_text.color.r, rhythmic_text.color.g, rhythmic_text.color.b, 0);
        audio_src.volume = 0f;
        
        yield return new WaitForSeconds(1);
        
        audio_src.PlayOneShot(audio_clips[0]);
        Log("Playing park!");
        
        Log("Fading in - audio!");
        StartCoroutine(Audio_Fade(audio_target_vol));
        
        yield return new WaitForSeconds(1);
        Log("Playing children!");
        audio_src.PlayOneShot(audio_clips[1]);
        
        yield return new WaitForSeconds(3);
        LogW("Playing boom!");
        audio_src.PlayOneShot(audio_clips[2]);
        
        Log("Fading in - text!");
        while (xesign_text.color.a < 1f)
        {
            xesign_text.color = new Color(xesign_text.color.r, xesign_text.color.g, xesign_text.color.b, xesign_text.color.a + Time.deltaTime);
            yield return null;
        }
        
        yield return new WaitForSeconds(3);
        
        LogE("Starting progress!");
        progress_indicator.Play();
        
        Log("Fading out - text!");
        while (xesign_text.color.a > 0f)
        {
            xesign_text.color = new Color(xesign_text.color.r, xesign_text.color.g, xesign_text.color.b, xesign_text.color.a - Time.deltaTime);
            yield return null;
        }

        Log("Fading in - branding!");
        rhythmic_text.gameObject.SetActive(true);
        while (rhythmic_text.color.a < 1f)
        {
            rhythmic_text.color = new Color(rhythmic_text.color.r, rhythmic_text.color.g, rhythmic_text.color.b, rhythmic_text.color.a + Time.deltaTime);
            yield return null;
        }
        
        yield return new WaitForSeconds(2);
        
        Log("Fading out - audio!");
        while (audio_src.volume > 0)
        {
            audio_src.volume -= fade_factor * Time.deltaTime;
            //xesign_text.color = new Color(xesign_text.color.r, xesign_text.color.g, xesign_text.color.b, xesign_text.color.a - Time.deltaTime);
            yield return null;
        }
    }
}
