using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{
    public static Clock Instance;
    SongController SongController;

    void Awake()
    {
        Instance = this;
        SongController = SongController.Instance;

        // reset clocks!
        seconds = 0; tick = 0; bar = 0; beat = 0; subbeat = 0; zPos = 0;
        lastTick = 0; lastBar = 0; lastBeat = 0; lastSubbeat = 0;
    }

    // Clocks
    public float seconds; // This is fed by SongController's mainAudioSource
    public float tick;
    public float bar;
    public float beat;
    public float subbeat;
    public float zPos;

    int lastTick;
    int lastBar;
    int lastBeat;
    int lastSubbeat;

    double slopMsCounter = 0;

    public event EventHandler<int> OnTick;
    public event EventHandler<int> OnBar;
    public event EventHandler<int> OnBeat;
    public event EventHandler<int> OnSubbeat;

    public event EventHandler OnPlayerSlop;

    // Update every frame (?, maybe FixedUpdate would be a better idea?)
    void Update()
    {
        if (!SongController.IsPlaying) return;

        // Smoothly interpolate clock ticks
        float step = Time.unscaledDeltaTime * SongController.songSpeed;
        Vector3 currentPoint = new Vector3(0, 0, seconds);
        Vector3 targetPoint = new Vector3(0, 0, SongController.songLength);
        Vector3 finalPoint = Vector3.MoveTowards(currentPoint, targetPoint, step);

        // Main clock value is seconds
        seconds = finalPoint.z;

        // Set tick, bar, beat and subbeat values based on seconds
        tick = SongController.secInTick * seconds;
        bar = tick / SongController.measureTicks; // 1920
        beat = tick / SongController.beatTicks; // 480
        subbeat = tick / SongController.subbeatTicks; // 240
        zPos = SongController.tickInzPos * tick;

        // Invoke events if last integer values aren't the same as current (changed!)
        if ((int)tick != lastTick) OnTick?.Invoke(this, (int)tick);
        if ((int)bar != lastBar) OnBar?.Invoke(this, (int)bar);
        if ((int)beat != lastBeat) OnBeat?.Invoke(this, (int)beat);
        if ((int)subbeat != lastSubbeat) OnSubbeat?.Invoke(this, (int)subbeat);

        // Update last ticks
        lastTick = (int)tick;
        lastBar = (int)bar;
        lastBeat = (int)beat;
        lastSubbeat = (int)subbeat;

        // Slop timer logic
        slopMsCounter += Time.deltaTime * 1000;
        if ((int)slopMsCounter >= RhythmicGame.SlopMs)
            PlayerSlop();
    }

    // Slop timer
    void PlayerSlop()
    {
        OnPlayerSlop?.Invoke(this, null);
        slopMsCounter = 0;
    }
}
