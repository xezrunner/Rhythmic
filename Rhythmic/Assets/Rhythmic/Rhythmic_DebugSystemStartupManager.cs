using System;
using System.Collections.Generic;

public class Rhythmic_DebugSystemStartupManager : DebugSystemStartupManager
{
    public override List<Type> GetStartupComsTypeList()
    {
        List<Type> list = base.GetStartupComsTypeList();
        list.Add(typeof(TrackStreamerDebugCom));

        return list;
    }
}