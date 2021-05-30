using System;
using UnityEngine;

public class PowerupAttribute : Attribute
{
    public PowerupAttribute(PowerupType type, string name = null, string audio_name = "", int bar_length = 4, bool auto_destroy = true)
    { Type = type; if (name != null) Name = name; Audio_Name = audio_name; Bar_Length = bar_length; Auto_Destroy = auto_destroy; }

    public PowerupType Type;
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
    public virtual void Clock_OnBar(object sender, int e)
    {
        bars_left = (start_bar + Attribute.Bar_Length) - e;
        if (e >= start_bar + Attribute.Bar_Length)
        {
            OnPowerupFinished();
            if (Attribute.Auto_Destroy) Destroy();
        }
    }
    public virtual void OnPowerupFinished() { }

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
        // This procedure should determine whether we have our own GameObject and make decisions based on that! For now, we're only removing the component.
        Destroy(this);
    }

}