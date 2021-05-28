using System;
using TMPro;
using UnityEngine;

[Flags]
public enum PowerupType
{
    All = -2,
    UNKNOWN = -1,
    None = 0,
    Generic = 1,
    Slowmo = 1 << 1
}

/// TODO:
/// - We will want to let songs have their own powerup possibility configurations.
public partial class PlayerPowerupManager : MonoBehaviour
{
    public static PlayerPowerupManager Instance;

    SongController SongController { get { return SongController.Instance; } }
    TrackStreamer TrackStreamer { get { return TrackStreamer.Instance; } }
    TracksController TracksController { get { return TracksController.Instance; } }

    public TMP_Text UI_Powerup_Label;

    // This controls which powerups can be generated on the tracks.
    public PowerupType Configuration = PowerupType.All;

    void Awake() => Instance = this;
    private void Start() => RegisterCommands();

    /// Commands:
    static bool registered_commands;
    void RegisterCommands()
    {
        if (registered_commands) return;

        DebugConsole.RegisterCommand(current_powerup);
        DebugConsole.RegisterCommand(deploy_powerup);
        DebugConsole.RegisterCommand(inventorize_powerup);

        registered_commands = true;
    }
    static void current_powerup() => Logger.LogConsole("Current powerup: %", Instance?.Current_Powerup);
    static void deploy_powerup() => Instance?.DeployCurrentPowerup();
    static void inventorize_powerup(string[] args)
    {
        if (args.Length < 1 || !Instance) return;

        if (args[0] == "clear")
        {
            Instance.Current_Powerup?.Destroy();
            Instance.Current_Powerup = null;
        }
		// TODO: bad bad bad!
        else if (char.IsDigit(args[0][0]))
        {
            int digit = args[0][0].ToString().ParseInt();
            if (digit > 1)
                digit = 1 << digit;

            Instance?.InventorizePowerup((PowerupType)digit);
        }
        else Instance?.InventorizePowerup((PowerupType)Enum.Parse(typeof(PowerupType), args[0]));
    }

    /// TODO:
    /// We might want to store multiple powerups for specific game modes / settings.
    public Powerup Current_Powerup = null;
    public void InventorizePowerup(PowerupType type)
    {
        if (type == PowerupType.None)
        {
            ClearPowerups(); 
            return;
        }
        else
        {
            Type t = GetPowerupForType(type);
            if (t != null)
            {
                ClearPowerups();

                Powerup p = (Powerup)gameObject.AddComponent(t);
                Current_Powerup = p;
                Logger.Log("Instantiated powerup module - type: %".M(), type.ToString());

                UI_Powerup_Label.text = p.Attribute?.Name;

                return;
            }
        }

        Logger.LogError("Could not instantiate powerup module - type: %".M(), type.ToString());
    }

    void ClearPowerups()
    {
        // Destory current powerup, in case it hasn't been deployed yet.
        if (Current_Powerup && !Current_Powerup.Deployed) Current_Powerup.Destroy();
        Instance.Current_Powerup = null;

        UI_Powerup_Label.text = "";
    }

    //public void DeployCurrentPowerup() => Current_Powerup?.Deploy();
    public void DeployCurrentPowerup()
    {
        if (Current_Powerup) Current_Powerup.Deploy();
        else Logger.LogWarning("No powerup in inventory - deploy failed.".M());

        // TODO: Control for whether powerups should be removed from inventory.
        /// TODO: Remove only ourselves from the future inventory of powerups!
        ClearPowerups();
    }

    public void STREAMER_GeneratePowerupMap()
    {
        for (int i = 0; i < TracksController.MainTracks_Count; ++i)
        {
            for (int x = 0; x < SongController.songLengthInMeasures; ++x)
            {
                if (x % 2 != 0) continue;
                TrackStreamer.metaMeasures[i, x].Powerup = PowerupType.Slowmo;
            }
        }
    }
}