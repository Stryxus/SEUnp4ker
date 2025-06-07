using System.Diagnostics;
using System.Runtime.InteropServices;

namespace unlib;

public static class Platform
{
    public static void OpenFileManager(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Process.Start(new ProcessStartInfo("explorer.exe", path) { UseShellExecute = true });
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Process.Start("xdg-open", path);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) Process.Start("open", path);
    }

    public static void OpenWebpage(Uri url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Process.Start(new ProcessStartInfo { FileName = url.AbsoluteUri, UseShellExecute = true });
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Process.Start("xdg-open", url.AbsoluteUri);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) Process.Start("open", url.AbsoluteUri);
    }
}