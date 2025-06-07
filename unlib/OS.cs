namespace unlib;

public static class Os
{
    public static OsType Type { get; private set; }

    public static bool IsWindows => Type == OsType.Windows;

    public static bool IsLinux => Type == OsType.Linux;

    public static bool IsAndroid => Type == OsType.Android;

    public static bool IsMacOsx => Type == OsType.MacOsx;

    public static bool IsiPhone => Type == OsType.IPhone;

    static Os()
    {
        Type = (Environment.OSVersion.Platform != PlatformID.Win32NT) ? ((Environment.OSVersion.Platform == PlatformID.Unix) ? OsType.Linux : ((Environment.OSVersion.Platform == PlatformID.MacOSX) ? OsType.MacOsx : OsType.Windows)) : OsType.Windows;
    }
}

public enum OsType
{
    Windows,
    Linux,
    Android,
    MacOsx,
    IPhone
}