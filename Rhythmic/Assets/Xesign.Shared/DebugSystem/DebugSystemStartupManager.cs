using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugSystemStartupManager
{
    /// You have to override this function in another class and return
    /// a list of types to load at DebugSystem startup.
    public virtual List<Type> GetStartupComsTypeList() { return new List<Type>() { typeof(DebugConsole) }; }
}