using System.Collections;
using UnityEngine;

public enum HealthOption { Normal = 0, Invincible = 1, Off = 2}

public partial class AmpPlayer : MonoBehaviour
{
    public HealthOption HealthOption = HealthOption.Normal;
    float Health = 8f;

    /// <summary>
    /// Damages the player, lowering health. <br/>
    /// 0 = show damage indicators, don't lower health
    /// </summary>
    public void HurtPlayer(int damage = 1)
    {
        if (HealthOption > 0) return;
    }

    public partial void HealthUpdate()
    {
        // Draw health HUD etc...
        // Handle differnet HealthOption cases
    }    
}