using System;

public static class EnumExtensions {
    public static bool HasFlag_Any(this Enum target_enum, params Enum[] flags) {
        if (flags.Length == 0) return false;
        foreach (Enum f in flags) {
            if (target_enum.HasFlag(f)) return true;
        }
        return false;
    }
    public static bool HasFlag_All(this Enum target_enum, params Enum[] flags) {
        if (flags.Length == 0) return false;
        foreach (Enum f in flags) {
            if (!target_enum.HasFlag(f)) return false;
        }
        return true;
    }
}