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
            WorkingArchive = PackageUtilities.ReserveRandomArchive();
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
            WorkingArchive = PackageUtilities.ReserveRandomArchive();
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
            WorkingArchive = PackageUtilities.ReserveRandomArchive();
            ZipFileStream = new(WorkingArchive, FileMode.OpenOrCreate);
            ArchiveMemoryStream = new(ZipFileStream, TarEntryFormat.Pax, true);
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
        {
            FileList.Add(fileEntry.Replace(sourceLocation + GetFolderCharacter(), ""));
        }

    }

    /// <summary>
    /// Saves current changes to the destination archive. Note: If you use MemoryStream for the archive, it will no-op. Use Save() instead.
    /// </summary>
    private void CloseArchive()
    {
        ArchiveMemoryStream.Dispose();
        if (isMemoryStream)
        {
            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password))
                if (!string.IsNullOrEmpty(IVKey) && !string.IsNullOrWhiteSpace(IVKey)) EncryptedPackageUtilities.CompressArchive(ZipStream, DestinationLocation.ToString(), Password, IVKey, CompressionOption);
                else EncryptedPackageUtilities.CompressArchive(ZipStream, DestinationLocation.ToString(), Password, CompressionOption);
            else PackageUtilities.CompressArchive(ZipStream, DestinationLocation.ToString(), CompressionOption);
            ZipStream.Close();
        }
        else
        {
            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password))
                if (!string.IsNullOrEmpty(IVKey) && !string.IsNullOrWhiteSpace(IVKey)) EncryptedPackageUtilities.CompressArchive(WorkingArchive, DestinationLocation.ToString(), Password, IVKey, CompressionOption);
                else EncryptedPackageUtilities.CompressArchive(WorkingArchive, DestinationLocation.ToString(), Password, CompressionOption);
            else PackageUtilities.CompressArchive(WorkingArchive, DestinationLocation.ToString(), CompressionOption);
        }
    }

    /// <summary>
    /// Reloads the archive. If you have a password set, it will attempt to decrypt the package first. Note: If you use MemoryStream archive, it will no-op. Use Save() instead.
    /// </summary>
    private void ReloadArchive()
    {
        if (isMemoryStream)
        {
            if (!ZipStream.CanRead)
            {
                FileStream fileLoader = File.Open(DestinationLocation.ToString(), FileMode.Open);
                if (!string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password))
                    if (!string.IsNullOrEmpty(IVKey) && !string.IsNullOrWhiteSpace(IVKey)) EncryptedPackageUtilities.DecompressArchive(fileLoader, out ZipStream, Password, IVKey);
                    else EncryptedPackageUtilities.DecompressArchive(fileLoader, out ZipStream, Password);
                else PackageUtilities.DecompressArchive(fileLoader, out ZipStream);
                fileLoader.Close();
            }
            ArchiveMemoryStream = new(ZipStream, TarEntryFormat.Pax, true);
        }
        else
        {
            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password))
                if (!string.IsNullOrEmpty(IVKey) && !string.IsNullOrWhiteSpace(IVKey)) EncryptedPackageUtilities.DecompressArchive(DestinationLocation.ToString(), WorkingArchive, Password, IVKey);
                else EncryptedPackageUtilities.DecompressArchive(DestinationLocation.ToString(), WorkingArchive, Password);
            else PackageUtilities.DecompressArchive(DestinationLocation.ToString(), WorkingArchive);
            ZipFileStream = new(WorkingArchive, FileMode.OpenOrCreate);
            ArchiveMemoryStream = new(ZipFileStream, TarEntryFormat.Pax, true);
        }
    }

    /// <summary>
    /// Saves the changes to the destination archive. If password is set, it will attempt to encrypt it.
    /// </summary>
    public void Save()
    {
        CloseArchive();
        ReloadArchive();
    }

    public void Finish()
    {
        CloseArchive();
        if (isMemoryStream) ZipStream.Close();
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
