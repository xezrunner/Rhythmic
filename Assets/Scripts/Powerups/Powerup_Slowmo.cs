using UnityEngine;

[Powerup(PowerupType.Slowmo, "Slow motion", "slowmo_deploy", bar_length: 3)]
public class Powerup_Slowmo : Powerup
{
    GenericSongController SongController;
    SongInfo song_info;
    RhythmicGame Props;
    
    public float Slowdown_Value = 0.75f;
    public float Slowdown_Ramp_Ticks = 960;
    float slowdown_ramp_sec = 1f;

    void Start()
    {
        SongController = GenericSongController.Instance;
        song_info = SongController.song_info;
        Props = SongController.GameProps;

        // TODO: Difficulty-based slowdown values!
        Slowdown_Value = song_info.slowmo_rate;
        Slowdown_Ramp_Ticks = Props.slowdown_ramp_ticks;
        slowdown_ramp_sec = SongController.time_units.sec_in_tick * Slowdown_Ramp_Ticks;
    }

    public override void Deploy()
    {
        base.Deploy();

        SongController.SetSongTimescale_Smooth(Slowdown_Value, slowdown_ramp_sec);
        Logger.Log("Timescale set to 0.75 | Clock: %, target: % | timeout: %".T(this).AddColor(Colors.ConvertToFloatColor(new Color(128, 0, 128, 255))), Clock.Fbar, start_bar + Attribute.Bar_Length, Attribute.Bar_Length);
    }

    public override void OnPowerupFinished()
    {
        SongController.SetSongTimescale_Smooth(1f, slowdown_ramp_sec);
        Logger.Log("Timescale re-set to 1.00".T(this).AddColor(Colors.ConvertToFloatColor(new Color(128, 0, 128, 255))));
    }
}