﻿using System.Formats.Tar;
using System.IO.Compression;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Halva.Package.Core.Managers;
public class PackageReader(string packageLocation, string password = "", string ivKey = "")
{
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
        using (FileStream fs = new(PackageLocation, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
        {
            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    PackageUtilities.CreateKey(out AesCng cngEncryptionKit, Password, IvKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                        {
                            using (TarReader tarReader = new(decompressionStream))
                            {
                                ExtractFileWorkload(fileName, destinationPath, tarReader);
                            }
                        }
                    }

                }
                else
                {
                    PackageUtilities.CreateKey(out Aes cngEncryptionKit, Password, IvKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                        {
                            using (TarReader tarReader = new(decompressionStream))
                            {
                                ExtractFileWorkload(fileName, destinationPath, tarReader);
                            }
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
                        ExtractFileWorkload(fileName, destinationPath, tarReader);
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
        if (!string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password))
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                PackageUtilities.CreateKey(out AesCng cngEncryptionKit, Password, IvKey);
                using (FileStream fs = new(PackageLocation, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                {
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                        {
                            using (TarReader tarReader = new(decompressionStream))
                            {
                                await ExtractFileWorkloadAsync(fileName, destinationPath, tarReader, abortToken);
                            }
                        }
                    }
                }

            }
            else
            {
                PackageUtilities.CreateKey(out Aes cngEncryptionKit, Password, IvKey);
                using (FileStream fs = new(PackageLocation, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                {
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                        {
                            using (TarReader tarReader = new(decompressionStream))
                            {
                                await ExtractFileWorkloadAsync(fileName, destinationPath, tarReader, abortToken);
                            }
                        }
                    }
                }

            }
        else
            using (FileStream fs = new(PackageLocation, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                using (BrotliStream decompressionStream = new(fs, CompressionMode.Decompress))
                {
                    using (TarReader tarReader = new(decompressionStream))
                    {
                        await ExtractFileWorkloadAsync(fileName, destinationPath, tarReader, abortToken);
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
        using (FileStream fs = new(PackageLocation, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan))
            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password))
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    PackageUtilities.CreateKey(out AesCng cngEncryptionKit, Password, IvKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                        {
                            using (TarReader tarReader = new(decompressionStream))
                            {
                                UpdateWorkload(in tarReader, in TargetFolder);
                            }
                        }
                    }
                        
                }
                else
                {
                    PackageUtilities.CreateKey(out Aes cngEncryptionKit, Password, IvKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                        {
                            using (TarReader tarReader = new(decompressionStream))
                            {
                                UpdateWorkload(in tarReader, in TargetFolder);
                            }
                        }
                    }
                }
            else
                using (BrotliStream decompressionStream = new(fs, CompressionMode.Decompress))
                using (TarReader tarReader = new(decompressionStream))
                    UpdateWorkload(in tarReader, in TargetFolder);
    }

    private void UpdateWorkload(in TarReader tarReader, in string targetFolder)
    {
        TarEntry tempEntry;
        do
        {
            tempEntry = tarReader.GetNextEntry(true);
            if (tempEntry == null) continue;
            string normalizedPath = PackageUtilities.NormalizePath(tempEntry.Name)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string targetName = Path.Combine(targetFolder, normalizedPath);
            if (File.Exists(targetName))
            {
                ReadOnlySpan<byte> archiveHashSpan;
                ReadOnlySpan<byte> targetHashSpan;
                using (Stream archivedFile = tempEntry.DataStream)
                using (FileStream targetFile = new(targetName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan))
                {
                    XxHash128 archiveHash = new();
                    XxHash128 targetHash = new();

                    archiveHash.Append(archivedFile);
                    targetHash.Append(targetFile);

                    archiveHashSpan = archiveHash.GetCurrentHash();
                    targetHashSpan = targetHash.GetCurrentHash();
                }
                if (!archiveHashSpan.SequenceEqual(targetHashSpan))
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
        using (FileStream fs = new(PackageLocation, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan))
            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password))
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    PackageUtilities.CreateKey(out AesCng cngEncryptionKit, Password, IvKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                        {
                            using (TarReader tarReader = new(decompressionStream))
                            {
                                FastUpdateWorkload(in tarReader, in TargetFolder);
                            }
                        }
                    }

                }
                else
                {
                    PackageUtilities.CreateKey(out Aes cngEncryptionKit, Password, IvKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                        {
                            using (TarReader tarReader = new(decompressionStream))
                            {
                                FastUpdateWorkload(in tarReader, in TargetFolder);
                            }
                        }
                    }
                }
            else
                using (BrotliStream decompressionStream = new(fs, CompressionMode.Decompress))
                {
                    using (TarReader tarReader = new(decompressionStream))
                    {
                        FastUpdateWorkload(in tarReader, in TargetFolder);
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
            string normalizedPath = PackageUtilities.NormalizePath(tempEntry.Name).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string targetName = Path.Combine(TargetFolder, normalizedPath);
            if (File.Exists(targetName))
            {
                FileInfo targetFile = new(targetName);
                if (tempEntry.DataStream.Length != targetFile.Length || tempEntry.ModificationTime != targetFile.LastWriteTimeUtc)
                {
                    tempEntry.ExtractToFile(targetName, true);
                }
            }
            else
            {
                string dirPath = Path.GetDirectoryName(targetName);
                if (dirPath != null && !Directory.Exists(dirPath))
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
        using (FileStream fs = new(PackageLocation, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password))
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    PackageUtilities.CreateKey(out AesCng cngEncryptionKit, Password, IvKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                        {
                            using (TarReader tarReader = new(decompressionStream))
                            {
                                await UpdateWorkloadAsync(tarReader, TargetFolder, abortToken);
                            }
                        }
                    }

                }
                else
                {
                    PackageUtilities.CreateKey(out Aes cngEncryptionKit, Password, IvKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                        {
                            using (TarReader tarReader = new(decompressionStream))
                            {
                                await UpdateWorkloadAsync(tarReader, TargetFolder, abortToken);
                            }
                        }
                    }
                }
            else
                using (BrotliStream decompressionStream = new(fs, CompressionMode.Decompress))
                using (TarReader tarReader = new(decompressionStream))
                    await UpdateWorkloadAsync(tarReader, TargetFolder, abortToken);
    }

    private async Task UpdateWorkloadAsync(TarReader tarReader, string targetFolder, CancellationToken abortToken = default)
    {
        TarEntry tempEntry;
        do
        {
            tempEntry = await tarReader.GetNextEntryAsync(true, abortToken);
            if (tempEntry == null) continue;
            string normalizedPath = PackageUtilities.NormalizePath(tempEntry.Name)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string targetName = Path.Combine(targetFolder, normalizedPath);
            if (File.Exists(targetName))
            {
                ReadOnlySpan<byte> archiveHashSpan;
                ReadOnlySpan<byte> targetHashSpan;
                using (Stream archivedFile = tempEntry.DataStream)
                using (FileStream targetFile = new(targetName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
                {
                    XxHash128 archiveHash = new();
                    XxHash128 targetHash = new();

                    archiveHash.Append(archivedFile);
                    targetHash.Append(targetFile);

                    archiveHashSpan = archiveHash.GetCurrentHash();
                    targetHashSpan = targetHash.GetCurrentHash();
                }
                if (!archiveHashSpan.SequenceEqual(targetHashSpan))
                {
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
        using (FileStream fs = new(PackageLocation, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password))
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    PackageUtilities.CreateKey(out AesCng cngEncryptionKit, Password, IvKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                        {
                            using (TarReader tarReader = new(decompressionStream))
                            {
                                await FastUpdateWorkloadAsync(tarReader, TargetFolder, abortToken);
                            }
                        }
                    }

                }
                else
                {
                    PackageUtilities.CreateKey(out Aes cngEncryptionKit, Password, IvKey);
                    using (CryptoStream cryptoStream = new(fs, cngEncryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (BrotliStream decompressionStream = new(cryptoStream, CompressionMode.Decompress))
                        {
                            using (TarReader tarReader = new(decompressionStream))
                            {
                                await FastUpdateWorkloadAsync(tarReader, TargetFolder, abortToken);
                            }
                        }
                    }
                }
            else
                using (BrotliStream decompressionStream = new(fs, CompressionMode.Decompress))
                using (TarReader tarReader = new(decompressionStream))
                    await FastUpdateWorkloadAsync(tarReader, TargetFolder, abortToken);
    }

    private async Task FastUpdateWorkloadAsync(TarReader tarReader, string TargetFolder, CancellationToken abortToken = default)
    {
        TarEntry tempEntry;
        do
        {
            tempEntry = await tarReader.GetNextEntryAsync(true, abortToken);
            if (tempEntry != null)
            {
                string targetName = Path.Combine(!(TargetFolder[..0] != Path.DirectorySeparatorChar.ToString()) ? TargetFolder + Path.DirectorySeparatorChar : TargetFolder, PackageUtilities.NormalizePath(tempEntry.Name).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                if (tempEntry != null)
                    if (File.Exists(targetName))
                    {
                        FileInfo targetFile = new(targetName);
                        if (tempEntry.DataStream.Length != targetFile.Length || tempEntry.ModificationTime != targetFile.LastWriteTimeUtc)
                        {
                            await tempEntry.ExtractToFileAsync(targetName, true, abortToken);
                        }
                    }
                    else
                    {
                        if (!Directory.Exists(targetName.Replace(Path.GetFileName(targetName), "").TrimEnd(Path.DirectorySeparatorChar))) Directory.CreateDirectory(targetName.Replace(Path.GetFileName(targetName), "").TrimEnd(Path.DirectorySeparatorChar));
                        await tempEntry.ExtractToFileAsync(targetName, true, abortToken);
                    }
            }
        }
        while (tempEntry != null);
    }
}
