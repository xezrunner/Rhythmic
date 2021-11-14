using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Logger;

public enum GameMode { Rhythmic = 0, Amplitude2016 = 1 }

public class GameState : MonoBehaviour
{
    public static GameState Instance;

    void Awake() { Instance = this; }

    void Start()
    {
        // Initialize Variables:
        INIT_Variables();

        // -----
        // Testing:

        // Initialize SongSystem:
        INIT_SongSystem("allthetime");
    }

    public GameMode game_mode;

    public static bool INIT_DebugPrintVariables = false;
    void INIT_Variables()
    {
        // TODO
        ConfigurationFile file = new ConfigurationFile(@"H:\Repositories\Rhythmic-git\Rhythmic\Assets\Variables.conf");

        // Debug print:
        if (INIT_DebugPrintVariables)
        {
            Log("\n---------------");
            Log("Configuration file dump:   name: %", file.name.AddColor(Colors.Application));
            foreach (var b in file.directory)
            {
                Log("  - [Section]: %", b.Key.AddColor(Colors.Application));
                for (int i = 0; i < b.Value.Count; i++)
                {
                    var c = b.Value[i];
                    string value = c.value_obj.ToString();
                    if (c.type == ConfigurationFile.Entry_Type.List)
                    {
                        value = "";
                        ConfigurationFile.Entry<List<ConfigurationFile.Value>> e = (ConfigurationFile.Entry<List<ConfigurationFile.Value>>)c;
                        foreach (ConfigurationFile.Value v in e.value)
                            value += v.value_obj.ToString() + " ";
                        value = value.Substring(0, value.Length - 1);
                    }
                    Log("    - [%]: name: %  type: %  value: '%'", i, c.name.AddColor(Colors.Application), c.type, value);
                }
            }
            Log("---------------");
        }
    }

    public SongSystem song_system;
    void INIT_SongSystem(string song_name, GameMode game_mode = GameMode.Amplitude2016)
    {
        song_system = SongSystem.CreateSongSystem();
        bool success = song_system.LoadSong(song_name, game_mode);

        if (!success) LogE("Failed to load song % (game_mode: %).", song_name, game_mode);
    }
}
