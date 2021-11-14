using System.Collections.Generic;
using UnityEngine;
using static Logger;

public class Test : MonoBehaviour
{
    void Start()
    {
        AMP_MoggSong a = new AMP_MoggSong(@"G:\amp_ps3\songs\allthetime\allthetime.moggsong");
        Log("mogg_path: %  midi_path: %" , a.mogg_path, a.midi_path);

        ConfigurationFile file = new ConfigurationFile(@"H:\Repositories\Rhythmic-git\Rhythmic\Assets\Variables.conf");
        Log("---------------");
        Log("Configuration file dump: %", file.name.AddColor(Colors.Application));
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
                    ConfigurationFile.Entry<List<ConfigurationFile.Value>> e = (ConfigurationFile.Entry < List < ConfigurationFile.Value >>)c;
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
