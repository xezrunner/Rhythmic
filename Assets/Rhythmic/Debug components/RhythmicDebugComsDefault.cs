using System;
using System.Collections.Generic;

/// This class determines which Debug System components to load at startup.
public class RhythmicDebugComsDefault : DebugSystemStartupComs
{
    List<Type> list = new List<Type>()
    {
        typeof(ExampleDebugCom),
    };

    public override List<Type> GetStartupComsTypeList() => list;
}
