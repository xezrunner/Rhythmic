using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.InputSystem.Controls;

public static class QuickInput
{
    public static bool was_pressed(params ButtonControl[] keys) {
        foreach(var key in keys)
            if (key.wasPressedThisFrame) return true;
        return false;
    }
    public static bool is_pressed(params ButtonControl[] keys) {
        foreach(var key in keys)
            if (key.isPressed) return true;
        return false;
    }

    public static bool was_released(params ButtonControl[] keys) {
        foreach(var key in keys)
            if (key.wasReleasedThisFrame) return true;
        return false;
    }
}
