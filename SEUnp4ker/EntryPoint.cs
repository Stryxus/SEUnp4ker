using unlib;

namespace unp4k;

public static class EntryPoint
{
    // CIG seemingly do not store any record of where Star Citizen is installed in any parsable format due to the launcher being Chromium-based.
    private static DirectoryInfo DefaultOutputDirectory { get; } = new(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "unp4k"));
    private static DirectoryInfo DefaultExtractionDirectory { get; } = new(Path.Join(DefaultOutputDirectory.FullName, "output"));
    private static FileInfo Defaultp4KFile { get; } = Os.IsWindows ?
        new FileInfo(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Roberts Space Industries", "StarCitizen", "LIVE", "Data.p4k")) :
        new FileInfo(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "unp4k", "Data.p4k"));

    private static readonly string Manual = 
                "Extracts Star Citizen's Data.p4k into a directory of choice and optionally converts them into standard XML or JSON formats!" + '\n' + '\n' +
               @"\" + '\n' +
                " | Repository: https://github.com/dolkensp/unp4k" + '\n' +
               @" |\" + '\n' +
                " | - Required Arguments:" + '\n' +
                " | | -i or -input: The input file path." + '\n' +
                " | | -o or -output: The output directory path." + '\n' +
                " | |" + '\n' +
                " | - Optional Arguments:" + '\n' +
                " | | -f   or --filter:    Allows you to filter in the files you want." + '\n' +
                " | | -d   or --details:   Enabled detailed logging including errors." + '\n' +
                " | | -j   or --json:      Converts all CryXML to JSON while retaining standard XML files." + '\n' +
                " | | -ow  or --overwrite: Overwrites files that already exist." + '\n' +
                " | | -y   or --accept:    Don't ask for input, just continue. Recommended for automated systems." + '\n' +
                " |/" + '\n' +
                "/" + '\n';
    
    internal static void PreInit()
    {
        Logger.ClearBuffer();
        Logger.SetTitle($"unp4k: Pre-Initializing...");
        // Parse the arguments and do what they represent
        for (var i = 0; i < Globals.Arguments?.Count; i++)
        {
            switch (Globals.Arguments[i].ToLowerInvariant())
            {
                case "-i":
                case "--input":
                    Globals.P4KFile                   = new FileInfo(Globals.Arguments[i + 1]);
                    break;
                case "-o":
                case "--output":
                    Globals.OutDirectory             = new DirectoryInfo(Globals.Arguments[i + 1]);
                    break;
                case "-t":
                case "--threads":
                {
                    if (int.TryParse(Globals.Arguments[i + 1], out var threads)) Globals.ThreadLimit = threads;
                    else throw new InvalidCastException(Globals.Arguments[i + 1]);
                    break;
                }
                case "-f":
                case "--filter":
                    Globals.Filters                  = [.. Globals.Arguments[i + 1].Split(',')];
                    break;
                case "-d":
                case "--details":
                    Globals.ShouldPrintDetailedLogs = true;
                    break;
                case "-j":
                case "--json":
                    Globals.ShouldConvertToJson        = true;
                    break;
                case "-ow":
                case "--overwrite":
                    Globals.ShouldOverwrite       = true;
                    break;
                case "-y":
                case "--accept":
                    Globals.ShouldAcceptEverything   = true;
                    break;
            }
        }

        var hasInput = Globals.P4KFile is not null;
        var hasOutput = Globals.OutDirectory is not null;
        
        if (!hasInput)
        {
            Console.Write("Please provide the absolute Data.p4k file path: ");
            var input = Console.ReadLine();
            if (input is not null)
            {
                Globals.P4KFile = new FileInfo(input);
                hasInput = true;
            }
            else Globals.P4KFile = Defaultp4KFile;
        }

        if (!hasOutput)
        {
            Console.Write("Please provide the absolute output directory path: ");
            var output = Console.ReadLine();
            if (output is not null)
            {
                Globals.OutDirectory = new DirectoryInfo(output);
                hasOutput = true;
            }
            else Globals.OutDirectory = DefaultExtractionDirectory;
        }

        if (!hasInput || !hasOutput)
        {
            // Show the user the manual if there are missing arguments.
            Logger.Write($"{Manual}{(!hasInput ? $"\nNO INPUT Data.p4k PATH HAS BEEN DECLARED. USING DEFAULT PATH {Defaultp4KFile.FullName}" : string.Empty)}" +
                         $"{($"\nNO OUTPUT DIRECTORY PATH HAS BEEN DECLARED. ALL EXTRACTS WILL GO INTO {DefaultExtractionDirectory.FullName}")}\n" +
                         "Press any key to continue...");
        
            Console.ReadKey();
        }
        Logger.ClearBuffer();
    }

    internal static bool Init()
    {
        Logger.SetTitle($"unp4k: Initializing...");
        
        // Default any of the null argument declared variables.
        Globals.P4KFile ??= Defaultp4KFile;
        Globals.OutDirectory ??= DefaultExtractionDirectory;
        if (!Globals.P4KFile.Exists)
        {
            Logger.LogError($"Input path '{Globals.P4KFile.FullName}' does not exist!");
            Logger.LogError($"Make sure you have the path pointing to a Star Citizen Data.p4k file!");
            if (!Globals.ShouldAcceptEverything) Console.ReadKey();
            return false;
        }
        if (!Globals.OutDirectory.Exists) Globals.OutDirectory.Create();
		return true;
	}

    internal static bool PostInit()
    {
        Logger.SetTitle($"unp4k: Post-Initializing...");
        if (Globals.ShouldAcceptEverything) return true;
        // Show the user any warning if anything worrisome is detected.
        var newLineCheck = false;
        if (Os.IsLinux && Environment.UserName.Equals("root", StringComparison.CurrentCultureIgnoreCase))
        {
            newLineCheck = true;
            Logger.NewLine();
            Logger.LogWarn("LINUX ROOT WARNING:" + '\n' +
                           "unp4k has been run as root via the sudo command!" + '\n' +
                           "This may cause issues due to the home path being '/root/'!");
        }
        if (Globals.Filters.Contains("*.*"))
        {
            if (newLineCheck) Logger.NewLine();
            else newLineCheck = true;
            Logger.LogWarn("ENORMOUS JOB WARNING:" + '\n' + 
                           "unp4k has been run with no filters or the *.* filter!" + '\n' +
                           $"When extracted, it will take up a lot of storage space and queues 100,000's of tasks in the process.");
        }
        if (Globals.ShouldOverwrite)
        {
            if (newLineCheck) Logger.NewLine();
            Logger.LogWarn("OVERWRITE ENABLED:" + '\n' +
                           "unp4k has been run with the overwrite option!" + '\n' +
                           "Overwriting files will potentially take much longer than choosing a new empty directory!");
        }
        
        if (!newLineCheck) return true;
        return Logger.AskUserInput("Proceed?");
    }
}