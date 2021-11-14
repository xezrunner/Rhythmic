using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Logger;

public class GameState : MonoBehaviour
{
    void Start()
    {
        // Initialize Variables:
        INIT_Variables();

        // Create DebugSystem in debug builds!
    }

    void INIT_Variables()
    {
        // TODO
        ConfigurationFile file = new ConfigurationFile(@"H:\Repositories\Rhythmic-git\Rhythmic\Assets\Variables.conf");

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
