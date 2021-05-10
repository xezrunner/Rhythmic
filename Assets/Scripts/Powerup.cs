using System;
using UnityEngine;

public class PowerupAttribute : Attribute
{
    public PowerupAttribute(PowerupType type) => Type = type;
    public PowerupType Type;
}

public class Powerup : MonoBehaviour
{
    public string Name;
    public int Deploy_Count = 0; // NonSerialized?

    public virtual void Deploy()
    {
        Logger.Log("Powerup deployed! name: % | it has been deployed % times.".TM(), Name, Deploy_Count);
        ++Deploy_Count;
    }
}