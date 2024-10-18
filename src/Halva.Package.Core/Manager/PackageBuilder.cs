using System.Formats.Tar;
using System.IO.Compression;
using System.Text;
using Halva.Package.Core.Utilities;

namespace Halva.Package.Core.Manager;
public class PackageBuilder : IDisposable
{
    private bool disposedValue;

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

    public PackageBuilder(string destination, bool useMemoryStream)
    {
        DestinationLocation = new StringBuilder(destination);
        if (useMemoryStream)
        {
            isMemoryStream = true;
            ZipStream = new();
            ArchiveMemoryStream = new(ZipStream, TarEntryFormat.Pax, true);
        }
        else
        {
            WorkingArchive = ReserveRandomArchive();
            ZipFileStream = new(WorkingArchive, FileMode.OpenOrCreate);
            ArchiveMemoryStream = new(ZipFileStream, TarEntryFormat.Pax, true);
        }
    }

    public PackageBuilder(string destination, string password, bool useMemoryStream)
    {
        DestinationLocation = new StringBuilder(destination);
        Password = password;
        if (useMemoryStream)
        {
            isMemoryStream = true;
            ZipStream = new();
            ArchiveMemoryStream = new(ZipStream, TarEntryFormat.Pax, true);
        }
        else
        {
            WorkingArchive = ReserveRandomArchive();
            ZipFileStream = new(WorkingArchive, FileMode.OpenOrCreate);
            ArchiveMemoryStream = new(ZipFileStream, TarEntryFormat.Pax, true);
        }
    }

    public PackageBuilder(string destination, string password, string iv, bool useMemoryStream)
    {
        DestinationLocation = new StringBuilder(destination);
        Password = password;
        IVKey = iv;
        if (useMemoryStream)
        {
            isMemoryStream = true;
            ZipStream = new();
            ArchiveMemoryStream = new(ZipStream, TarEntryFormat.Pax, true);
        }
        else
        {
            WorkingArchive = ReserveRandomArchive();
            ZipFileStream = new(WorkingArchive, FileMode.OpenOrCreate);
            ArchiveMemoryStream = new(ZipFileStream, TarEntryFormat.Pax, true);
        }
    }

    public static string ReserveRandomArchive()
    {
        string tempString = "TempArchive_";
        Random _random = new();
        int check;
        do
        {
            check = _random.Next(99999);
        }
        while (File.Exists(Path.Combine(Path.GetTempPath(), tempString + check + ".tmp")));
        return Path.Combine(Path.GetTempPath(), tempString + check + ".tmp");
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
        {
            FileList.Add(fileEntry.Replace(sourceLocation + GetFolderCharacter(), ""));
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
