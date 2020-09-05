using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class tut_test_script : MonoBehaviour
{
    SongController SongController { get { return SongController.Instance; } }
    TracksController TracksController { get { return (TracksController)TracksController.Instance; } }
    CatcherController CatcherController { get { return CatcherController.Instance; } }
    Player Player { get { return Player.Instance; } }

    public AudioSource audioSrc;
    public List<AudioClip> audioClips;

    public List<GameObject> tut0_start_text;
    public CanvasRenderer tut0_start_panel;
    public CanvasRenderer tut0_end_panel;

    enum VO { Intro = 0, Streak = 1, Bass = 2, Synth = 3, Success = 4, Fail = 5 }

    int currentTrackID = 0;
    int targetMeasure = 0;

    public void Start()
    {
        BeginScript();
    }

    async void BeginScript()
    {
        if (SongController.songName != "tut0" || Input.GetKey(KeyCode.X))
        return;

        if (SceneManager.GetSceneByName("Loading").isLoaded)
        {
            while (RhythmicGame.IsLoading)
                await Task.Delay(200);
        }

        tut0_start_panel.gameObject.SetActive(true);
        tut0_start_text.ForEach(g => g.SetActive(false));

        while (RhythmicGame.IsLoading)
            await Task.Delay(200);

        Player.ScoreText.gameObject.SetActive(false);
        Player.IsMultiplierEnabled = false;

        TracksController.OnTrackCaptureStart += TracksController_OnTrackCaptureStart;
        TracksController.OnTrackCaptured += TracksController_OnTrackCaptured;

        // hide other tracks
        TracksController.Tracks.ForEach(t => { if (t.ID > 0) t.TUT_SetTrackEnabledState(false); });

        tut0_start_text.ForEach(g => g.SetActive(true));
        PlayVO(VO.Intro);
        await Task.Delay(2000);
        Player.BeginPlay();
        await Task.Delay(1000);
        startPanelFading = true; fadeValue = 1f;
        await Task.Delay(7000);
        tut0_start_text.ForEach(g => g.SetActive(false));
        Player.ScoreText.gameObject.SetActive(true);
    }

    void PlayVO(VO line) { audioSrc.PlayOneShot(audioClips[(int)line]); }

    private async void TracksController_OnTrackCaptureStart(object sender, int[] e)
    {
        Player.ScoreText.gameObject.SetActive(false);

        CatcherController.FindNextMeasureEnabled = false;
        CatcherController.ShouldHit.Clear();
        Track track = TracksController.Tracks[e[0]];
        //track.trackMeasures.ForEach(m => { if (m.measureNum >= e[2]) m.CaptureMeasureImmediate(); });
        track.TUT_IsTrackEnabled = false;

        targetMeasure = CatcherController.CurrentMeasureID + 2;

        await Task.Delay(500);
        PlayVO(VO.Streak);

        while (CatcherController.CurrentMeasureID != targetMeasure)
            await Task.Delay(100);

        if (currentTrackID == 0)
            PlayVO(VO.Bass);
        else if (currentTrackID == 1)
            PlayVO(VO.Synth);
        else
        {
            //await Task.Delay(2000);
            PlayVO(VO.Success);
            ShowEndPanel();
            await Task.Delay(4000);
            DoFailTest();
            await Task.Delay(3000);
            RhythmicGame.Restart();
            return;
        }

        NextTrack();
    }

    public async void DoFailTest()
    {
        int msCounter = 50;

        for (float i = 1f; i > 0f; i -= 0.1f)
        {
            await Task.Delay(msCounter);
            RhythmicGame.SetTimescale(i);
            msCounter -= 5;
        }
    }

    void ShowEndPanel()
    {
        tut0_end_panel.gameObject.SetActive(!endPanelFading);
        tut0_end_panel.SetAlpha(0f); fadeValue = 0f;
        endPanelFading = !endPanelFading;
    }

    float fadeValue = 0f;
    bool endPanelFading = false;
    bool startPanelFading = false;
    bool worldAnimPlayed = false;
    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Y))
            ShowEndPanel();
        */

        CanvasRenderer panel = null;
        if (endPanelFading || startPanelFading)
            panel = startPanelFading ? tut0_start_panel : tut0_end_panel;

        if (endPanelFading)
            fadeValue += Time.deltaTime / 2;
        else if (startPanelFading)
            fadeValue -= Time.deltaTime / 2;

        if (endPanelFading || startPanelFading)
            panel.SetAlpha(Mathf.Clamp(fadeValue, 0f, 1f));

        if (endPanelFading & fadeValue >= 1f)
            endPanelFading = false;
        else if (startPanelFading & fadeValue <= 0f)
            startPanelFading = false;

        if (SongController.songName != "tut0" & SongController.IsPlaying & !worldAnimPlayed)
        { GameObject.Find("WORLD_TUT").GetComponent<Animation>().Play(); worldAnimPlayed = true; }
    }

    void NextTrack()
    {
        currentTrackID++; // increase counter
        TracksController.Tracks[currentTrackID].TUT_SetTrackEnabledState();

        Player.SetScore(0);
        Player.ScoreText.gameObject.SetActive(true);

        // get next track
        Track track = TracksController.Tracks[currentTrackID];
        track.trackMeasures.ForEach(m => { if (m.measureNum <= CatcherController.CurrentMeasureID) m.CaptureMeasureImmediate(true); }); // capture prev unplayed measures
        track.TUT_SetTrackEnabledState(true); // enable next track

        // find next ShouldHit notes
        CatcherController.FindNextMeasureEnabled = true;
        CatcherController.FindNextMeasuresNotes();
    }

    private void TracksController_OnTrackCaptured(object sender, int[] e)
    {

    }
}
