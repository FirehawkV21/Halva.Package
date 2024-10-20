using System.Formats.Tar;
using System.IO.Compression;
using System.Text;

namespace Halva.Package.Core.Managers;
public sealed class PackageBuilder : IDisposable
{
    private bool disposedValue;
    private readonly CompressorEngine engine = new();

    /// <summary>
    /// The location of the final acrhive.
    /// </summary>
    public StringBuilder DestinationLocation { get; set; }
    /// <summary>
    /// The list of files of the archive.
    /// </summary>
    public List<string> FileList { get; set; } = [];
    /// <summary>
    /// The memory stream that handles the archive.
    /// </summary>
    public TarWriter ArchiveMemoryStream { get; set; }
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
    private MemoryStream ZipStream;
    private FileStream ZipFileStream;

    public PackageBuilder(string destination, bool useMemoryStream = false, string password = "", string iv = "")
    {
        DestinationLocation = new StringBuilder(destination);
        Password = password;
        IVKey = iv;
        if (useMemoryStream)
        {
            isMemoryStream = true;
            ZipStream = new();
            ArchiveMemoryStream = new(ZipStream, TarEntryFormat.Pax, false);
        }
        else
        {
            WorkingArchive = PackageUtilities.ReserveRandomArchive();
            ZipFileStream = new(WorkingArchive, FileMode.OpenOrCreate);
            ArchiveMemoryStream = new(ZipFileStream, TarEntryFormat.Pax, false);
        }
    }

    /// <summary>
    /// Adds a specified file to the list. This method is better suited for preserving the folder structure.
    /// </summary>
    /// <param name="source">The base folder that holds the file.</param>
    /// <param name="fileRelativeLocation">The relative location of the file.</param>
    public void AddFileToList(string source, string fileRelativeLocation)
    {
        FileList.Add(fileRelativeLocation);
    }

    /// <summary>
    /// Adds files from a specific folder. The folder relative location is used to avoid messing up the folder structure.
    /// </summary>
    /// <param name="sourceLocation">The location of the source folder</param>
    /// <param name="SourceFolderRelativeLocation">The relatice location of the source folder.</param>
    public void AddFilesFromAFolder(string sourceLocation, string SourceFolderRelativeLocation)
    {
        List<string> tempList = PullFilesFromFolder(Path.Combine(sourceLocation, SourceFolderRelativeLocation));

        foreach (string fileEntry in tempList)
            FileList.Add(fileEntry.Replace(sourceLocation + GetFolderCharacter(), ""));

    }

    public void Commit()
    {
        foreach (string file in FileList)
        {
            ArchiveMemoryStream.WriteEntry(file, "");
        }
        if (isMemoryStream)
        {
            PackageUtilities.CompressArchive(ZipStream, DestinationLocation.ToString(), CompressionOption, Password, IVKey);
            ArchiveMemoryStream.Dispose();
            ZipStream.Dispose();
            ZipStream = new();
            ArchiveMemoryStream = new(ZipStream, TarEntryFormat.Pax, false);
        }
        else
        {
            ArchiveMemoryStream.Dispose();
            ZipFileStream.Close();
            PackageUtilities.CompressArchive(WorkingArchive, DestinationLocation.ToString(), CompressionOption, Password, IVKey);
            File.Delete(WorkingArchive);
            WorkingArchive = PackageUtilities.ReserveRandomArchive();
            ZipFileStream = new(WorkingArchive, FileMode.OpenOrCreate);
            ArchiveMemoryStream = new(ZipFileStream, TarEntryFormat.Pax, false);

        }
    }

    public async Task CommitAsync(CancellationToken abortToken = default)
    {
        foreach (string file in FileList)
        {
            await ArchiveMemoryStream.WriteEntryAsync(file, "", abortToken);
        }
        if (isMemoryStream)
        {
            await PackageUtilities.CompressArchiveAsync(ZipStream, DestinationLocation.ToString(), CompressionOption, Password, IVKey, abortToken);
            ArchiveMemoryStream.Dispose();
            ZipStream.Dispose();
            ZipStream = new();
            ArchiveMemoryStream = new(ZipStream, TarEntryFormat.Pax, true);
        }
        else
        {
            ArchiveMemoryStream.Dispose();
            ZipFileStream.Close();
            await PackageUtilities.CompressArchiveAsync(WorkingArchive, DestinationLocation.ToString(), CompressionOption, Password, IVKey, abortToken);
            File.Delete(WorkingArchive);
            WorkingArchive = PackageUtilities.ReserveRandomArchive();
            ZipFileStream = new(WorkingArchive, FileMode.OpenOrCreate);
            ArchiveMemoryStream = new(ZipFileStream, TarEntryFormat.Pax, false);
        }
    }

    public static List<string> PullFilesFromFolder(string source)
    {
        IEnumerable<string> foundFiles = Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories);
        return foundFiles.ToList();
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
                DestinationLocation?.Clear();
                FileList.Clear();
                FileList = null;
                ArchiveMemoryStream.Dispose();
                ZipStream?.Close();
                if (File.Exists(WorkingArchive)) File.Delete(WorkingArchive);
            }

            disposedValue = true;
        }
    }
}
