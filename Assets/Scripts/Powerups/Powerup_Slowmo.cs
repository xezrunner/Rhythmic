using UnityEngine;

[Powerup(PowerupType.Slowmo, "Slow motion", "slowmo_deploy")]
public class Powerup_Slowmo : Powerup
{
    public float Slowdown_Value = 0.75f;

    public override void Deploy()
    {
        base.Deploy();

        RhythmicGame.SetTimescale(Slowdown_Value);
        Logger.Log("Timescale set to 0.75 | Clock: %, target: % | timeout: %".T(this).AddColor(Colors.ConvertToFloatColor(new Color(128,0,128, 255))), Clock.Fbar, start_bar + Attribute.Bar_Length, Attribute.Bar_Length);
    }

    public override void Clock_OnBar(object sender, int e)
    {
        base.Clock_OnBar(sender, e);
        Logger.Log("Tick: % / %".T(this).AddColor(Colors.ConvertToFloatColor(new Color(128, 0, 128, 255))), (Attribute.Bar_Length - bars_left), Attribute.Bar_Length);
    }

    public override void OnPowerupFinished()
    {
        RhythmicGame.SetTimescale(1f); // TODO: Retain original timescale! | TODO: smoothness!
        Logger.Log("Timescale re-set to 1.00".T(this).AddColor(Colors.ConvertToFloatColor(new Color(128, 0, 128, 255))));
    }
}