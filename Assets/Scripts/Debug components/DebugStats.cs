using UnityEngine;

public enum StatsMode
{
    None = 0, ShortShort = 1, Short = 2, Long = 3,
    Default = None, DefaultAutoLoad = Short
}

[DebugComponent(DebugComponentFlag.DebugStats, DebugComponentType.Component, true, 30)]
public class DebugStats : DebugComponent
{
    public static DebugStats _Instance;
    public static RefDebugComInstance Instance;
    void Awake()
    {
        _Instance = this;
        Instance = new RefDebugComInstance(this);
    }

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
        base.UI_Main();

        // SELF DEBUG:
        if (IsSelfDebug)
        {
            AddLine("STATS SELF DEBUG: ");
            AddLine($"Stats mode: {StatsMode} | IsEnabled: {IsEnabled}");
            AddLine($"Stats text length: {Text.Length}");
            AddLine($"Stats update frequency (ms): {Attribute.UpdateFrequencyInMs}", 2);
        }

        // Stats mode
        if (StatsMode == StatsMode.None) return;

        // SongController status (disabled):
        if (!SongController.IsEnabled) AddLine($"SongController Disabled");

        // World name:
        if (StatsMode > StatsMode.ShortShort && WorldSystem.Instance)
            AddLine($"World: {WorldSystem.Name}");

        // No stats from this point onwards if SongController and others don't exist!
        if (!SongController.IsEnabled) return;
        if (!TracksController || !Clock || !AmpPlayerLocomotion)
        {
            AddLine("\nMajor gameplay components are missing! G_LOGIC_MISSING 0x00000000".AddColor(Colors.Error));
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
            foreach (AmpTrack t in TracksController.Instance.Tracks)
            {
                trackNames += $"{t.TrackName.AddColor(t.IsTrackCaptured ? Color.white : AmpTrack.Colors.ColorFromInstrument(t.Instrument) * 1.25f, t.IsTrackFocused ? 1 : 0.40f)}" +
                                                                                        $"{(t.RealID > TracksController.songTracks.Count ? $"F{t.RealID}" : "")}".AddColor(0.5f) + "  ";
            }

            string trackCount = $"({TracksController.Instance.Tracks.Length})".AddColor(1, 1, 1, 0.80f);

            AddLine($"Tracks: {trackNames}{trackCount}");
        }
        // Target notes:
        if (StatsMode > StatsMode.ShortShort)
        {
            string s = "Target notes: ";
            for (int i = 0; i < TracksController.targetNotes.Length; i++)
            {
                AmpNote n = TracksController.targetNotes[i];
                AmpTrack t = TracksController.MainTracks[i];
                if (!n | !t) s += "null ";
                else
                    s += $"{t.TrackName}: ".AddColor(t.Color) +
                     $"[ {n.ID} " + $"{n.TotalID} ]  ".AddColor(.42f);
            }
            AddLine(s);
        }
        // Track sequences:
        if (StatsMode > StatsMode.ShortShort)
        {
            string s = "Track sequences: ";
            foreach (AmpTrack t in TracksController.MainTracks)
            {
                s += $"{t.TrackName}: ".AddColor(t.Color) + "[";
                foreach (AmpTrackSection m in t.Sequences)
                    s += $" {m.ID}";
                s += " ]  ";
            }
            AddLine(s);
        }

        // Slop stats:
        if (StatsMode > StatsMode.ShortShort)
            AddLine($"Slop: {SongController.SlopMs} ms " + $"({SongController.SlopPos} m)".AddColor(1, 1, 1, 0.80f), -1);
        // Song start distance offset stats:
        if (StatsMode > StatsMode.ShortShort)
            AddLine($"Start distance offset: {SongController.StartDistance}");

        // Timescale stats:
        if (StatsMode > StatsMode.ShortShort)
            AddLine($"Timescale: [song: {SongController.songTimeScale.ToString("0.00")}]  [world: {Time.timeScale.ToString("0.00")}]", -1);

        // Clock stats:
        AddLine($"Clock seconds: {Clock.seconds}".AddColor(1, 1, 1, 0.8f));
        AddLine($"Clock bar: {(int)Clock.bar}  Fbar: {Clock.Fbar}  ".AddColor(1, 1, 1, 0.8f) + $"({SongController.songLengthInMeasures})".AddColor(1, 1, 1, 0.6f));
        AddLine($"Clock beat: {(int)Clock.beat % 8} ({(int)Clock.beat})".AddColor(1, 1, 1, 0.8f));

        // More goes here...
    }
}