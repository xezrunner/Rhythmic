using System;
using UnityEngine;

public class PowerupAttribute : Attribute
{
    public PowerupAttribute(PowerupType type, string name = null, string audio_name = "") { Type = type; if (name != null) Name = name; Audio_Name = audio_name; }

    public PowerupType Type;
    public string Name = "Powerup";
    public string Audio_Name = "";
}

public class Powerup : MonoBehaviour
{
    public PlayerPowerupManager PlayerPowerupManager { get { return PlayerPowerupManager.Instance; } }

    public PowerupAttribute Attribute { get { return (PowerupAttribute)System.Attribute.GetCustomAttribute(GetType(), typeof(PowerupAttribute)); } }

    public bool Deployed = false; // NonSerialized?
    public int Deploy_Count = 0; // NonSerialized?

    public virtual void Deploy()
    {
        Deployed = true;
        ++Deploy_Count;

        Logger.Log("Powerup deployed! name: % | it has been deployed % times.".TM(), Attribute?.Name, Deploy_Count);

        // Play sound: | TODO: We might want to play sounds from the Powerup manager.
        if (Attribute.Audio_Name != "") PlayerPowerupManager.PlayOneShot(Attribute.Audio_Name);
    }

    public virtual void Destroy()
    {
        // This procedure should determine whether we have our own GameObject and make decisions based on that! For now, we're only removing the component.
        Destroy(this);
    }
}