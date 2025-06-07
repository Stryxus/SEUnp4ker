using System.Runtime.InteropServices;

namespace unp4k;

public static class EntryPoint
{
    private static DirectoryInfo DefaultOutputDirectory { get; } = new(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "unp4k"));
    private static DirectoryInfo DefaultExtractionDirectory { get; } = new(Path.Join(DefaultOutputDirectory.FullName, "output"));
    private static FileInfo? Defaultp4KFile { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
        new FileInfo(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Roberts Space Industries", "StarCitizen", "LIVE", "Data.p4k")) : null;

    private static readonly string Manual = 
                "Extracts Star Engine's .p4k archive into a directory of choice and optionally unforge it all into JSON!" + '\n' + '\n' +
               @"\" + '\n' +
                " | Repository: https://github.com/Stryxus/SEUnp4ker" + '\n' +
               @" |\" + '\n' +
                " | | -h or --help: Print out the manual." + '\n' +
                " | - Required Arguments:" + '\n' +
                " | | -i or -input: The input file path." + '\n' +
                " | | -o or -output: The output directory path." + '\n' +
                " | |" + '\n' +
                " | - Optional Arguments:" + '\n' +
                " | | -f   or --filter:    Allows you to filter in the files you want." + '\n' +
                " | | -d   or --details:   Enabled detailed logging including errors." + '\n' +
                " | | -j   or --json:      Converts all CryXML to JSON." + '\n' +
                " | | -ow  or --overwrite: Overwrites files that already exist." + '\n' +
                " | | -y   or --yes:       Don't ask for input, just continue. Recommended for automation." + '\n' +
                " |/" + '\n' +
                "/" + '\n';

    internal static void Init(List<string>? args = null)
    {
        Logger.ClearBuffer();
        // Parse arguments
        for (var i = 0; i < args?.Count; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "-h":
                case "--help":
                    Logger.Log(Manual);
                    Environment.Exit(0);
                    break;
                case "-i":
                case "--input":
                    Globals.P4KFile = new FileInfo(args[i + 1]);
                    break;
                case "-o":
                case "--output":
                    Globals.OutDirectory = new DirectoryInfo(args[i + 1]);
                    break;
                case "-t":
                case "--threads":
                {
                    if (int.TryParse(args[i + 1], out var threads)) Globals.ThreadLimit = threads;
                    else throw new InvalidCastException(args[i + 1]);
                    break;
                }
                case "-f":
                case "--filter":
                    Globals.Filters = [.. args[i + 1].Split(',')];
                    break;
                case "-d":
                case "--details":
                    Globals.ShouldPrintDetailedLogs = true;
                    break;
                case "-j":
                case "--json":
                    Globals.ShouldConvertToJson = true;
                    break;
                case "-ow":
                case "--overwrite":
                    Globals.ShouldOverwrite = true;
                    break;
                case "-y":
                case "--yes":
                    Globals.ShouldAcceptEverything = true;
                    break;
            }
        }
        
        // Check for argument based input/output paths and ask for them if not declared.
        var acceptingInputs = true;
        while (acceptingInputs)
        {
            if (Globals.P4KFile is null)
            {
                Console.Write("Please provide the absolute Data.p4k file path: ");
                var input = Console.ReadLine();
                Globals.P4KFile = input is not null ? new FileInfo(input) : Defaultp4KFile;
            }

            if (Globals.OutDirectory is null)
            {
                Console.Write("Please provide the absolute output directory path: ");
                var output = Console.ReadLine();
                Globals.OutDirectory = output is not null ? new DirectoryInfo(output) : DefaultExtractionDirectory;
            }

            if (Globals.P4KFile is null)
            {
                Logger.LogError("A .p4k file has not been specified or it does not exist and SEUnp4ker cannot discern its location.\n\nExiting...");
                Environment.Exit(1);
            }
            else if (!Globals.P4KFile.Exists || !Globals.P4KFile.FullName.EndsWith(".p4k"))
            {
                Logger.LogError($"Input path '{Globals.P4KFile.FullName}' does not exist or is not a valid P4K file.");
                Logger.LogError($"Make sure you have the path pointing to a Star Engine Data.p4k file!");
                if (!Globals.ShouldAcceptEverything) Console.ReadKey();
            }
            else
            {
                if (Globals.OutDirectory.Exists) return;
                Logger.LogWarn($"Output path '{Globals.OutDirectory.FullName}' does not exist! Creating...");
                Globals.OutDirectory.Create();
                acceptingInputs = false;
            }
        }
        
        // Show the user any warning if anything worrisome is detected.
        var question = string.Empty;
        var askQuestion = false;
        
        if (Globals.Filters.Contains("*.*"))
        {
            question += "\nENORMOUS JOB WARNING: unp4k has been run with no filters or the *.* filter!\n" +
                        "This extraction job will take up a large amount of space and take quite some time to complete!";
            askQuestion = true;
        }
        
        if (Globals.ShouldOverwrite)
        {
            question += "\nOVERWRITE ENABLED: unp4k has been run with the overwrite option!\n" +
                        "Overwriting files will take much longer than choosing a new empty directory!\n";
            askQuestion = true;
        }

        if (!askQuestion) return;
        if (Logger.AskUserInput(question += "\n\nProceed")) return;
        Logger.LogInfo("Exiting...");
        Environment.Exit(0);
    }
}