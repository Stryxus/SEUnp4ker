using System.Text;

namespace unlib;

public static class Logger
{
    public static event Action<int, int, string?>? OnLog;

    private static void PushLog(LogPackage pckg)
    {
        switch (pckg.ClearMode)
        {
            case 0:
            {
                var originalColor = Console.ForegroundColor;
                switch (pckg.Level)
                {
                    case -2: // Write
                        Console.Write(pckg.Message);
                        break;
                    case -1: // WriteLine
                        Console.WriteLine(pckg.Message);
                        break;
                    case 0: // Info
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        WriteTimestamp("INF");
                        Console.WriteLine(pckg.Message);
                        break;
                    case 1: // Warning
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        WriteTimestamp("WRN");
                        Console.WriteLine(pckg.Message);
                        break;
                    case 2: // Error
                        Console.ForegroundColor = ConsoleColor.Red;
                        WriteTimestamp("ERR");
                        Console.WriteLine(pckg.Message);
                        break;
                    case 3: // Fatal/Exception
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        WriteTimestamp("FTL");
                        Console.WriteLine(pckg.Message);
                        break;
                    default: // Default to Info
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        WriteTimestamp("INF");
                        Console.WriteLine(pckg.Message);
                        break;
                }
                Console.ForegroundColor = originalColor;
                break;
            }
            case 3:
                Console.Clear();
                break;
            case 2:
                Console.WriteLine();
                break;
            case 1:
                Console.WriteLine(pckg.Message);
                break;
        }

        OnLog?.Invoke(pckg.ClearMode, pckg.Level, pckg.Message);
    }

    private static void WriteTimestamp(string level)
    {
        var ts = DateTime.Now.ToString("HH:mm:ss.fff");
        Console.Write($"[{ts} {level}] ");
    }

    public static void Write(object? msg)
    {
        LogPackage pckg = default;
        pckg.Level = -2;
        pckg.Message = msg is not null ? msg.ToString() : "null";
        PushLog(pckg);
    }

    public static void WriteLine(object? msg)
    {
        LogPackage pckg = default;
        pckg.Level = -1;
        pckg.Message = msg is not null ? msg.ToString() : "null";
        PushLog(pckg);
    }

    public static void Log(object? msg)
    {
        LogPackage pckg = default;
        pckg.Level = -1;
        pckg.Message = msg is not null ? msg.ToString() : "null";
        PushLog(pckg);
    }

    public static void LogInfo(object? msg)
    {
        LogPackage pckg = default;
        pckg.Level = 0;
        pckg.Message = msg is not null ? msg.ToString() : "null";
        PushLog(pckg);
    }

    public static void LogWarn(object? msg)
    {
        LogPackage pckg = default;
        pckg.Level = 1;
        pckg.Message = msg is not null ? msg.ToString() : "null";
        PushLog(pckg);
    }

    public static void LogError(object? msg)
    {
        LogPackage pckg = default;
        pckg.Level = 2;
        pckg.Message = msg is not null ? msg.ToString() : "null";
        PushLog(pckg);
    }

    public static void LogException<T>(T e) where T : Exception
    {
        LogPackage pckg = default;
        pckg.Level = 3;
        pckg.Message = $"Source: {e.Source}\n | Data: {e.Data}\n | Message: {e.Message}\n | StackTrace: {e.StackTrace}";
        PushLog(pckg);
    }

    public static void NewLine(int lines = 1)
    {
        if (lines < 1) lines = 1;
        for (var i = 0; i < lines; i++)
        {
            LogPackage pckg = default;
            pckg.ClearMode = 2;
            PushLog(pckg);
        }
    }

    public static void DivideBuffer()
    {
        StringBuilder b = new();
        for (var i = 0; i < Console.BufferWidth - 1; i++) b.Append('-');
        LogPackage pckg = default;
        pckg.ClearMode = 1;
        pckg.Message = b.ToString();
        PushLog(pckg);
    }

    public static void ClearLine(string? content = null)
    {
        StringBuilder b = new(content ?? string.Empty);
        for (var i = 0; i < Console.BufferWidth - 1; i++) b.Append(' ');
        Console.Write("\r{0}", b);
    }

    public static void ClearBuffer()
    {
        LogPackage pckg = default;
        pckg.ClearMode = 3;
        PushLog(pckg);
    }

    public static void SetTitle(object msg)
    {
        Console.Title = msg.ToString() ?? string.Empty;
    }

    private struct LogPackage
    {
        internal int ClearMode { get; set; }
        internal int Level { get; set; }
        internal string? Message { get; set; }
    }

    public static bool AskUserInput(string question)
    {
        char? c = null;
        while (c is null)
        {
            NewLine();
            Console.Write($"{question}: [y/n]: ");
            c = Console.ReadKey().KeyChar.ToString().ToLower()[0];
            if (c is 'y' or 'n') continue;
            Console.Error.WriteLine("Please input y for yes or n for no!");
            c = null;
        }
        NewLine(2);
        return c is 'y';
    }
}
