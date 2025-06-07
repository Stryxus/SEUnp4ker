using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace unp4ker;

public class P4KFile : IDisposable
{
    private ZipFile? P4K { get; set; }
    private List<ZipEntry> Entries { get; set; } = [];
    
    public int EntryCount => Entries.Count;
    
    public P4KFile(FileInfo? p4KFile)
    {
        P4K = new ZipFile(p4KFile?.Open(FileMode.Open, FileAccess.Read, FileShare.None))
        {
            UseZip64 = UseZip64.On,
        }; // The Data.p4k must be locked while it is being read to avoid corruption.
        P4K.KeysRequired += (_, e) => e.Key = [0x5E, 0x7A, 0x20, 0x02, 0x30, 0x2E, 0xEB, 0x1A, 0x3B, 0xB6, 0x17, 0xC3, 0x0F, 0xDE, 0x1E, 0x47];
        foreach (var entry in P4K) Entries.Add(entry);
    }
    
    public void FilterEntries(Func<ZipEntry, bool> where) => Entries = [..Entries.Where(where)];
    
    public void OrderBy<TU>(Func<ZipEntry, TU> order) => Entries = [..Entries.OrderBy(order)];

    public ParallelQuery<ZipEntry> GetParallelEnumerator(int threads, ParallelMergeOptions merge, Func<ZipEntry, int, ZipEntry> func) => Entries.AsParallel().AsOrdered().WithDegreeOfParallelism(threads).WithMergeOptions(merge).Select(func);
    
    public void Extract(ZipEntry entry, FileInfo extractionFile)
    {
        var decomBuffer = new byte[65536];
        if (extractionFile.Directory is { Exists: false }) extractionFile.Directory.Create();
        else if (extractionFile.Exists) extractionFile.Delete();
        var fs = extractionFile.Open(FileMode.Create, FileAccess.Write, FileShare.ReadWrite); // Dont want people accessing incomplete files.
        if (P4K != null)
        {
            var decompStream = P4K.GetInputStream(entry);
            StreamUtils.Copy(decompStream, fs, decomBuffer);
            decompStream.Close();
        }

        fs.Close();
    }

	public void Dispose()
    {
        P4K?.Close();
        GC.SuppressFinalize(this);
    }
}