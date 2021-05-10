using System.Collections.Generic;
using UnityEngine;

public class PlayerPowerupManager : MonoBehaviour
{
    void Awake() => Logger.Log("initialized!".T(this));

    public List<Powerup> Powerups = new List<Powerup>();
}