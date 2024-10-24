using System.Formats.Tar;
using System.IO.Hashing;
using System.Text;

namespace Halva.Package.Core.Managers;
public sealed class PackageReader : IDisposable
{
    private bool disposedValue;

    /// <summary>
    /// The location of the source files.
    /// </summary>
    private StringBuilder SourceLocation { get; set; }
    /// <summary>
    /// The memory stream that handles the archive.
    /// </summary>
    private TarReader ArchiveMemoryStream { get; set; }
    /// <summary>
    /// The temporary archive where changes are being worked on.
    /// </summary>
    private string WorkingArchive { get; set; }

    /// <summary>
    /// The password of the archive. Don't forget to fill this in if you are working with encrypted packages.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// The initialisation Vector for an encrypted archive. This is necessary if an encrypted package used a custom IV.
    /// </summary>
    public string IVKey { get; set; }

    private readonly bool isMemoryStream;
    private readonly MemoryStream ZipStream;
    private readonly FileStream ZipFileStream;

    /// <summary>
    /// Opens an existing Halva 2 archive for reading and extracting files.
    /// </summary>
    /// <param name="source">The source package to open.</param>
    /// <param name="useMemoryStream">Loads the decompressed package to a <see cref="MemoryStream"/>. Suitable for smaller packages.</param>
    /// <param name="password">The password for the archive. Required for opening encrypted archives.</param>
    /// <param name="iv">The Initialisation Vector for encrypted archives.</param>
    public PackageReader(string source, bool useMemoryStream = false, string password = "", string iv = "")
    {
        SourceLocation = new StringBuilder(source);
        Password = password;
        IVKey = iv;
        if (useMemoryStream)
        {
            isMemoryStream = true;
            ZipStream = new();
            isMemoryStream = true;
            PackageUtilities.DecompressArchive(File.OpenRead(source), ZipStream, password, iv);
            ZipStream.Position = 0;
            ArchiveMemoryStream = new(ZipStream, true);
        }
        else
        {
            WorkingArchive = PackageUtilities.ReserveRandomArchive();
            PackageUtilities.DecompressArchive(source, WorkingArchive, password, iv);
            ZipFileStream = new(WorkingArchive, FileMode.Open);
            ArchiveMemoryStream = new(ZipFileStream, true);
        }
    }

    public void ExtractFile(string entry, string exportLocation)
    {
        TarEntry fileEntry;
        do
        {
            fileEntry = ArchiveMemoryStream.GetNextEntry();
            if (fileEntry == null) break;
        } while (fileEntry.Name != entry);
        fileEntry?.ExtractToFile(Path.Combine(exportLocation, entry), false);
        ReloadArchive();
    }

    public async Task ExtractFileAsync(string entry, string exportLocation, CancellationToken abortToken = default)
    {
        TarEntry fileEntry;
        do
        {
            fileEntry = await ArchiveMemoryStream.GetNextEntryAsync(cancellationToken: abortToken);
            if (fileEntry == null) break;
        } while (fileEntry.Name != entry);
        if (fileEntry != null) await fileEntry?.ExtractToFileAsync(Path.Combine(exportLocation, entry), true, abortToken);
        await Task.Run(ReloadArchive, abortToken);
    }

    /// <summary>
    /// Exports the files that are different between the archive and the target folder.
    /// </summary>
    /// <param name="TargetFolder">The folder where the files to update are.</param>
    /// 
    public void UpdateFromArchive(string TargetFolder)
    {
        TarEntry tempEntry;
        do
        {
            tempEntry = ArchiveMemoryStream.GetNextEntry();
            if (tempEntry != null)
            {
                string targetName = Path.Combine(!(TargetFolder[..0] != Path.DirectorySeparatorChar.ToString()) ? TargetFolder + Path.DirectorySeparatorChar : TargetFolder, PackageUtilities.NormalizePath(tempEntry.Name).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                if (tempEntry != null)
                    if (File.Exists(targetName))
                    {
                        ReadOnlySpan<byte> originalFileSignature;
                        ReadOnlySpan<byte> targetFileSignature;
                        using (Stream archivedFile = tempEntry.DataStream)
                        {
                            using (FileStream targetFile = new(targetName, FileMode.Open, FileAccess.Read))
                            {
                                XxHash128 archiveHash = new();
                                XxHash128 targetHash = new();
                                archiveHash.Append(archivedFile);
                                targetHash.Append(targetFile);
                                originalFileSignature = archiveHash.GetCurrentHash();
                                targetFileSignature = targetHash.GetCurrentHash();
                            }
                            tempEntry.DataStream.Position = 0;
                            if (originalFileSignature != targetFileSignature) tempEntry.ExtractToFile(targetName, true);
                        }
                    }
                    else
                        tempEntry.ExtractToFile(targetName, true);
            }
        }
        while (tempEntry != null);
        ReloadArchive();
    }

    /// <summary>
    /// Exports the files that are different between the archive and the target folder. This is the async version.
    /// </summary>
    /// <param name="TargetFolder">The folder where the files to update are.</param>
    /// <param name="abortToken">The <see cref="CancellationToken"/> used to abort the task.</param>
    /// 
    public async Task UpdateFromArchiveAsync(string TargetFolder, CancellationToken abortToken = default)
    {
        TarEntry tempEntry;
        string targetName;
        do
        {
            tempEntry = ArchiveMemoryStream.GetNextEntry();
            if (tempEntry != null)
            {
                targetName = Path.Combine(!(TargetFolder[..0] != Path.DirectorySeparatorChar.ToString()) ? TargetFolder + Path.DirectorySeparatorChar : TargetFolder, PackageUtilities.NormalizePath(tempEntry.Name).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                if (File.Exists(targetName))
                {
                    byte[] originalFileSignature;
                    byte[] targetFileSignature;
                    using (Stream archivedFile = tempEntry.DataStream)
                    {
                        using (FileStream targetFile = new(targetName, FileMode.Open, FileAccess.Read))
                        {
                            XxHash128 archiveHash = new();
                            XxHash128 targetHash = new();
                            await archiveHash.AppendAsync(archivedFile, abortToken);
                            originalFileSignature = archiveHash.GetCurrentHash();
                            await targetHash.AppendAsync(targetFile, abortToken);
                            targetFileSignature = targetHash.GetCurrentHash();
                        }
                        if (originalFileSignature != targetFileSignature)
                            await tempEntry.ExtractToFileAsync(targetName, true, abortToken);
                    }

                }
                else
                    await tempEntry.ExtractToFileAsync(targetName, true, abortToken);
            }
        }
        while (tempEntry != null);
        await ReloadArchiveAsync(abortToken);
    }

    public void ReloadArchive()
    {
        ArchiveMemoryStream.Dispose();
        if (isMemoryStream)
        {
            ZipStream.Position = 0;
            ArchiveMemoryStream = new(ZipStream, true);
        }
        else
        {
            ZipFileStream.Position = 0;
            ArchiveMemoryStream = new(ZipFileStream, true);
        }

    }

    public async Task ReloadArchiveAsync(CancellationToken abortToken = default)
    {
        await ArchiveMemoryStream.DisposeAsync();
        if (isMemoryStream)
        {
            ZipStream.Position = 0;
            ArchiveMemoryStream = new(ZipStream, true);
        }
        else
        {
            ZipFileStream.Position = 0;
            ArchiveMemoryStream = new(ZipFileStream, true);
        }

    }

    /// <summary>
    /// Removes the archive and deletes the temp archive.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                SourceLocation?.Clear();
                ArchiveMemoryStream.Dispose();
                ZipStream?.Close();
                ZipFileStream?.Dispose();
                if (File.Exists(WorkingArchive)) File.Delete(WorkingArchive);
            }
            disposedValue = true;
        }
    }
}
