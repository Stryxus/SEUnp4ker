using System.Diagnostics;

namespace unlib;

public static class Platform
{
    public static void OpenFileManager(string path)
    {
        switch (Os.Type)
        {
            case OsType.Windows:
                Process.Start("explorer", path);
                break;
            case OsType.Linux:
                Process.Start("nautilus", path);
                break;
            case OsType.MacOsx:
                Process.Start(path);
                break;
            case OsType.Android:
                Process.Start(path);
                break;
            case OsType.IPhone:
                Process.Start(path);
                break;
        }
    }

    public static void OpenWebpage(Uri url)
    {
        switch (Os.Type)
        {
            case OsType.Windows:
                Process.Start(new ProcessStartInfo { FileName = url.AbsoluteUri, UseShellExecute = true });
                break;
            case OsType.Linux:
                Process.Start(new ProcessStartInfo { FileName = url.AbsoluteUri, UseShellExecute = true });
                break;
            case OsType.MacOsx:
                Process.Start(new ProcessStartInfo { FileName = url.AbsoluteUri, UseShellExecute = true });
                break;
            case OsType.Android:
                Process.Start(new ProcessStartInfo { FileName = url.AbsoluteUri, UseShellExecute = true });
                break;
            case OsType.IPhone:
                Process.Start(new ProcessStartInfo { FileName = url.AbsoluteUri, UseShellExecute = true });
                break;
        }
    }
}