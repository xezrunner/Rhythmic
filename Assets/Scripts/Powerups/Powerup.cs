using System;
using UnityEngine;

public class PowerupAttribute : Attribute
{
    public PowerupAttribute(PowerupType type, string name = null) { Type = type; if (name != null) Name = name; }
    public string Name = "Powerup";
    public PowerupType Type;
}

public class Powerup : MonoBehaviour
{
    public bool Deployed = false; // NonSerialized?
    public int Deploy_Count = 0; // NonSerialized?

    public PowerupAttribute Attribute { get { return (PowerupAttribute)System.Attribute.GetCustomAttribute(GetType(), typeof(PowerupAttribute)); } }

    public virtual void Deploy()
    {
        Deployed = true;
        ++Deploy_Count;

        Logger.Log("Powerup deployed! name: % | it has been deployed % times.".TM(), Attribute?.Name, Deploy_Count);
    }

    public virtual void Destroy()
    {
        Logger.LogWarning("This procedure should determine whether we have our own GameObject and make decisions based on that!\n".TM(this) + 
                   "For now, we're only removing the component."); // TODO!
        Destroy(this);
    }
}