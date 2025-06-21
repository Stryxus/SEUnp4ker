using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using ICSharpCode.SharpZipLib.Zip;
using unlib;
using unp4ker;

namespace unp4k;

public class Worker : IDisposable
{
	private P4KFile P4K { get; }
	
	private int _isDecompressCount = 0;
	private int _isLockedCount = 0;
	private long _bytesSize = 0L;
	private bool _additionalFiles = false;

	public Worker()
	{
		P4K = new P4KFile(Globals.P4KFile);
		P4K.FilteredOrderedEntries = P4K.FilterEntries(entry =>
		{
			if (Globals.Filters.Count != 0 && !Globals.Filters.Any(o => entry.Name.Contains(o))) return false;
			FileInfo f = new(Path.Join(Globals.OutDirectory?.FullName, entry.Name));
			
			var isDecompress = entry.CanDecompress;
			var isLocked = entry.IsCrypted;
			var fileExists = f.Exists;
			var fileLength = fileExists ? f.Length : 0L;
			var entryLength = entry.CompressedSize;
			if (fileExists && !Globals.ShouldOverwrite && !Globals.ShouldPrintDetailedLogs)
			{
				_additionalFiles = true;
				if (_bytesSize - fileLength > 0L) _bytesSize -= fileLength;
				else _bytesSize = 0L;
			}
			else
			{
				_bytesSize += entryLength;
				if (!isDecompress) _isDecompressCount++;
				if (isLocked) _isLockedCount++;
			}
			return !isLocked && (Globals.ShouldOverwrite || Globals.ShouldPrintDetailedLogs || !fileExists || fileLength != entryLength);
		}).OrderBy(x => x).ToList();
	}

	public void Process()
    {
		var outputDrive = Platform.GetOutputDrive(Globals.OutDirectory);
		if (outputDrive == null) return;
		var summary =
				@"\" + '\n' +
				$" |              Output Path | {Globals.OutDirectory?.FullName}" + '\n' +
				$" |                Partition | {outputDrive.VolumeLabel} | {outputDrive.DriveType} | {outputDrive.DriveFormat}" + '\n' +
				$" |     Available Free Space | {outputDrive.AvailableFreeSpace / 1000000000D:#,##0.000000000} GB" + '\n' +
				$" | Estimated Required Space | {(!Globals.ShouldOverwrite && _additionalFiles ? "An Additional " : string.Empty)}" +
																				$"{_bytesSize / 1000000000D:#,##0.000000000} GB" + '\n' +
				 " |                          | " + '\n' +
				$" |               File Count | {P4K.FilteredOrderedEntries.Count:#,##0}" +
																				$"{(!Globals.ShouldOverwrite && _additionalFiles ? " Additional Files" : string.Empty)}" +
																				$"{(Globals.Filters.Count != 0 ? $" Filtered From {string.Join(",", Globals.Filters)}" : string.Empty)}" + '\n' +
				$" |       Files Incompatible | {_isDecompressCount:#,##0}" +
																				$"{(!Globals.ShouldOverwrite && _additionalFiles ? " Additional Files" : string.Empty)}" +
																				$"{(Globals.Filters.Count != 0 ? $" Filtered From {string.Join(",", Globals.Filters)}" : string.Empty)}" + '\n' +
				$" |             Files Locked | {_isLockedCount:#,##0}" +
																				$"{(!Globals.ShouldOverwrite && _additionalFiles ? " Additional Files" : string.Empty)}" +
																				$"{(Globals.Filters.Count != 0 ? $" Filtered From {string.Join(",", Globals.Filters)}" : string.Empty)}" + '\n' +
				 " |                          | " + '\n' +
				$" | Overwrite Existing Files | {Globals.ShouldOverwrite}" + '\n' +
				 " |                          | " + '\n' +
				 " |                          | Speed highly depends on and is bottlenecked in almost all cases by your drives Random IO (Many small files) speeds." + '\n' +
				 " |                          | Tools like CrystalDiskMark call this 4kRnd (4k bytes random read/write)." + '\n' +
				@"/";
		
		if (outputDrive.AvailableFreeSpace + (Globals.ShouldOverwrite ? Globals.OutDirectory?.GetFiles("*.*", SearchOption.AllDirectories).Sum(x => x.Length) : 0) < _bytesSize)
		{
			Logger.Log("The output path you have chosen is on a partition which does not have enough available free space!" + '\n' + summary);
			if (!Globals.ShouldAcceptEverything) Console.ReadKey();
		}
		else Logger.NewLine();
		
		Logger.Log("Pre-Process Summary" + '\n' + summary);
		if (!Logger.AskUserInput("Proceed?")) Environment.Exit(0);
		Logger.ClearBuffer();
		
	    // Start the extraction process
		var tasksDone = 0;
	    Stopwatch overallTime = new();
        Stopwatch fileTime = new();
        overallTime.Start();

        if (P4K.Archive.Count is not 0)
        {
	        BlockingCollection<ZipEntry> outputQueue = new((int)P4K.Archive.Count);
            var output = Task.Run(() => Print(outputQueue, fileTime));
            var pwi = P4K.GetParallelEnumerator(Globals.ThreadLimit, ParallelMergeOptions.NotBuffered, (entry, _) =>
            {
                if (Globals.ShouldPrintDetailedLogs) fileTime.Restart();
                FileInfo extractionFile = new(Path.Join(Globals.OutDirectory?.FullName, entry.Name).Replace('\\', Path.DirectorySeparatorChar));
                try 
                { 
                    P4K.Extract(entry, extractionFile);
                }
                catch (Exception e)
                {
                    if (Globals.ShouldPrintDetailedLogs) Logger.LogException(e);
                    Globals.FileErrors++;
                }
                Interlocked.Increment(ref tasksDone);
                return entry;
            });

            foreach (var item in pwi) outputQueue.Add(item);
            outputQueue.CompleteAdding();
            output.Wait();

            void Print(BlockingCollection<ZipEntry> queue, Stopwatch timeTaken) // TODO: Add ability to send errors through to this and flag the square as red.
            {
	            foreach (var entry in queue.GetConsumingEnumerable())
	            {
		            var percentage = (tasksDone is 0 ? 0D : 100D * tasksDone / P4K.FilteredOrderedEntries.Count).ToString("000.000");
		            if (Globals.ShouldPrintDetailedLogs)
		            {
			            Logger.LogInfo($"{percentage}% \e[92m■\e[39m > {entry.Name.Replace('\\', Path.DirectorySeparatorChar)}" + '\n' +
			                           @"\" + '\n' +
			                           $" | Date Last Modified: {entry.DateTime}" + '\n' +
			                           $" | Compression Method: {entry.CompressionMethod}" + '\n' +
			                           $" | Compressed Size   : {entry.CompressedSize  / 1000000D:#,##0.000} MB" + '\n' +
			                           $" | Uncompressed Size : {entry.Size            / 1000000D:#,##0.000} MB" + '\n' +
			                           $" | Time Taken:         {timeTaken.ElapsedMilliseconds / 1000D:#,##0.000} seconds" + '\n' +
			                           @"/");
		            }
		            else Logger.LogInfo($"{percentage}% \e[92m■\e[39m > {entry.Name[(entry.Name.LastIndexOf('/') + 1)..].Replace('\\', Path.DirectorySeparatorChar)}");
		            Logger.SetTitle($"unp4k: {percentage}%");
	            }
            }
        }
        else Logger.LogInfo("No extraction work to be done!");
        
        overallTime.Stop();
        Logger.NewLine();
        Logger.Log(
            "Extraction Complete" + '\n' +
            @"\" + '\n' +
            $" | File Errors: {Globals.FileErrors:#,##0}" + '\n' +
            $" | Time Taken: {(float)overallTime.ElapsedMilliseconds / 60000:#,##0.000} minutes");
        
        if (Globals.OutDirectory is not null && Logger.AskUserInput("Would you like to open the output directory before exiting?")) Platform.OpenFileManager(Globals.OutDirectory.FullName);
    }

	public void Dispose()
	{
		P4K.Dispose();
	}
}
