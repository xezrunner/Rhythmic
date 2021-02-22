using System;
using UnityEngine;

public enum StatsMode
{
    None = 0,
    ShortShort = 1,
    Short = 2,
    Long = 3,

    Default = Short
}

[DebugComponent(DebugComponentFlag.DebugUI, DebugComponentType.Component)]
public class DebugStats : DebugComponent
{
    public static DebugStats Instance;

    DebugUI DebugUI { get { return DebugUI.Instance; } }
    WorldSystem WorldSystem { get { return WorldSystem.Instance; } }
    SongController SongController { get { return SongController.Instance; } }
    TracksController TracksController { get { return TracksController.Instance; } }
    Clock Clock { get { return Clock.Instance; } }
    AmpPlayerLocomotion AmpPlayerLocomotion { get { return AmpPlayerLocomotion.Instance; } }

    public bool IsSelfDebug = false;
    public bool IsEnabled = true;
    public StatsMode StatsMode = StatsMode.Default;

    public float UpdateFrequencyInMs = 0;

    void Awake() => Instance = this;

    string statsText;
    string AddLine(string line = "", int linesToAdd = 1)
    {
        statsText += $"{line}";

        // Add newlines:
        for (int i = 0; i < linesToAdd; i++)
            statsText += '\n';

        return statsText;
    }

    public string Stats()
    {
        statsText = "";

        // SELF DEBUG:
        if (IsSelfDebug)
        {
            AddLine("STATS SELF DEBUG: ");
            AddLine($"Stats mode: {StatsMode} | IsEnabled: {IsEnabled}");
            AddLine($"Stats text length: {statsText.Length}");
            AddLine($"Stats update frequency (ms): {UpdateFrequencyInMs}", 2);
        }

        // SongController status (disabled):
        if (!SongController.IsEnabled) AddLine($"SongController Disabled");

        // World name:
        if (StatsMode > StatsMode.ShortShort && WorldSystem.Instance)
            AddLine($"World: {WorldSystem.Name}");

        // No stats from this point onwards if SongController and others don't exist!
        if (!SongController.IsEnabled) return statsText;
        if (!TracksController || !Clock || !AmpPlayerLocomotion)
        {
            AddLine();
            AddLine("Major gameplay components are null!".AddColor(Colors.Error));
            return statsText;
        }

        // Song stats:
        AddLine($"Song name: {SongController.songName}  BPM: {SongController.songBpm}", 2);

        // Tracks stats:
        {
            int trackCount = TracksController.Instance.Tracks.Count;
            string trackNames = "";
            TracksController.Instance.Tracks.ForEach(t => trackNames += $"{t.TrackName}  ");

            AddLine($"Tracks: {trackNames}({trackCount})", 2);
        }

        // Slop stats:
        if (StatsMode > StatsMode.ShortShort)
            AddLine($"SlopMs: {SongController.SlopMs}  SlopPos: {SongController.SlopPos}", 2);

        // Timescale stats:
        if (StatsMode > StatsMode.ShortShort)
            AddLine($"Timscale: [world: {Time.timeScale.ToString("0.00")}]  [song: {SongController.songTimeScale.ToString("0.00")}]");

        // Clock stats:
        AddLine($"Clock seconds: {Clock.seconds}");
        AddLine($"Clock bar: {(int)Clock.bar}");
        AddLine($"Clock beat: {(int)Clock.beat % 8} ({(int)Clock.beat})");
        AddLine($"Locomotion distance: {AmpPlayerLocomotion.DistanceTravelled}");

        // More goes here...

        return statsText;
    }

    float elapsedSinceLastUpdate;
    void Update()
    {
        if (!IsEnabled || UpdateFrequencyInMs < 0) return;

        if (UpdateFrequencyInMs > 0)
        {
            // Keep track of time (ms)
            elapsedSinceLastUpdate += Time.unscaledDeltaTime * 1000;

            // Check update frequency
            if (elapsedSinceLastUpdate > UpdateFrequencyInMs)
                elapsedSinceLastUpdate = 0;
            else
                return;
        }

        // Set stats string in DebugUI
        if (DebugUI) DebugUI.Text = Stats();
    }
}