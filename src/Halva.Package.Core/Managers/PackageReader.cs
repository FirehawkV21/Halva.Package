﻿using System.Formats.Tar;
using System.IO.Compression;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Halva.Package.Core.Managers;
public class PackageReader(string packageLocation, string password = "", string ivKey = "")
{
    private const int DefaultBufferSize = 81920;
    public string Password { get; set; } = password;
    public string IvKey { get; set; } = ivKey;
    public string PackageLocation { get; set; } = packageLocation;

    /// <summary>
    /// Exrtacts a file from the package to the destination path.
    /// </summary>
    /// <param name="fileName">The file you want to extract.</param>
    /// <param name="destinationPath">The destination of the file.</param>
    public void ExtractFile(string fileName, string destinationPath)
    {
        string normalizedDestinationPath = Path.GetFullPath(destinationPath);
        using (FileStream fs = new(PackageLocation, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, FileOptions.SequentialScan))
        {
            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password))
            {
                using (CryptoStream cryptoStream = new(fs, PackageUtilities.GetEncryptionKey(Password, IvKey).CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                    {
                        using (TarReader tarReader = new(decompressionStream))
                        {
                            ExtractFileWorkload(fileName, normalizedDestinationPath, tarReader);
                        }
                    }
                }
            }
            else
            {
                using (BrotliStream decompressionStream = new(fs, CompressionMode.Decompress))
                {
                    using (TarReader tarReader = new(decompressionStream))
                    {
                        ExtractFileWorkload(fileName, normalizedDestinationPath, tarReader);
                    }
                }
            }
        }
    }

    private void ExtractFileWorkload(in string fileName, in string destinationPath, in TarReader tarReader)
    {
        TarEntry entry = tarReader.GetNextEntry();
        while (entry != null)
        {
            if (entry.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase))
            {
                string dir = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                entry.ExtractToFile(destinationPath, true);
                break;
            }
            entry = tarReader.GetNextEntry();
        }
    }

    /// <summary>
    /// Exrtacts a file from the package to the destination path asynchronously.
    /// </summary>
    /// <param name="fileName">The file you want to extract.</param>
    /// <param name="destinationPath">The destination of the file.</param>
    /// <param name="abortToken">The cancellation token to abort the operation.</param>
    public async Task ExtractFileAsync(string fileName, string destinationPath, CancellationToken abortToken = default)
    {
        string normalizedDestinationPath = Path.GetFullPath(destinationPath);
        using (FileStream fs = new(PackageLocation, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, FileOptions.SequentialScan | FileOptions.Asynchronous))
        {
            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password))
                using (CryptoStream cryptoStream = new(fs, PackageUtilities.GetEncryptionKey(Password, IvKey).CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                    {
                        using (TarReader tarReader = new(decompressionStream))
                        {
                            await ExtractFileWorkloadAsync(fileName, normalizedDestinationPath, tarReader, abortToken);
                        }
                    }
                }
            else
                using (BrotliStream decompressionStream = new(fs, CompressionMode.Decompress))
                {
                    using (TarReader tarReader = new(decompressionStream))
                    {
                        await ExtractFileWorkloadAsync(fileName, normalizedDestinationPath, tarReader, abortToken);
                    }
                }
        }

    }

    private async Task ExtractFileWorkloadAsync(string fileName, string destinationPath, TarReader tarReader, CancellationToken abortToken)
    {
        TarEntry entry = await tarReader.GetNextEntryAsync(cancellationToken: abortToken);
        while (entry != null)
        {
            if (entry.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase))
            {
                string dir = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                await entry.ExtractToFileAsync(destinationPath, true, abortToken);
                break;
            }
            entry = await tarReader.GetNextEntryAsync(cancellationToken: abortToken);
        }
    }

    /// <summary>
    /// Updates the files in the target folder from the archive.
    /// </summary>
    /// <param name="TargetFolder">The folder that has the files that you want to update.</param>
    public void UpdateFromArchive(string TargetFolder)
    {
        string normalizedTargetFolder = Path.GetFullPath(TargetFolder);
        using (FileStream fs = new(PackageLocation, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, FileOptions.SequentialScan))
        {
            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password))
                using (CryptoStream cryptoStream = new(fs, PackageUtilities.GetEncryptionKey(Password, IvKey).CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                    {
                        using (TarReader tarReader = new(decompressionStream))
                        {
                            UpdateWorkload(in tarReader, in normalizedTargetFolder);
                        }
                    }
                }
            else
                using (BrotliStream decompressionStream = new(fs, CompressionMode.Decompress))
                {
                    using (TarReader tarReader = new(decompressionStream))
                    {
                        UpdateWorkload(in tarReader, in normalizedTargetFolder);
                    }
                }
        }

    }

    private void UpdateWorkload(in TarReader tarReader, in string targetFolder)
    {
        TarEntry tempEntry;
        do
        {
            tempEntry = tarReader.GetNextEntry(true);
            if (tempEntry == null || tempEntry.DataStream == null) continue;
            string normalizedPath = NormalizePath(tempEntry.Name).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string targetName = Path.Combine(targetFolder, normalizedPath);
            if (File.Exists(targetName))
            {
                bool areEqual = false;
                using (FileStream targetFile = new(targetName, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, FileOptions.SequentialScan))
                {
                    XxHash128 archiveHash = new();
                    XxHash128 targetHash = new();
                    archiveHash.Append(tempEntry.DataStream);
                    targetHash.Append(targetFile);
                    areEqual = archiveHash.GetCurrentHash().SequenceEqual(targetHash.GetCurrentHash());
                }

                if (!areEqual)
                {
                    tempEntry.DataStream.Position = 0;
                    tempEntry.ExtractToFile(targetName, true);
                }
            }
            else
            {
                string dir = Path.GetDirectoryName(targetName);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                tempEntry.ExtractToFile(targetName, true);
            }

        } while (tempEntry != null);
    }

    /// <summary>
    /// Updates the files in the target folder from the archive.
    /// </summary>
    /// <param name="TargetFolder">The folder that has the files that you want to update by using file metadata. </param>
    public void FastUpdateFromArchive(string TargetFolder)
    {
        string normalizedTargetFolder = Path.GetFullPath(TargetFolder);
        using (FileStream fs = new(PackageLocation, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, FileOptions.SequentialScan))
        {
            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password))
            {
                using (CryptoStream cryptoStream = new(fs, PackageUtilities.GetEncryptionKey(Password, IvKey).CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                    {
                        using (TarReader tarReader = new(decompressionStream))
                        {
                            FastUpdateWorkload(in tarReader, in normalizedTargetFolder);
                        }
                    }
                }
            }
            else
                using (BrotliStream decompressionStream = new(fs, CompressionMode.Decompress))
                {
                    using (TarReader tarReader = new(decompressionStream))
                    {
                        FastUpdateWorkload(in tarReader, in normalizedTargetFolder);
                    }
                }
        }
    }

    private void FastUpdateWorkload(in TarReader tarReader, in string TargetFolder)
    {
        TarEntry tempEntry;
        do
        {
            tempEntry = tarReader.GetNextEntry(false);
            if (tempEntry == null) continue;
            string normalizedPath = NormalizePath(tempEntry.Name).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string targetName = Path.Combine(TargetFolder, normalizedPath);
            if (File.Exists(targetName))
            {
                FileInfo targetFile = new(targetName);
                if (tempEntry.Length != targetFile.Length || tempEntry.ModificationTime > targetFile.LastWriteTimeUtc)
                {
                    tempEntry.ExtractToFile(targetName, true);
                }
            }
            else
            {
                string dirPath = Path.GetDirectoryName(targetName);
                if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                tempEntry.ExtractToFile(targetName, true);
            }
        }
        while (tempEntry != null);
    }

    /// <summary>
    /// Updates the files in the target folder from the archive asynchronously.
    /// </summary>
    /// <param name="TargetFolder">The folder that has the files that you want to update.</param>
    /// <param name="abortToken">The cancellation token to abort the operation.</param>
    public async Task UpdateFromArchiveAsync(string TargetFolder, CancellationToken abortToken = default)
    {
        string normalizedTargetFolder = Path.GetFullPath(TargetFolder);
        using (FileStream fs = new(PackageLocation, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, FileOptions.SequentialScan | FileOptions.Asynchronous))
            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password))
            {
                using (CryptoStream cryptoStream = new(fs, PackageUtilities.GetEncryptionKey(Password, IvKey).CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                    {
                        using (TarReader tarReader = new(decompressionStream))
                        {
                            await UpdateWorkloadAsync(tarReader, normalizedTargetFolder, abortToken);
                        }
                    }
                }

            }
            else
            {
                using (BrotliStream decompressionStream = new(fs, CompressionMode.Decompress))
                {
                    using (TarReader tarReader = new(decompressionStream))
                    {
                        await UpdateWorkloadAsync(tarReader, normalizedTargetFolder, abortToken);
                    }
                }
            }
    }

    private async Task UpdateWorkloadAsync(TarReader tarReader, string targetFolder, CancellationToken abortToken = default)
    {
        TarEntry tempEntry;
        do
        {
            tempEntry = await tarReader.GetNextEntryAsync(true, abortToken);
            if (tempEntry == null || tempEntry.DataStream == null) continue;
            string normalizedPath = NormalizePath(tempEntry.Name).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string targetName = Path.Combine(targetFolder, normalizedPath);
            if (File.Exists(targetName))
            {
                bool areEqual = false;
                using (FileStream targetFile = new(targetName, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
                {
                    XxHash128 archiveHash = new();
                    XxHash128 targetHash = new();
                    archiveHash.Append(tempEntry.DataStream);
                    targetHash.Append(targetFile);
                    areEqual = archiveHash.GetCurrentHash().SequenceEqual(targetHash.GetCurrentHash());
                }
                if (!areEqual)
                {
                    tempEntry.DataStream.Position = 0;
                    await tempEntry.ExtractToFileAsync(targetName, true, abortToken);
                }
            }
            else
            {
                string dir = Path.GetDirectoryName(targetName);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                await tempEntry.ExtractToFileAsync(targetName, true, abortToken);
            }

        } while (tempEntry != null);
    }

    /// <summary>
    /// Updates the files in the target folder from the archive asynchronously by checking the metadata. This is suitable for cases where accuracy isn't required.
    /// </summary>
    /// <param name="TargetFolder">The folder that has the files that you want to update.</param>
    /// <param name="abortToken">The cancellation token to abort the operation.</param>
    public async Task FastUpdateFromArchiveAsync(string TargetFolder, CancellationToken abortToken = default)
    {
        string normalizedTargetFolder = Path.GetFullPath(TargetFolder);
        using (FileStream fs = new(PackageLocation, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, FileOptions.SequentialScan | FileOptions.Asynchronous))
            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password))
                using (CryptoStream cryptoStream = new(fs, PackageUtilities.GetEncryptionKey(Password, IvKey).CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                    {
                        using (TarReader tarReader = new(decompressionStream))
                        {
                            await FastUpdateWorkloadAsync(tarReader, normalizedTargetFolder, abortToken);
                        }
                    }
                }
            else
                using (BrotliStream decompressionStream = new(fs, CompressionMode.Decompress))
                {
                    using (TarReader tarReader = new(decompressionStream))
                    {
                        await FastUpdateWorkloadAsync(tarReader, normalizedTargetFolder, abortToken);
                    }
                }
    }

    private async Task FastUpdateWorkloadAsync(TarReader tarReader, string TargetFolder, CancellationToken abortToken = default)
    {
        TarEntry tempEntry;
        do
        {
            tempEntry = await tarReader.GetNextEntryAsync(false, abortToken);
            if (tempEntry == null) continue;

            string normalizedPath = NormalizePath(tempEntry.Name).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string targetName = Path.Combine(TargetFolder, normalizedPath);
            if (File.Exists(targetName))
            {
                FileInfo targetFile = new(targetName);
                if (tempEntry.Length != targetFile.Length || tempEntry.ModificationTime > targetFile.LastWriteTimeUtc)
                {
                    await tempEntry.ExtractToFileAsync(targetName, true, abortToken);
                }
            }
            else
            {
                string dirPath = Path.GetDirectoryName(targetName);
                if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                await tempEntry.ExtractToFileAsync(targetName, true, abortToken);
            }

        }
        while (tempEntry != null);
    }

    /// <summary>
    /// Normalizes a file path to use the correct directory separator character for the current platform.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>The given path, normalized to the OS'.</returns>
    private static string NormalizePath(string path) => path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);

}