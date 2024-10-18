using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Halva.Package.Core.Utilities;

namespace Halva.Package.Core.Manager;
public class PackageReader : IDisposable
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
    private MemoryStream ZipStream;
    private FileStream ZipFileStream;

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
            WorkingArchive = ReserveRandomArchive();
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
            WorkingArchive = ReserveRandomArchive();
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
            WorkingArchive = ReserveRandomArchive();
            EncryptedPackageUtilities.DecompressArchive(source, WorkingArchive, password, iv);
            ZipFileStream = new(WorkingArchive, FileMode.Open);
            ArchiveMemoryStream = new(ZipFileStream, true);
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
