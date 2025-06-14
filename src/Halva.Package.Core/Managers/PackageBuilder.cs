﻿using System.Formats.Tar;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Halva.Package.Core.Models;

namespace Halva.Package.Core.Managers;
public class PackageBuilder(string destinationLocation, string password = "", string ivKey = "")
{
    public string Password { get; set; } = password;
    public string IvKey { get; set; } = ivKey;
    /// <summary>
    /// The list of files of the archive.
    /// </summary>
    internal List<TarFileList> FileList { get; set; } = [];
    public CompressionLevel CompressionOption { get; set; } = CompressionLevel.Optimal;
    public StringBuilder DestinationLocation { get; set; } = new(destinationLocation);

    /// <summary>
    /// Adds a file to the list of files to be archived.
    /// </summary>
    /// <param name="source">The parent folder for the file.</param>
    /// <param name="fileRelativeLocation">The relative location of the file in the parent folder.</param>
    public void AddFileToList(string source, string fileRelativeLocation) => FileList.Add(new TarFileList(Path.Combine(source.TrimEnd(Path.DirectorySeparatorChar), fileRelativeLocation.TrimStart(Path.DirectorySeparatorChar)), fileRelativeLocation));

    /// <summary>
    /// Adds files from a folder to the list of files to be archived.
    /// </summary>
    /// <param name="source">The parent folder for the file.</param>
    /// <param name="SourceFolderRelativeLocation">The relative location of the folder that has the files in the parent folder.</param>
    public void AddFilesFromAFolder(string sourceLocation, string SourceFolderRelativeLocation = "")
    {
        List<string> tempList;
        if (!string.IsNullOrEmpty(SourceFolderRelativeLocation) || !string.IsNullOrWhiteSpace(SourceFolderRelativeLocation))
            tempList = PullFilesFromFolder(Path.Combine(sourceLocation.TrimEnd(Path.DirectorySeparatorChar), SourceFolderRelativeLocation.TrimStart(Path.DirectorySeparatorChar)));
        else tempList = PullFilesFromFolder(sourceLocation);


        foreach (string fileEntry in tempList)
            FileList.Add(new TarFileList(fileEntry, fileEntry.Replace(sourceLocation, "")));

    }

    /// <summary>
    /// Pulls files from a folder recursively.
    /// </summary>
    /// <param name="source">The source folder.</param>
    /// <returns>A list of files from the folder.</returns>
    public static List<string> PullFilesFromFolder(string source)
    {
        IEnumerable<string> foundFiles = Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories);
        return [.. foundFiles];
    }

    /// <summary>
    /// Creates the archive file with the files added to the list.
    /// </summary>
    public void Commit()
    {
        using (FileStream fs = new(DestinationLocation.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan))
            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password))
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    PackageUtilities.CreateKey(out AesCng cngEncryptionKit, Password, IvKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateEncryptor(), CryptoStreamMode.Write))
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
                {
                    PackageUtilities.CreateKey(out Aes cngEncryptionKit, Password, IvKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateEncryptor(), CryptoStreamMode.Write))
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
        using (FileStream fs = new(DestinationLocation.ToString(), FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password))
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    PackageUtilities.CreateKey(out AesCng cngEncryptionKit, Password, IvKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateEncryptor(), CryptoStreamMode.Write))
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
                        
                }
                else
                {
                    PackageUtilities.CreateKey(out Aes cngEncryptionKit, Password, IvKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateEncryptor(), CryptoStreamMode.Write))
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
