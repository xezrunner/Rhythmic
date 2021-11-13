using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MouseUtility
{
    static (bool, bool) current_state;

    public static void Set((bool, bool) state) => Set(state.Item1, state.Item2);
    public static void Set(bool locked = true, bool visible = false)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = visible;
        current_state = new(locked, visible);
    }

    public static void Lock() => Set();
    public static void Unlock() => Set(false, true);

    public static (bool, bool) Get() => current_state;
}