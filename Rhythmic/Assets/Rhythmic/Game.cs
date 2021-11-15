using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static Logger;

public enum GameType { Rhythmic = 0, Amplitude2016 = 1 }
public enum GameState { Startup = 0, Ingame = 1, Editor = 2, UNKNOWN = -1 }

public class Game : MonoBehaviour
{
    public static Game Instance;

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

    public GameType game_type;

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
                    if (c.type == ConfigEntryType.List)
                    {
                        value = "";
                        ConfigEntry<List<ConfigValue>> e = (ConfigEntry<List<ConfigValue>>)c;
                        foreach (ConfigValue v in e.value)
                            value += v.value_obj.ToString() + " ";
                        value = value.Substring(0, value.Length - 1);
                    }
                    Log("    - [%]: name: %  type: %  value: '%'", i, c.name.AddColor(Colors.Application), c.type, value);
                }
            }
            Log("---------------");
        }

        FieldInfo[] vars_fieldinfo = typeof(Variables).GetFields();
        foreach (var section in file.directory)
        {
            foreach (ConfigEntry e in section.Value)
            {
                foreach (FieldInfo f in vars_fieldinfo)
                {
                    if (e.name == f.Name)
                    {
                        if (e.value_type != f.FieldType)
                        {
                            LogE("Warning: variable '%' expects type '%' - given: '%'. Ignoring.".TM(this), e.name, f.FieldType, e.value_type);
                            break;
                        }
                        f.SetValue(null, e.value_obj);
                        // Log("Applied: '%' -> value: '%'".TM(this), e.name, f.GetValue(null));
                    }
                }
            }
        }

        // Handle special stuff here...
    }

    public SongSystem song_system;
    void INIT_SongSystem(string song_name, GameType game_type = GameType.Amplitude2016)
    {
        song_system = SongSystem.CreateSongSystem();
        bool success = song_system.LoadSong(song_name, game_type);

        if (!success) LogE("Failed to load song % (game_type: %).", song_name, game_type);
    }
}
