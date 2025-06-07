namespace unp4k;

public static class Globals
{
    internal static List<string>? Arguments = null;

    public static FileInfo? P4KFile { get; internal set; }
    public static DirectoryInfo? OutDirectory { get; internal set; }
    public static List<string> Filters { get; internal set; } = [];

    public static bool ShouldPrintDetailedLogs { get; internal set; }
    public static bool ShouldConvertToJson { get; internal set; }
    public static bool ShouldOverwrite { get; internal set; }
    public static bool ShouldAcceptEverything { get; internal set; }

    public static int ThreadLimit { get; internal set; } = Environment.ProcessorCount;
    public static int FileErrors { get; internal set; }
}