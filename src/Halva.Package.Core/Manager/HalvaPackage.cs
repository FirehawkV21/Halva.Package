using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Halva.Package.Core.Utilities;

namespace Halva.Package.Core.Manager;

/// <summary>
/// This is the base class for the Halva Package. You should use either HalvaPackage or EncryptedHalvaPackage.
/// </summary>
public sealed class HalvaPackage : IDisposable, IHalvaPackage
{
    private bool disposedValue;

    /// <summary>
    /// The location of the source files.
    /// </summary>
    public StringBuilder SourceLocation { get; set; }
    /// <summary>
    /// The location of the final acrhive.
    /// </summary>
    public StringBuilder DestinationLocation { get; set; }
    /// <summary>
    /// The list of files of the archive.
    /// </summary>
    public List<string> FileList { get; set; } = new List<string>();
    /// <summary>
    /// The memory stream that handles the archive.
    /// </summary>
    public ZipArchive ArchiveMemoryStream { get; set; }
    /// <summary>
    /// The temporary archive where changes are being worked on.
    /// </summary>
    public string WorkingArchive { get; set; }

    /// <summary>
    /// Gets the character used for path designation.
    /// </summary>
    /// <returns>Either "\\" (in Windows) or "/" (Unix systems).</returns>
    public static string GetFolderCharacter() => (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ? "\\" : "/";
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

    /// <summary>
    /// Creates an empty Encrypted Halva Package.
    /// </summary>
    public HalvaPackage(bool useMemoryStream)
    {
        if (useMemoryStream)
        {
            isMemoryStream = true;
            ZipStream = new();
            ArchiveMemoryStream = new(ZipStream, ZipArchiveMode.Create, true);
        }
        else
        {
            WorkingArchive = ReserveRandomArchive();
            ArchiveMemoryStream = ZipFile.Open(WorkingArchive, ZipArchiveMode.Create);
        }
    }

    /// <summary>
    /// Opens a Halva Package for editing and/or extracting files.
    /// </summary>
    /// <param name="source">The source archive.</param>
    public HalvaPackage(string source, bool useMemoryStream)
    {
        WorkingArchive = ReserveRandomArchive();
        SourceLocation = new StringBuilder(Path.GetDirectoryName(source));
        DestinationLocation = new StringBuilder(source);
        if (useMemoryStream)
        {
            isMemoryStream = true;
            PackageUtilities.DecompressArchive(File.OpenRead(source), out ZipStream);
            ArchiveMemoryStream = new(ZipStream, ZipArchiveMode.Update, true);
        }
        else
        {
            PackageUtilities.DecompressArchive(DestinationLocation.ToString(), WorkingArchive);
            ArchiveMemoryStream = ZipFile.Open(WorkingArchive, ZipArchiveMode.Update);
        }
        FileList = PullFiles(ArchiveMemoryStream);
    }


    /// <summary>
    /// Creates a Halva package using the source folder as the input and sets the destination folder for the final package. It will automatically put the files in the input folder to a temporary archive.
    /// </summary>
    /// <param name="source">The source folder.</param>
    /// <param name="destination">The location of the final archive.</param>
    public HalvaPackage(string source, string destination, bool useMemoryStream)
    {
        SourceLocation = new StringBuilder(source);
        DestinationLocation = new StringBuilder(destination);
        List<string> foundFilesList = PullFilesFromFolder(source);
        if (useMemoryStream)
        {
            isMemoryStream = true;
            ZipStream = new();
            ArchiveMemoryStream = new(ZipStream, ZipArchiveMode.Create, true);
        }
        else
        {
            WorkingArchive = ReserveRandomArchive();
            ArchiveMemoryStream = ZipFile.Open(WorkingArchive, ZipArchiveMode.Create);
        }
        foreach (string file in foundFilesList)
        {
            AddFileToList(file);
        }
    }
    /// <summary>
    /// Opens an encrypted Halva Package for editing and/or extracting files.
    /// </summary>
    /// <param name="PassKey">The password for the archive.</param>
    /// <param name="source">The source archive.</param>
    public HalvaPackage(in string PassKey, string source, bool useMemoryStream)
    {
        WorkingArchive = ReserveRandomArchive();
        SourceLocation = new StringBuilder(Path.GetDirectoryName(source));
        DestinationLocation = new StringBuilder(source);
        Password = PassKey;
        if (useMemoryStream)
        {
            isMemoryStream = true;
            EncryptedPackageUtilities.DecompressArchive(File.OpenRead(source), out ZipStream, PassKey);
            ArchiveMemoryStream = new(ZipStream, ZipArchiveMode.Update, true);
        }
        else
        {
            EncryptedPackageUtilities.DecompressArchive(DestinationLocation.ToString(), WorkingArchive, Password);
            ArchiveMemoryStream = ZipFile.Open(WorkingArchive, ZipArchiveMode.Update);
        }
        FileList = PullFiles(ArchiveMemoryStream);
    }

    public HalvaPackage(in string PassKey, in string IV, string source, bool useMemoryStream)
    {
        WorkingArchive = ReserveRandomArchive();
        SourceLocation = new StringBuilder(Path.GetDirectoryName(source));
        DestinationLocation = new StringBuilder(source);
        IVKey = IV;
        Password = PassKey;
        if (useMemoryStream)
        {
            isMemoryStream = true;
            EncryptedPackageUtilities.DecompressArchive(File.OpenRead(source), out ZipStream, PassKey, IV);
            ArchiveMemoryStream = new(ZipStream, ZipArchiveMode.Update, true);
        }
        else
        {
            EncryptedPackageUtilities.DecompressArchive(DestinationLocation.ToString(), WorkingArchive, Password, IVKey);
            ArchiveMemoryStream = ZipFile.Open(WorkingArchive, ZipArchiveMode.Update);
        }
        FileList = PullFiles(ArchiveMemoryStream);
    }

    /// <summary>
    /// Creates an encrypted Halva package using the source folder as the input and sets the destination folder for the final package. It will automatically put the files in the input folder to a temporary archive.
    /// </summary>
    /// <param name="PassKey">The password for the archive.</param>
    /// <param name="source">The source folder.</param>
    /// <param name="destination">The location of the final archive.</param>
    public HalvaPackage(in string PassKey, string source, string destination, bool useMemoryStream)
    {
        SourceLocation = new StringBuilder(source);
        DestinationLocation = new StringBuilder(destination);
        Password = PassKey;
        List<string> foundFilesList = PullFilesFromFolder(source);
        if (useMemoryStream)
        {
            isMemoryStream = true;
            ZipStream = new();
            ArchiveMemoryStream = new(ZipStream, ZipArchiveMode.Create, true);
        }
        else {
            WorkingArchive = ReserveRandomArchive();
            ArchiveMemoryStream = ZipFile.Open(WorkingArchive, ZipArchiveMode.Create); 
        }
        foreach (string file in foundFilesList)
        {
            AddFileToList(file);
        }
    }

    public HalvaPackage(in string PassKey, in string IV, string source, string destination, bool useMemoryStream)
    {
        SourceLocation = new StringBuilder(source);
        DestinationLocation = new StringBuilder(destination);
        Password = PassKey;
        IVKey = IV;
        List<string> foundFilesList = PullFilesFromFolder(source);
        if (useMemoryStream)
        {
            isMemoryStream = true;
            ZipStream = new();
            ArchiveMemoryStream = new(ZipStream, ZipArchiveMode.Create, true);
        }
        else
        {
            WorkingArchive = ReserveRandomArchive();
            ArchiveMemoryStream = ZipFile.Open(WorkingArchive, ZipArchiveMode.Create);
        }
        foreach (string file in foundFilesList)
        {
            AddFileToList(file);
        }
    }

    /// <summary>
    /// Retrieves the name of the temporary archive.
    /// </summary>
    /// <returns>The name of the temporary archive.</returns>
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
    /// Adds a specified file to the list. This method only works if the file is in the source folder.
    /// </summary>
    /// <param name="fileLocation">The location of the file.</param>
    public void AddFileToList(string fileLocation)
    {
        FileList.Add(fileLocation.Replace(SourceLocation + GetFolderCharacter(), ""));
        ArchiveMemoryStream.CreateEntryFromFile(fileLocation, fileLocation.Replace(SourceLocation + GetFolderCharacter(), ""), CompressionLevel.NoCompression);
    }

    /// <summary>
    /// Adds a specified file to the list. This method is better suited for preserving the folder structure.
    /// </summary>
    /// <param name="source">The base folder that holds the file.</param>
    /// <param name="fileRelativeLocation">The relative location of the file.</param>
    public void AddFileToList(string source, string fileRelativeLocation)
    {
        FileList.Add(fileRelativeLocation);
        ArchiveMemoryStream.CreateEntryFromFile(Path.Combine(source, fileRelativeLocation), fileRelativeLocation, CompressionLevel.NoCompression);
    }

    /// <summary>
    /// Adds files from a specific folder.
    /// </summary>
    /// <param name="source">The location of the source folder.</param>
    public void AddFilesFromAFolder(string source)
    {
        List<string> tempList = PullFilesFromFolder(source);

        foreach (string fileEntry in tempList)
        {
            ArchiveMemoryStream.CreateEntryFromFile(fileEntry,
                fileEntry.Replace(source + GetFolderCharacter(), ""), CompressionLevel.NoCompression);
            FileList.Add(fileEntry.Replace(source + GetFolderCharacter(), ""));
        }

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
            ArchiveMemoryStream.CreateEntryFromFile(fileEntry,
                fileEntry.Replace(sourceLocation + GetFolderCharacter(), ""), CompressionLevel.NoCompression);
            FileList.Add(fileEntry.Replace(sourceLocation + GetFolderCharacter(), ""));
        }

    }

    /// <summary>
    /// Removes a specified file from the archive.
    /// </summary>
    /// <param name="fileLocation">The file you want to remove. This must be in a relative path. ("folder1/file.extension")</param>
    public void RemoveFileFromList(string fileLocation)
    {
        ZipArchiveEntry entry = ArchiveMemoryStream.GetEntry(fileLocation);
        if (entry == null) return;
        entry.Delete();
        FileList.Remove(fileLocation);
    }

    /// <summary>
    /// Finds all of the files in a folder.
    /// </summary>
    /// <param name="source">The folder to scan for files.</param>
    /// <returns>A list of files in the specified folder.</returns>
    public static List<string> PullFilesFromFolder(string source)
    {
        IEnumerable<string> foundFiles = Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories);
        return foundFiles.ToList();
    }

    /// <summary>
    /// Pulls a list of files from the archive.
    /// </summary>
    /// <param name="inputStream">The input archive.</param>
    /// <returns>A list of files in the archive.</returns>
    public static List<string> PullFiles(ZipArchive inputStream)
    {
        List<string> foundFiles = new();
        foreach (ZipArchiveEntry entry in inputStream.Entries)
        {
            foundFiles.Add(entry.FullName);
        }

        return foundFiles;
    }

    /// <summary>
    /// Extracts a specified file to a location specified.
    /// </summary>
    /// <param name="entry">The file you want to extract. This must be in a relative path. ("folder1\\file.extension")</param>
    /// <param name="exportLocation">The location where the file will be extracted to.</param>
    public void ExtractFile(string entry, string exportLocation)
    {
        ZipArchiveEntry candidateFile = ArchiveMemoryStream.GetEntry(entry);
        candidateFile.ExtractToFile(exportLocation, true);

    }

    /// <summary>
    /// Exports the files that are different between the archive and the target folder.
    /// </summary>
    /// <param name="TargetFolder">The folder where the files to update are.</param>
    public void UpdateFromArchive(string TargetFolder)
    {
        foreach (ZipArchiveEntry entry in ArchiveMemoryStream.Entries)
        {
            if (File.Exists(Path.Combine(TargetFolder, entry.FullName)))
            {
                ReadOnlySpan<byte> originalFileSignature;
                ReadOnlySpan<byte> targetFileSignature;
                using (Stream archivedFile = entry.Open())
                {
#if NET7_0_OR_GREATER
                    using (FileStream targetFile = new(Path.Combine(TargetFolder, entry.FullName), FileMode.Open, FileAccess.Read))
                    {
                        originalFileSignature = SHA256.HashData(archivedFile);
                        targetFileSignature = SHA256.HashData(targetFile);
                    }
#else
                    using (SHA256 algo = SHA256.Create())
                    using (FileStream targetFile = new(Path.Combine(TargetFolder, entry.FullName), FileMode.Open, FileAccess.Read))
                    {
                        originalFileSignature = algo.ComputeHash(archivedFile);
                        targetFileSignature = algo.ComputeHash(targetFile);
                    }
#endif
                }
                if (originalFileSignature != targetFileSignature)
                {
                    ExtractFile(entry.FullName, Path.Combine(TargetFolder, entry.FullName));
                }
            }
            else
            {
                ExtractFile(entry.FullName, Path.Combine(TargetFolder, entry.FullName));
            }
        }
    }



    /// <summary>
    /// Updates the files in the archive with the files found in specified folder.
    /// </summary>
    /// <param name="SourceFolder">The folder specified.</param>
    public void UpdateArchive(string SourceFolder)
    {
        List<string> SourceFolderFiles = PullFilesFromFolder(SourceFolder);
        foreach (string file in SourceFolderFiles)
        {
            ZipArchiveEntry entry = ArchiveMemoryStream.GetEntry(file.Replace(SourceFolder + GetFolderCharacter(), ""));
            if (entry != null)
            {
                ReadOnlySpan<byte> originalFileSignature;
                ReadOnlySpan<byte> targetFileSignature;
                using (Stream archivedFile = entry.Open())
                {
#if NET7_0_OR_GREATER
                    using (FileStream targetFile = new(file, FileMode.Open, FileAccess.Read))
                    {
                        originalFileSignature = SHA256.HashData(archivedFile);
                        targetFileSignature = SHA256.HashData(targetFile);
                    }
#else
                    using (SHA256 algo = SHA256.Create())
                    using (FileStream targetFile = new(file, FileMode.Open, FileAccess.Read))
                    {
                        originalFileSignature = algo.ComputeHash(archivedFile);
                        targetFileSignature = algo.ComputeHash(targetFile);
                    }
#endif
                }
                if (originalFileSignature != targetFileSignature)
                {
                    RemoveFileFromList(entry.FullName);
                    AddFileToList(SourceFolder, file.Replace(SourceFolder + GetFolderCharacter(), ""));
                }
            }
            else
            {
                AddFileToList(SourceFolder, file.Replace(SourceFolder + GetFolderCharacter(), ""));
            }
        }
    }

    /// <summary>
    /// Removes the archive object and deletes the temp archive if requested.
    /// </summary>
    /// <param name="disposing">Set to true to delete the temp archive.</param>
    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                DestinationLocation?.Clear();
                SourceLocation?.Clear();
                FileList.Clear();
                FileList = null;
                ArchiveMemoryStream.Dispose();
                ZipStream?.Close();
                if (File.Exists(WorkingArchive)) File.Delete(WorkingArchive);
            }

            disposedValue = true;
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
            ArchiveMemoryStream = new ZipArchive(ZipStream, ZipArchiveMode.Update, true);
        }
        else
        {
            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password))
                if (!string.IsNullOrEmpty(IVKey) && !string.IsNullOrWhiteSpace(IVKey)) EncryptedPackageUtilities.DecompressArchive(DestinationLocation.ToString(), WorkingArchive, Password, IVKey);
                else EncryptedPackageUtilities.DecompressArchive(DestinationLocation.ToString(), WorkingArchive, Password);
            else PackageUtilities.DecompressArchive(DestinationLocation.ToString(), WorkingArchive);
            ArchiveMemoryStream = ZipFile.Open(WorkingArchive, ZipArchiveMode.Update);
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
}
