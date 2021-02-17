using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class DebugComponents
{
    public static List<KeyValuePair<DebugComponentAttribute, object>> MetaComponents
    {
        get // This needs to be a property to always give the latest result.
        {
            return new List<KeyValuePair<DebugComponentAttribute, object>>()
            {
                new KeyValuePair<DebugComponentAttribute, object>(DebugUI.Attribute, new object[]{ DebugUI.Instance, DebugUI.Prefab }),
                new KeyValuePair<DebugComponentAttribute, object>(DebugKeys.Attribute, DebugKeys.Instance)
            };
        }
    }
}