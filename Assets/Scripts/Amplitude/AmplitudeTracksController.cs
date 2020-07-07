using Assets.Scripts.Amplitude;
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
    MidiReader reader;
    AmplitudeConductor conductor { get { return GameObject.Find("AMPController").GetComponent<AmplitudeConductor>(); } }

    public int BPM;

    /// <summary>
    /// TODO: Possibly play and read the notes in here instead of MidiReader
    /// </summary>

    void Start()
    {
        Debug.LogFormat("AMP_TRACKS: Using Amplitude track controller!");
        Debug.LogFormat("AMP_TRACKS: Starting MidiReader...");

        // TODO: init MidiReader properly, with arguments
        // for now, it inits with default props inside of itself
        reader = gameObject.AddComponent<MidiReader>();
        reader.OnNoteEvent += Midireader_OnNoteEvent;
        Debug.LogFormat("MidiReader: created");

        src_drums = conductor.src_drums;
        src_bass = conductor.src_bass;
        src_synth = conductor.src_synth;
        src_bgclick = conductor.src_bgclick;

        reader.LoadMIDI();
        BPM = reader.bpm;

        PopulateTracks();
        AMP_PopulateTracks();
    }

    public void AMP_PopulateTracks()
    {
        Debug.LogFormat(string.Format("AMP_TRACKS [{0}]: Populating tracks...", reader.songName)); // TODO: songName should be in TracksController!

        int counter = 1; // TODO: CATCH tracks are offset by +1 in the mid
        foreach (AmplitudeTrack track in trackList)
        {
            if (counter > 3) // TODO: early test
                break;

            Debug.LogFormat(string.Format("AMP_TRACKS/PopulateTracks(): working on {0} [{1}]...",
                track.name, track.Instrument));

            int trackID;
            if (track.ID.HasValue) // TODO: Tracks should have IDs when generated on the fly!
                trackID = track.ID.Value;
            else // for now, we'll just count the ID
                trackID = counter;

            // init midi properties inside Track from MidiReader
            track.ID = trackID - 1;
            track.ticks = reader.ticks;
            track.bpm = reader.bpm;
            track.TrackMidiEvents = reader.GetNoteOnEventsFromTrack(trackID);
            track.reader = reader;

            // Create the notes!
            track.AMP_PopulateLanes();

            Debug.LogFormat("AMP_TRACKS/PopulateTracks(): Finished populating track! ({0} notes)", track.TrackMidiEvents.Count);
            counter++;
        }
    }

    bool m_playStarted = false;
    public void Update()
    {
        if (m_playStarted)
            return;

        if (Input.GetKeyUp(KeyCode.Space))
        {
            m_playStarted = true;
            var playerctrl = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
            playerctrl.IsPlayerMoving = true;
            float bps = (float)BPM / 60f;
            float sbm = 1f / bps;
            //playerctrl.movementSpeed = 10f;
            //PlayMusic();
            //midireader.StartPlayback(1);
        }
    }

    AudioSource src_drums;
    AudioSource src_bass;
    AudioSource src_synth;
    AudioSource src_bgclick;

    public AudioClip drums;
    public AudioClip bass;
    public AudioClip synth;
    public AudioClip bgclick;

    public void PlayBGClick()
    {
        src_bgclick.PlayScheduled(AudioSettings.dspTime);
    }

    public void ChangeVoumeByID(int id, float volume)
    {
        switch (id)
        {
            case 0:
                src_drums.volume = volume; break;
            case 1:
                src_bass.volume = volume; break;
            case 2:
                src_synth.volume = volume; break;
        }
    }

    public void UpdateTracksVolume(Track targetTrack)
    {
        foreach (Track track in trackList)
        {
            if (track == targetTrack)
                ChangeVoumeByID(track.ID.Value, 1.1f);
            else
                ChangeVoumeByID(track.ID.Value, 0f);
        }
    }

    #region audio
    class AudioPlaybackEngine : IDisposable
    {
        private readonly IWavePlayer outputDevice;
        private readonly MixingSampleProvider mixer;

        public AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
        {
            outputDevice = new WaveOutEvent();
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
            mixer.ReadFully = true;
            outputDevice.Init(mixer);
            outputDevice.Play();
        }

        public void PlaySound(string fileName)
        {
            var input = new AudioFileReader(fileName);
            AddMixerInput(new AutoDisposeFileReader(input));
        }

        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            if (input.WaveFormat.Channels == mixer.WaveFormat.Channels)
            {
                return input;
            }
            if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }
            throw new NotImplementedException("Not yet implemented this channel count conversion");
        }

        public void PlaySound(CachedSound sound)
        {
            AddMixerInput(new CachedSoundSampleProvider(sound));
        }

        private void AddMixerInput(ISampleProvider input)
        {
            mixer.AddMixerInput(ConvertToRightChannelCount(input));
        }

        public void Dispose()
        {
            outputDevice.Dispose();
        }

        public static readonly AudioPlaybackEngine Instance = new AudioPlaybackEngine(44100, 2);
    }

    class CachedSound
    {
        public float[] AudioData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }
        public CachedSound(string audioFileName)
        {
            using (var audioFileReader = new AudioFileReader(audioFileName))
            {
                // TODO: could add resampling in here if required
                WaveFormat = audioFileReader.WaveFormat;
                var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
                var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
                int samplesRead;
                while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    wholeFile.AddRange(readBuffer.Take(samplesRead));
                }
                AudioData = wholeFile.ToArray();
            }
        }
    }

    class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound cachedSound;
        private long position;

        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            this.cachedSound = cachedSound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var availableSamples = cachedSound.AudioData.Length - position;
            var samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(cachedSound.AudioData, position, buffer, offset, samplesToCopy);
            position += samplesToCopy;
            return (int)samplesToCopy;
        }

        public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
    }

    class AutoDisposeFileReader : ISampleProvider
    {
        private readonly AudioFileReader reader;
        private bool isDisposed;
        public AutoDisposeFileReader(AudioFileReader reader)
        {
            this.reader = reader;
            this.WaveFormat = reader.WaveFormat;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (isDisposed)
                return 0;
            int read = reader.Read(buffer, offset, count);
            if (read == 0)
            {
                reader.Dispose();
                isDisposed = true;
            }
            return read;
        }

        public WaveFormat WaveFormat { get; private set; }
    }
    #endregion

    private void Midireader_OnNoteEvent(object sender, EventArgs e)
    {
        //UnityEditor.EditorApplication.Beep();
    }
}
