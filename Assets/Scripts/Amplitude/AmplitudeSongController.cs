using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Amplitude
{
    public class AmplitudeSongController : MonoBehaviour
    {
        /// <summary>
        /// TODO: Implement better functionality
        /// </summary>

        //Song beats per minute - this is determined by the song you're trying to sync up to
        public float songBpm;

        //The number of seconds for each song beat
        public float secPerBeat;

        //Current song position, in seconds
        public float songPosition;

        //Current song position, in beats
        public float songPositionInBeats;

        //How many seconds have passed since the song started
        public float dspSongTime;

        //The offset to the first beat of the song in seconds
        public float firstBeatOffset;

        //An AudioSource attached to this GameObject that will play the music.
        public AudioSource src_drums;
        public AudioSource src_bass;
        public AudioSource src_synth;
        public AudioSource src_bgclick;

        public AudioClip drums;
        public AudioClip bass;
        public AudioClip synth;
        public AudioClip bgclick;

        void Start()
        {
            src_drums = gameObject.AddComponent<AudioSource>();
            src_bass = gameObject.AddComponent<AudioSource>();
            src_synth = gameObject.AddComponent<AudioSource>();
            src_bgclick = gameObject.AddComponent<AudioSource>();

            //Load the AudioSource attached to the ampctrl GameObject
            //src_bgclick = GetComponent<AudioSource>();

            //Calculate the number of seconds in each beat
            secPerBeat = 60f / songBpm;
        }

        public void PlayMusic()
        {
            // Assign clips to AudioSources
            src_drums.clip = drums;
            src_bass.clip = bass;
            src_synth.clip = synth;
            src_bgclick.clip = bgclick;

            //Record the time when the music starts
            dspSongTime = (float)AudioSettings.dspTime;

            //Start the music
            src_bgclick.PlayScheduled(AudioSettings.dspTime);
            src_drums.PlayScheduled(AudioSettings.dspTime);
            src_bass.PlayScheduled(AudioSettings.dspTime);
            src_synth.PlayScheduled(AudioSettings.dspTime);
        }

        void Update()
        {
            //determine how many seconds since the song started
            songPosition = (float)(AudioSettings.dspTime - dspSongTime - firstBeatOffset);

            //determine how many beats since the song started
            songPositionInBeats = songPosition / secPerBeat;
        }
    }
}
