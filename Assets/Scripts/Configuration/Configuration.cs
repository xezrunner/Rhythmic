using System;
using System.Collections.Generic;

public class Configuration
{
    public Configuration(string file_name = "")
    {
        this.file_name = file_name;
        Sections = new Dictionary<string, Dictionary<string, string>>();
        if (config_auto_global_section) AddSection(SECTION_GLOBAL);
    }

    public string file_name;
    // public File file_handle;
    public string config_name; // Should be the same as file_name without ext in case there isn't one defined within the file!

    public bool is_local;
    public bool is_hotreload;
    public int priority = 0;

    public const string SECTION_GLOBAL = "global";
    // [section: [vars]]
    public Dictionary<string, Dictionary<string, string>> Sections;

    public static bool config_auto_global_section = true;
    public void AddSection(string name)
    {
        if (!name.IsEmpty() && !Sections.ContainsKey(name))
            Sections.Add(name, new Dictionary<string, string>());
    }

    public static bool config_auto_section_creation = true;
    public bool AddVariable(string name, string value) => AddVariable(null, name, value);
    public bool AddVariable(string section, string name, string value)
    {
        // Default to global section if no section was given.
        if (section == null) section = SECTION_GLOBAL;

        // Check whether section exists:
        if (!Sections.ContainsKey(section))
        {
            if (!config_auto_section_creation)
            {
                Logger.LogConsoleW("Config variable '%' requested section '%', but it did not exist. Policy does not allow automatic section creation. ('%')", name, section, nameof(config_auto_section_creation));
                return false;
            }

            Logger.LogConsoleW("Config variable '%' requested section '%', but it did not exist. Section was automatically created due to policy ('%').", name, section, nameof(config_auto_section_creation));
            AddSection(section);
        }

        // Add variable to the appropriate section:
        Sections[section].Add(name, value);

        return true;
    }
    
    public string GetVariable(string name)
    {
        // TODO: simplify!
        foreach (var section in Sections)
        {
            foreach (var entry in section.Value)
            {
                if (entry.Key == name)
                    return entry.Value;
            }
        }

        Logger.LogW("Could not find variable '%' ('%')".T(this), name, config_name);
        return null;
    }

    public string GetVariable(string section, string name)
    {
        return Sections[section][name];
    }
}