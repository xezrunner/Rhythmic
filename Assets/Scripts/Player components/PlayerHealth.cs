using System.Collections;
using UnityEngine;

public enum HealthOption { Normal = 0, Invincible = 1, Off = 2}

public partial class Player : MonoBehaviour
{
    [Header("Health")]
    public HealthOption HealthMode = HealthOption.Normal;
    public float Health = 8f;

    /// <summary>
    /// Damages the player, lowering health. <br/>
    /// 0 = show damage indicators, don't lower health
    /// </summary>
    public void HurtPlayer(int damage = 1)
    {
        if (HealthMode > 0) return;
    }

    partial void HealthUpdate()
    {
        // Draw health HUD etc...
        // Handle differnet HealthOption cases
    }    
}