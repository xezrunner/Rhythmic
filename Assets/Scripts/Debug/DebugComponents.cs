using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class DebugComponents
{
    // This needs to be a method to always give the latest result.
    /// <summary>Get the list of debug components' metadata that can be used.</summary>
    public static List<KeyValuePair<DebugComponentAttribute, object>> GetMetaComponents()
    {
        return new List<KeyValuePair<DebugComponentAttribute, object>>()
            {
                new KeyValuePair<DebugComponentAttribute, object>(DebugUI.Attribute, new object[]{ DebugUI.Instance, DebugUI.Prefab }),
                new KeyValuePair<DebugComponentAttribute, object>(DebugKeys.Attribute, DebugKeys.Instance),
                new KeyValuePair<DebugComponentAttribute, object>(DebugStats.Attribute, DebugStats.Instance)
            };
    }
}