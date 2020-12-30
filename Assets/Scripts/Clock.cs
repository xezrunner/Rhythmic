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
        //seconds = 0; tick = 0; bar = 0; beat = 0; subbeat = 0; zPos = 0;
        //lastTick = 0; lastBar = 0; lastBeat = 0; lastSubbeat = 0;
    }

    // Clocks
    public float seconds; // This is fed by SongController's mainAudioSource
    public float tick;
    public float bar;
    public float beat;
    public float subbeat;
    public float zPos;

    // F stands for Floor (TODO: ambigious between float?)
    public int Fbar { get { return Mathf.FloorToInt(bar); } }
    public int Fbeat { get { return Mathf.FloorToInt(beat); } }

    int lastTick = -1;
    int lastBar = -1;
    int lastBeat = -1;
    int lastSubbeat = -1;

    double slopMsCounter = 0;

    public event EventHandler<int> OnTick;
    public event EventHandler<int> OnBar;
    public event EventHandler<int> OnBeat;
    public event EventHandler<int> OnSubbeat;

    public event EventHandler OnPlayerSlop;

    // Main clock loop
    void FixedUpdate()
    {
        if (!SongController.IsPlaying) return;

        // Smoothly interpolate clock ticks
        float step = 1 * SongController.songSpeed * Time.unscaledDeltaTime;
        seconds = Mathf.MoveTowards(seconds, SongController.songLength, step); // Main clock value is seconds
        //seconds = Mathf.MoveTowards(SongController.songPosition, SongController.songLength, step);

        // Set tick, bar, beat and subbeat values based on seconds
        tick = SongController.tickInSec * seconds; // 1
        bar = tick / SongController.measureTicks; // 1920
        beat = tick / SongController.beatTicks; // 480
        subbeat = tick / SongController.subbeatTicks; // 240
        zPos = SongController.posInTick * tick;

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
