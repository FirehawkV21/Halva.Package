using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Halva.Package.Core.Utilities;

/// <summary>
/// A set of utilities for simple workloads (when working with encrypted archives).
/// </summary>
public static class EncryptedPackageUtilities
{
    /// <summary>
    /// The location of the temporary archive.
    /// </summary>
    public static readonly string TempArchive = Path.Combine(Path.GetTempPath(), "TempArchive_");

    /// <summary>
    /// Compresses the encrypted archive.
    /// </summary>
    /// <param name="inputArchive">The input archive.</param>
    /// <param name="outputArchive">The output archive.</param>
    /// <param name="password">The archive's password.</param>
    public static void CompressArchive(in string inputArchive, in string outputArchive, in string password) => CompressArchive(inputArchive, outputArchive, password, CompressionLevel.Optimal);

    public static void CompressArchive(in string inputArchive, in string outputArchive, in string password, in string ivKey) => CompressArchive(inputArchive, outputArchive, password, ivKey, CompressionLevel.Optimal);

    /// <summary>
    /// Compresses the encrypted archive.
    /// </summary>
    /// <param name="inputArchive">The input archive.</param>
    /// <param name="outputArchive">The output archive.</param>
    /// <param name="password">The archive's password.</param>
    /// <param name="compression">Sets the compression level.</param>
    public static void CompressArchive(in string inputArchive, in string outputArchive, in string password, CompressionLevel compression)
    {
        CreateKey(out Aes encryptionKit, password);
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, encryptionKit.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
        {
            inputStream.CopyTo(compressorStream);
        }
        encryptionKit.Dispose();
    }

    public static void CompressArchive(in string inputArchive, in string outputArchive, in string password, in string IVkey, CompressionLevel compression)
    {
        CreateKey(out Aes encryptionKit, password, IVkey);
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, encryptionKit.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
        {
            inputStream.CopyTo(compressorStream);
        }
        encryptionKit.Dispose();
    }

    /// <summary>
    /// Decompresses the archive.
    /// </summary>
    /// <param name="inputArchive">The input archive.</param>
    /// <param name="password">The password of the archive.</param>
    /// <param name="workerArchive">The location for the temp file (that will hold the decompressed archive).</param>
    public static void DecompressArchive(in string inputArchive, in string workerArchive, in string password)
    {
        CreateKey(out Aes encryptionKit, password);
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(workerArchive))
        using (CryptoStream cryptStream = new(inputStream, encryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
        {
            decompressorStream.CopyTo(outputStream);
        }
        encryptionKit.Dispose();
    }

    public static void DecompressArchive(in string inputArchive, in string workerArchive, in string password, in string IVkey)
    {
        CreateKey(out Aes encryptionKit, password, IVkey);
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(workerArchive))
        using (CryptoStream cryptStream = new(inputStream, encryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
        {
            decompressorStream.CopyTo(outputStream);
        }
        encryptionKit.Dispose();
    }

#if NET8_0_OR_GREATER

    public static void CompressArchive(in MemoryStream inputArchive, in string outputArchive, in string password) => CompressArchive(inputArchive, outputArchive, password, CompressionLevel.Optimal);

    public static void CompressArchive(in MemoryStream inputArchive, in string outputArchive, in string password, in string ivKey) => CompressArchive(inputArchive, outputArchive, password, ivKey, CompressionLevel.Optimal);

    /// <summary>
    /// Compresses the encrypted archive.
    /// </summary>
    /// <param name="inputArchive">The input archive.</param>
    /// <param name="outputArchive">The output archive.</param>
    /// <param name="password">The archive's password.</param>
    /// <param name="compression">Sets the compression level.</param>
    public static void CompressArchive(in MemoryStream inputArchive, in string outputArchive, in string password, CompressionLevel compression)
    {
        inputArchive.Position = 0;
        CreateKey(out Aes encryptionKit, password);
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, encryptionKit.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
        {
            inputArchive.CopyTo(compressorStream);
        }
        encryptionKit.Dispose();
    }

    public static void CompressArchive(in MemoryStream inputArchive, in string outputArchive, in string password, in string IVkey, CompressionLevel compression)
    {
        inputArchive.Position = 0;
        CreateKey(out Aes encryptionKit, password, IVkey);
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, encryptionKit.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
        {
            inputArchive.CopyTo(compressorStream);
        }
        encryptionKit.Dispose();
    }

    /// <summary>
    /// Decompresses the archive.
    /// </summary>
    /// <param name="inputArchive">The input archive.</param>
    /// <param name="password">The password of the archive.</param>
    /// <param name="workerArchive">The location for the temp file (that will hold the decompressed archive).</param>
    public static void DecompressArchive(in Stream inputStream, out MemoryStream uncompressedStream, in string password)
    {
        inputStream.Position = 0;
        uncompressedStream = new();
        CreateKey(out Aes encryptionKit, password);
        using (CryptoStream cryptStream = new(inputStream, encryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
        {
            decompressorStream.CopyTo(uncompressedStream);
        }
        encryptionKit.Dispose();
    }

    public static void DecompressArchive(in Stream inputStream, out MemoryStream uncompressedStream, in string password, in string IVkey)
    {
        inputStream.Position = 0;
        uncompressedStream = new();
        CreateKey(out Aes encryptionKit, password, IVkey);
        using (CryptoStream cryptStream = new(inputStream, encryptionKit.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
        {
            decompressorStream.CopyTo(uncompressedStream);
        }
        encryptionKit.Dispose();
    }
#endif

    /// <summary>
    /// Sets up the AES Encryptor/Decryptor
    /// </summary>
    /// <param name="encryptor">The Aes Encryptor/Decryptor used in code.</param>
    /// <param name="password">The password of the archive.</param>
    private static void CreateKey (out Aes encryptor, in string password)
    {
        encryptor = Aes.Create();
        encryptor.KeySize = 256;
        encryptor.Padding = PaddingMode.PKCS7;
        byte[] hashCode = SHA512.HashData(Encoding.UTF8.GetBytes(password));
        Rfc2898DeriveBytes key = new(password, hashCode, 50000, HashAlgorithmName.SHA512);
        encryptor.Key = key.GetBytes(encryptor.KeySize / 8);
        encryptor.IV = key.GetBytes(encryptor.BlockSize / 8);
        key.Dispose();
    }

    /// <summary>
    /// Sets up the Aes Encryptor/Decryptor with IV used in code.
    /// </summary>
    /// <param name="encryptor">The Aes Encryptor/Decryptor to initialize.</param>
    /// <param name="password">The password of the archive.</param>
    /// <param name="ivKey">The IV for the archive.</param>
    private static void CreateKey (out Aes encryptor, in string password, in string ivKey)
    {
        encryptor = Aes.Create();
        encryptor.KeySize = 256;
        encryptor.Padding = PaddingMode.PKCS7;
        byte[] hashCode = SHA512.HashData(Encoding.UTF8.GetBytes(password));
        byte[] hashIV = SHA512.HashData(Encoding.UTF8.GetBytes(ivKey));
        Rfc2898DeriveBytes key = new(password, hashCode, 50000, HashAlgorithmName.SHA512);
        Rfc2898DeriveBytes vectorKey = new(ivKey, hashIV, 50000, HashAlgorithmName.SHA512);
        encryptor.Key = key.GetBytes(encryptor.KeySize / 8);
        encryptor.IV = vectorKey.GetBytes(encryptor.BlockSize / 8);
        key.Dispose();
        vectorKey.Dispose();
    }

    /// <summary>
    /// Creates a Halva package from a folder.
    /// </summary>
    /// <param name="input">The source folder that the archive will be created from.</param>
    /// <param name="archiveLocation">The location of the final archive.</param>
    /// <param name="password">The archive's password.</param>
    public static void CreateArchiveFromFolder(in string input, in string archiveLocation, in string password)
    {
#if NET8_0_OR_GREATER
        CreateArchive(input, archiveLocation, true, password);
#else
        Random random = new();
        string archive = TempArchive + random.Next(9999) + ".tmp";
        if (File.Exists(archive)) File.Delete(archive);
        ZipFile.CreateFromDirectory(input, archive, CompressionLevel.NoCompression, false);
        CompressArchive(archive, archiveLocation, password);
        File.Delete(archive);
#endif
    }

    public static void CreateArchiveFromFolder(in string input, in string archiveLocation, in string password, in string iv)
    {
#if NET8_0_OR_GREATER
        CreateArchive(input, archiveLocation, true, password, iv);
#else
        Random random = new();
        string archive = TempArchive + random.Next(9999) + ".tmp";
        if (File.Exists(archive)) File.Delete(archive);
        ZipFile.CreateFromDirectory(input, archive, CompressionLevel.NoCompression, false);
        CompressArchive(archive, archiveLocation, password, iv);
        File.Delete(archive);
#endif
    }

    /// <summary>
    /// Exports all files from an encrypted Halva package.
    /// </summary>
    /// <param name="inputArchive">The archive you want to decompress.</param>
    /// <param name="exportDestination">The location where the contents of the archive will be exported.</param>
    /// <param name="password">The archive's password.</param>
    public static void ExportFromArchive(in string inputArchive, in string exportDestination, in string password)
    {
#if NET8_0_OR_GREATER
        ExportFiles(inputArchive, exportDestination, true, password);
#else
        Random random = new();
        string archive = TempArchive + random.Next(9999) + ".tmp";
        if (File.Exists(archive)) File.Delete(archive);
        if (!Directory.Exists(exportDestination)) Directory.CreateDirectory(exportDestination);
        DecompressArchive(inputArchive, archive, password);
        ZipFile.ExtractToDirectory(archive, exportDestination, true);
        File.Delete(archive);
#endif

    }

    public static void ExportFromArchive(in string inputArchive, in string exportDestination, in string password, in string ivKey)
    {
#if NET8_0_OR_GREATER
        ExportFiles(inputArchive, exportDestination, true, password, ivKey);
#else
        Random random = new();
        string archive = TempArchive + random.Next(9999) + ".tmp";
        if (File.Exists(archive)) File.Delete(archive);
        if (!Directory.Exists(exportDestination)) Directory.CreateDirectory(exportDestination);
        DecompressArchive(inputArchive, archive, password, ivKey);
        ZipFile.ExtractToDirectory(archive, exportDestination, true);
        File.Delete(archive);
#endif

    }
#if NET8_0_OR_GREATER
    /// <summary>
    /// Creates a Halva package from a folder.
    /// </summary>
    /// <param name="input">The source folder that the archive will be created from.</param>
    /// <param name="archiveLocation">The location of the final archive.</param>
    /// <param name="password">The archive's password.</param>
    public static void CreateArchive(in string input, in string archiveLocation, bool useMemoryStream, in string password)
    {
        if (useMemoryStream)
        {
            MemoryStream stream = new();
            ZipFile.CreateFromDirectory(input, stream, CompressionLevel.NoCompression, false);
            CompressArchive(stream, archiveLocation, password);
        }
        else
        {
            Random random = new();
            string archive = TempArchive + random.Next(9999) + ".tmp";
            if (File.Exists(archive)) File.Delete(archive);
            ZipFile.CreateFromDirectory(input, archive, CompressionLevel.NoCompression, false);
            CompressArchive(archive, archiveLocation, password);
            File.Delete(archive);
        }

    }

    public static void CreateArchive(in string input, in string archiveLocation, bool useMemoryStream, in string password, in string iv)
    {
        if (useMemoryStream)
        {
            MemoryStream stream = new();
            ZipFile.CreateFromDirectory(input, stream, CompressionLevel.NoCompression, false);
            CompressArchive(stream, archiveLocation, password, iv);
        }
        else
        {
            Random random = new();
            string archive = TempArchive + random.Next(9999) + ".tmp";
            if (File.Exists(archive)) File.Delete(archive);
            ZipFile.CreateFromDirectory(input, archive, CompressionLevel.NoCompression, false);
            CompressArchive(archive, archiveLocation, password, iv);
            File.Delete(archive);
        }
    }
    public static void ExportFiles(in string inputArchive, in string exportDestination, bool useMemoryStream, in string password)
    {
        if (useMemoryStream)
        {
            MemoryStream uncompressedStream;
            DecompressArchive(File.OpenRead(inputArchive), out uncompressedStream, password);
            ZipFile.ExtractToDirectory(uncompressedStream, exportDestination, true);
            uncompressedStream.Close();
        }
        else
        {
            Random random = new();
            string archive = TempArchive + random.Next(9999) + ".tmp";
            if (File.Exists(archive)) File.Delete(archive);
            if (!Directory.Exists(exportDestination)) Directory.CreateDirectory(exportDestination);
            DecompressArchive(inputArchive, archive, password);
            ZipFile.ExtractToDirectory(archive, exportDestination, true);
            File.Delete(archive);
        }

    }

    public static void ExportFiles(in string inputArchive, in string exportDestination, bool useMemoryStream, in string password, in string ivKey)
    {
        if (useMemoryStream)
        {
            MemoryStream uncompressedStream;
            DecompressArchive(File.OpenRead(inputArchive), out uncompressedStream, password, ivKey);
            ZipFile.ExtractToDirectory(uncompressedStream, exportDestination, true);
            uncompressedStream.Close();
        }
        else
        {
            Random random = new();
            string archive = TempArchive + random.Next(9999) + ".tmp";
            if (File.Exists(archive)) File.Delete(archive);
            if (!Directory.Exists(exportDestination)) Directory.CreateDirectory(exportDestination);
            DecompressArchive(inputArchive, archive, password, ivKey);
            ZipFile.ExtractToDirectory(archive, exportDestination, true);
            File.Delete(archive);
        }
    }
#endif
}
