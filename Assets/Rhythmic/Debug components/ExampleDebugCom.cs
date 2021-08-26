using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleDebugCom : DebugCom
{
    public override string Main()
    {
        Write("This is a test - %, %, %", 1, 2, 3);
        return text;
    }
}
