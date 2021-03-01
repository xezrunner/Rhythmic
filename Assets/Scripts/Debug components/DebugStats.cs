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
            AddLine("\nMajor gameplay components are null!".AddColor(Colors.Error));
            return;
        }

        // Song stats:
        AddLine($"Song: {SongController.songName}  " + $"BPM: {SongController.songBpm}".AddColor(0.6f), 2);

        /// --- END OF BASIC INFO ---
        /// Add stuff below:

        // Locomotion stats:
        {
            AddLine($"Locomotion dist: {AmpPlayerLocomotion.DistanceTravelled}");
            AddLine($"Locomotion pos: {AmpPlayerLocomotion.transform.position}");
            AddLine($"Locomotion rot: [non-interp: {AmpPlayerLocomotion.NonInterpolatable.rotation.eulerAngles}]  " +
                                    //$"[interp: {AmpPlayerLocomotion.Interpolatable.rotation.eulerAngles}]");
                                    $"[smooth: {AmpPlayerLocomotion.SmoothDuration.ToString().AddColor(AmpPlayerLocomotion.SmoothEnabled ? Color.green : new Color(1, 1, 1, 0.4f))}]");
            if (RhythmicGame.IsTunnelMode) AddLine($"Tunnel rot: {AmpPlayerLocomotion.TunnelRotation.z}");
            AddLine();
        }

        // Tracks stats:
        {
            string trackNames = $"{(RhythmicGame.IsTunnelMode ? "\nTunnel mode ON  " : "")}";
            TracksController.Instance.Tracks.ForEach(t => trackNames += $"{t.TrackName.AddColor(t.IsTrackCaptured ? Color.white : AmpTrack.Colors.ColorFromInstrument(t.Instrument) * 1.25f, t.IsTrackFocused ? 1 : 0.40f)}" +
                                                                        $"{(t.RealID > TracksController.songTracks.Count ? $"F{t.RealID}" : "")}".AddColor(0.5f) + "  ");
            string trackCount = $"({TracksController.Instance.Tracks.Count})".AddColor(1, 1, 1, 0.80f);

            AddLine($"Tracks: {trackNames}{trackCount}", 2);
        }

        // Slop stats:
        if (StatsMode > StatsMode.ShortShort)
            AddLine($"Slop: {SongController.SlopMs} ms " + $"({SongController.SlopPos} m)".AddColor(1, 1, 1, 0.80f));
        // Song start distance offset stats:
        if (StatsMode > StatsMode.ShortShort)
            AddLine($"Start distance offset: {SongController.StartDistance}");

        // Timescale stats:
        if (StatsMode > StatsMode.ShortShort)
            AddLine($"Timscale: [song: {SongController.songTimeScale.ToString("0.00")}]  [world: {Time.timeScale.ToString("0.00")}]", -1);

        // Clock stats:
        AddLine($"Clock seconds: {Clock.seconds}".AddColor(1, 1, 1, 0.8f));
        AddLine($"Clock bar: {(int)Clock.bar}  Fbar: {Clock.Fbar}  ".AddColor(1, 1, 1, 0.8f) + $"({SongController.songLengthInMeasures})".AddColor(1, 1, 1, 0.6f));
        AddLine($"Clock beat: {(int)Clock.beat % 8} ({(int)Clock.beat})".AddColor(1, 1, 1, 0.8f));

        // More goes here...
    }
}