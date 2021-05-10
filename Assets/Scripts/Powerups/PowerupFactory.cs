using System;

public partial class PlayerPowerupManager
{
    public Type GetPowerupForType(PowerupType type)
    {
        switch (type)
        {
            case PowerupType.Generic: return typeof(Powerup);
            case PowerupType.Special: return typeof(Powerup);
            case PowerupType.Special2: return typeof(Powerup);

            case PowerupType.UNKNOWN: return null;
            default: return null;
        }
    }
}