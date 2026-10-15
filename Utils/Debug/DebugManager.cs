using System.Collections.Generic;

public static class DebugManager
{
    static Dictionary<string, int> debugFlags = new Dictionary<string, int>();

    public static int GetFlag(string flag)
    {
        if (debugFlags.ContainsKey(flag))
            return debugFlags[flag];
        return 0;
    }

    public static void SetFlag(string flag, int value)
    {
        debugFlags[flag] = value;
        Logger.Log($"Flag {flag} set to {value}");
    }
}