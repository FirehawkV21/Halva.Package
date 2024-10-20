using System.IO.Compression;
using System.Runtime.Versioning;
using System.Security.Cryptography;

namespace Halva.Package.Core;
internal sealed class CompressorEngine
{
    #region Unencrypted Compression Methods
    /// <summary>
    /// Compresses the archive, with a selected compression level.
    /// </summary>
    /// <param name="inputArchive"></param>
    /// <param name="outputArchive"></param>
    /// <param name="Compression"></param>
    internal void CompressFile(in MemoryStream inputArchive, in string outputArchive, CompressionLevel Compression = CompressionLevel.Optimal)
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
    internal void CompressFile(in string inputArchive, in string outputArchive, CompressionLevel Compression = CompressionLevel.Optimal)
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
    internal void DecompressFile(in Stream inputStream, out MemoryStream uncompressedStream)
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
    internal void DecompressFile(in string inputArchive, in string workerArchive)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(workerArchive))
        using (BrotliStream decompressorStream = new(inputStream, CompressionMode.Decompress))
        {
            decompressorStream.CopyTo(outputStream);
        }
    }
    #endregion

    #region Unencreypted Files Methods (Async)
    /// <summary>
    /// Compresses the archive, with a selected compression level.
    /// </summary>
    /// <param name="inputArchive"></param>
    /// <param name="outputArchive"></param>
    /// <param name="Compression"></param>
    internal async Task CompressFileAsync(Stream inputArchive, string outputArchive, CompressionLevel Compression = CompressionLevel.Optimal, CancellationToken abortToken = default)
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
    internal async Task CompressFileAsync(string inputArchive, string outputArchive, CompressionLevel Compression = CompressionLevel.Optimal, CancellationToken abortToken = default)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(outputArchive))
        using (BrotliStream compressorStream = new(outputStream, Compression))
        {
            await inputStream.CopyToAsync(compressorStream);
        }
    }

    /// <summary>
    /// Decompresses the archive.
    /// </summary>
    /// <param name="inputStream">The input archive in a stream.</param>
    /// <param name="uncompressedStream">The stream that will accept the uncompressed Stream.</param>
    internal async Task DecompressFileAsync(Stream inputStream, MemoryStream uncompressedStream, CancellationToken abortToken = default)
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
    internal async Task DecompressFileAsync(string inputArchive, string workerArchive, CancellationToken abortToken = default)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(workerArchive))
        using (BrotliStream decompressorStream = new(inputStream, CompressionMode.Decompress))
        {
            await decompressorStream.CopyToAsync(outputStream, abortToken);
        }
    }
    #endregion

    #region Main Encrypted Package Compression Code
    internal void CompressEncryptedFile(ref Aes encryptionKey, string inputArchive, string outputArchive, CompressionLevel compression)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, encryptionKey.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
            inputStream.CopyTo(compressorStream);
        encryptionKey.Dispose();
    }

    internal void CompressEncryptedFile(ref Aes encryptionKey, in Stream inputArchive, in string outputArchive, in CompressionLevel compression = CompressionLevel.Optimal)
    {
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, encryptionKey.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
            inputArchive.CopyTo(compressorStream);
        encryptionKey.Dispose();
    }

    [SupportedOSPlatform("windows")]
    internal void CompressEncryptedFile(ref AesCng encryptionKey, string inputArchive, string outputArchive, CompressionLevel compression = CompressionLevel.Optimal)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, encryptionKey.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
            inputStream.CopyTo(compressorStream);
        encryptionKey.Dispose();
    }

    [SupportedOSPlatform("windows")]
    internal void CompressEncryptedFile(ref AesCng encryptionKey, in Stream inputStream, in string outputArchive, in CompressionLevel compression = CompressionLevel.Optimal)
    {
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, encryptionKey.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
            inputStream.CopyTo(compressorStream);
        encryptionKey.Dispose();
    }
    #endregion

    #region Main Encrypted Package Compression Code (Async)
    internal async Task CompressEncryptedFileAsync(Aes aesEncryptionKey, string inputArchive, string outputArchive, CompressionLevel compression, CancellationToken abortToken)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, aesEncryptionKey.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
            await inputStream.CopyToAsync(compressorStream, abortToken);
    }

    [SupportedOSPlatform("windows")]
    internal async Task CompressEncryptedFileAsync(AesCng cngEncryptionKey, string inputArchive, string outputArchive, CompressionLevel compression = CompressionLevel.Optimal, CancellationToken abortToken = default)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, cngEncryptionKey.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
            await inputStream.CopyToAsync(compressorStream, abortToken);
    }

    internal async Task CompressEncryptedFileAsync(Aes encryptionKey, Stream inputArchive, string outputArchive, CompressionLevel compression = CompressionLevel.Optimal, CancellationToken abortToken = default)
    {
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, encryptionKey.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
            await inputArchive.CopyToAsync(compressorStream, abortToken);
    }

    [SupportedOSPlatform("windows")]
    internal async Task CompressEncryptedFileAsync(AesCng cngEncryptionKey, Stream inputStream, string outputArchive, CompressionLevel compression = CompressionLevel.Optimal, CancellationToken abortToken = default)
    {
        using (FileStream outputStream = File.Create(outputArchive))
        using (CryptoStream cryptStream = new(outputStream, cngEncryptionKey.CreateEncryptor(), CryptoStreamMode.Write))
        using (BrotliStream compressorStream = new(cryptStream, compression))
            await inputStream.CopyToAsync(compressorStream, abortToken);
    }
    #endregion

    #region Main Encrypted Package Decompression Code

    internal void DecompressEncryptedFile(ref Aes encryptionKey, in string inputArchive, in string workerArchive)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(workerArchive))
        using (CryptoStream cryptStream = new(inputStream, encryptionKey.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
            decompressorStream.CopyTo(outputStream);
        encryptionKey.Dispose();
    }

    [SupportedOSPlatform("windows")]
    internal void DecompressEncryptedFile(ref AesCng encryptionKey, in string inputArchive, in string workerArchive)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(workerArchive))
        using (CryptoStream cryptStream = new(inputStream, encryptionKey.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
            decompressorStream.CopyTo(outputStream);
        encryptionKey.Dispose();
    }

    [SupportedOSPlatform("windows")]
    internal void DecompressEncryptedFile(ref AesCng encryptionKey, Stream inputStream, MemoryStream uncompressedStream)
    {
        using (CryptoStream cryptStream = new(inputStream, encryptionKey.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
            decompressorStream.CopyTo(uncompressedStream);
        encryptionKey.Dispose();
    }

    internal void DecompressEncryptedFile(ref Aes encryptionKey, Stream inputStream, in MemoryStream uncompressedStream)
    {
        using (CryptoStream cryptStream = new(inputStream, encryptionKey.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
            decompressorStream.CopyTo(uncompressedStream);
        encryptionKey.Dispose();
    }
    #endregion

    #region Main Encrypted Package Decompression Code (Async)

    internal async Task DecompressEncryptedFileAsync(Aes encryptionKey, string inputArchive, string workerArchive, CancellationToken abortToken = default)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(workerArchive))
        using (CryptoStream cryptStream = new(inputStream, encryptionKey.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
            await decompressorStream.CopyToAsync(outputStream, abortToken);
        encryptionKey.Dispose();
    }

    [SupportedOSPlatform("windows")]
    internal async Task DecompressEncryptedFileAsync(AesCng encryptionKey, string inputArchive, string workerArchive, CancellationToken abortToken = default)
    {
        using (FileStream inputStream = File.OpenRead(inputArchive))
        using (FileStream outputStream = File.Create(workerArchive))
        using (CryptoStream cryptStream = new(inputStream, encryptionKey.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
            await decompressorStream.CopyToAsync(outputStream, abortToken);
        encryptionKey.Dispose();
    }

    [SupportedOSPlatform("windows")]
    internal async Task DecompressEncryptedFileAsync(AesCng encryptionKey, Stream inputStream, MemoryStream uncompressedStream, CancellationToken abortToken = default)
    {
        using (CryptoStream cryptStream = new(inputStream, encryptionKey.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
            await decompressorStream.CopyToAsync(uncompressedStream, abortToken);
    }

    internal async Task DecompressEncryptedFileAsync(Aes encryptionKey, Stream inputStream, MemoryStream uncompressedStream, CancellationToken abortToken = default)
    {
        using (CryptoStream cryptStream = new(inputStream, encryptionKey.CreateDecryptor(), CryptoStreamMode.Read))
        using (BrotliStream decompressorStream = new(cryptStream, CompressionMode.Decompress))
            await decompressorStream.CopyToAsync(uncompressedStream, abortToken);
    }
    #endregion
}
