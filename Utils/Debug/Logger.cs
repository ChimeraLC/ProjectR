using Godot;

/// <summary> Level of displayed message </summary>
public enum LogLevel
{
    debug,
    info,
    warn,
    error,
    fatal
}

/// <summary> Log channels to have individual log level cutoffs </summary>
public enum LogChannel
{
    Default = LogLevel.debug
}

public static class Logger
{
    public static void Log(string message, LogLevel level = LogLevel.info, LogChannel channel = LogChannel.Default)
    {
        if (level >= (LogLevel) channel)
        {
            if (level >= LogLevel.error) 
            {
                GD.PushError(message);
            }
            else if (level == LogLevel.warn)
            {
                GD.PushWarning(message);
            }
            else GD.Print(message);
        }

        if (OS.IsDebugBuild())
        {
            if (level >= LogLevel.fatal)
            {
                GameManager.EndGame();
            }
        }
    }
}
