using UnityEngine;

[Powerup(PowerupType.Slowmo, "Slow motion powerup", "slowmo_deploy")]
public class Powerup_Slowmo : Powerup
{
    Clock Clock;

    public int Timeout_Bars = 4;
    public float Slowdown_Value = 0.75f;

    public override void Deploy()
    {
        base.Deploy();
        Clock = Clock.Instance;
        Clock.OnBar += Clock_OnBar;

        target_bar = Clock.Fbar + Timeout_Bars;
        RhythmicGame.SetTimescale(Slowdown_Value);
        Logger.Log("Timescale set to 0.75 | Clock: %, target: % | timeout: %".T(this).AddColor(Colors.ConvertToFloatColor(new Color(128,0,128, 255))), Clock.Fbar, target_bar, Timeout_Bars);
    }

    int target_bar = 0;
    private void Clock_OnBar(object sender, int e)
    {
        Logger.Log("Tick: % / %".T(this).AddColor(Colors.ConvertToFloatColor(new Color(128, 0, 128, 255))), (e - target_bar + Timeout_Bars), target_bar);
        if (e < target_bar) return;

        RhythmicGame.SetTimescale(1f); // TODO: Retain original timescale! | TODO: smoothness!
        Logger.Log("Timescale re-set to 1.00".T(this).AddColor(Colors.ConvertToFloatColor(new Color(128, 0, 128, 255))));

        Clock.OnBar -= Clock_OnBar;
        Destroy();
    }
}