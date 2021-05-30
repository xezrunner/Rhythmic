using System;
using UnityEngine;

public class PowerupAttribute : Attribute
{
    public PowerupAttribute(PowerupType type, string name = null, string audio_name = "", int bar_length = 4, bool auto_destroy = true, string prefab_path = "")
    { Type = type; if (name != null) Prefab_Path = prefab_path; Name = name; Audio_Name = audio_name; Bar_Length = bar_length; Auto_Destroy = auto_destroy; }

    public PowerupType Type;
    public string Prefab_Path = "";
    public string Name = "Powerup";
    public string Audio_Name = "";

    public int Bar_Length = 4;
    public bool Auto_Destroy = true;
}

public class Powerup : MonoBehaviour
{
    public Clock Clock { get { return Clock.Instance; } }
    public PlayerPowerupManager PlayerPowerupManager { get { return PlayerPowerupManager.Instance; } }
    public PowerupAttribute Attribute { get { return (PowerupAttribute)System.Attribute.GetCustomAttribute(GetType(), typeof(PowerupAttribute)); } }

    [NonSerialized] public bool Deployed = false;

    // Clock:
    [NonSerialized] public int start_bar;
    [NonSerialized] public int bars_left = -1;
    public static bool debug_clock_tick = true;
    public virtual void Clock_OnBar(object sender, int e)
    {
        if (!Deployed) return;

        bars_left = (start_bar + Attribute.Bar_Length) - e;
        if (debug_clock_tick) Logger.Log("Tick: % / %".T(GetType().Name), (Attribute.Bar_Length - bars_left), Attribute.Bar_Length);

        if (e >= start_bar + Attribute.Bar_Length)
        {
            OnPowerupFinished();
            if (Attribute.Auto_Destroy) Destroy();
        }
    }
    public virtual void OnPowerupFinished() => Deployed = false;

    public virtual void Deploy()
    {
        start_bar = Clock.Fbar;
        Clock.OnBar += Clock_OnBar;

        Deployed = true;

        Logger.Log("Powerup deployed! name: %.".TM(), Attribute?.Name);

        // Play sound: | TODO: We might want to play sounds from the Powerup manager.
        if (Attribute.Audio_Name != "") PlayerPowerupManager.PlayOneShot(Attribute.Audio_Name);
    }
    public virtual void Destroy()
    {
        Clock.OnBar -= Clock_OnBar;

        if (Attribute.Prefab_Path != "")
            Destroy(gameObject);
        else
            Destroy(this);
    }

}