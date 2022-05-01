using System.IO.Compression;
using System.Runtime.InteropServices;
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

    /// <summary>
    /// Compresses the encrypted archive.
    /// </summary>
    /// <param name="inputArchive">The input archive.</param>
    /// <param name="outputArchive">The output archive.</param>
    /// <param name="password">The archive's password.</param>
    /// <param name="compression">Sets the compression level.</param>
    public static void CompressArchive(in string inputArchive, in string outputArchive, in string password, CompressionLevel compression)
    {
        Aes encryptionKit;
        CreateKey(out encryptionKit, password);
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
        Aes encryptionKit;
        CreateKey(out encryptionKit, password, IVkey);
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
        Random random = new();
        string archive = TempArchive + random.Next(9999) + ".tmp";
        if (File.Exists(archive)) File.Delete(archive);
        ZipFile.CreateFromDirectory(input, archive, CompressionLevel.NoCompression, false);
        CompressArchive(archive, archiveLocation, password);
        File.Delete(archive);
    }

    /// <summary>
    /// Exports all files from an encrypted Halva package.
    /// </summary>
    /// <param name="inputArchive">The archive you want to decompress.</param>
    /// <param name="exportDestination">The location where the contents of the archive will be exported.</param>
    /// <param name="password">The archive's password.</param>
    public static void ExportFromArchive(in string inputArchive, in string exportDestination, in string password)
    {
        Random random = new();
        string archive = TempArchive + random.Next(9999) + ".tmp";
        if (File.Exists(archive)) File.Delete(archive);
        DecompressArchive(inputArchive, archive, password);
        ZipFile.ExtractToDirectory(archive, exportDestination, true);
        File.Delete(archive);

    }
}
