using UnityEngine.InputSystem.Controls;

public enum KeyPressOp { OR, AND }

public static class InputHandler
{
    public static bool IsPressed(KeyPressOp op, params ButtonControl[] args)
    {
        int counter = 0;
        foreach (ButtonControl k in args)
            if (k.isPressed) counter++;

        if (op == KeyPressOp.OR) return (counter > 0);
        else return (counter == args.Length);
    }
    public static bool IsPressed(params ButtonControl[] args) => IsPressed(KeyPressOp.OR, args); // One or more
    public static bool ArePressed(params ButtonControl[] args) => IsPressed(KeyPressOp.AND, args); // At the same time

    public enum FramePressKind { WasPressedOnThisFrame, WasReleasedOnThisFrame }
    static bool WasFrame(KeyPressOp op, FramePressKind kind, params ButtonControl[] args)
    {
        int counter = 0;
        foreach (ButtonControl k in args)
            if (kind == FramePressKind.WasReleasedOnThisFrame ? k.wasReleasedThisFrame : k.wasPressedThisFrame) counter++;

        if (op == KeyPressOp.OR) return (counter > 0);
        else return (counter == args.Length);
    }

    public static bool WasPressed(KeyPressOp op, params ButtonControl[] args) => WasFrame(op, FramePressKind.WasPressedOnThisFrame, args);
    public static bool WasReleased(KeyPressOp op, params ButtonControl[] args) => WasFrame(op, FramePressKind.WasPressedOnThisFrame, args);
    public static bool WasPressed(params ButtonControl[] args) => WasPressed(KeyPressOp.OR, args);
    public static bool WasReleased(params ButtonControl[] args) => WasReleased(KeyPressOp.OR, args);
}