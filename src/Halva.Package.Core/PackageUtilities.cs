using System.Collections.Concurrent;
using System.Formats.Tar;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IO;

namespace Halva.Package.Core;

/// <summary>
/// A set of utilities for simple workloads.
/// </summary>
public static class PackageUtilities
{
    private static readonly RecyclableMemoryStreamManager.Options InitialOptions = new()
    {
        BlockSize = 4 * 1024 * 1024,  // 4 MB
        LargeBufferMultiple = 32 * 1024 * 1024,  // 32 MB
        MaximumBufferSize = 32 * 1024 * 1024,  // 32 MB
        AggressiveBufferReturn = true,
        MaximumLargePoolFreeBytes = 256 * 1024 * 1024,  // 256 MB
        MaximumSmallPoolFreeBytes = 1024 * 1024,  // 1 MB

#if DEBUG
        GenerateCallStacks = true,
#endif
        ZeroOutBuffer = true,
    };
    static internal RecyclableMemoryStreamManager MemoryStreamManager { get; private set; } = new(InitialOptions);

    /// <summary>
    /// Sets the options for the MemoryStreamManager. The settings adjust how the MemoryStreamManager handles the pool used by the library. This needs to run before any operation with the library runs (including any new <see cref="Managers.PackageBuilder"/> or <see cref="Managers.PackageReader"/> variables that you'll initialize), so they can applied properly.
    /// </summary>
    /// <param name="options">The settings that you want to apply to the memory manager.</param>
    public static void SetMemoryStreamOptions(RecyclableMemoryStreamManager.Options options) => MemoryStreamManager = new(options);

    private static readonly CompressorEngine compressor = new();
    /// <summary>
    /// The location of a temporary archive.
    /// </summary>
    public static readonly string TempArchive = Path.Combine(Path.GetTempPath(), "TempArchive_");

    /// <summary>
    /// Creates a Halva package from a folder.
    /// </summary>
    /// <param name="input">The folder that will be used as source.</param>
    /// <param name="archiveLocation">The location of the package.</param>
    /// <param name="useMemoryStream">Use MemoryStream for temp storage. Suitable for smaller archives (less than 4GB).</param>
    public static void BuildArchiveFromFolder(in string input, in string archiveLocation, CompressionLevel compression = CompressionLevel.Optimal, bool useMemoryStream = false, string password = "", string ivKey = "")
    {
        if (useMemoryStream)
        {
            MemoryStream fileWrite = new();
            TarFile.CreateFromDirectory(input, fileWrite, false);
            CompressArchive(fileWrite, archiveLocation, compression, password, ivKey);
        }
        else
        {
            Random random = new();
            string archive = TempArchive + random.Next(9999) + ".tmp";
            if (File.Exists(archive)) File.Delete(archive);
            TarFile.CreateFromDirectory(input, archive, false);
            CompressArchive(archive, archiveLocation, compression, password, ivKey);
            File.Delete(archive);
        }
    }

    public static async Task BuildArchiveFromFolderAsync(string input, string archiveLocation, CompressionLevel compression = CompressionLevel.Optimal, bool useMemoryStream = false, string password = "", string ivKey = "")
    {
        if (useMemoryStream)
        {
            RecyclableMemoryStream fileWrite = MemoryStreamManager.GetStream();
            await TarFile.CreateFromDirectoryAsync(input, fileWrite, false);
            await CompressArchiveAsync(fileWrite, archiveLocation, compression, password, ivKey);
            await fileWrite.DisposeAsync();
        }
        else
        {
            string archive = ReserveRandomArchive();
            if (File.Exists(archive)) File.Delete(archive);
            await TarFile.CreateFromDirectoryAsync(input, archive, false);
            await CompressArchiveAsync(archive, archiveLocation, compression, password, ivKey);
            File.Delete(archive);
        }
    }

    /// <summary>
    /// Exports all files from a Halva package.
    /// </summary>
    /// <param name="inputArchive">The Halva package for input.</param>
    /// <param name="destination">The location for extracting the files.</param>
    public static void ExportFromArchive(in string inputArchive, in string destination, bool useMemoryStream = false, string password = "", string ivKey = "") => ExportFiles(inputArchive, destination, useMemoryStream, password, ivKey, false);

    public static async Task ExportFromArchiveAsync(string inputArchive, string destination, bool useMemoryStream = false, string password = "", string ivKey = "", bool useMultiThread = false, CancellationToken abortToken = default) => await ExportFilesAsync(inputArchive, destination, useMemoryStream, password, ivKey, useMultiThread, abortToken);

    /// <summary>
    /// Compresses the encrypted archive.
    /// </summary>
    /// <param name="inputArchive">The input archive.</param>
    /// <param name="outputArchive">The output archive.</param>
    /// <param name="password">The archive's password.</param>
    /// <param name="compression">Sets the compression level.</param>

    public static void CompressArchive(in string inputArchive, in string outputArchive, CompressionLevel compression = CompressionLevel.Optimal, in string password = "", in string IVkey = "")
    {
        if (password != "")
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                CreateKey(out AesCng cngEncryptionKit, password, IVkey);
                compressor.CompressEncryptedFile(ref cngEncryptionKit, inputArchive, outputArchive, compression);
            }
            else
            {
                CreateKey(out Aes encryptionKit, password, IVkey);
                compressor.CompressEncryptedFile(ref encryptionKit, inputArchive, outputArchive, compression);
            }
        else
            compressor.CompressFile(inputArchive, outputArchive, compression);

    }

    /// <summary>
    /// Compresses the encrypted archive.
    /// </summary>
    /// <param name="inputStream">The input archive.</param>
    /// <param name="outputArchive">The output archive.</param>
    /// <param name="password">The archive's password.</param>
    /// <param name="compression">Sets the compression level.</param>
    public static void CompressArchive(in Stream inputStream, in string outputArchive, CompressionLevel compression = CompressionLevel.Optimal, in string password = "", in string IVkey = "")
    {
        inputStream.Position = 0;
        if (password != "")
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                CreateKey(out AesCng cngEncryptionKit, password, IVkey);
                compressor.CompressEncryptedFile(ref cngEncryptionKit, inputStream, outputArchive, compression);
            }
            else
            {
                CreateKey(out Aes encryptionKit, password, IVkey);
                compressor.CompressEncryptedFile(ref encryptionKit, inputStream, outputArchive, compression);
            }
        else
            compressor.CompressFile(inputStream, outputArchive, compression);
    }

    public static void DecompressArchive(in Stream inputStream, ref RecyclableMemoryStream outputStream, in string password = "", in string IVkey = "")
    {
        if (password != "")
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                CreateKey(out AesCng cngEncryptionKit, password, IVkey);
                compressor.DecompressEncryptedFile(ref cngEncryptionKit, inputStream, outputStream);
            }
            else
            {
                CreateKey(out Aes encryptionKit, password, IVkey);
                compressor.DecompressEncryptedFile(ref encryptionKit, inputStream, outputStream);
            }
        else
            compressor.DecompressFile(inputStream, out outputStream);
    }

    public static void DecompressArchive(in string inputStream, string outputStream, in string password = "", in string IVkey = "")
    {
        if (password != "")
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                CreateKey(out AesCng cngEncryptionKit, password, IVkey);
                compressor.DecompressEncryptedFile(ref cngEncryptionKit, inputStream, outputStream);
            }
            else
            {
                CreateKey(out Aes encryptionKit, password, IVkey);
                compressor.DecompressEncryptedFile(ref encryptionKit, inputStream, outputStream);
            }
        else
            compressor.DecompressFile(inputStream, outputStream);
    }


    public static async Task CompressArchiveAsync(string inputArchive, string outputArchive, CompressionLevel compression = CompressionLevel.Optimal, string password = "", string IVkey = "", CancellationToken abortToken = default)
    {
        if (password != "")
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                CreateKey(out AesCng cngEncryptionKit, password, IVkey);
                await compressor.CompressEncryptedFileAsync(cngEncryptionKit, inputArchive, outputArchive, compression, abortToken);
            }
            else
            {
                CreateKey(out Aes encryptionKey, password, IVkey);
                await compressor.CompressEncryptedFileAsync(encryptionKey, inputArchive, outputArchive, compression, abortToken);
            }
        else
            await compressor.CompressFileAsync(inputArchive, outputArchive, compression, abortToken);

    }

    public static async Task CompressArchiveAsync(Stream inputStream, string outputArchive, CompressionLevel compression = CompressionLevel.Optimal, string password = "", string IVkey = "", CancellationToken abortToken = default)
    {
        inputStream.Position = 0;
        if (password != "")
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                CreateKey(out AesCng cngEncryptionKit, password, IVkey);
                await compressor.CompressEncryptedFileAsync(cngEncryptionKit, inputStream, outputArchive, compression, abortToken);
            }
            else
            {
                CreateKey(out Aes encryptionKit, password, IVkey);
                await compressor.CompressEncryptedFileAsync(encryptionKit, inputStream, outputArchive, compression, abortToken);
            }
        else
            await compressor.CompressFileAsync(inputStream, outputArchive, compression, abortToken);
    }




    /// <summary>
    /// Exports all files from a Halva package.
    /// </summary>
    /// <param name="inputArchive">The Halva package for input.</param>
    /// <param name="destination">The location for extracting the files.</param>
    public static void ExportFiles(in string inputArchive, string destination, bool useMemoryStream = false, string password = "", string ivKey = "", bool multiExtract= false)
    {
        if (useMemoryStream)
        {
            RecyclableMemoryStream stream;
            if (password != "")
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    stream = MemoryStreamManager.GetStream();
                    CreateKey(out AesCng encryptionKey, in password, in ivKey);
                    compressor.DecompressEncryptedFile(ref encryptionKey, File.OpenRead(inputArchive), stream);
                }
                else
                {
                    stream = MemoryStreamManager.GetStream();
                    CreateKey(out Aes encryptionKey, in password, in ivKey);
                    compressor.DecompressEncryptedFile(ref encryptionKey, File.OpenRead(inputArchive), stream);
                }
            else compressor.DecompressFile(File.OpenRead(inputArchive), out stream);
            stream.Position = 0;
            if (!Directory.Exists(destination)) Directory.CreateDirectory(destination);
            if (multiExtract)
            {
                TarReader reader = new(stream);
                ConcurrentBag<StreamObject> entries = [];
                TarEntry tempentry;
                do
                {
                    tempentry = reader.GetNextEntry(false);
                    if (tempentry != null)
                    {
                        StreamObject streamObject = new()
                        {
                            Stream = MemoryStreamManager.GetStream(),
                            Name = tempentry.Name
                        };
                        tempentry.DataStream.CopyTo(streamObject.Stream);
                        streamObject.Name = tempentry.Name;
                        streamObject.Stream.Position = 0;
                        entries.Add(streamObject);
                    }
                } while (tempentry != null);
                Parallel.ForEach(entries, async (entry, token) =>
                {

                    string path = Path.Combine(destination, NormalizePath(entry.Name));
                    string targetDirectory = path.Replace(Path.GetFileName(path), "");
                    if (!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory);
                    using (FileStream fileStream = new(path, FileMode.Create, FileAccess.Write, FileShare.None, 131072))
                    {
                        await entry.Stream.CopyToAsync(fileStream);
                    }
                    await entry.Stream.DisposeAsync();
                });
                reader.Dispose();
            }
            else
            {
                TarFile.ExtractToDirectoryAsync(stream, destination, true);
            }
            stream.Dispose();
        }
        else
        {
            string archive = ReserveRandomArchive();
            if (password != "")
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    CreateKey(out AesCng encryptionKey, in password, in ivKey);
                    compressor.DecompressEncryptedFile(ref encryptionKey, inputArchive, archive);
                }
                else
                {
                    CreateKey(out Aes encryptionKey, in password, in ivKey);
                    compressor.DecompressEncryptedFile(ref encryptionKey, inputArchive, archive);
                }
            else compressor.DecompressFile(inputArchive, archive);
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);
            using (FileStream stream = new(archive, FileMode.Open, FileAccess.Read, FileShare.Read, 131072))
            {
                if (multiExtract)
                {
                    TarReader reader = new(stream);
                    ConcurrentBag<StreamObject> entries = [];
                    TarEntry tempentry;
                    do
                    {
                        tempentry = reader.GetNextEntry(false);
                        if (tempentry != null)
                        {
                            StreamObject streamObject = new()
                            {
                                Stream = MemoryStreamManager.GetStream(),
                                Name = tempentry.Name
                            };
                            tempentry.DataStream.CopyTo(streamObject.Stream);
                            streamObject.Name = tempentry.Name;
                            streamObject.Stream.Position = 0;
                            entries.Add(streamObject);
                        }
                    } while (tempentry != null);
                    Parallel.ForEach(entries, async (entry, token) =>
                    {

                        string path = Path.Combine(destination, NormalizePath(entry.Name));
                        string targetDirectory = path.Replace(Path.GetFileName(path), "");
                        if (!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory);
                        using (FileStream fileStream = new(path, FileMode.Create, FileAccess.Write, FileShare.None, 131072))
                        {
                            await entry.Stream.CopyToAsync(fileStream);
                        }
                        await entry.Stream.DisposeAsync();
                    });
                    reader.Dispose();
                }
                else
                {
                    TarFile.ExtractToDirectoryAsync(stream, destination, true);
                }
            }
            File.Delete(archive);
        }
    }

    public static async Task ExportFilesAsync(string inputArchive, string destination, bool useMemoryStream = false, string password = "", string ivKey = "", bool multiExtract = false, CancellationToken abortToken = default)
    {
        if (useMemoryStream)
        {
            MemoryStream stream;
            if (password != "")
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    CreateKey(out AesCng encryptionKey, in password, in ivKey);
                    using (FileStream fileStream = new(inputArchive, FileMode.Open, FileAccess.Read, FileShare.None, 131072, useAsync: true))
                    { 
                    stream = await compressor.DecompressEncryptedFileAsync(encryptionKey, fileStream, abortToken);
                    }
                    encryptionKey.Dispose();
                }
                else
                {
                    CreateKey(out Aes encryptionKey, in password, in ivKey);
                    using (FileStream fileStream = new(inputArchive, FileMode.Open, FileAccess.Read, FileShare.None, 4096, useAsync: true))
                    {
                        stream = await compressor.DecompressEncryptedFileAsync(encryptionKey, fileStream, abortToken);
                    }
                    encryptionKey.Dispose();
                }
            else stream = await compressor.DecompressFileAsync(File.OpenRead(inputArchive), abortToken);
            stream.Position = 0;
            if (!Directory.Exists(destination)) Directory.CreateDirectory(destination);
            if (multiExtract)
            {
                TarReader reader = new(stream);
                ConcurrentBag<StreamObject> entries = [];
                TarEntry tempentry;
                do
                {
                    tempentry = await reader.GetNextEntryAsync(false, abortToken);
                    if (tempentry != null)
                    {
                        StreamObject streamObject = new()
                        {
                            Stream = MemoryStreamManager.GetStream(),
                            Name = tempentry.Name
                        };
                        await tempentry.DataStream.CopyToAsync(streamObject.Stream, abortToken);
                        streamObject.Name = tempentry.Name;
                        streamObject.Stream.Position = 0;
                        entries.Add(streamObject);
                    }
                } while (tempentry != null);
                await Parallel.ForEachAsync(entries, abortToken, async (entry, token) =>
                {

                    string path = Path.Combine(destination, NormalizePath(entry.Name));
                    string targetDirectory = path.Replace(Path.GetFileName(path), "");
                    if (!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory);
                    using (FileStream fileStream = new(path, FileMode.Create, FileAccess.Write, FileShare.None, 131072, useAsync: true))
                    {
                        await entry.Stream.CopyToAsync(fileStream, abortToken);
                    }
                    await entry.Stream.DisposeAsync();
                });
            }
            else
            {
                TarFile.ExtractToDirectory(stream, destination, true);
            }
            await stream.DisposeAsync();
        }
        else
        {
            string archive = ReserveRandomArchive();
            if (password != "")
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    CreateKey(out AesCng encryptionKey, in password, in ivKey);
                    await compressor.DecompressEncryptedFileAsync(encryptionKey, inputArchive, archive, abortToken);
                    encryptionKey.Dispose();
                }
                else
                {
                    CreateKey(out Aes encryptionKey, in password, in ivKey);
                    await compressor.DecompressEncryptedFileAsync(encryptionKey, inputArchive, archive, abortToken);
                    encryptionKey.Dispose();
                }
            else await compressor.DecompressFileAsync(inputArchive, archive, abortToken);
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);
            using (FileStream stream = new(archive, FileMode.Open, FileAccess.Read, FileShare.Read, 131072, true))
            {
                if (multiExtract)
                {
                    TarReader reader = new(stream);
                    ConcurrentBag<StreamObject> entries = [];
                    TarEntry tempentry;
                    do
                    {
                        tempentry = await reader.GetNextEntryAsync(false, abortToken);
                        if (tempentry != null)
                        {
                            StreamObject streamObject = new()
                            {
                                Stream = MemoryStreamManager.GetStream(),
                                Name = tempentry.Name
                            };
                            await tempentry.DataStream.CopyToAsync(streamObject.Stream, abortToken);
                            streamObject.Name = tempentry.Name;
                            streamObject.Stream.Position = 0;
                            entries.Add(streamObject);
                        }
                    } while (tempentry != null);
                    await Parallel.ForEachAsync(entries, abortToken, async (entry, token) =>
                    {

                        string path = Path.Combine(destination, NormalizePath(entry.Name));
                        string targetDirectory = path.Replace(Path.GetFileName(path), "");
                        if (!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory);
                        using (FileStream fileStream = new(path, FileMode.Create, FileAccess.Write, FileShare.None, 131072, useAsync: true))
                        {
                            await entry.Stream.CopyToAsync(fileStream, abortToken);
                        }
                        await entry.Stream.DisposeAsync();
                    });
                }
                else
                {
                    await TarFile.ExtractToDirectoryAsync(stream, destination, true, abortToken);
                }
            }
            File.Delete(archive);
        }
    }

    #region Encryption Key Handling
    /// <summary>
    /// Sets up the Aes Encryptor/Decryptor with IV used in code.
    /// </summary>
    /// <param name="encryptor">The Aes Encryptor/Decryptor to initialize.</param>
    /// <param name="password">The password of the archive.</param>
    /// <param name="ivKey">The IV for the archive.</param>
    private static void CreateKey(out Aes encryptor, in string password, in string ivKey = "")
    {
        encryptor = Aes.Create();
        encryptor.KeySize = 256;
        encryptor.Padding = PaddingMode.PKCS7;
        byte[] hashCode = SHA512.HashData(Encoding.UTF8.GetBytes(password));
        byte[] hashIV = !string.IsNullOrEmpty(ivKey) && !string.IsNullOrWhiteSpace(ivKey) ? SHA512.HashData(Encoding.UTF8.GetBytes(ivKey)) : SHA512.HashData(hashCode);
        Rfc2898DeriveBytes key = new(password, hashCode, 50000, HashAlgorithmName.SHA512);
        Rfc2898DeriveBytes vectorKey = new(ivKey, hashIV, 50000, HashAlgorithmName.SHA512);
        encryptor.Key = key.GetBytes(encryptor.KeySize / 8);
        encryptor.IV = vectorKey.GetBytes(encryptor.BlockSize / 8);
        key.Dispose();
        vectorKey.Dispose();
    }

    [SupportedOSPlatform("windows")]
    /// <summary>
    /// Sets up the Aes Encryptor/Decryptor with IV used in code (Using Windows' Cryptography Next Generation).
    /// </summary>
    /// <param name="encryptor">The Aes Encryptor/Decryptor to initialize.</param>
    /// <param name="password">The password of the archive.</param>
    /// <param name="ivKey">The IV for the archive.</param>
    private static void CreateKey(out AesCng encryptor, in string password, in string ivKey = "")
    {
        encryptor = new AesCng
        {
            KeySize = 256,
            Padding = PaddingMode.PKCS7
        };
        byte[] hashCode = SHA512.HashData(Encoding.UTF8.GetBytes(password));
        byte[] hashIV = !string.IsNullOrEmpty(ivKey) && !string.IsNullOrWhiteSpace(ivKey) ? SHA512.HashData(Encoding.UTF8.GetBytes(ivKey)) : SHA512.HashData(hashCode);
        Rfc2898DeriveBytes key = new(password, hashCode, 50000, HashAlgorithmName.SHA512);
        Rfc2898DeriveBytes vectorKey = new(ivKey, hashIV, 50000, HashAlgorithmName.SHA512);
        encryptor.Key = key.GetBytes(encryptor.KeySize / 8);
        encryptor.IV = vectorKey.GetBytes(encryptor.BlockSize / 8);
        key.Dispose();
        vectorKey.Dispose();
    }
    #endregion

    static internal string ReserveRandomArchive()
    {
        string tempString = "TempArchive_";
        Random _random = new();
        int check;
        do
            check = _random.Next(99999);
        while (File.Exists(Path.Combine(Path.GetTempPath(), tempString + check + ".tmp")));
        return Path.Combine(Path.GetTempPath(), tempString + check + ".tmp");
    }

    static internal string NormalizePath(string path)
    {
        string temp = path.TrimStart('/', '\\');
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return temp.Replace('/', Path.DirectorySeparatorChar);
        else return temp.Replace('\\', Path.DirectorySeparatorChar);
    }

    public class StreamObject
    {
        public RecyclableMemoryStream Stream { get; set; }
        public string Name { get; set; }
    }
}
