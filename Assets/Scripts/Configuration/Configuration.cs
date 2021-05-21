using System.Collections.Generic;

public class Configuration
{
    public string file_name;
    // public File file_handle;
    public string config_name; // Should be the same as file_name without ext in case there isn't one defined within the file!

    public bool is_hotreload;
    public bool is_local;

    public Dictionary<string, object> Variables = new Dictionary<string, object>();

    public void AddVariable(string name, object value)
    {
        // TODO: errors
        if (name == "") return;
        if (Variables.ContainsKey(name)) return;

        Variables.Add(name, value);
    }
}