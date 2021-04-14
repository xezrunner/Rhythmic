using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public static class DebugFunctionality
{
    // A collection of debug functions.

    public static DebugUI DebugUI { get { return DebugUI.Instance; } }
    public static Clock Clock { get { return Clock.Instance; } }
    public static SongController SongController { get { return SongController.Instance; } }
    public static TracksController TracksController { get { return TracksController.Instance; } }

    public static void AddToDebugLine(string text) => DebugUI.AddToDebugLine(text);

    // ----
    #region Android song switching
#if UNITY_ANDROID
    int android_songcounter = 0;

    public static void Android_HandleSongSwitching()
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

    public static void AMP_ChangeSong(string value)
    {
        SongController.songName = value;
        AddToDebugLine($"Song changed: {value} - press R to restart.");
    }
    public static void HandleSongSwitching()
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

    public static void DEBUG_DisableWorld()
    {
        var world = GameObject.Find("World");
        var worldCamera = GameObject.Find("WorldCamera");

        world.SetActive(!world.activeInHierarchy); worldCamera.SetActive(!world.activeInHierarchy);
    }
    public static void DEBUG_ToggleRenderingPath()
    {
        if (Camera.main.renderingPath == RenderingPath.Forward)
            Camera.main.renderingPath = RenderingPath.DeferredShading;
        else
            Camera.main.renderingPath = RenderingPath.Forward;

        Logger.LogMethod(Camera.main.renderingPath.ToString(), "Debug");
    }
    public static void DEBUG_SetFramerateLock(int framerate, bool vsync = false)
    {
        RhythmicGame.SetFramerate(framerate, vsync ? 1 : 0);
    }
    public static void DEBUG_SetPreferredResolution(Vector2 resolution)
    {
        RhythmicGame.PreferredResolution = resolution;
        RhythmicGame.SetResolution(RhythmicGame.PreferredResolution);
    }

    // -----

    public static void DEBUG_ToggleTunnelMode()
    {
        RhythmicGame.IsTunnelMode = !RhythmicGame.IsTunnelMode;
        AddToDebugLine($"Tunnel mode: {(RhythmicGame.IsTunnelMode ? "ON" : "OFF")} - press R to restart.");
    }

    public static void DEBUG_RefreshSequencesNotes(bool currentTrack = false)
    {
        TracksController.Instance.RefreshSequences(currentTrack ? TracksController.CurrentTrack : null);
        TracksController.Instance.RefreshTargetNotes(currentTrack ? TracksController.CurrentTrack : null);
        Logger.LogWarning($"Debug: Refreshing with {(currentTrack ? "current" : "no")} track specified!");
    }
    public static void DEBUG_CaptureMeasureAmount(AmpTrack track = null, int count = 7, int start = -1)
    {
        if (start == -1)
            start = Clock.Fbar;

        if (track == null)
            TracksController.CaptureMeasureAmount(start, count, TracksController.MainTracks);
        else
            TracksController.CaptureMeasureAmount(start, count, track);
    }
    public static void DEBUG_RestoreCapturedTracks()
    {
        Logger.LogMethodE("not yet implemented!");
    }

    public static void DEBUG_OffsetSong(float offset = 2f)
    {
        AmpPlayerLocomotion.Instance.DistanceTravelled += offset * SongController.posInSec;
        SongController.Instance.OffsetSong(offset);
        Clock.Instance.seconds += offset;
    }
    public static bool? prevSmooth;
    public static void HandleSongOffsetting()
    {
        if (!prevSmooth.HasValue) prevSmooth = AmpPlayerLocomotion.Instance.SmoothEnabled;
        AmpPlayerLocomotion.Instance.SmoothEnabled = false; // Disable smoothing in Locomotion
        //if (SongController.IsPlaying) SongController.TogglePause();

        if (!Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.numpad8Key.wasPressedThisFrame) // DOESN'T WORK FOR SOME REASON
        {
            DEBUG_OffsetSong(SongController.TickToSec(SongController.measureTicks * 4));

            Clock.Instance.bar = SongController.SecToTick(Clock.Instance.seconds) / SongController.measureTicks;

            for (int i = Clock.Instance.Fbar; i < Clock.Instance.Fbar + RhythmicGame.HorizonMeasures + 1; i++)
                TrackStreamer.Instance.StreamMeasure(i, -1, RhythmicGame.FastStreamingLevel.HasFlag(FastStreamingLevel.Measures));
        }
        else if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.numpad9Key.isPressed)
            DEBUG_OffsetSong(1);
        else if ((Keyboard.current.leftAltKey.isPressed && Keyboard.current.numpad9Key.isPressed) || (AmpPlayerInputHandler.IsActive && Gamepad.current.dpad.up.isPressed))
            DEBUG_OffsetSong(0.05f);
        else if (Keyboard.current.leftShiftKey.isPressed && Keyboard.current.numpad9Key.isPressed)
            DEBUG_OffsetSong(2);
        /* backwards - UNSTABLE */
        else if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.numpad3Key.isPressed)
            DEBUG_OffsetSong(-2f);
        else if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.numpad3Key.isPressed)
            DEBUG_OffsetSong(-0.1f);

        else // Restore smoothing in Locomotion
            AmpPlayerLocomotion.Instance.SmoothEnabled = prevSmooth.Value;
    }
    public static void DEBUG_HandleTimescale()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Keypad8)) // up
            RhythmicGame.SetTimescale(Mathf.Clamp(Time.timeScale + 0.1f, 0, 1));
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Keypad2)) // down
            RhythmicGame.SetTimescale(Mathf.Clamp(Time.timeScale - 0.1f, 0, 1));
        if (Input.GetKeyDown(KeyCode.Keypad1)) // one
            RhythmicGame.SetTimescale(1f);
    }
}