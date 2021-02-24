using UnityEngine;

public enum StatsMode
{
    None = 0, ShortShort = 1, Short = 2, Long = 3,
    Default = Short
}

[DebugComponent(DebugComponentFlag.DebugStats, DebugComponentType.Component, 0)]
public class DebugStats : DebugComponent
{
    public static RefDebugComInstance Instance;
    void Awake() => Instance = new RefDebugComInstance(this);

    Clock Clock { get { return Clock.Instance; } }
    WorldSystem WorldSystem { get { return WorldSystem.Instance; } }
    SongController SongController { get { return SongController.Instance; } }
    TracksController TracksController { get { return TracksController.Instance; } }
    AmpPlayerLocomotion AmpPlayerLocomotion { get { return AmpPlayerLocomotion.Instance; } }

    public bool IsSelfDebug = false;
    public bool IsEnabled = true;
    public StatsMode StatsMode = StatsMode.Default;

    public override void UI_Main()
    {
        // SELF DEBUG:
        if (IsSelfDebug)
        {
            AddLine("STATS SELF DEBUG: ");
            AddLine($"Stats mode: {StatsMode} | IsEnabled: {IsEnabled}");
            AddLine($"Stats text length: {Text.Length}");
            AddLine($"Stats update frequency (ms): {Attribute.UpdateFrequencyInMs}", 2);
        }

        // SongController status (disabled):
        if (!SongController.IsEnabled) AddLine($"SongController Disabled");

        // World name:
        if (StatsMode > StatsMode.ShortShort && WorldSystem.Instance)
            AddLine($"World: {WorldSystem.Name}");

        // No stats from this point onwards if SongController and others don't exist!
        if (!SongController.IsEnabled) return;
        if (!TracksController || !Clock || !AmpPlayerLocomotion)
        {
            AddLine();
            AddLine("Major gameplay components are null!".AddColor(Colors.Error));
            return;
        }

        // Song stats:
        AddLine($"Song name: {SongController.songName}  BPM: {SongController.songBpm}", 2);

        // Tracks stats:
        {
            string trackNames = "";
            TracksController.Instance.Tracks.ForEach(t => trackNames += $"{t.TrackName.AddColor(AmpTrack.Colors.ColorFromInstrument(t.Instrument) * 1.25f)}  ");
            string trackCount = $"({TracksController.Instance.Tracks.Count})".AddColor(1, 1, 1, 0.67f);

            AddLine($"Tracks: {trackNames}{trackCount}", 2);
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
    }
}