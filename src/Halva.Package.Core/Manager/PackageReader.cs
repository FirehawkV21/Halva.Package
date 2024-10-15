using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Halva.Package.Core.Utilities;

namespace Halva.Package.Core.Manager;
public class PackageReader
{
    private bool disposedValue;

    /// <summary>
    /// The location of the source files.
    /// </summary>
    public StringBuilder SourceLocation { get; set; }
    /// <summary>
    /// The list of files of the archive.
    /// </summary>
    public List<string> FileList { get; set; } = [];
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

    public PackageReader(string destination, string password, bool useMemoryStream)
    {
        SourceLocation = new StringBuilder(destination);
        Password = password;
        if (useMemoryStream)
        {
            isMemoryStream = true;
            ZipStream = new();
            isMemoryStream = true;
            EncryptedPackageUtilities.DecompressArchive(File.OpenRead(destination), out ZipStream, password);
            ArchiveMemoryStream = new(ZipStream, true);
        }
        else
        {
            WorkingArchive = ReserveRandomArchive();
            EncryptedPackageUtilities.DecompressArchive(destination, WorkingArchive, password);
            ZipFileStream = new(WorkingArchive, FileMode.Open);
            ArchiveMemoryStream = new(ZipFileStream, true);
        }
    }

    public PackageReader(string destination, string password, string iv, bool useMemoryStream)
    {
        SourceLocation = new StringBuilder(destination);
        Password = password;
        IVKey = iv;
        if (useMemoryStream)
        {
            isMemoryStream = true;
            ZipStream = new();
            isMemoryStream = true;
            EncryptedPackageUtilities.DecompressArchive(File.OpenRead(destination), out ZipStream, password, iv);
            ArchiveMemoryStream = new(ZipStream, true);
        }
        else
        {
            WorkingArchive = ReserveRandomArchive();
            EncryptedPackageUtilities.DecompressArchive(destination, WorkingArchive, password, iv);
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
}
