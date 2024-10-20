using System.Formats.Tar;
using System.IO.Compression;
using System.IO.Hashing;
using System.Text;
using Halva.Package.Core.Utilities;

namespace Halva.Package.Core.Manager;
public sealed class PackageReader : IDisposable
{
    private bool disposedValue;

    /// <summary>
    /// The location of the source files.
    /// </summary>
    public StringBuilder SourceLocation { get; set; }
    /// <summary>
    /// The memory stream that handles the archive.
    /// </summary>
    public TarReader ArchiveMemoryStream { get; set; }
    /// <summary>
    /// The temporary archive where changes are being worked on.
    /// </summary>
    public string WorkingArchive { get; set; }

    /// <summary>
    /// Gets the character used for path designation.
    /// </summary>
    /// <returns>Either "\\" (in Windows) or "/" (Unix systems).</returns>
    private static string GetFolderCharacter() => Path.DirectorySeparatorChar.ToString();
    /// <summary>
    /// Adjusts the compression of the final archive.
    /// </summary>
    public CompressionLevel CompressionOption { get; set; } = CompressionLevel.Optimal;
    /// <summary>
    /// The password of the archive. Don't forget to fill this in if you are working with encrypted packages.
    /// </summary>
    public string Password { get; set; }

    public string IVKey { get; set; }

    private readonly bool isMemoryStream;
    private readonly MemoryStream ZipStream;
    private readonly FileStream ZipFileStream;

    public PackageReader(string source, bool useMemoryStream)
    {
        SourceLocation = new StringBuilder(source);
        if (useMemoryStream)
        {
            isMemoryStream = true;
            PackageUtilities.DecompressArchive(File.OpenRead(source), out ZipStream);
            ArchiveMemoryStream = new(ZipStream, true);
        }
        else
        {
            WorkingArchive = PackageUtilities.ReserveRandomArchive();
            PackageUtilities.DecompressArchive(source, WorkingArchive);
            ZipFileStream = new(WorkingArchive, FileMode.Open);
            ArchiveMemoryStream = new(ZipFileStream, true);
        }
    }

    public PackageReader(string source, bool useMemoryStream, string password)
    {
        SourceLocation = new StringBuilder(source);
        Password = password;
        if (useMemoryStream)
        {
            isMemoryStream = true;
            ZipStream = new();
            isMemoryStream = true;
            EncryptedPackageUtilities.DecompressArchive(File.OpenRead(source), out ZipStream, password);
            ArchiveMemoryStream = new(ZipStream, true);
        }
        else
        {
            WorkingArchive = PackageUtilities.ReserveRandomArchive();
            EncryptedPackageUtilities.DecompressArchive(source, WorkingArchive, password);
            ZipFileStream = new(WorkingArchive, FileMode.Open);
            ArchiveMemoryStream = new(ZipFileStream, true);
        }
    }

    public PackageReader(string source, bool useMemoryStream, string password, string iv)
    {
        SourceLocation = new StringBuilder(source);
        Password = password;
        IVKey = iv;
        if (useMemoryStream)
        {
            isMemoryStream = true;
            ZipStream = new();
            isMemoryStream = true;
            EncryptedPackageUtilities.DecompressArchive(File.OpenRead(source), out ZipStream, password, iv);
            ArchiveMemoryStream = new(ZipStream, true);
        }
        else
        {
            WorkingArchive = PackageUtilities.ReserveRandomArchive();
            EncryptedPackageUtilities.DecompressArchive(source, WorkingArchive, password, iv);
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
        fileEntry?.ExtractToFile(Path.Combine(exportLocation, entry), true);
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
        await fileEntry?.ExtractToFileAsync(Path.Combine(exportLocation, entry), true, abortToken);
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
                if (File.Exists(Path.Combine(TargetFolder, tempEntry.Name)))
                {
                    ReadOnlySpan<byte> originalFileSignature;
                    ReadOnlySpan<byte> targetFileSignature;
                    using (Stream archivedFile = tempEntry.DataStream)
                    {
                        using (FileStream targetFile = new(Path.Combine(TargetFolder, tempEntry.Name), FileMode.Open, FileAccess.Read))
                        {
                            XxHash128 archiveHash = new();
                            XxHash128 targetHash = new();
                            archiveHash.Append(archivedFile);
                            targetHash.Append(targetFile);
                            originalFileSignature = archiveHash.GetCurrentHash();
                            targetFileSignature = targetHash.GetCurrentHash();
                        }
                    }
                    if (originalFileSignature != targetFileSignature)
                    {
                        tempEntry.ExtractToFile(Path.Combine(TargetFolder, tempEntry.Name), true);
                    }
                }
                else
                {
                    tempEntry.ExtractToFile(Path.Combine(TargetFolder, tempEntry.Name), true);
                }
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
        do
        {
            tempEntry = ArchiveMemoryStream.GetNextEntry();
            if (tempEntry != null)
            {
                if (File.Exists(Path.Combine(TargetFolder, tempEntry.Name)))
                {
                    byte[] originalFileSignature;
                    byte[] targetFileSignature;
                    using (Stream archivedFile = tempEntry.DataStream)
                    {
                        using (FileStream targetFile = new(Path.Combine(TargetFolder, tempEntry.Name), FileMode.Open, FileAccess.Read))
                        {
                            XxHash128 archiveHash = new();
                            XxHash128 targetHash = new();
                            await archiveHash.AppendAsync(archivedFile, abortToken);
                            originalFileSignature = archiveHash.GetCurrentHash();
                            await targetHash.AppendAsync(targetFile, abortToken);
                            targetFileSignature = targetHash.GetCurrentHash();
                        }
                    }
                    if (originalFileSignature != targetFileSignature)
                    {
                        await tempEntry.ExtractToFileAsync(Path.Combine(TargetFolder, tempEntry.Name), true, abortToken);
                    }
                }
                else
                {
                    await tempEntry.ExtractToFileAsync(Path.Combine(TargetFolder, tempEntry.Name), true, abortToken);
                }
            }
        }
        while (tempEntry != null);
        ReloadArchive();
    }

    public void ReloadArchive()
    {
        ArchiveMemoryStream.Dispose();
        ArchiveMemoryStream = isMemoryStream ? new(ZipStream, true) : new(ZipFileStream, true);
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
                if (File.Exists(WorkingArchive)) File.Delete(WorkingArchive);
            }
            disposedValue = true;
        }
    }
}
