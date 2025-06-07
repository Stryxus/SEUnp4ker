using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ICSharpCode.SharpZipLib.Zip;
using unlib;
using unp4ker;

namespace unp4k;

public static class Worker
{
	private static P4KFile? P4K { set; get; }

	private static List<string> FilteredOrderedEntries = [];

	internal static bool Process_P4K()
    {
	    var isDecompressCount = 0;
        var isLockedCount = 0;
        var bytesSize = 0L;
        var additionalFiles = false;

		P4K = new P4KFile(Globals.P4KFile);
		// Set up the stream from the Data.p4k and contain it as an ICSC ZipFile with the appropriate keys then enqueue all zip entries.
		// Filter out zip entries which cannot be decompressed and/or are locked behind a cipher.
		// Speed up the extraction by a large amount by filtering out the files which already exist and don't need updating.
		FilteredOrderedEntries = P4K.FilterEntries(entry =>
		{
			if (Globals.Filters.Count == 0 || Globals.Filters.Any(o => entry.Name.Contains(o)))
			{
				FileInfo f = new(Path.Join(Globals.OutDirectory?.FullName, entry.Name));
				var isDecompress = entry.CanDecompress;
				var isLocked = entry.IsCrypted;
				var fileExists = f.Exists;
				var fileLength = fileExists ? f.Length : 0L;
				var entryLength = entry.CompressedSize;
				if (fileExists && !Globals.ShouldOverwrite && !Globals.ShouldPrintDetailedLogs)
				{
					additionalFiles = true;
					if (bytesSize - fileLength > 0L) bytesSize -= fileLength;
					else bytesSize = 0L;
				}
				else
				{
					bytesSize += entryLength;
					if (!isDecompress) isDecompressCount++;
					if (isLocked) isLockedCount++;
				}
				return !isLocked && (Globals.ShouldOverwrite || Globals.ShouldPrintDetailedLogs || !fileExists || fileLength != entryLength);
			}
			else return false;
		}).OrderBy(x => x).ToList();

		var outputDrive = DriveInfo.GetDrives().First(x => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 
			x.Name == Globals.OutDirectory?.FullName[..3] : 
				new DirectoryInfo(x.Name).Exists);
		var summary =
				@"\" + '\n' +
				$" |                      Output Path | {Globals.OutDirectory?.FullName}" + '\n' +
				$" |                        Partition | {outputDrive.Name}" + '\n' +
				$" |   Partition Available Free Space | {outputDrive.AvailableFreeSpace / 1000000000D:#,##0.000000000} GB" + '\n' +
				$" |         Estimated Required Space | {(!Globals.ShouldOverwrite && additionalFiles ? "An Additional " : string.Empty)}" +
																				$"{bytesSize / 1000000000D:#,##0.000000000} GB" + '\n' +
				 " |                                  | " + '\n' +
				$" |                       File Count | {P4K.Archive.Count:#,##0}" +
																				$"{(!Globals.ShouldOverwrite && additionalFiles ? " Additional Files" : string.Empty)}" +
																				$"{(Globals.Filters.Count != 0 ? $" Filtered From {string.Join(",", Globals.Filters)}" : string.Empty)}" + '\n' +
				$" |               Files Incompatible | {isDecompressCount:#,##0}" +
																				$"{(!Globals.ShouldOverwrite && additionalFiles ? " Additional Files" : string.Empty)}" +
																				$"{(Globals.Filters.Count != 0 ? $" Filtered From {string.Join(",", Globals.Filters)}" : string.Empty)}" + '\n' +
				$" |                     Files Locked | {isLockedCount:#,##0}" +
																				$"{(!Globals.ShouldOverwrite && additionalFiles ? " Additional Files" : string.Empty)}" +
																				$"{(Globals.Filters.Count != 0 ? $" Filtered From {string.Join(",", Globals.Filters)}" : string.Empty)}" + '\n' +
				 " |                                  | " + '\n' +
				$" |         Overwrite Existing Files | {Globals.ShouldOverwrite}" + '\n' +
				 " |                                  | " + '\n' +
				 " |                                  | These estimates do not including the UnForging process!" + '\n' +
				 " |                                  | The speed this takes highly depends on your storage drives Random IO (Many small files) speeds." + '\n' +
				 " |                                  | Tools like CrystalDiskMark call this 4kRnd (4k bytes random read/write)." + '\n' +
				@"/";


		// Never allow the extraction to go through if the target storage drive has too little available space.
		if (outputDrive.AvailableFreeSpace + (Globals.ShouldOverwrite ? Globals.OutDirectory?.GetFiles("*.*", SearchOption.AllDirectories).Sum(x => x.Length) : 0) < bytesSize)
		{
			Logger.Log("The output path you have chosen is on a partition which does not have enough available free space!" + '\n' + summary);
			if (!Globals.ShouldAcceptEverything) Console.ReadKey();
			return false;
		}
		else Logger.NewLine();

		if (Globals.ShouldAcceptEverything) return true;
		// Give the user a summary of what unp4k/unforge is about to do and some statistics.
		Logger.Log("Pre-Process Summary" + '\n' + summary);
		return Logger.AskUserInput("Proceed?");
    }

    private static int _tasksDone;
    internal static void Extract()
    {
        // Time the extraction for those who are interested in it.
        Stopwatch overallTime = new();
        Stopwatch fileTime = new();
        overallTime.Start();

        // Extract each entry, then serializing it or the Forging it.
        Logger.NewLine(2);
        if (P4K is not null && P4K.Archive.Count is not 0) // This will never be null, but it makes the analyzer happy.
        {
	        BlockingCollection<ZipEntry> outputQueue = new((int)P4K.Archive.Count);
            var output = Task.Run(() => Print(outputQueue, fileTime));
            var pwi = P4K.GetParallelEnumerator(Globals.ThreadLimit, ParallelMergeOptions.NotBuffered, (entry, _) =>
            {
                if (Globals.ShouldPrintDetailedLogs) fileTime.Restart();
                FileInfo extractionFile = new(Path.Join(Globals.OutDirectory?.FullName, entry.Name));
                try 
                { 
                    P4K.Extract(entry, extractionFile);
                }
                catch (Exception e)
                {
                    if (Globals.ShouldPrintDetailedLogs) Logger.LogException(e);
                    Globals.FileErrors++;
                }
                Interlocked.Increment(ref _tasksDone);
                return entry;
            });

            foreach (var item in pwi) outputQueue.Add(item);
            outputQueue.CompleteAdding();
            output.Wait();

            void Print(BlockingCollection<ZipEntry> queue, Stopwatch timeTaken) // TODO: Add ability to send errors through to this and flag the square as red.
            {
	            if (P4K is null) return; // This will never be null but it makes the analyzer happy.
	            foreach (var entry in queue.GetConsumingEnumerable())
	            {
		            var percentage = (_tasksDone is 0 ? 0D : 100D * _tasksDone / P4K.Archive.Count).ToString("000.00000");
		            if (Globals.ShouldPrintDetailedLogs)
		            {
			            Logger.LogInfo($"{percentage}% \e[92m■\e[39m > {entry.Name}" + '\n' +
			                           @"\" + '\n' +
			                           $" | Date Last Modified: {entry.DateTime}" + '\n' +
			                           $" | Compression Method: {entry.CompressionMethod}" + '\n' +
			                           $" | Compressed Size:    {entry.CompressedSize  / 1000000000D:#,##0.000000000} GB" + '\n' +
			                           $" | Uncompressed Size:  {entry.Size            / 1000000000D:#,##0.000000000} GB" + '\n' +
			                           $" | Time Taken:         {timeTaken.ElapsedMilliseconds / 1000D:#,##0.000} seconds" + '\n' +
			                           @"/");
		            }
		            else Logger.LogInfo($"{percentage}% \e[92m■\e[39m > {entry.Name[(entry.Name.LastIndexOf('/') + 1)..]}");
		            Logger.SetTitle($"unp4k: {percentage}%");
	            }
            }
        }
        else Logger.LogInfo("No extraction work to be done!");

        // Print out the post-summary.
        overallTime.Stop();
        Logger.NewLine();
        Logger.Log(
            "Extraction Complete" + '\n' +
            @"\" + '\n' +
            $" |  File Errors: {Globals.FileErrors:#,##0}" + '\n' +
            $" |  Time Taken: {(float)overallTime.ElapsedMilliseconds / 60000:#,##0.000} minutes" + '\n' +
             " |  DO NOT OUTPUT to any form of SSD/NVMe too many times or else you risk degrading it.");

        // This will never be null, but it makes the analyzer happy.
        if (Globals.OutDirectory is not null && Logger.AskUserInput("Would you like to open the output directory? (Application will close on input)")) Platform.OpenFileManager(Globals.OutDirectory.FullName);
    }
}
