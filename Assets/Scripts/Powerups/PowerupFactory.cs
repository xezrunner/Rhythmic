using System;

public partial class PlayerPowerupManager
{
    public Type GetPowerupForType(PowerupType type)
    {
        switch (type)
        {
            case PowerupType.Generic: return typeof(Powerup);
            case PowerupType.Slowmo: return typeof(Powerup_Slowmo);

            case PowerupType.UNKNOWN: return null;
            default: return null;
        }
    }
}