using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using static Logger;

public class Game : MonoBehaviour {
    public static Game Instance;

    public virtual void Awake() => Instance = this;
    public virtual void Start() {
        // Initialize Variables:
        INIT_Variables();
    }

    public DebugSystemStartupManager debugsystem_startupmanager = new Rhythmic_DebugSystemStartupManager();

    // NOTE: Variable reading, hotreloading etc.. (probably this entire Game class)
    // should move to XesignShared, as it is a core engine thing. We'll let it be
    // overrideable so that we can load our custom variables as needed.
    #region Variables
    public static string VARIABLES_FilePath = Path.Combine(Application.dataPath, "Variables.conf"); //@"H:\Repositories\Rhythmic-git\Rhythmic\Assets\Variables.conf";
    public static bool   VARIABLES_AllowHotReload = true;
    public static bool   VARIABLES_DebugPrintVars = false;

    public DateTime VARIABLES_LastWriteTime;
    ConfigurationFile VARIABLES_ConfigFile = new ConfigurationFile(VARIABLES_FilePath, false);
    public virtual void INIT_Variables(bool hot_reload = false) {
        VARIABLES_LastWriteTime = File.GetLastWriteTime(VARIABLES_FilePath);
        VARIABLES_ConfigFile.ReadFromPath();

        // Debug print:
        if (VARIABLES_DebugPrintVars) {
            Log("\n---------------");
            Log("Configuration file dump:   name: %", VARIABLES_ConfigFile.name.AddColor(Colors.Application));
            foreach (var b in VARIABLES_ConfigFile.directory) {
                Log("  - [Section]: %", b.Key.AddColor(Colors.Application));
                for (int i = 0; i < b.Value.Count; i++) {
                    var c = b.Value[i];
                    string value = c.value_obj.ToString();
                    if (c.type == ConfigEntryType.List) {
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

        foreach (var sect in VARIABLES_ConfigFile.directory) {
            // Only load startup entries if not hot reloading!
            if (sect.Key == "Startup" && !hot_reload) {
                foreach (ConfigEntry<string[]> e in sect.Value) // Can you do type args here?
                {
                    if (e.type == ConfigEntryType.Command) {
                        if (e.name == "debugsystem") DebugSystem.CreateDebugSystemObject(debugsystem_startupmanager);
                        else DebugConsole.ExecuteCommand(e.name, e.value);
                    }
                }
            } else if (sect.Key == "Variables") {
                foreach (ConfigEntry e in sect.Value) {
                    bool found = false;
                    foreach (FieldInfo f in vars_fieldinfo) {
                        if (e.name == f.Name) {
                            found = true;

                            // HACK: Cast our type if we have an int but var expects float or double:
                            if (e.value_type == typeof(int) && (f.FieldType == typeof(float) || f.FieldType == typeof(double)))
                                e.value_type = f.FieldType;
                            else if (e.value_type == typeof(float) && (f.FieldType == typeof(int))) // TODO: Check!
                                e.value_type = f.FieldType;
                            else if (e.value_type != f.FieldType) {
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

    float variables_hotreload_check_elapsed_ms = 0;

    void UPDATE_VariablesHotReload() {
        if (!VARIABLES_AllowHotReload) return;

        if (variables_hotreload_check_elapsed_ms >= Variables.VARIABLES_HotReloadCheckMs) {
            if (File.GetLastWriteTime(VARIABLES_FilePath) <= VARIABLES_LastWriteTime) return;

            Log("Hot-reloading Variables.conf... (check elapsed: %ms)".T(this), variables_hotreload_check_elapsed_ms);
            variables_hotreload_check_elapsed_ms = 0;

            INIT_Variables(hot_reload: true);
        }

        variables_hotreload_check_elapsed_ms += Time.deltaTime * 1000f;
    }
    #endregion

    public virtual void Update() {
        UPDATE_VariablesHotReload();
    }
}
