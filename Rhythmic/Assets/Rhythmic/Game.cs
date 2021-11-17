using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
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

    public static string VARIABLES_FilePath = @"H:\Repositories\Rhythmic-git\Rhythmic\Assets\Variables.conf";
    public static bool VARIABLES_AllowHotReload = true;
    public static bool VARIABLES_DebugPrintVars = false;

    public DateTime VARIABLES_LastWriteTime;
    ConfigurationFile VARIABLES_ConfigFile = new ConfigurationFile(VARIABLES_FilePath, false);
    void INIT_Variables(bool hot_reload = false)
    {
        VARIABLES_LastWriteTime = File.GetLastWriteTime(VARIABLES_FilePath);
        VARIABLES_ConfigFile.ReadFromPath();

        // Debug print:
        if (VARIABLES_DebugPrintVars)
        {
            Log("\n---------------");
            Log("Configuration file dump:   name: %", VARIABLES_ConfigFile.name.AddColor(Colors.Application));
            foreach (var b in VARIABLES_ConfigFile.directory)
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

        // Load Variables section entries using reflection:
        FieldInfo[] vars_fieldinfo = typeof(Variables).GetFields();

        foreach (var sect in VARIABLES_ConfigFile.directory)
        {
            // Only load startup entries if not hot reloading!
            if (sect.Key == "Startup" && !hot_reload)
            {
                foreach (ConfigEntry<string[]> e in sect.Value) // Can you do type args here?
                {
                    if (e.type == ConfigEntryType.Command)
                    {
                        if (e.name == "debugsystem") DebugSystem.CreateDebugSystemObject();
                        else DebugConsole.ExecuteCommand(e.name, e.value);
                    }
                }
            }
            else if (sect.Key == "Variables")
            {
                foreach (ConfigEntry e in sect.Value)
                {
                    bool found = false;
                    foreach (FieldInfo f in vars_fieldinfo)
                    {
                        if (e.name == f.Name)
                        {
                            found = true;

                            // HACK: Cast our type if we have an int but var expects float or double:
                            if (e.value_type == typeof(int) && (f.FieldType == typeof(float) || f.FieldType == typeof(double)))
                                e.value_type = f.FieldType;
                            else if (e.value_type == typeof(float) && (f.FieldType == typeof(int))) // TODO: Check!
                                e.value_type = f.FieldType;
                            else if (e.value_type != f.FieldType)
                            {
                                LogE("Warning: variable '%' expects type '%' - given: '%'. Ignoring.".TM(this), e.name, f.FieldType, e.value_type);
                                break;
                            }
                            f.SetValue(null, e.value_obj);
                            // Log("Applied: '%' -> value: '%'".TM(this), e.name, f.GetValue(null));
                            break;
                        }
                    }
                    if (!found) LogW("Invalid variable '%' (type: % / %) (value: %)".TM(this), e.name, e.type, e.value_type, e.value_obj);
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

    float variables_hotreload_check_elapsed_ms = 0;

    void UPDATE_VariablesHotReload()
    {
        if (!VARIABLES_AllowHotReload) return;

        if (variables_hotreload_check_elapsed_ms >= Variables.VARIABLES_HotReloadCheckMs)
        {
            if (File.GetLastWriteTime(VARIABLES_FilePath) <= VARIABLES_LastWriteTime) return;

            Log("Hot-reloading Variables.conf... (check elapsed: %ms)".T(this), variables_hotreload_check_elapsed_ms);
            variables_hotreload_check_elapsed_ms = 0;

            INIT_Variables(hot_reload: true);
        }

        variables_hotreload_check_elapsed_ms += Time.deltaTime * 1000f;
    }

    void Update()
    {
        UPDATE_VariablesHotReload();
    }

}
