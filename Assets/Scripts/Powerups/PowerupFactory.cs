using System;

[Flags]
public enum PowerupType
{
    UNKNOWN = -1,
    None = 0,
    Generic = 1 << 0,
    Autocatch = 1 << 1,
    Slowmo = 1 << 2,
    Freestyle = 1 << 3,
    All = 0xff,
}

public partial class PlayerPowerupManager
{
    public Type GetPowerupForType(PowerupType type)
    {
        switch (type)
        {
            case PowerupType.Generic: return typeof(Powerup);
            case PowerupType.Autocatch: return typeof(Powerup_Autocatch);
            case PowerupType.Slowmo: return typeof(Powerup_Slowmo);
            case PowerupType.Freestyle: return typeof(Powerup_Freestyle);

            case PowerupType.UNKNOWN: return null;
            default: return null;
        }
    }
}