using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Halva.Package.Core.Models;

namespace Halva.Package.Core.Managers;
public sealed class PackageBuilder(string destinationLocation, string password = "", string ivKey = "")
{
    private readonly int bufferSize = 81920;
    public string Password { get; set; } = password;
    public string IvKey { get; set; } = ivKey;
    /// <summary>
    /// The list of files of the archive.
    /// </summary>
    internal List<TarFileList> FileList { get; set; } = [];
    /// <summary>
    /// Sets the compression level for the archive.
    /// </summary>
    public CompressionLevel CompressionOption { get; set; } = CompressionLevel.Optimal;
    /// <summary>
    /// The destination for the archive file.
    /// </summary>
    public StringBuilder DestinationLocation { get; set; } = new(destinationLocation);

    /// <summary>
    /// Adds a file to the list of files to be archived.
    /// </summary>
    /// <param name="source">The base folder that holds the file.</param>
    /// <param name="fileRelativeLocation">The relative location of the file.</param>
    public void AddFileToList(string source, string fileRelativeLocation)
    {
        string fullPath = Path.Combine(Path.GetFullPath(source), fileRelativeLocation.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"The file '{fullPath}' does not exist.");
        }
        FileList.Add(new TarFileList(fullPath, fileRelativeLocation));
    }

    /// <summary>
    /// Adds files from a folder to the list of files to be archived.
    /// </summary>
    /// <param name="source">The parent folder for the file.</param>
    /// <param name="SourceFolderRelativeLocation">The relative location of the folder that has the files in the parent folder.</param>
    public void AddFilesFromAFolder(string sourceLocation, string SourceFolderRelativeLocation = "")
    {
        string normalizedSourceLocation = Path.GetFullPath(Path.TrimEndingDirectorySeparator(sourceLocation));
        string combinedPath = string.IsNullOrEmpty(SourceFolderRelativeLocation) ? normalizedSourceLocation : Path.Combine(normalizedSourceLocation, SourceFolderRelativeLocation.TrimStart(Path.DirectorySeparatorChar));
        if (!Directory.Exists(combinedPath))
        {
            throw new DirectoryNotFoundException($"The directory '{combinedPath}' does not exist.");
        }
        foreach (string fileEntry in Directory.EnumerateFiles(combinedPath, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(normalizedSourceLocation, fileEntry);
            FileList.Add(new TarFileList(fileEntry, relativePath));
        }
    }

    /// <summary>
    /// Creates the archive file with the files added to the list.
    /// </summary>
    public void Commit()
    {
        using (FileStream fs = new(DestinationLocation.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.SequentialScan))
            if (!string.IsNullOrWhiteSpace(Password))
            {
                using (CryptoStream cryptoStream = new(fs, PackageUtilities.GetEncryptionKey(Password, IvKey).CreateEncryptor(), CryptoStreamMode.Write))
                {
                    using (BrotliStream CompressionStream = new(cryptoStream, CompressionOption))
                    {
                        using (TarWriter _tarBuilder = new(CompressionStream, TarEntryFormat.Pax, false))
                        {
                            foreach (TarFileList file in FileList)
                                _tarBuilder.WriteEntry(file.FileLocation, file.FileEntry);
                        }
                    }
                }
            }
            else
                using (BrotliStream CompressionStream = new(fs, CompressionOption))
                {
                    using (TarWriter _tarBuilder = new(CompressionStream, TarEntryFormat.Pax, false))
                    {
                        foreach (TarFileList file in FileList)
                            _tarBuilder.WriteEntry(file.FileLocation, file.FileEntry);
                    }
                }
    }

    /// <summary>
    /// Creates the archive file with the files added to the list asynchronously.
    /// </summary>
    /// <param name="abortToken">The cancellation token to abort the task.</param>
    /// <returns>A task that handles the creation of the archive.</returns>
    public async Task CommitAsync(CancellationToken abortToken = default)
    {
        using (FileStream fs = new(DestinationLocation.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
            if (!string.IsNullOrWhiteSpace(Password))
                    using (CryptoStream cryptoStream = new(fs, PackageUtilities.GetEncryptionKey(Password, IvKey).CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (BrotliStream CompressionStream = new(cryptoStream, CompressionOption))
                        {
                            using (TarWriter _tarBuilder = new(CompressionStream, TarEntryFormat.Pax, false))
                            {
                                foreach (TarFileList file in FileList)
                                    await _tarBuilder.WriteEntryAsync(file.FileLocation, file.FileEntry, abortToken);
                            }
                        }
                    }
            else
                using (BrotliStream CompressionStream = new(fs, CompressionOption))
                {
                    using (TarWriter _tarBuilder = new(CompressionStream, TarEntryFormat.Pax, false))
                    {
                        foreach (TarFileList file in FileList)
                            await _tarBuilder.WriteEntryAsync(file.FileLocation, file.FileEntry, abortToken);
                    }
                }
    }
}
