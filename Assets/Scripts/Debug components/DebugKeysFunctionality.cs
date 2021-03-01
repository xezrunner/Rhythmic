using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

// A collection of debug functions used in the DebugKeys debug component.
// TODO: repurpose as a general debug functionality class?

public partial class DebugKeys
{
    DebugUI DebugUI { get { return DebugUI.Instance; } }

    Clock Clock { get { return Clock.Instance; } }
    SongController SongController { get { return SongController.Instance; } }
    TracksController TracksController { get { return TracksController.Instance; } }

    void AddToDebugLine(string text) => DebugUI.AddToDebugLine(text);

    // ----

    #region Android song switching
#if UNITY_ANDROID
    int android_songcounter = 0;

    void Android_HandleSongSwitching()
    {
        // Test code for changing songs on an Android device
        if (Touchscreen.current != null)
        {
            if (Touchscreen.current.press.wasPressedThisFrame)
            {
                if (android_songcounter == 4) android_songcounter = 0;
                switch (android_songcounter)
                {
                    case 0:
                        AMP_ChangeSong("tut0"); break;
                    case 1:
                        AMP_ChangeSong("perfectbrain"); break;
                    case 2:
                        AMP_ChangeSong("dreamer"); break;
                    case 3:
                        AMP_ChangeSong("dalatecht"); break;
                }
                android_songcounter++;
            }
        }
    }
#endif
    #endregion

    public void AMP_ChangeSong(string value)
    {
        SongController.songName = value;
        AddToDebugLine($"Song changed: {value} - press R to restart.");
    }
    void HandleSongSwitching()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
            AMP_ChangeSong("tut0");
        else if (Input.GetKeyDown(KeyCode.Alpha1))
            AMP_ChangeSong("perfectbrain");
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            AMP_ChangeSong("dreamer");
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            AMP_ChangeSong("dalatecht");

#if UNITY_ANDROID
      Android_HandleSongSwitching();  
#endif
    }

    // -----

    void DEBUG_DisableWorld()
    {
        var world = GameObject.Find("World");
        var worldCamera = GameObject.Find("WorldCamera");

        world.SetActive(!world.activeInHierarchy); worldCamera.SetActive(!world.activeInHierarchy);
    }
    void DEBUG_ToggleRenderingPath()
    {
        if (Camera.main.renderingPath == RenderingPath.Forward)
            Camera.main.renderingPath = RenderingPath.DeferredShading;
        else
            Camera.main.renderingPath = RenderingPath.Forward;

        Logger.LogMethod(Camera.main.renderingPath.ToString(), "Debug");
    }
    void DEBUG_SetFramerateLock(int framerate)
    {
        RhythmicGame.SetFramerate(framerate, Input.GetKey(KeyCode.LeftShift) ? 1 : 0);
    }
    void DEBUG_SetPreferredResolution(Vector2 resolution)
    {
        RhythmicGame.PreferredResolution = resolution;
        RhythmicGame.SetResolution(RhythmicGame.PreferredResolution);
    }

    // -----

    void DEBUG_ToggleTunnelMode()
    {
        RhythmicGame.IsTunnelMode = !RhythmicGame.IsTunnelMode;
        AddToDebugLine($"Tunnel mode: {(RhythmicGame.IsTunnelMode ? "ON" : "OFF")} - press R to restart.");
    }

    void DEBUG_RefreshSequencesNotes(bool currentTrack = false)
    {
        TracksController.Instance.RefreshSequences(currentTrack ? TracksController.CurrentTrack : null);
        TracksController.Instance.RefreshTargetNotes(currentTrack ? TracksController.CurrentTrack : null);
        Logger.LogWarning($"Debug: Refreshing with {(currentTrack ? "current" : "no")} track specified!");
    }
    void DEBUG_CaptureMeasureAmount(AmpTrack track = null, int count = 7, int start = -1)
    {
        if (start == -1)
            start = Clock.Fbar;

        if (track == null)
            TracksController.CaptureMeasureAmount(start, count, TracksController.Tracks);
        else
            TracksController.CaptureMeasureAmount(start, count, track);
    }
    void DEBUG_RestoreCapturedTracks()
    {
        Logger.LogMethodE("not yet implemented!");
    }

    void DEBUG_OffsetSong(float offset = 2f)
    {
        AmpPlayerLocomotion.Instance.DistanceTravelled += offset * SongController.posInSec;
        SongController.Instance.OffsetSong(offset);
        Clock.Instance.seconds += offset;
    }
    bool? prevSmooth;
    void HandleSongOffsetting()
    {
        if (!prevSmooth.HasValue) prevSmooth = AmpPlayerLocomotion.Instance.SmoothEnabled;
        AmpPlayerLocomotion.Instance.SmoothEnabled = false; // Disable smoothing in Locomotion

        if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.numpad8Key.isPressed)
            DEBUG_OffsetSong(1);
        else if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.numpad8Key.isPressed)
            DEBUG_OffsetSong(0.1f);
        else if (Keyboard.current.leftShiftKey.isPressed && Keyboard.current.numpad8Key.isPressed)
            DEBUG_OffsetSong(2);
        /* backwards - UNSTABLE */
        else if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.numpad2Key.isPressed)
            DEBUG_OffsetSong(-2f);
        else if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.numpad2Key.isPressed)
            DEBUG_OffsetSong(-0.1f);

        else // Restore smoothing in Locomotion
            AmpPlayerLocomotion.Instance.SmoothEnabled = prevSmooth.Value;
    }
    void DEBUG_HandleTimescale()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Keypad8)) // up
            RhythmicGame.SetTimescale(Mathf.Clamp(Time.timeScale + 0.1f, 0, 1));
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Keypad2)) // down
            RhythmicGame.SetTimescale(Mathf.Clamp(Time.timeScale - 0.1f, 0, 1));
        if (Input.GetKeyDown(KeyCode.Keypad1)) // one
            RhythmicGame.SetTimescale(1f);
    }

    // -----

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
}