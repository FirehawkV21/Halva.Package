using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace Halva.Package.Core;

/// <summary>
/// A set of utilities for simple workloads.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "For safety reasons, using simplified using should be avoided (unless noted otherwise).")]
public static class PackageUtilities
{
    /// <summary>
    /// The location of a temporary archive.
    /// </summary>
    public static readonly string TempArchive = Path.Combine(Path.GetTempPath(), "TempArchive_");

    /// <summary>
    /// Exports all files from a Halva package.
    /// </summary>
    /// <param name="inputArchive">The Halva package for input.</param>
    /// <param name="destination">The location for extracting the files.</param>
    public static void ExportFromArchive(in string inputArchive, in string destination, bool useMemoryStream = false, string password = "", string ivKey = "")
    {
        ExportFiles(inputArchive, destination, useMemoryStream, password, ivKey);
    }

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
                CompressEncryptedFile(ref cngEncryptionKit, inputArchive, outputArchive, compression);
            }
            else
            {
                CreateKey(out Aes encryptionKit, password, IVkey);
                CompressEncryptedFile(ref encryptionKit, inputArchive, outputArchive, compression);
            }
        else
            CompressFile(inputArchive, outputArchive, compression);

    }

    /// <summary>
    /// Compresses the encrypted archive.
    /// </summary>
    /// <param name="inputStream">The input archive.</param>
    /// <param name="outputArchive">The output archive.</param>
    /// <param name="password">The archive's password.</param>
    /// <param name="compression">Sets the compression level.</param>
    public static void CompressArchive(in MemoryStream inputStream, in string outputArchive, CompressionLevel compression = CompressionLevel.Optimal, in string password = "", in string IVkey = "")
    {
        inputStream.Position = 0;
        if (password != "")
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                CreateKey(out AesCng cngEncryptionKit, password, IVkey);
                CompressEncryptedFile(ref cngEncryptionKit, inputStream, outputArchive, compression);
            }
            else
            {
                CreateKey(out Aes encryptionKit, password, IVkey);
                CompressEncryptedFile(ref encryptionKit, inputStream, outputArchive, compression);
            }
        else
            CompressFile(inputStream, outputArchive, compression);
    }

    public static void DecompressArchive(in Stream inputStream, MemoryStream outputStream, in string password = "", in string IVkey = "")
    {
        if (password != "")
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                CreateKey(out AesCng cngEncryptionKit, password, IVkey);
                DecompressEncryptedFile(ref cngEncryptionKit, inputStream, outputStream);
            }
            else
            {
                CreateKey(out Aes encryptionKit, password, IVkey);
                DecompressEncryptedFile(ref encryptionKit, inputStream, outputStream);
            }
        else
            DecompressFile(inputStream, out outputStream);
    }

    public static void DecompressArchive(in string inputStream, string outputStream, in string password = "", in string IVkey = "")
    {
        if (password != "")
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                CreateKey(out AesCng cngEncryptionKit, password, IVkey);
                DecompressEncryptedFile(ref cngEncryptionKit, inputStream, outputStream);
            }
            else
            {
                CreateKey(out Aes encryptionKit, password, IVkey);
                DecompressEncryptedFile(ref encryptionKit, inputStream, outputStream);
            }
        else
            DecompressFile(inputStream,outputStream);
    }


    public static async Task CompressArchiveAsync(string inputArchive, string outputArchive, CompressionLevel compression = CompressionLevel.Optimal, string password = "", string IVkey = "", CancellationToken abortToken = default)
    {
        if (password != "")
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                CreateKey(out AesCng cngEncryptionKit, password, IVkey);
                await CompressEncryptedFileAsync(cngEncryptionKit, inputArchive, outputArchive, compression, abortToken);
            }
            else
            {
                CreateKey(out Aes encryptionKey, password, IVkey);
                await CompressEncryptedFileAsync(encryptionKey, inputArchive, outputArchive, compression, abortToken);
            }
        else
            await CompressFileAsync(inputArchive, outputArchive, compression);

    }

    public static async Task CompressArchiveAsync(Stream inputStream, string outputArchive, CompressionLevel compression = CompressionLevel.Optimal, string password = "", string IVkey = "", CancellationToken abortToken = default)
    {
        inputStream.Position = 0;
        if (password != "")
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                CreateKey(out AesCng cngEncryptionKit, password, IVkey);
                await CompressEncryptedFileAsync(cngEncryptionKit, inputStream, outputArchive, compression, abortToken);
            }
            else
            {
                CreateKey(out Aes encryptionKit, password, IVkey);
                await CompressEncryptedFileAsync(encryptionKit, inputStream, outputArchive, compression, abortToken);
            }
        else
            await CompressFileAsync(inputStream, outputArchive, compression, abortToken);
    }

    public static async Task DecompressArchiveAsync(Stream inputStream, MemoryStream outputStream, CompressionLevel compression = CompressionLevel.Optimal, string password = "", string IVkey = "", CancellationToken abortToken = default)
    {
        if (password != "")
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                CreateKey(out AesCng cngEncryptionKit, password, IVkey);
                await DecompressEncryptedFileAsync(cngEncryptionKit, inputStream, outputStream);
            }
            else
            {
                CreateKey(out Aes encryptionKit, password, IVkey);
                await DecompressEncryptedFileAsync(encryptionKit, inputStream, outputStream);
            }
        else
            await DecompressFileAsync(inputStream, outputStream);
    }

    #region Unencrypted Compression Methods
    /// <summary>
    /// Compresses the archive, with a selected compression level.
    /// </summary>
    /// <param name="inputArchive"></param>
    /// <param name="outputArchive"></param>
    /// <param name="Compression"></param>
    private static void CompressFile(in MemoryStream inputArchive, in string outputArchive, CompressionLevel Compression = CompressionLevel.Optimal)
    {
        inputArchive.Position = 0;
        using (FileStream outputStream = File.Create(outputArchive))
        using (BrotliStream compressorStream = new(outputStream, Compression))
            inputArchive.CopyTo(compressorStream);
    }

    /// <summary>
    /// Compresses the archive, with a selected compression level.
    /// </summary>
    /// <param name="inputArchive"></param>
    /// <param name="outputArchive"></param>
    /// <param name="Compression"></param>
    public static void CompressArchive(in string inputArchive, in string outputArchive, CompressionLevel Compression)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(outputArchive))
        using (BrotliStream compressorStream = new(outputStream, Compression))
        {
            inputStream.CopyTo(compressorStream);
        }
    }

    /// <summary>
    /// Decompresses the archive.
    /// </summary>
    /// <param name="inputStream">The input archive in a stream.</param>
    /// <param name="uncompressedStream">The stream that will accept the uncompressed Stream.</param>
    private static void DecompressFile(in Stream inputStream, out MemoryStream uncompressedStream)
    {
        inputStream.Position = 0;
        uncompressedStream = new MemoryStream();
        using (BrotliStream decompressorStream = new(inputStream, CompressionMode.Decompress))
            decompressorStream.CopyTo(uncompressedStream);
    }

    /// <summary>
    /// Decompresses the archive.
    /// </summary>
    /// <param name="inputArchive">The input archive.</param>
    /// <param name="workerArchive">The location for the temp file (that will hold the decompressed archive).</param>
    public static void DecompressFile(in string inputArchive, in string workerArchive)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(workerArchive))
        using (BrotliStream decompressorStream = new(inputStream, CompressionMode.Decompress))
        {
            decompressorStream.CopyTo(outputStream);
        }
    }

    /// <summary>
    /// Compresses the archive, with a selected compression level.
    /// </summary>
    /// <param name="inputArchive"></param>
    /// <param name="outputArchive"></param>
    /// <param name="Compression"></param>
    private static async Task CompressFileAsync(Stream inputArchive, string outputArchive, CompressionLevel Compression = CompressionLevel.Optimal, CancellationToken abortToken = default)
    {
        inputArchive.Position = 0;
        using (FileStream outputStream = File.Create(outputArchive))
        using (BrotliStream compressorStream = new(outputStream, Compression))
            await inputArchive.CopyToAsync(compressorStream, abortToken);
    }

    /// <summary>
    /// Compresses the archive, with a selected compression level.
    /// </summary>
    /// <param name="inputArchive"></param>
    /// <param name="outputArchive"></param>
    /// <param name="Compression"></param>
    public static void CompressFilesAsync(in string inputArchive, in string outputArchive, CompressionLevel Compression)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(outputArchive))
        using (BrotliStream compressorStream = new(outputStream, Compression))
        {
            inputStream.CopyToAsync(compressorStream);
        }
    }

    /// <summary>
    /// Decompresses the archive.
    /// </summary>
    /// <param name="inputStream">The input archive in a stream.</param>
    /// <param name="uncompressedStream">The stream that will accept the uncompressed Stream.</param>
    private static async Task DecompressFileAsync(Stream inputStream, MemoryStream uncompressedStream, CancellationToken abortToken = default)
    {
        inputStream.Position = 0;
        uncompressedStream = new MemoryStream();
        using (BrotliStream decompressorStream = new(inputStream, CompressionMode.Decompress))
            await decompressorStream.CopyToAsync(uncompressedStream, abortToken);
    }

    /// <summary>
    /// Decompresses the archive.
    /// </summary>
    /// <param name="inputArchive">The input archive.</param>
    /// <param name="workerArchive">The location for the temp file (that will hold the decompressed archive).</param>
    public static async Task DecompressFileAsync(string inputArchive, string workerArchive, CancellationToken abortToken = default)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(workerArchive))
        using (BrotliStream decompressorStream = new(inputStream, CompressionMode.Decompress))
        {
            await decompressorStream.CopyToAsync(outputStream, abortToken);
        }
    }
    #endregion

    /// <summary>
    /// Creates a Halva package from a folder.
    /// </summary>
    /// <param name="input">The folder that will be used as source.</param>
    /// <param name="archiveLocation">The location of the package.</param>
    /// <param name="useMemoryStream">Use MemoryStream for temp storage. Suitable for smaller archives (less than 4GB).</param>
    public static void BuildArchiveFromFolder(in string input, in string archiveLocation, bool useMemoryStream = false, string password = "", string ivKey = "")
    {
        if (useMemoryStream)
        {
            MemoryStream fileWrite = new();
            TarFile.CreateFromDirectory(input, fileWrite, false);
            CompressFile(fileWrite, archiveLocation);
        }
        else
        {
            Random random = new();
            string archive = TempArchive + random.Next(9999) + ".tmp";
            if (File.Exists(archive)) File.Delete(archive);
            TarFile.CreateFromDirectory(input, archive, false);
            CompressFile(archive, archiveLocation);
            File.Delete(archive);
        }
    }

    /// <summary>
    /// Exports all files from a Halva package.
    /// </summary>
    /// <param name="inputArchive">The Halva package for input.</param>
    /// <param name="destination">The location for extracting the files.</param>
    public static void ExportFiles(in string inputArchive, in string destination, bool useMemoryStream = false, string password = "", string ivKey = "")
    {
        if (useMemoryStream)
        {
            MemoryStream stream = new();
            if (password != "")
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    CreateKey(out AesCng encryptionKey, in password, in ivKey);
                    DecompressEncryptedFile(ref encryptionKey, File.OpenRead(inputArchive), stream);
                }
                else
                {
                    CreateKey(out Aes encryptionKey, in password, in ivKey);
                    DecompressEncryptedFile(ref encryptionKey, File.OpenRead(inputArchive), stream);
                }
            else DecompressFile(File.OpenRead(inputArchive), out stream);
            if (!Directory.Exists(destination)) Directory.CreateDirectory(destination);
            TarFile.ExtractToDirectory(stream, destination, true);
            stream.Close();
        }
        else
        {
            string archive = ReserveRandomArchive();
            if (password != "")
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    CreateKey(out AesCng encryptionKey, in password, in ivKey);
                    DecompressEncryptedFile(ref encryptionKey, inputArchive, archive);
                }
                else
                {
                    CreateKey(out Aes encryptionKey, in password, in ivKey);
                    DecompressEncryptedFile(ref encryptionKey, inputArchive, archive);
                }
            else DecompressFile(inputArchive, archive);
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);
            TarFile.ExtractToDirectory(archive, destination, true);
            File.Delete(archive);
        }
    }

    /// <summary>
    /// Compresses the archive.
    /// </summary>
    /// <param name="inputArchive"></param>
    /// <param name="outputArchive"></param>
    /// <param name="Compression"></param>
    public static void CompressFile(in string inputArchive, in string outputArchive, CompressionLevel Compression = CompressionLevel.Optimal)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(outputArchive))
        using (BrotliStream compressorStream = new(outputStream, Compression))
            inputStream.CopyTo(compressorStream);
    }

    #region Async Unencrypted Methods
    /// <summary>
    /// Compresses the archive.
    /// </summary>
    /// <param name="inputArchive"></param>
    /// <param name="outputArchive"></param>
    /// <param name="Compression"></param>
    public static async Task CompressFileAsync(string inputArchive, string outputArchive, CompressionLevel Compression = CompressionLevel.Optimal)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(outputArchive))
        using (BrotliStream compressorStream = new(outputStream, Compression))
            await inputStream.CopyToAsync(compressorStream);
    }
    #endregion

    #region Encryption Key Handling
    /// <summary>
    /// Sets up the Aes Encryptor/Decryptor with IV used in code.
    /// </summary>
    /// <param name="encryptor">The Aes Encryptor/Decryptor to initialize.</param>
    /// <param name="password">The password of the archive.</param>
    /// <param name="ivKey">The IV for the archive.</param>
    public static void CreateKey(out Aes encryptor, in string password, in string ivKey = "")
    {
        encryptor = Aes.Create();
        encryptor.KeySize = 256;
        encryptor.Padding = PaddingMode.PKCS7;
        byte[] hashCode = SHA512.HashData(Encoding.UTF8.GetBytes(password));
        byte[] hashIV = !string.IsNullOrEmpty(ivKey) && !string.IsNullOrWhiteSpace(ivKey) ? SHA512.HashData(Encoding.UTF8.GetBytes(ivKey)) : hashCode;
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
    public static void CreateKey(out AesCng encryptor, in string password, in string ivKey = "")
    {
        encryptor = new AesCng
        {
            KeySize = 256,
            Padding = PaddingMode.PKCS7
        };
        byte[] hashCode = SHA512.HashData(Encoding.UTF8.GetBytes(password));
        byte[] hashIV = !string.IsNullOrEmpty(ivKey) && !string.IsNullOrWhiteSpace(ivKey) ? SHA512.HashData(Encoding.UTF8.GetBytes(ivKey)) : hashCode;
        Rfc2898DeriveBytes key = new(password, hashCode, 50000, HashAlgorithmName.SHA512);
        Rfc2898DeriveBytes vectorKey = new(ivKey, hashIV, 50000, HashAlgorithmName.SHA512);
        encryptor.Key = key.GetBytes(encryptor.KeySize / 8);
        encryptor.IV = vectorKey.GetBytes(encryptor.BlockSize / 8);
        key.Dispose();
        vectorKey.Dispose();
    }
    #endregion

    #region Main Encrypted Package Compression Code
    private static void CompressEncryptedFile(ref Aes encryptionKey, string inputArchive, string outputArchive, CompressionLevel compression)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, encryptionKey.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
            inputStream.CopyTo(compressorStream);
        encryptionKey.Dispose();
    }

    private static void CompressEncryptedFile(ref Aes encryptionKey, in Stream inputArchive, in string outputArchive, in CompressionLevel compression = CompressionLevel.Optimal)
    {
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, encryptionKey.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
            inputArchive.CopyTo(compressorStream);
        encryptionKey.Dispose();
    }

    [SupportedOSPlatform("windows")]
    private static void CompressEncryptedFile(ref AesCng encryptionKey, string inputArchive, string outputArchive, CompressionLevel compression = CompressionLevel.Optimal)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, encryptionKey.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
            inputStream.CopyTo(compressorStream);
        encryptionKey.Dispose();
    }

    [SupportedOSPlatform("windows")]
    private static void CompressEncryptedFile(ref AesCng encryptionKey, in Stream inputStream, in string outputArchive, in CompressionLevel compression = CompressionLevel.Optimal)
    {
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, encryptionKey.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
            inputStream.CopyTo(compressorStream);
        encryptionKey.Dispose();
    }
    #endregion

    #region Main Encrypted Package Compression Code (Async)
    private static async Task CompressEncryptedFileAsync(Aes aesEncryptionKey, string inputArchive, string outputArchive, CompressionLevel compression, CancellationToken abortToken)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, aesEncryptionKey.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
            await inputStream.CopyToAsync(compressorStream, abortToken);
    }

    [SupportedOSPlatform("windows")]
    private static async Task CompressEncryptedFileAsync(AesCng cngEncryptionKey, string inputArchive, string outputArchive, CompressionLevel compression = CompressionLevel.Optimal, CancellationToken abortToken = default)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, cngEncryptionKey.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
            await inputStream.CopyToAsync(compressorStream, abortToken);
    }

    private static async Task CompressEncryptedFileAsync(Aes encryptionKey, Stream inputArchive, string outputArchive, CompressionLevel compression = CompressionLevel.Optimal, CancellationToken abortToken = default)
    {
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, encryptionKey.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
            await inputArchive.CopyToAsync(compressorStream, abortToken);
    }

    [SupportedOSPlatform("windows")]
    private static async Task CompressEncryptedFileAsync(AesCng cngEncryptionKey, Stream inputStream, string outputArchive, CompressionLevel compression = CompressionLevel.Optimal, CancellationToken abortToken = default)
    {
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, cngEncryptionKey.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
            await inputStream.CopyToAsync(compressorStream, abortToken);
    }
    #endregion

    #region Main Encrypted Package Decompression Code

    private static void DecompressEncryptedFile(ref Aes encryptionKey, in string inputArchive, in string workerArchive)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(workerArchive))
        using (CryptoStream cryptStream = new(inputStream, encryptionKey.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
            decompressorStream.CopyTo(outputStream);
        encryptionKey.Dispose();
    }

    [SupportedOSPlatform("windows")]
    private static void DecompressEncryptedFile(ref AesCng encryptionKey, in string inputArchive, in string workerArchive)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(workerArchive))
        using (CryptoStream cryptStream = new(inputStream, encryptionKey.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
            decompressorStream.CopyTo(outputStream);
        encryptionKey.Dispose();
    }

    [SupportedOSPlatform("windows")]
    private static void DecompressEncryptedFile(ref AesCng encryptionKey, Stream inputStream, MemoryStream uncompressedStream)
    {
        using (CryptoStream cryptStream = new(inputStream, encryptionKey.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
            decompressorStream.CopyTo(uncompressedStream);
        encryptionKey.Dispose();
    }

    private static void DecompressEncryptedFile(ref Aes encryptionKey, Stream inputStream, in MemoryStream uncompressedStream)
    {
        using (CryptoStream cryptStream = new(inputStream, encryptionKey.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
            decompressorStream.CopyTo(uncompressedStream);
        encryptionKey.Dispose();
    }
    #endregion

    #region Main Encrypted Package Decompression Code

    private static async Task DecompressEncryptedFileAsync(Aes encryptionKey, string inputArchive, string workerArchive)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(workerArchive))
        using (CryptoStream cryptStream = new(inputStream, encryptionKey.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
            await decompressorStream.CopyToAsync(outputStream);
        encryptionKey.Dispose();
    }

    [SupportedOSPlatform("windows")]
    private static async Task DecompressEncryptedFileAsync(AesCng encryptionKey, string inputArchive, string workerArchive)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(workerArchive))
        using (CryptoStream cryptStream = new(inputStream, encryptionKey.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
            await decompressorStream.CopyToAsync(outputStream);
        encryptionKey.Dispose();
    }

    [SupportedOSPlatform("windows")]
    private static async Task DecompressEncryptedFileAsync(AesCng encryptionKey, Stream inputStream, MemoryStream uncompressedStream)
    {
        using (CryptoStream cryptStream = new(inputStream, encryptionKey.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
            await decompressorStream.CopyToAsync(uncompressedStream);
        encryptionKey.Dispose();
    }

    private static async Task DecompressEncryptedFileAsync(Aes encryptionKey, Stream inputStream, MemoryStream uncompressedStream)
    {
        using (CryptoStream cryptStream = new(inputStream, encryptionKey.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
            await decompressorStream.CopyToAsync(uncompressedStream);
        encryptionKey.Dispose();
    }
    #endregion

    public static string ReserveRandomArchive()
    {
        string tempString = "TempArchive_";
        Random _random = new();
        int check;
        do
            check = _random.Next(99999);
        while (File.Exists(Path.Combine(Path.GetTempPath(), tempString + check + ".tmp")));
        return Path.Combine(Path.GetTempPath(), tempString + check + ".tmp");
    }
}
