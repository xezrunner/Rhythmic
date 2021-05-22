using System;
using System.Collections.Generic;

public class Configuration
{
    public Configuration(string file_name = "")
    {
        this.file_name = file_name;
        Sections = new Dictionary<string, Dictionary<string, string>>();
    }

    /*
    public Configuration() { }

    public Configuration(string file_name, string config_name = null, bool is_hotreload = false, bool is_local = false)
    {
        if (file_name.IsEmpty() && config_name.IsEmpty())
            throw new Exception("A Configuration class did not have a file_name or a config_name.");

        Sections = new Dictionary<string, Dictionary<string, object>>();

        this.file_name = file_name;
        // Assign file name without ext as config_name if none was given
        if (config_name.IsEmpty())
            this.config_name = file_name.RemoveExt();

        this.is_hotreload = is_hotreload;
        this.is_local = is_local;
    }
    */

    public string file_name;
    // public File file_handle;
    public string config_name; // Should be the same as file_name without ext in case there isn't one defined within the file!

    public bool is_hotreload;
    public bool is_local;

    public const string SECTION_GLOBAL = "global";
    // [section: [vars]]
    public Dictionary<string, Dictionary<string, string>> Sections;

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
}